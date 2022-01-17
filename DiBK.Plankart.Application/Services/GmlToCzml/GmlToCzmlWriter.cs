using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace DiBK.Plankart.Application.Services.GmlToCzml
{
    internal class GmlToCzmlWriter : JsonTextWriter
    {
        private static readonly Regex _prefixRegex = new(@"^(?!(@xlink|@xmlns)).*?:|@", RegexOptions.Compiled);

        public GmlToCzmlWriter(TextWriter textWriter) : base(textWriter) { }

        public override void WritePropertyName(string name)
        {
            var propName = _prefixRegex.Replace(name, string.Empty);

            base.WritePropertyName(propName);
        }
    }
}
