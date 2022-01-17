using System.Collections.Generic;

namespace DiBK.Plankart.Application.Models.Map
{
    public class CzmlDataCollection
    {
        public string Type { get; } = "CzmlDataCollection";
        public List<string> CzmlStrings { get; } = new();
    }
}
