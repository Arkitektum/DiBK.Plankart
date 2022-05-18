using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DiBK.Plankart.Application.Models.Map;

public class CzmlDataCollection
{
    public string Type { get; } = "CzmlDataCollection";
    public List<JArray> CzmlStrings { get; } = new();
}