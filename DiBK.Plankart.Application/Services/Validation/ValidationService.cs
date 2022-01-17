using DiBK.Plankart.Application.Exceptions;
using DiBK.Plankart.Application.Models.Validation;
using DiBK.Plankart.Application.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiBK.Plankart.Application.Services
{
    public class ValidationService : IValidationService
    {
        private readonly HttpClient _httpClient;
        private readonly ValidationSettings _settings;
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(
            HttpClient httpClient,
            IOptions<ValidationSettings> options,
            ILogger<ValidationService> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateAsync(IFormFile file)
        {
            var report = await RunValidationAsync(file);
            var failedRules = report.Rules.Where(rule => rule.Status == "FAILED");
            var xsdRule = failedRules.SingleOrDefault(rule => rule.Id == _settings.XsdRuleId);
            var result = new ValidationResult { Id = report.CorrelationId };

            if (xsdRule != null)
            {
                result.XsdValidationMessages = xsdRule.Messages.Select(message => message.Message).ToList();
                return result;
            }

            var dimensions = await GmlHelper.GetDimensionsAsync(file.OpenReadStream());
            var epsgRuleId = dimensions == 2 ? _settings.Epsg2dRuleId : _settings.Epsg3dRuleId;
            var epsgRule = failedRules.SingleOrDefault(rule => rule.Id == epsgRuleId);

            if (epsgRule != null)
            {
                result.EpsgValidationMessages = epsgRule.Messages.Select(message => message.Message).ToList();
                return result;
            }

            result.Rules = failedRules.ToList();

            return result;
        }

        private async Task<ValidationReport> RunValidationAsync(IFormFile file)
        {
            try
            {
                using var content = new MultipartFormDataContent
                {
                    { new StreamContent(file.OpenReadStream()), "plankart2D", file.FileName }
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, _settings.ApiUrl)
                {
                    Content = content
                };

                request.Headers.Add("system", "GML-kart - Fellestjenester PLAN");

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ValidationReport>(responseString);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Kunne ikke validere filen '{file.FileName}'!");
                throw new CouldNotValidateException($"Kunne ikke validere filen '{file.FileName}'!", exception);
            }
        }
    }
}
