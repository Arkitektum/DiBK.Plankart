namespace DiBK.Plankart.Application.Models.Map
{
    public class OlMapDocument : MapDocument
    {
        public GeoJsonFeatureCollection GeoJson { get; init; } = new();
    }
}
