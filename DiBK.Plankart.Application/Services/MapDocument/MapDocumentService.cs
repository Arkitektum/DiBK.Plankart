using DiBK.Plankart.Application.Exceptions;
using DiBK.Plankart.Application.Models.Map;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wmhelp.XPath2;

namespace DiBK.Plankart.Application.Services
{
    public class MapDocumentService : IMapDocumentService
    {
        private static readonly Regex _epsgRegex =
            new(@"^(http:\/\/www\.opengis\.net\/def\/crs\/EPSG\/0\/|^urn:ogc:def:crs:EPSG::)(?<epsg>\d+)$", RegexOptions.Compiled);

        private readonly IValidationService _validationService;
        private readonly IGmlToGeoJsonService _gmlToGeoJsonService;

        public MapDocumentService(
            IValidationService validationService,
            IGmlToGeoJsonService gmlToGeoJsonService)
        {
            _validationService = validationService;
            _gmlToGeoJsonService = gmlToGeoJsonService;
        }

        public async Task<MapDocument> CreateMapDocument(IFormFile file)
        {
            var validationResult = await _validationService.ValidateAsync(file);

            if (!validationResult.XsdValidated)
            {
                return new MapDocument 
                { 
                    FileName = file.FileName,
                    FileSize = file.Length,
                    ValidationResult = validationResult 
                };
            }

            var document = await LoadXDocument(file);

            if (document == null)
                return null;

            var (Id, Name) = GetIdAndName(document);

            return new MapDocument
            {
                Id = Id,
                Name = Name,
                Epsg = GetEpsg(document),
                FileName = file.FileName,
                FileSize = file.Length,
                GeoJson = _gmlToGeoJsonService.CreateGeoJsonDocument(document, new() { { "RpPåskrift", "tekstplassering" } }),
                ValidationResult = validationResult
            };
        }

        private static (string Id, string Name) GetIdAndName(XDocument document)
        {
            var navn = document.Root.XPath2SelectElement("//*:Arealplan/*:plannavn")?.Value;
            var kommunenr = document.Root.XPath2SelectElement("//*:Arealplan/*:nasjonalArealplanId//*:kommunenummer")?.Value;
            var planident = document.Root.XPath2SelectElement("//*:Arealplan/*:nasjonalArealplanId//*:planidentifikasjon")?.Value;
            string id = null;

            if (!string.IsNullOrWhiteSpace(navn))
                navn = navn.Trim();

            if (!string.IsNullOrWhiteSpace(kommunenr) && !string.IsNullOrWhiteSpace(planident))
                id = $"{kommunenr.Trim()}_{planident.Trim()}";

            return (id, navn);
        }

        private static string GetEpsg(XDocument document)
        {
            var srsName = document.XPath2SelectOne<XAttribute>("(//*[@srsName]/@srsName)[1]")?.Value;

            if (srsName == null)
                return null;

            var match = _epsgRegex.Match(srsName);

            return match.Success ? $"EPSG:{match.Groups["epsg"].Value}" : null;
        }

        private static async Task<XDocument> LoadXDocument(IFormFile file)
        {
            try
            {
                return await XDocument.LoadAsync(file.OpenReadStream(), LoadOptions.None, new CancellationToken());
            }
            catch (Exception exception)
            {
                throw new CouldNotLoadXDocumentException("Kunne ikke laste plankartet.", exception);
            }
        }
    }
}
