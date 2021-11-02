using DiBK.Plankart.Application.Models.Validation;

namespace DiBK.Plankart.Application.Models.Map
{
    public class MapDocument
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public Epsg Epsg { get; set; }
        public string VerticalDatum { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public GeoJsonFeatureCollection GeoJson { get; set; } = new();
        public ValidationResult ValidationResult { get; set; }
    }
}
