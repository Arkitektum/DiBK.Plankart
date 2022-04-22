using System;
using System.IO;
using Arkitektum.Cesium.Ion.RestApiSharp;
using DiBK.Plankart.Application.Services;
using Xunit;

namespace DiBK.Plankart.Application.Tests.Integration;

public class FetchAndUploadTerrainDataTest : IDisposable
{
    private const string Filename = "terraindata.tiff";
    private const string AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiI2MjcxMThmNC00YjRlLTQxN2EtOWVlYy01ZjlkMDI4OTk1MDYiLCJpZCI6Njk1ODcsImlhdCI6MTYzODk3MjM0Mn0.gk-hx6X_EMGF5iRzvKLLlu0dNNFUoIFe65HA83ZY7IE";
    private int? _assetId;

    [Fact]
    public void ShouldFetchAndUploadTerrainData()
    {
        using var stream = new HeightDataFetcher().FetchHighestResolutionAsStreamAsync("298500.0,6694500.0,300500.0,6696500.0", 25832).Result;
        using var writeFileStream = new FileStream(Filename, FileMode.Create, FileAccess.Write);

        stream.CopyTo(writeFileStream);
        writeFileStream.Flush();

        writeFileStream.Dispose();

        using var readFileStream = new FileStream(Filename, FileMode.Open, FileAccess.Read);
        _assetId = new CesiumIonResourceUploader().UploadTerrainModelAsync("nattlandsfjellet-highres-test", readFileStream).Result;

        Assert.NotNull(_assetId);
    }


    public void Dispose()
    {
        if (File.Exists(Filename))
            File.Delete(Filename);

        if (_assetId.HasValue) new CesiumIonClient(AccessToken).DeleteAssetAsync(_assetId.Value);
    }
}