using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Arkitektum.Cesium.Ion.RestApiSharp.Models;
using DiBK.Plankart.Application.Extensions;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Application.Services;

public class CesiumIonTerrainAssetManager : ICesiumIonTerrainAssetManager
{
    private readonly ICesiumIonAssetService _cesiumIonAssetService;
    private readonly ITerrainResourceService _terrainResourceService;
    private readonly IUnitOfWork _unitOfWork;

    public CesiumIonTerrainAssetManager(
        ICesiumIonAssetService cesiumIonAssetService,
        ITerrainResourceService terrainResourceService,
        IUnitOfWork unitOfWork)
    {
        _cesiumIonAssetService = cesiumIonAssetService;
        _terrainResourceService = terrainResourceService;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> GetCesiumTerrainAssetId(TerrainLocation terrainLocation)
    {
        await TryPreProcessAssetCleanup();

        var enclosingAsset = _unitOfWork.TerrainResourceRepository.FirstOrDefault(a => a.Encloses(terrainLocation.Envelope));
        if (enclosingAsset != null)
        {
            enclosingAsset.LastAccessed = DateTime.Now;
            enclosingAsset.NumberOfUsages++;
            await _unitOfWork.CommitAsync();
            return enclosingAsset.CesiumIonAssetId;
        }

        var cesiumIonAsset = await CreateTerrainResourceAsync(terrainLocation);

        if (cesiumIonAsset == null)
            return 0;

        _unitOfWork.TerrainResourceRepository.Add(cesiumIonAsset);
        await TryPostProcessAssetCleanup(cesiumIonAsset);
        await _unitOfWork.CommitAsync();

        return cesiumIonAsset.CesiumIonAssetId;
    }

    private async Task TryPreProcessAssetCleanup()
    {
        try
        {
            Debug.WriteLine("Deleting corrupt assets..");
            await DeleteCorruptAssetsAsync();

            Debug.WriteLine("Deleting unmapped assets..");
            await DeleteUnmappedAssetsAsync();

            Debug.WriteLine("Deleting stale assets..");
            await DeleteStaleAssets();
        }
        catch (Exception e)
        {
            Debug.WriteLine("PreProcess cleanup failed: \n" + e);
        }
    }

    private async Task TryPostProcessAssetCleanup(CesiumIonTerrainResource terrainResource)
    {
        try
        {
            Debug.WriteLine("Deleting redundant assets..");
            await DeleteRedundantAssetsAsync(terrainResource);
        }
        catch (Exception e)
        {
            Debug.WriteLine("PostProcess cleanup failed: \n" + e);
        }
    }


    public async Task<CesiumIonTerrainResource> CreateTerrainResourceAsync(TerrainLocation terrainLocation)
    {
        return await _terrainResourceService.CreateTerrainResourceAsync(terrainLocation);
    }

    private async Task<List<AssetMetadata>> GetCorruptAssetsAsync()
    {
        var assets = await _cesiumIonAssetService.GetAssetsAsync();
        return assets?.Where(a => a.IsCorrupt()).ToList();
    }

    private async Task DeleteRedundantAssetsAsync(CesiumIonTerrainResource terrainResource)
    {
        var idsOfCesiumAssetsToDelete =
            _unitOfWork.TerrainResourceRepository.DeleteRedundantAssets(terrainResource);

        if (idsOfCesiumAssetsToDelete == null)
            return;

        Debug.WriteLine($"Deleting asset(s): {idsOfCesiumAssetsToDelete.Aggregate("", (s, i) => $"{s}, {i}")}");

        await _cesiumIonAssetService.DeleteAssetsAsync(idsOfCesiumAssetsToDelete);
    }

    private async Task DeleteCorruptAssetsAsync()
    {
        var idsOfCesiumAssetsToDelete = await _cesiumIonAssetService.DeleteAssetsAsync(await GetCorruptAssetsAsync());

        if (idsOfCesiumAssetsToDelete == null || !idsOfCesiumAssetsToDelete.Any())
            return;

        Debug.WriteLine($"Deleting asset(s): {idsOfCesiumAssetsToDelete.Aggregate("", (s, i) => $"{s}, {i}")}");

        _unitOfWork.TerrainResourceRepository.RemoveRangeByCesiumIonAssetsIds(idsOfCesiumAssetsToDelete);
    }

    private async Task DeleteUnmappedAssetsAsync()
    {
        var assets = await _cesiumIonAssetService.GetAssetsAsync();

        if (assets == null || !assets.Any())
            return;

        _unitOfWork.TerrainResourceRepository.DeleteUnmappedTerrainResources(assets);

        var resources = _unitOfWork.TerrainResourceRepository.GetAll();

        if (resources == null)
            return;

        // 1179236 is the ID of an asset that should not be deleted.
        // todo: Consider creating a separate Cesium Ion account for the terrain models
        foreach (var asset in assets.Where(a => a.Id != 1179236 && resources.All(r => r.CesiumIonAssetId != a.Id)))
            await _cesiumIonAssetService.DeleteAssetAsync(asset.Id);
    }

    private async Task DeleteStaleAssets()
    {
        var resources = _unitOfWork.TerrainResourceRepository.GetAll();
        var idsOfCesiumAssetsToDelete = await MakeSureMax4GbIsUsedAsync(resources.ToList());

        Debug.WriteLine($"Deleting asset(s): {idsOfCesiumAssetsToDelete.Aggregate("", (s, i) => $"{s}, {i}")}");

        _unitOfWork.TerrainResourceRepository.RemoveRangeByCesiumIonAssetsIds(idsOfCesiumAssetsToDelete);
    }

    private async Task<IEnumerable<int>> MakeSureMax4GbIsUsedAsync(List<CesiumIonTerrainResource> terrainResources)
    {
        var assets = await _cesiumIonAssetService.GetAssetsAsync();

        if (assets == null || !assets.Any())
            return null;

        var totalBytes = assets.Aggregate(0L, (i, asset) => i + asset.Bytes);
        var totalGigaBytes = totalBytes / Math.Pow(1024, 3);

        if (totalGigaBytes < 4)
            return null;

        var idsOfTerrainResourcesToDelete = new List<int>();

        terrainResources.Sort((a1, a2) => a1.PriorityScore.CompareTo(a2.PriorityScore));
        var assetIndex = 0;
        while (totalGigaBytes > 4)
        {
            var terrainResourceToDelete = terrainResources[assetIndex];

            idsOfTerrainResourcesToDelete.Add(terrainResourceToDelete.Id);

            var assetToDelete = assets.FirstOrDefault(a => a.Id == terrainResourceToDelete.CesiumIonAssetId);

            if (assetToDelete == default(AssetMetadata))
                continue;

            await _cesiumIonAssetService.DeleteAssetAsync(terrainResourceToDelete.CesiumIonAssetId);

            totalGigaBytes -= assetToDelete.Bytes / Math.Pow(1024, 3);
            assetIndex++;
        }

        return idsOfTerrainResourcesToDelete;
    }
}