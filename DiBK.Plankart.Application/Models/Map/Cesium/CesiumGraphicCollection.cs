using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DiBK.Plankart.Application.Models.Map.Cesium
{
    internal class CesiumGraphicCollection
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Enum Type { get; set; }
        public List<CesiumGraphic> CesiumGraphics { get; set; }

        public JArray CzmlRepresentation => GetCzmlRepresentation();

        private JArray GetCzmlRepresentation()
        {
            var jsonArray = new JArray();

            var czmlDocObject = new JObject
            {
                new JProperty("id", "document"),
                new JProperty("name", $"{Name}_{Id}"),
                new JProperty("version", "1.0")
            };

            jsonArray.Add(czmlDocObject);

            foreach (var cesiumGraphic in CesiumGraphics)
            {
                jsonArray.Add(cesiumGraphic.CzmlRepresentation);
            }

            return jsonArray;
        }
    }
}
