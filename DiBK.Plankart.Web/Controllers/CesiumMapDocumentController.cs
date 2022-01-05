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
    public class CesiumMapDocumentController : BaseController
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        private readonly IMapDocumentService _cesiumMapDocumentService;

        public CesiumMapDocumentController(
            IMapDocumentService mapDocumentService,
            ILogger<CesiumMapDocumentController> logger) : base(logger)
        {
            _cesiumMapDocumentService = mapDocumentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMapDocument(IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest();

                var document = await _cesiumMapDocumentService.CreateMapDocument(file);
                var serialized = JsonConvert.SerializeObject(document, JsonSerializerSettings);

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
