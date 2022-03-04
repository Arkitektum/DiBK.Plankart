using System.Collections.Generic;
using System.Xml.Linq;
using DiBK.Plankart.Application.Models.Map;

namespace DiBK.Plankart.Application.Services;

public interface IGmlToCzmlService
{
    CzmlDataCollection CreateCzmlCollection(XDocument document, Envelope envelope, Dictionary<string, string> geoElementMappings);
}