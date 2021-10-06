﻿using Newtonsoft.Json.Linq;

namespace DiBK.Plankart.Application.Models
{
    public class GeoJsonFeature
    {
        public string Type { get; } = "Feature";
        public GeoJsonGeometry Geometry { get; set; }
        public JObject Properties { get; set; }
    }
}
