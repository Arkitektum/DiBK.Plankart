using Arkitektum.XmlSchemaValidator.Validator;
using DiBK.Plankart.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiBK.Plankart.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GmlToGeoJsonController : BaseController
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        private readonly IXmlSchemaValidator _xmlSchemaValidator;
        private readonly IGmlToGeoJsonService _gmlToGeoJsonService;

        public GmlToGeoJsonController(
            IXmlSchemaValidator xmlSchemaValidator,
            IGmlToGeoJsonService gmlToGeoJsonService,
            ILogger<GmlToGeoJsonController> logger) : base(logger)
        {
            _xmlSchemaValidator = xmlSchemaValidator;
            _gmlToGeoJsonService = gmlToGeoJsonService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGeoJsonDocument(IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest();

                var messages = _xmlSchemaValidator.Validate("Plankart", file.OpenReadStream());

                if (messages.Any())
                    return BadRequest("Ugyldig GML-fil");

                var document = await _gmlToGeoJsonService.CreateGeoJsonDocument(file);
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
