using DiBK.Plankart.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Arkitektum.Cesium.Ion.RestApiSharp;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CesiumIonAssetController : BaseController
    {
        private readonly ITerrainResourceService _terrainResourceService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _cesiumIonAccessToken;

        public CesiumIonAssetController(
            ITerrainResourceService terrainResourceService,
            IUnitOfWork unitOfWork,
            IAccessTokenProvider accessTokenProvider,
            ILogger<CesiumIonAssetController> logger) : base(logger)
        {
            _terrainResourceService = terrainResourceService;
            _unitOfWork = unitOfWork;
            _cesiumIonAccessToken = accessTokenProvider.CesiumIonToken();
        }

        [HttpPost("/TerrainData")]
        public async Task<IActionResult> CreateTerrainData([FromBody] TerrainRequest terrainRequest)
        {
            try
            {
                var enclosingAsset = _unitOfWork.AssetRepository.Find(a => a.Encloses(terrainRequest.Envelope));
                if (enclosingAsset != null)
                {
                    enclosingAsset.LastAccessed = DateTime.Now;
                    await _unitOfWork.CommitAsync();
                    return Ok(enclosingAsset.CesiumIonAssetId);
                }

                var cesiumIonAsset = await _terrainResourceService.CreateTerrainResourceAsync(terrainRequest);

                if (cesiumIonAsset == null)
                    return Problem(detail: "Was not able to create terrain resource", statusCode: 500);

                cesiumIonAsset.LastAccessed = cesiumIonAsset.Added = DateTime.Now;
                _unitOfWork.AssetRepository.Add(cesiumIonAsset);
                await AssetCleanup(cesiumIonAsset);
                await _unitOfWork.CommitAsync();

                return Ok(cesiumIonAsset.CesiumIonAssetId);
            }
            catch (Exception exception)
            {
                var result = HandleException(exception);

                if (result != null)
                    return result;

                throw;
            }
        }

        private async Task AssetCleanup(CesiumIonAsset asset)
        {
            var cesiumIonClient = new CesiumIonClient(_cesiumIonAccessToken);
            foreach (var entity in _unitOfWork.AssetRepository.Entities.Where(e => e.IsEnclosedBy(asset)))
            {
                await cesiumIonClient.DeleteAssetAsync(entity.CesiumIonAssetId);
                _unitOfWork.AssetRepository.Remove(entity);
            }
        }
    }
}
