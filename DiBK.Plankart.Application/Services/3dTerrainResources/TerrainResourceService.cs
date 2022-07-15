using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiBK.Plankart.Application.Models.Map;
using DiBK.Plankart.Application.Utils;

namespace DiBK.Plankart.Application.Services;

public class TerrainResourceService : ITerrainResourceService
{
    private readonly ICesiumIonAssetUploader _cesiumIonAssetUploader;
    private readonly IHeightDataFetcher _heightDataFetcher;

    public TerrainResourceService(
        ICesiumIonAssetUploader cesiumIonAssetUploader,
        IHeightDataFetcher heightDataFetcher)
    {
        _cesiumIonAssetUploader = cesiumIonAssetUploader;
        _heightDataFetcher = heightDataFetcher;
    }

    public async Task<CesiumIonTerrainResource> CreateTerrainResourceAsync(TerrainLocation terrainLocation)
    {
        var filename = Guid.NewGuid() + ".zip";

        await using var stream = await _heightDataFetcher.FetchHighestResolutionAsStreamAsync(terrainLocation.Envelope, terrainLocation.EpsgCode);
            
        await WriteFileStreamAsync(filename, stream);

        await using var readFileStream =  new FileStream(filename, FileMode.Open, FileAccess.Read);

        var cesiumIonAssetId = await _cesiumIonAssetUploader.UploadTerrainModelAsync(filename, readFileStream);

        if (cesiumIonAssetId == null)
            return null;

        await IoCleanUpAsync(readFileStream);

        var envelopeArray = terrainLocation.Envelope.Split(',')
            .Select(c => double.Parse(c, ApplicationConfig.DoubleFormatInfo)).ToList();

        return new CesiumIonTerrainResource
        {
            CesiumIonAssetId = cesiumIonAssetId.Value,
            EpsgCode = terrainLocation.EpsgCode,
            South = envelopeArray[0],
            West = envelopeArray[1],
            North = envelopeArray[2],
            East = envelopeArray[3],
            Added = DateTime.Now,
            LastAccessed = DateTime.Now,
            NumberOfUsages = 1,
        };
    }

    private static async Task WriteFileStreamAsync(string filename, Stream stream)
    {
        //todo: Må denne kanskje heller lagres i en BlobStorage på Azure?
        //todo: S3-ressursen som filen/data skal lastes opp til senere krever at det er en fil med et filnavn som lastes opp...
        try
        {
            stream.Position = 0;
            await using var writeFileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(writeFileStream);
            writeFileStream.Flush();
        }
        catch (Exception e)
        {
            throw new IOException($"An error occurred while trying to write to file '{filename}':\n{e}");
        }
    }

    private static async Task IoCleanUpAsync(FileStream readFileStream)
    {
        await readFileStream.DisposeAsync();

        if (File.Exists(readFileStream.Name)) 
            File.Delete(readFileStream.Name);
    }
}