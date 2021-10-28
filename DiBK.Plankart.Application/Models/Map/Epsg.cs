using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DiBK.Plankart.Application.Models.Map
{
    public class Epsg
    {
        private static readonly Regex _epsgRegex =
            new(@"^(http:\/\/www\.opengis\.net\/def\/crs\/EPSG\/0\/|^urn:ogc:def:crs:EPSG::)(?<epsg>\d+)$", RegexOptions.Compiled);

        private static readonly Dictionary<string, string> _descriptions = new()
        {
            { "25832", "UTM sone 32 (EUREF89/WGS84)" },
            { "25833", "UTM sone 33 (EUREF89/WGS84)" },
            { "25835", "UTM sone 35 (EUREF89/WGS84)" }
        };

        public string Code { get; set; }
        public string Description { get; set; }

        private Epsg(string code, string description)
        {
            Code = code;
            Description = description;
        }

        public static Epsg Create(string srsName)
        {
            var match = _epsgRegex.Match(srsName);
            var code = match.Success ? match.Groups["epsg"].Value : null;

            if (code == null)
                return null;

            var description = _descriptions.ContainsKey(code) ? _descriptions[code] : null;

            return new Epsg($"EPSG:{code}", description);
        }
    }
}
