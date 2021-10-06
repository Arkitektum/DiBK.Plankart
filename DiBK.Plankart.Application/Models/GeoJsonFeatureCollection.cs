using System.Collections.Generic;

namespace DiBK.Plankart.Application.Models
{
    public class GeoJsonFeatureCollection
    {
        public string Type { get; } = "FeatureCollection";
        public List<GeoJsonFeature> Features { get; set; } = new List<GeoJsonFeature>();
    }
}
