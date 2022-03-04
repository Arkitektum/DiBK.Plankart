namespace DiBK.Plankart.Application.Models.Map
{
    public class Envelope
    {
        public Epsg Epsg { get; set; }
        public int Dimension { get; set; }
        public string [] LowerCorner { get; set; }
        public string [] UpperCorner { get; set; }
    }
}
