using DiBK.Plankart.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CesiumIonAssetController : BaseController
    {
        private readonly ITerrainResourceService _terrainResourceService;

        public CesiumIonAssetController(
            ITerrainResourceService terrainResourceService,
            ILogger<CesiumIonAssetController> logger) : base(logger)
        {
            _terrainResourceService = terrainResourceService;
        }

        [HttpPost("/TerrainData")]
        public async Task<IActionResult> CreateTerrainData([FromBody] TerrainRequest terrainRequest)
        {
            try
            {
                return Ok(await _terrainResourceService.CreateTerrainResourceAsync(terrainRequest));
            }
            catch (Exception exception)
            {
                var result = HandleException(exception);

                if (result != null)
                    return result;

                throw;
            }
        }
    }
}
