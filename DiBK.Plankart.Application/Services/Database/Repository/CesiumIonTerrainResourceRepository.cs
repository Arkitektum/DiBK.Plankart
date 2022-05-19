using System.Collections.Generic;
using System.Linq;
using Arkitektum.Cesium.Ion.RestApiSharp.Models;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Application.Services;

public class CesiumIonTerrainResourceRepository : GenericRepository<CesiumIonResourceDbContext, CesiumIonTerrainResource>
{
    public CesiumIonTerrainResourceRepository(CesiumIonResourceDbContext dbContext) : base(dbContext)
    {
    }

    public void DeleteUnmappedTerrainResources(IEnumerable<AssetMetadata> mappedCesiumIonAssets)
    {
        if (mappedCesiumIonAssets == null)
            return;

        foreach (var asset in DbContext.TerrainResources)
        {
            if (mappedCesiumIonAssets.Any(a => a.Id == asset.CesiumIonAssetId))
                continue;

            DbContext.TerrainResources.Remove(asset);
        }
    }

    public IEnumerable<int> DeleteRedundantAssets(CesiumIonTerrainResource terrainResource)
    {
        var assetsToDelete = FindAssetsEnclosedByWithMargin(terrainResource, 500.0);

        if (assetsToDelete == null || !assetsToDelete.Any())
            return null;

        DbContext.TerrainResources.RemoveRange(assetsToDelete);

        return assetsToDelete.Select(a => a.CesiumIonAssetId);
    }

    public void RemoveRangeByCesiumIonAssetsIds(IEnumerable<int> cesiumIonAssetsIds)
    {
        if (cesiumIonAssetsIds == null)
            return;

        foreach (var id in cesiumIonAssetsIds)
        {
            var asset = DbContext.TerrainResources.FirstOrDefault(t => t.CesiumIonAssetId == id);
            if (asset != default(CesiumIonTerrainResource))
                DbContext.TerrainResources.Remove(asset);
        }
    }

    private IEnumerable<CesiumIonTerrainResource> FindAssetsEnclosedByWithMargin(CesiumIonTerrainResource newResource, double margin)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery, because EnclosesWithMargin is a custom method.
        // For more info, see: https://stackoverflow.com/questions/57872910/the-linq-expression-could-not-be-translated-and-will-be-evaluated-locally
        foreach (var existingResource in DbContext.TerrainResources)
        {
            if (newResource.EnclosesWithMargin(existingResource, margin))
                yield return existingResource;
        }
    }
}