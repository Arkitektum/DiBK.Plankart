namespace DiBK.Plankart.Application.Models
{
    public class MapDocument
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Epsg { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public GeoJsonFeatureCollection GeoJson { get; set; } = new();
    }
}
