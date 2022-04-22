using System.Threading.Tasks;
using DiBK.Plankart.Application.Models.Map;
using DiBK.Plankart.Application.Services;
using Xunit;

namespace DiBK.Plankart.Application.Tests.Services
{
    public class TerrainResourceTests
    {
        [Fact]
        public void ShouldCreateTerrainResource()
        {
            var terrainRequest = new TerrainRequest
            {
                Envelope = "298500.0,6694500.0,300500.0,6696500.0", 
                EpsgCode = 25832
            };

            var assetId = CreateResourceAsync(terrainRequest).Result;

        }

        private static async Task<int?> CreateResourceAsync(TerrainRequest terrainRequest)
        {
            return await new TerrainResourceService(new CesiumIonResourceUploader(), new HeightDataFetcher())
                .CreateTerrainResourceAsync(terrainRequest);
        }
    }
}
