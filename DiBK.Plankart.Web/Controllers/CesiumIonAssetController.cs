using DiBK.Plankart.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using DiBK.Plankart.Application.Models.Map;
using Microsoft.Extensions.Configuration;
using static DiBK.Plankart.Application.Configuration;

namespace DiBK.Plankart.Controllers;

[ApiController]
[Route("[controller]")]
public class CesiumIonAssetController : BaseController
{
    private readonly ICesiumIonTerrainAssetManager _cesiumIonTerrainAssetManager;
    private readonly IConfiguration _configuration;

    public CesiumIonAssetController(
        ICesiumIonTerrainAssetManager cesiumIonTerrainAssetManager,
        IConfiguration configuration,
        ILogger<CesiumIonAssetController> logger) : base(logger)
    {
        _cesiumIonTerrainAssetManager = cesiumIonTerrainAssetManager;
        _configuration = configuration;
    }

    [HttpGet("/CesiumIonToken")]
    public IActionResult GetCesiumIonAccessToken()
    {
        try
        {
            return Ok(_configuration[AccessTokensCesiumIon]);
        }
        catch (Exception exception)
        {
            var result = HandleException(exception);

            if (result != null)
                return result;

            throw;
        }
    }

    [HttpPost("/TerrainData")]
    public async Task<IActionResult> CreateTerrainData([FromBody] TerrainLocation terrainLocation)
    {
        try
        {
            var terrainAssetId = await _cesiumIonTerrainAssetManager.GetCesiumTerrainAssetId(terrainLocation);

            return terrainAssetId == 0
                ? Problem(detail: "Was not able to create terrain resource", statusCode: 500)
                : Ok(terrainAssetId);
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