using DiBK.Plankart.Application.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wmhelp.XPath2;

namespace DiBK.Plankart.Application.Services
{
    public class PlankartService : MapServiceBase, IPlankartService
    {
        public PlankartService(
            IGmlToGeoJsonService gmlToGeoJsonService) : base(gmlToGeoJsonService)
        {
        }

        public async Task<MapDocument> CreateMapDocument(IFormFile file)
        {
            var document = await LoadXDocument(file);

            if (document == null)
                return null;

            var (Id, Name) = GetIdAndName(document);

            return CreateMapDocument(Id, Name, file, document, new() { { "RpPåskrift", "tekstplassering" } });
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
    }
}
