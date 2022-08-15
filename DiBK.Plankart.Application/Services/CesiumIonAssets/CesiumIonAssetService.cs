using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arkitektum.Cesium.Ion.RestApiSharp;
using Arkitektum.Cesium.Ion.RestApiSharp.Models;
using Microsoft.Extensions.Configuration;
using static DiBK.Plankart.Application.Configuration;

namespace DiBK.Plankart.Application.Services;

public class CesiumIonAssetService : ICesiumIonAssetService
{
    private readonly CesiumIonClient _client;

    public CesiumIonAssetService(IConfiguration configuration)
    {
        _client = new CesiumIonClient(configuration[AccessTokensCesiumIon]);
    }

    public async Task<List<AssetMetadata>> GetAssetsAsync()
    {
        return await _client.GetAssetListAsync();
    }

    public async Task DeleteAssetAsync(int assetId)
    {
        await _client.DeleteAssetAsync(assetId);
    }

    public async Task DeleteAssetsAsync(IEnumerable<int> assetIds)
    {
        foreach (var assetId in assetIds)
            await _client.DeleteAssetAsync(assetId);
    }

    public async Task<IEnumerable<int>> DeleteAssetsAsync(List<AssetMetadata> assets)
    {
        if (assets == null)
            return null;

        foreach (var asset in assets)
            await _client.DeleteAssetAsync(asset.Id);

        return assets.Select(a => a.Id);
    }
}