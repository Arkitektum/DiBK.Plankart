using System.IO;
using System.Threading.Tasks;
using Arkitektum.Cesium.Ion.RestApiSharp;
using Arkitektum.Cesium.Ion.RestApiSharp.Services;
using Arkitektum.Cesium.Ion.RestApiSharp.Util;
using Microsoft.Extensions.Configuration;

namespace DiBK.Plankart.Application.Services;

public class CesiumIonAssetUploader : ICesiumIonAssetUploader
{
    private readonly IConfiguration _configuration;

    public CesiumIonAssetUploader(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<int?> UploadTerrainModelAsync(string assetName, FileStream assetFileStream)
    {
        var asset = AssetFactory.Create(assetName, AssetType.TERRAIN, SourceType.RASTER_TERRAIN,
            attribution: "Kartverket",
            description: "Se kartkatalogen - [Høyde DTM Sømløs WCS](https://kartkatalog.geonorge.no/metadata/hoeyde-dtm-smls-wcs/311782b2-26fe-4d1d-93e9-c90b45d5bd8b)."
        );

        var accessToken = _configuration["DibkPlankartCesiumAccessToken"];
        return await new CesiumIonClient(accessToken).UploadAssetAsync(asset, assetFileStream);
    }
}