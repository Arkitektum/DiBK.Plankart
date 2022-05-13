using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiBK.Plankart.Application.Models.Map;
using DiBK.Plankart.Application.Utils;

namespace DiBK.Plankart.Application.Services
{
    public class TerrainResourceService : ITerrainResourceService
    {
        private readonly ICesiumIonResourceUploader _cesiumIonResourceUploader;
        private readonly IHeightDataFetcher _heightDataFetcher;

        public TerrainResourceService(
            ICesiumIonResourceUploader cesiumIonResourceUploader,
            IHeightDataFetcher heightDataFetcher)
        {
            _cesiumIonResourceUploader = cesiumIonResourceUploader;
            _heightDataFetcher = heightDataFetcher;
        }

        public async Task<CesiumIonAsset> CreateTerrainResourceAsync(TerrainRequest terrainRequest)
        {
            var filename = Guid.NewGuid() + ".zip";

            await using var stream = await _heightDataFetcher.FetchHighestResolutionAsStreamAsync(terrainRequest.Envelope, terrainRequest.EpsgCode);
            
            await WriteFileStreamAsync(filename, stream);

            var readFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            var cesiumIonAssetId = await _cesiumIonResourceUploader.UploadTerrainModelAsync(filename, readFileStream);

            if (cesiumIonAssetId == null)
                return null;

            await IoCleanUpAsync(readFileStream);

            var envelopeArray = terrainRequest.Envelope.Split(',')
                .Select(c => double.Parse(c, ApplicationConfig.DoubleFormatInfo)).ToList();

            return new CesiumIonAsset
            {
                CesiumIonAssetId = cesiumIonAssetId.Value,
                EpsgCode = terrainRequest.EpsgCode,
                South = envelopeArray[0],
                West = envelopeArray[1],
                North = envelopeArray[2],
                East = envelopeArray[3],
            };
        }

        private static async Task WriteFileStreamAsync(string filename, Stream stream)
        {
            stream.Position = 0;
            await using var writeFileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(writeFileStream);
            writeFileStream.Flush();
        }

        private static async Task IoCleanUpAsync(FileStream readFileStream)
        {
            await readFileStream.DisposeAsync();

            if (File.Exists(readFileStream.Name))
                File.Delete(readFileStream.Name);
        }
    }
}
