using DiBK.Plankart.Application.Models.Validation;

namespace DiBK.Plankart.Application.Models.Map
{
    public class MapDocument
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string Title { get; init; }
        public Envelope Envelope { get; init; }
        public Epsg Epsg { get; init; }
        public string VerticalDatum { get; init; }
        public string FileName { get; init; }
        public long FileSize { get; init; }
        public ValidationResult ValidationResult { get; set; }
        public GeoJsonFeatureCollection GeoJson { get; init; } = new();
        public int? CesiumAssetId { get; set; }
        public ValidationResult ValidationResult3d { get; set; }
        public CzmlDataCollection CzmlData { get; set; } = new();
    }

    public class MapDocument3D
    {
        public ValidationResult ValidationResult { get; set; }
        public CzmlDataCollection CzmlData { get; set; } = new();
    }
}
