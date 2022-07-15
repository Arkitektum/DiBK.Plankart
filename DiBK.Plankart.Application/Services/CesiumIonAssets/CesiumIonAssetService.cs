using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arkitektum.Cesium.Ion.RestApiSharp;
using Arkitektum.Cesium.Ion.RestApiSharp.Models;
using DiBK.Plankart.Application.Extensions;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Application.Services;

public class CesiumIonAssetService : ICesiumIonAssetService
{
    private readonly ITerrainResourceService _terrainResourceService;
    private readonly CesiumIonClient _client;

    public CesiumIonAssetService(
        ITerrainResourceService terrainResourceService,
        IAccessTokenProvider accessTokenProvider)
    {
        _terrainResourceService = terrainResourceService;
        _client = new CesiumIonClient(accessTokenProvider.CesiumIonToken());
    }

    public async Task<List<AssetMetadata>> GetAssetsAsync()
    {
        return await _client.GetAssetListAsync();
    }

    public async Task<IEnumerable<int>> DeleteCorruptAssetsAsync()
    {
        return await DeleteAssetsAsync(await GetCorruptAssetsAsync());
    }

    public async Task<IEnumerable<int>> MakeSureMax4GbIsUsedAsync(List<CesiumIonTerrainResource> terrainResources)
    {
        var assets = await _client.GetAssetListAsync();

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

            await _client.DeleteAssetAsync(terrainResourceToDelete.CesiumIonAssetId);

            totalGigaBytes -= assetToDelete.Bytes / Math.Pow(1024, 3);
            assetIndex++;
        }

        return idsOfTerrainResourcesToDelete;
    }

    public async Task DeleteAssetsFromIdsAsync(IEnumerable<int> assetIds)
    {
        foreach (var assetId in assetIds)
            await _client.DeleteAssetAsync(assetId);
    }

    public async Task<CesiumIonTerrainResource> CreateTerrainResourceAsync(TerrainLocation terrainLocation)
    {
        return await _terrainResourceService.CreateTerrainResourceAsync(terrainLocation);
    }

    public async Task DeleteUnmappedAssetsAsync(IEnumerable<CesiumIonTerrainResource> resources)
    {
        if (resources == null)
            return;
        
        var assets = await _client.GetAssetListAsync();

        if (assets == null || !assets.Any())
            return;

        // 1179236 is the ID of an asset that should not be deleted.
        // todo: Consider creating a separate Cesium Ion account for the terrain models
        foreach (var asset in assets.Where(a => a.Id != 1179236 && resources.All(r => r.CesiumIonAssetId != a.Id)))
            await _client.DeleteAssetAsync(asset.Id);
    }

    private async Task<List<AssetMetadata>> GetCorruptAssetsAsync()
    {
        var assets = await _client.GetAssetListAsync();
        return assets?.Where(a => a.IsCorrupt()).ToList();
    }

    private async Task<IEnumerable<int>> DeleteAssetsAsync(List<AssetMetadata> assets)
    {
        if (assets == null)
            return null;

        foreach (var asset in assets)
            await _client.DeleteAssetAsync(asset.Id);

        return assets.Select(a => a.Id);
    }
}