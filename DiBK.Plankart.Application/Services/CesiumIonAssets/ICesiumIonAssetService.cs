using System.Collections.Generic;
using System.Threading.Tasks;
using Arkitektum.Cesium.Ion.RestApiSharp.Models;

namespace DiBK.Plankart.Application.Services;

public interface ICesiumIonAssetService
{
    Task<List<AssetMetadata>> GetAssetsAsync();
    Task DeleteAssetAsync(int assetId);
    Task DeleteAssetsAsync(IEnumerable<int> assetIds);
    Task<IEnumerable<int>> DeleteAssetsAsync(List<AssetMetadata> assets);
}