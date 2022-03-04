using System.IO;
using System.Runtime.CompilerServices;
using Arkitektum.Cesium.Ion.RestApiSharp;
using Arkitektum.Cesium.Ion.RestApiSharp.Services;
using Arkitektum.Cesium.Ion.RestApiSharp.Util;

[assembly: InternalsVisibleTo("DiBK.Plankart.Application.Tests")]
namespace DiBK.Plankart.Application.Services;

internal class CesiumIonResourceUploader
{
    private const string AccessToken = @"";
    public int? UploadTerrainModel(string assetName, FileStream assetFileStream)
    {
        var asset = AssetFactory.Create(assetName, AssetType.TERRAIN, SourceType.RASTER_TERRAIN,
            attribution: "Kartverket",
            description: "Se kartkatalogen - [Høyde DTM Sømløs WCS](https://kartkatalog.geonorge.no/metadata/hoeyde-dtm-smls-wcs/311782b2-26fe-4d1d-93e9-c90b45d5bd8b)."
        );
        var assetId = new CesiumIonClient(AccessToken).UploadAssetAsync(asset, assetFileStream);

        return assetId.Result;
    }
}