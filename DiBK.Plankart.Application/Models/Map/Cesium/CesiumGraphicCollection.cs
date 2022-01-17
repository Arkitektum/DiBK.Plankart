using System;
using System.Collections.Generic;
using System.Linq;

namespace DiBK.Plankart.Application.Models.Map.Cesium
{
    internal class CesiumGraphicCollection
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Enum Type { get; set; }
        public List<CesiumGraphic> CesiumGraphics { get; set; }

        public string CzmlRepresentation => GetCzmlRepresentation();

        private string GetCzmlRepresentation()
        {
            return
                "[\n" +
                    "{" +
                        "\"id\": \"document\"," +
                        $"\"name\": \"{Name}_{Id}\"," +
                        "\"version\": \"1.0\"" +
                    "}," +
                    CesiumGraphics.Aggregate("", (s, graphic) => s + graphic.CzmlRepresentation).TrimEnd(',') +
                "\n];";
        }
    }
}
