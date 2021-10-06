using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;

namespace DiBK.Plankart.Application.Services
{
    public class GmlToJsonWriter : JsonTextWriter
    {
        private static readonly Regex _prefixRegex = new(@"^(?!(@xlink|@xmlns)).*?:|@", RegexOptions.Compiled);

        public GmlToJsonWriter(TextWriter writer) : base(writer) { }

        public override void WritePropertyName(string name)
        {
            var propName = _prefixRegex.Replace(name, string.Empty);

            base.WritePropertyName(propName);
        }
    }
}
