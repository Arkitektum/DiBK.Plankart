using DiBK.Plankart.Application.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wmhelp.XPath2;

namespace DiBK.Plankart.Application.Services
{
    public abstract class MapServiceBase
    {
        protected static readonly Regex _epsgRegex =
            new(@"^(http:\/\/www\.opengis\.net\/def\/crs\/EPSG\/0\/|^urn:ogc:def:crs:EPSG::)(?<epsg>\d+)$", RegexOptions.Compiled);

        private readonly IGmlToGeoJsonService _gmlToGeoJsonService;

        protected MapServiceBase(
            IGmlToGeoJsonService gmlToGeoJsonService)
        {
            _gmlToGeoJsonService = gmlToGeoJsonService;
        }

        protected MapDocument CreateMapDocument(string Id, string Name, IFormFile file, XDocument document, Dictionary<string, string> geoElementMappings = null)
        {
            return new MapDocument
            {
                Id = Id,
                Name = Name,
                Epsg = GetEpsg(document),
                FileName = file.FileName,
                FileSize = file.Length,
                GeoJson = _gmlToGeoJsonService.CreateGeoJsonDocument(document, geoElementMappings)
            };
        }

        protected static string GetEpsg(XDocument document)
        {
            var srsName = document.XPath2SelectOne<XAttribute>("(//*[@srsName]/@srsName)[1]")?.Value;

            if (srsName == null)
                return null;

            var match = _epsgRegex.Match(srsName);

            return match.Success ? $"EPSG:{match.Groups["epsg"].Value}" : null;
        }

        protected static async Task<XDocument> LoadXDocument(IFormFile file)
        {
            try
            {
                return await XDocument.LoadAsync(file.OpenReadStream(), LoadOptions.None, new CancellationToken());
            }
            catch
            {
                return null;
            }
        }
    }
}
