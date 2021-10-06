namespace DiBK.Plankart.Application.Models
{
    public class GeoJsonDocument
    {
        public string FileName { get; set; }
        public string EPSG { get; set; }
        public GeoJsonFeatureCollection FeatureCollection { get; set; } = new GeoJsonFeatureCollection();

        public GeoJsonDocument()
        {
        }

        public GeoJsonDocument(string fileName, string epsg)
        {
            FileName = fileName;
            EPSG = epsg;
        }
    }
}
