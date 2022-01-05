using System;
using System.Collections.Generic;
using System.Linq;

namespace DiBK.Plankart.Application.Models.Map.Cesium
{
    internal class CesiumGraphicCollection
    {
        public string Id { get; set; }
        public Enum Type { get; set; }
        public List<CesiumGraphic> CesiumGraphics { get; set; }

        public string CzmlRepresentation => GetCzmlRepresentation();

        private string GetCzmlRepresentation()
        {
            return
                "[\n" +
                    "\t{\n" +
                        "\t\tid: 'document',\n" +
                        $"\t\tname: '{Id}',\n" +
                        "\t\tversion: '1.0'\n" +
                    "\t},\n" +
                    CesiumGraphics.Aggregate("", (s, graphic) => s + graphic.CzmlRepresentation) +
                "]";
        }
    }
}
