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

        if (!assetsToDelete.Any())
            return null;

        DbContext.TerrainResources.RemoveRange(assetsToDelete);

        return assetsToDelete.Select(a => a.CesiumIonAssetId);
    }

    public void RemoveRangeByCesiumIonAssetsIds(IEnumerable<int> cesiumIonAssetsIds)
    {
        foreach (var id in cesiumIonAssetsIds)
        {
            var asset = DbContext.TerrainResources.FirstOrDefault(t => t.CesiumIonAssetId == id);
            if (asset != default(CesiumIonTerrainResource))
                DbContext.TerrainResources.Remove(asset);
        }
    }

    private IQueryable<CesiumIonTerrainResource> FindAssetsEnclosedByWithMargin(CesiumIonTerrainResource terrainResource, double margin)
    {
        return DbContext.TerrainResources.Where(e => terrainResource.EnclosesWithMargin(e, margin));
    }
}