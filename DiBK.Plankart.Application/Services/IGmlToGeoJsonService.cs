using DiBK.Plankart.Application.Models;
using System.Collections.Generic;
using System.Xml.Linq;

namespace DiBK.Plankart.Application.Services
{
    public interface IGmlToGeoJsonService
    {
        GeoJsonFeatureCollection CreateGeoJsonDocument(XDocument document, Dictionary<string, string> geoElementMappings = null);
    }
}
