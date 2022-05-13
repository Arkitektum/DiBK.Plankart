using System.IO;
using System.Threading.Tasks;
using Arkitektum.Cesium.Ion.RestApiSharp;
using Arkitektum.Cesium.Ion.RestApiSharp.Services;
using Arkitektum.Cesium.Ion.RestApiSharp.Util;

namespace DiBK.Plankart.Application.Services;

public class CesiumIonResourceUploader : ICesiumIonResourceUploader
{
    private readonly string _accessToken;

    public CesiumIonResourceUploader(IAccessTokenProvider accessTokenProvider)
    {
        _accessToken = accessTokenProvider.CesiumIonToken();
    }

    public async Task<int?> UploadTerrainModelAsync(string assetName, FileStream assetFileStream)
    {
        var asset = AssetFactory.Create(assetName, AssetType.TERRAIN, SourceType.RASTER_TERRAIN,
            attribution: "Kartverket",
            description: "Se kartkatalogen - [Høyde DTM Sømløs WCS](https://kartkatalog.geonorge.no/metadata/hoeyde-dtm-smls-wcs/311782b2-26fe-4d1d-93e9-c90b45d5bd8b)."
        );

        return await new CesiumIonClient(_accessToken).UploadAssetAsync(asset, assetFileStream);
    }
}