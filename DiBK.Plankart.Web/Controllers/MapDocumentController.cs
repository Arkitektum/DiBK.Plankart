using DiBK.Plankart.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;

namespace DiBK.Plankart.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MapDocumentController : BaseController
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        private readonly IMapDocumentService _mapDocumentService;
        private readonly IMultipartRequestService _multipartRequestService;

        public MapDocumentController(
            IMapDocumentService mapDocumentService,
            IMultipartRequestService multipartRequestService,
            ILogger<MapDocumentController> logger) : base(logger)
        {
            _mapDocumentService = mapDocumentService;
            _multipartRequestService = multipartRequestService;
        }

        [HttpPost("/MapDocument")]
        public async Task<IActionResult> CreateMapDocument()
        {
            try
            {
                var file = await _multipartRequestService.GetFileFromMultipart();

                if (file == null)
                    return BadRequest();

                var document = await _mapDocumentService.CreateMapDocumentAsync(file);
                var serialized = JsonConvert.SerializeObject(document, _jsonSerializerSettings);

                return Ok(serialized);
            }
            catch (Exception exception)
            {
                var result = HandleException(exception);

                if (result != null)
                    return result;

                throw;
            }
        }
        
        [HttpPost("/MapDocument3D")]
        public async Task<IActionResult> Create3dMapDocument()
        {
            try
            {
                var file = await _multipartRequestService.GetFileFromMultipart();

                if (file == null)
                    return BadRequest();

                var mapDocument3d = await _mapDocumentService.UpdateWith3dDataAsync(file);
                var serialized = JsonConvert.SerializeObject(mapDocument3d, _jsonSerializerSettings);
                
                return Ok(serialized);
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
