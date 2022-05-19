using System.Collections.Generic;
using System.Threading.Tasks;
using Arkitektum.Cesium.Ion.RestApiSharp.Models;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Application.Services;

public interface ICesiumIonAssetService
{
    Task<List<AssetMetadata>> GetAssetsAsync();
    Task<IEnumerable<int>> DeleteCorruptAssetsAsync();
    Task<IEnumerable<int>> MakeSureMax4GbIsUsedAsync(List<CesiumIonTerrainResource> terrainResources);
    Task DeleteAssetsFromIdsAsync(IEnumerable<int> assetIds);
    Task<CesiumIonTerrainResource> CreateTerrainResourceAsync(TerrainLocation terrainLocation);
    Task DeleteUnmappedAssetsAsync(IEnumerable<CesiumIonTerrainResource> resources);
}