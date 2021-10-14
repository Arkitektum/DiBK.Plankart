using Arkitektum.XmlSchemaValidator.Validator;
using DiBK.Plankart.Application.Models.Validation;
using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Rules.Gml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Reguleringsplanforslag.Rules.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static DiBK.Plankart.Application.Helpers.ValidationHelper;

namespace DiBK.Plankart.Application.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IRuleValidator _validator;
        private readonly IXmlSchemaValidator _xsdValidator;
        private readonly IStaticDataService _staticDataService;
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(
            IRuleValidator validator,
            IXmlSchemaValidator xsdValidator,
            IStaticDataService staticDataService,
            ILogger<ValidationService> logger)
        {
            _validator = validator;
            _xsdValidator = xsdValidator;
            _staticDataService = staticDataService;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateAsync(IFormFile file)
        {
            var inputData = new InputData(file.OpenReadStream(), file.FileName, null);

            var result = new ValidationResult
            {
                XsdValidationMessages = _xsdValidator.Validate("Reguleringsplanforslag", inputData.Stream)
            };

            if (!result.XsdValidated)
                return result;

            result.Rules = await Validate(inputData);

            return result;
        }

        private async Task<IEnumerable<ValidationRule>> Validate(InputData inputData)
        {
            inputData.Stream.Seek(0, SeekOrigin.Begin);

            var plankartValidationData = PlankartValidationData.Create(GmlDocument.Create(inputData), await GetKodelister());
            var gmlValidationData = GmlValidationData.Create(plankartValidationData.Plankart2D.SingleOrDefault());

            _validator.Validate(gmlValidationData, options =>
            {
                options.SkipRule<KoordinatreferansesystemForKart3D>();
            });

            _validator.Validate(plankartValidationData, options =>
            {
                options.SkipGroup("Plankart3D");
                options.SkipGroup("Planbestemmelser");
                options.SkipGroup("PlankartOgPlanbestemmelser");
                options.SkipGroup("Oversendelse");
            });

            plankartValidationData.Dispose();
            gmlValidationData.Dispose();

            var failedRules = _validator.GetExecutedRules().Where(rule => rule.Status == Status.FAILED);

            return CreateValidationRules(failedRules);
        }

        private async Task<Kodelister> GetKodelister()
        {
            return new Kodelister
            {
                Arealformål = await _staticDataService.GetArealformål(),
                Feltnavn = await _staticDataService.GetFeltnavnArealformål(),
                Hensynskategori = await _staticDataService.GetHensynskategori()
            };
        }
    }
}
