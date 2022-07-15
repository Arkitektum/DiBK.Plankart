using DiBK.Plankart.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Controllers;

[ApiController]
[Route("[controller]")]
public class CesiumIonAssetController : BaseController
{
    private readonly ICesiumIonAssetService _cesiumIonAssetService;
    private readonly IUnitOfWork _unitOfWork;

    public CesiumIonAssetController(
        ICesiumIonAssetService cesiumIonAssetService,
        IUnitOfWork unitOfWork,
        ILogger<CesiumIonAssetController> logger) : base(logger)
    {
        _cesiumIonAssetService = cesiumIonAssetService;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("/TerrainData")]
    public async Task<IActionResult> CreateTerrainData([FromBody] TerrainLocation terrainLocation)
    {
        try
        {
            await TryPreProcessAssetCleanup();

            var enclosingAsset = _unitOfWork.TerrainResourceRepository.FirstOrDefault(a => a.Encloses(terrainLocation.Envelope));
            if (enclosingAsset != null)
            {
                enclosingAsset.LastAccessed = DateTime.Now;
                enclosingAsset.NumberOfUsages++;
                await _unitOfWork.CommitAsync();
                return Ok(enclosingAsset.CesiumIonAssetId);
            }

            var cesiumIonAsset = await _cesiumIonAssetService.CreateTerrainResourceAsync(terrainLocation);

            if (cesiumIonAsset == null)
                return Problem(detail: "Was not able to create terrain resource", statusCode: 500);

            _unitOfWork.TerrainResourceRepository.Add(cesiumIonAsset);
            await TryPostProcessAssetCleanup(cesiumIonAsset);
            await _unitOfWork.CommitAsync();

            return Ok(cesiumIonAsset.CesiumIonAssetId);
        }
        catch (Exception exception)
        {
            var result = HandleException(exception);

            if (result != null)
                return result;

            throw;
        }
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

    private async Task DeleteRedundantAssetsAsync(CesiumIonTerrainResource terrainResource)
    {
        var idsOfCesiumAssetsToDelete =
            _unitOfWork.TerrainResourceRepository.DeleteRedundantAssets(terrainResource);

        if (idsOfCesiumAssetsToDelete == null)
            return;

        Debug.WriteLine($"Deleting asset(s): {idsOfCesiumAssetsToDelete.Aggregate("", (s, i) => $"{s}, {i.ToString()}")}");

        await _cesiumIonAssetService.DeleteAssetsFromIdsAsync(idsOfCesiumAssetsToDelete);
    }

    private async Task DeleteCorruptAssetsAsync()
    {
        var idsOfCesiumAssetsToDelete = await _cesiumIonAssetService.DeleteCorruptAssetsAsync();

        if (idsOfCesiumAssetsToDelete == null || !idsOfCesiumAssetsToDelete.Any())
            return;

        Debug.WriteLine($"Deleting asset(s): {idsOfCesiumAssetsToDelete.Aggregate("", (s, i) => $"{s}, {i.ToString()}")}");

        _unitOfWork.TerrainResourceRepository.RemoveRangeByCesiumIonAssetsIds(idsOfCesiumAssetsToDelete);
    }

    private async Task DeleteUnmappedAssetsAsync()
    {
        var assets = await _cesiumIonAssetService.GetAssetsAsync();
        _unitOfWork.TerrainResourceRepository.DeleteUnmappedTerrainResources(assets);

        var resources = _unitOfWork.TerrainResourceRepository.GetAll();
        await _cesiumIonAssetService.DeleteUnmappedAssetsAsync(resources);
    }

    private async Task DeleteStaleAssets()
    {
        var resources = _unitOfWork.TerrainResourceRepository.GetAll();
        var idsOfCesiumAssetsToDelete = await _cesiumIonAssetService.MakeSureMax4GbIsUsedAsync(resources.ToList());

        Debug.WriteLine($"Deleting asset(s): {idsOfCesiumAssetsToDelete.Aggregate("", (s, i) => $"{s}, {i.ToString()}")}");

        _unitOfWork.TerrainResourceRepository.RemoveRangeByCesiumIonAssetsIds(idsOfCesiumAssetsToDelete);
    }
}