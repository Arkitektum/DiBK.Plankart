using System;
using System.IO;
using System.Threading.Tasks;
using DiBK.Plankart.Application.Models.Map;

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

        public async Task<int?> CreateTerrainResourceAsync(TerrainRequest terrainRequest)
        {
            var filename = Guid.NewGuid() + ".tiff";

            await using var stream = await _heightDataFetcher.FetchHighestResolutionAsStreamAsync(terrainRequest.Envelope, terrainRequest.EpsgCode);

            await using var writeFileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(writeFileStream);
            await writeFileStream.FlushAsync();
            await writeFileStream.DisposeAsync();

            await using var readFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            return await _cesiumIonResourceUploader.UploadTerrainModelAsync(filename, readFileStream);
        }
    }
}
