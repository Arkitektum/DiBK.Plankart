using System.Collections.Generic;
using System.Xml.Linq;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Application.Services;

public interface IGmlToCzmlService
{
    CesiumDataCollection CreateCzmlObject(XDocument document, string epsgCode, Dictionary<string, string> geoElementMappings);
}