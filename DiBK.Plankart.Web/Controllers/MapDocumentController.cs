using DiBK.Plankart.Application.Services;
using Microsoft.AspNetCore.Http;
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

        public MapDocumentController(
            IMapDocumentService mapDocumentService,
            ILogger<MapDocumentController> logger) : base(logger)
        {
            _mapDocumentService = mapDocumentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMapDocument(IFormFile file)
        {
            try
            {
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
    }
}
