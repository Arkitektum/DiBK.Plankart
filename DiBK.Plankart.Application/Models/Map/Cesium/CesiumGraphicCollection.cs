using System;
using System.Collections.Generic;

namespace DiBK.Plankart.Application.Models.Map.Cesium
{
    internal class CesiumGraphicCollection
    {
        public string Id { get; set; }
        public Enum Type { get; set; }
        public List<CesiumGraphic> CesiumGraphics { get; set; }
    }
}
