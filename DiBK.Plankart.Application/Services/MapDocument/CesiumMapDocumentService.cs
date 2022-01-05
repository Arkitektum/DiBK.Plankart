using DiBK.Plankart.Application.Exceptions;
using DiBK.Plankart.Application.Models.Map;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using DiBK.Plankart.Application.Services.CoordinateTransformation;
using Wmhelp.XPath2;

namespace DiBK.Plankart.Application.Services
{
    public class CesiumMapDocumentService : IMapDocumentService
    {
        private readonly IValidationService _validationService;
        private readonly IGmlToCzmlService _gmlToCzmlService;

        public CesiumMapDocumentService(
            IValidationService validationService,
            IGmlToCzmlService gmlToCzmlService)
        {
            _validationService = validationService;
            _gmlToCzmlService = gmlToCzmlService;
        }

        public async Task<MapDocument> CreateMapDocument(IFormFile file)
        {
            var validationResult = await _validationService.ValidateAsync(file);

            if (!validationResult.XsdValidated || !validationResult.EpsgValidated)
            {
                return new CesiumMapDocument
                { 
                    FileName = file.FileName,
                    FileSize = file.Length,
                    ValidationResult = validationResult 
                };
            }

            var document = await LoadXDocument(file);

            if (document == null)
                return null;

            var epsg = GetEpsg(document);

            return new CesiumMapDocument
            {
                Id = GetId(document),
                Name = GetName(document),
                Title = GetTitle(document),
                Epsg = epsg,
                VerticalDatum = GetVerticalDatum(document),
                FileName = file.FileName,
                FileSize = file.Length,
                ValidationResult = validationResult,
                Rectangle = GetRectangle(document, epsg),
                CesiumData = _gmlToCzmlService.CreateCzmlObject(document, epsg.Code, null)
            };
        }

        private static string GetId(XDocument document)
        {
            var kommunenr = document.Root.XPath2SelectElement("//*:Arealplan/*:nasjonalArealplanId//*:kommunenummer")?.Value;
            var planident = document.Root.XPath2SelectElement("//*:Arealplan/*:nasjonalArealplanId//*:planidentifikasjon")?.Value;
            string id = null;

            if (!string.IsNullOrWhiteSpace(kommunenr) && !string.IsNullOrWhiteSpace(planident))
                id = $"{kommunenr.Trim()}_{planident.Trim()}";

            return id;
        }

        private static string GetName(XDocument document)
        {
            return document.Root.XPath2SelectElement("//*:Arealplan/*:plannavn")?.Value.Trim();
        }

        private static string GetTitle(XDocument document)
        {
            var type = document.Root.XPath2SelectElement("//*:Arealplan/*:plantype")?.Value.Trim();

            return type switch
            {
                "34" => "Områderegulering for",
                "35" => "Detaljregulering for",
                _ => null
            };
        }

        private static Epsg GetEpsg(XDocument document)
        {
            var srsName = document.XPath2SelectOne<XAttribute>("(//*[@srsName]/@srsName)[1]")?.Value;

            if (srsName == null)
                return null;

            return Epsg.Create(srsName);
        }

        private static string GetVerticalDatum(XDocument document)
        {
            return document.XPath2SelectElement("//*:RpRegulertHøyde//*:høydereferansesystem")?.Value;
        }

        private Rectangle GetRectangle(XDocument document, Epsg epsg)
        {
            if (epsg == null)
                return null;

            var lowerCorner = document.XPath2SelectElement("//gml:boundedBy/gml:Envelope/gml:lowerCorner")?.Value.Split(' ');
            var upperCorner = document.XPath2SelectElement("//gml:boundedBy/gml:Envelope/gml:upperCorner")?.Value.Split(' ');

            var west =  double.Parse(lowerCorner?[0] ?? string.Empty);
            var south =  double.Parse(lowerCorner?[1] ?? string.Empty);

            var east =  double.Parse(upperCorner?[0] ?? string.Empty);
            var north =  double.Parse(upperCorner?[1] ?? string.Empty);

            var transformedLowerCorner = new CoordinateTransformator(int.Parse(epsg.Code.Remove(0, 5)), 4236)
                .Transform(west, south);
            var transformedUpperCorner = new CoordinateTransformator(int.Parse(epsg.Code.Remove(0, 5)), 4236)
                .Transform(east, north);


            return new Rectangle
            {
                West = transformedLowerCorner.X, South= transformedLowerCorner.Y,
                East = transformedUpperCorner.X, North = transformedUpperCorner.Y
            };
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
