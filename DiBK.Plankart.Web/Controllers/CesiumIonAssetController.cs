using DiBK.Plankart.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arkitektum.Cesium.Ion.RestApiSharp;
using Arkitektum.Cesium.Ion.RestApiSharp.Util;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Controllers;

[ApiController]
[Route("[controller]")]
public class CesiumIonAssetController : BaseController
{
    private readonly ITerrainResourceService _terrainResourceService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CesiumIonClient _cesiumIonClient;

    public CesiumIonAssetController(
        ITerrainResourceService terrainResourceService,
        IUnitOfWork unitOfWork,
        IAccessTokenProvider accessTokenProvider,
        ILogger<CesiumIonAssetController> logger) : base(logger)
    {
        _terrainResourceService = terrainResourceService;
        _unitOfWork = unitOfWork;
        _cesiumIonClient = new CesiumIonClient(accessTokenProvider.CesiumIonToken());
    }

    [HttpPost("/TerrainData")]
    public async Task<IActionResult> CreateTerrainData([FromBody] TerrainRequest terrainRequest)
    {
        try
        {
            var enclosingAsset = _unitOfWork.AssetRepository.Find(a => a.Encloses(terrainRequest.Envelope));
            if (enclosingAsset != null)
            {
                enclosingAsset.LastAccessed = DateTime.Now;
                enclosingAsset.NumberOfUsages++;
                await _unitOfWork.CommitAsync();
                return Ok(enclosingAsset.CesiumIonAssetId);
            }

            var cesiumIonAsset = await _terrainResourceService.CreateTerrainResourceAsync(terrainRequest);

            if (cesiumIonAsset == null)
                return Problem(detail: "Was not able to create terrain resource", statusCode: 500);

            _unitOfWork.AssetRepository.Add(cesiumIonAsset);
            await AssetCleanup(cesiumIonAsset);
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

    private async Task AssetCleanup(CesiumIonAsset asset)
    {
        await DeleteRedundantAssets(asset);

        await DeleteCorruptAssets();

        await MakeSureMax4GbIsUsed();
    }

    private async Task DeleteRedundantAssets(CesiumIonAsset asset)
    {
        var assetsToDelete = _unitOfWork.AssetRepository.Entities.Where(e => asset.EnclosesWithMargin(e, 500.0));

        if (!assetsToDelete.Any())
            return;

        foreach (var assetToDelete in assetsToDelete)
            await _cesiumIonClient.DeleteAssetAsync(assetToDelete.CesiumIonAssetId);

        _unitOfWork.AssetRepository.RemoveRange(assetsToDelete);
    }

    private async Task DeleteCorruptAssets()
    {
        var assets = await _cesiumIonClient.GetAssetListAsync();

        if (assets == null || !assets.Any())
            return;

        foreach (var asset in assets.Where(a =>
                     a.Status is AssetStatus.DATA_ERROR or AssetStatus.ERROR || 
                     a.Status != AssetStatus.COMPLETE && a.DateAdded.CompareTo(DateTime.Now.AddDays(-1)) < 0))
        {
            await _cesiumIonClient.DeleteAssetAsync(asset.Id);
            var dbAsset = _unitOfWork.AssetRepository.Find(e => e.CesiumIonAssetId == asset.Id);
            if (dbAsset != null)
                _unitOfWork.AssetRepository.Remove(dbAsset);
        }

        await _unitOfWork.CommitAsync();
    }

    private async Task MakeSureMax4GbIsUsed()
    {
        var cesiumAssets = await _cesiumIonClient.GetAssetListAsync();
        if (cesiumAssets == null || !cesiumAssets.Any())
            return;

        var totalBytes = cesiumAssets.Aggregate(0L, (i, asset) => i + asset.Bytes);
        var totalGigaBytes = totalBytes / Math.Pow(1024,3);

        if (totalGigaBytes < 4)
            return;

        var assets = _unitOfWork.AssetRepository.Entities;
        if (assets == null || !assets.Any())
            return;

        var assetsAsList = assets.ToList();

        assetsAsList.Sort((a1, a2) => a1.PriorityScore.CompareTo(a2.PriorityScore));
        var cesiumIdsOfAssetsToDelete = new List<int>();
        var assetIndex = 0;
        while (totalGigaBytes > 4)
        {
            var assetToDeleteFromDatabase = assetsAsList[assetIndex];
            cesiumIdsOfAssetsToDelete.Add(assetToDeleteFromDatabase.CesiumIonAssetId);

            var assetToDeleteFromCesiumIon = cesiumAssets.Find(c => c.Id == assetToDeleteFromDatabase.CesiumIonAssetId);

            if (assetToDeleteFromCesiumIon == null)
                continue;
                
            _unitOfWork.AssetRepository.Remove(assetToDeleteFromDatabase);
            await _cesiumIonClient.DeleteAssetAsync(assetToDeleteFromCesiumIon.Id);

            totalGigaBytes -= assetToDeleteFromCesiumIon.Bytes;
        }

        await _unitOfWork.CommitAsync();
    }
}