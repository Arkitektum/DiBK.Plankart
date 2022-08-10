﻿using DiBK.Plankart.Application.Exceptions;
using DiBK.Plankart.Application.Models.Map;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wmhelp.XPath2;
using static DiBK.Plankart.Application.Utils.ApplicationConfig;

namespace DiBK.Plankart.Application.Services
{
    public class MapDocumentService : IMapDocumentService
    {
        private readonly IValidationService _validationService;
        private readonly IGmlToGeoJsonService _gmlToGeoJsonService;
        private readonly IGmlToCzmlService _gmlToCzmlService;

        public MapDocumentService(
            IValidationService validationService,
            IGmlToGeoJsonService gmlToGeoJsonService,
            IGmlToCzmlService gmlToCzmlService)
        {
            _validationService = validationService;
            _gmlToGeoJsonService = gmlToGeoJsonService;
            _gmlToCzmlService = gmlToCzmlService;
        }

        public async Task<MapDocument> CreateMapDocumentAsync(IFormFile file)
        {
            var validationResult = await _validationService.ValidateAsync(file);

            if (!validationResult.XsdValidated || !validationResult.EpsgValidated)
            {
                return new MapDocument
                {
                    FileName = file.FileName,
                    FileSize = file.Length,
                    ValidationResult = validationResult 
                };
            }
            
            var document = await LoadXDocumentAsync(file);

            if (document == null)
                return null;

            var envelope = GetEnvelope(document);

            return new MapDocument
            {
                Id = GetId(document),
                Name = GetName(document),
                Title = GetTitle(document),
                Envelope = envelope,
                Epsg = envelope.Epsg,
                VerticalDatum = GetVerticalDatum(document),
                FileName = file.FileName,
                FileSize = file.Length,
                ValidationResult = validationResult,
                GeoJson = _gmlToGeoJsonService.CreateGeoJsonDocument(new XDocument(document), new() { { "RpPåskrift", "tekstplassering" } }),
            };
        }

        public async Task<MapDocument3D> UpdateWith3dDataAsync(IFormFile file)
        {
            var validationResult = await _validationService.ValidateAsync(file);

            if (!validationResult.XsdValidated || !validationResult.EpsgValidated)
                return new MapDocument3D { ValidationResult = validationResult };

            var document = await LoadXDocumentAsync(file);

            if (document == null)
                return null;

            var envelope = GetEnvelope(document);

            return new MapDocument3D
            {
                ValidationResult = validationResult,
                CzmlData = _gmlToCzmlService.CreateCzmlCollection(document, envelope),
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
                "31" => "Mindre reguleringsendring for",
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

        private static Envelope GetEnvelope(XDocument document)
        {
            const string xPathToEnvelope = "//*:FeatureCollection/*:boundedBy/*:Envelope";
            var envelope = document.XPath2SelectElement(xPathToEnvelope);
            var lowerCornerValue = document.XPath2SelectElement($"{xPathToEnvelope}/*:lowerCorner").Value;
            var upperCornerValue = document.XPath2SelectElement($"{xPathToEnvelope}/*:upperCorner").Value;
            var envelopeAsString = $"{lowerCornerValue.Replace(' ', ',')},{upperCornerValue.Replace(' ', ',')}";

            return new Envelope
            {
                Epsg = GetEpsg(document),
                Dimension = int.Parse(envelope.Attribute("srsDimension").Value),
                LowerCorner = lowerCornerValue.Split(' '),
                UpperCorner = upperCornerValue.Split(' '),
                AsString = envelopeAsString,
            };
        }

        private static string GetVerticalDatum(XDocument document)
        {
            return document.XPath2SelectElement("//*:RpRegulertHøyde//*:høydereferansesystem")?.Value;
        }

        private static async Task<XDocument> LoadXDocumentAsync(IFormFile file)
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
