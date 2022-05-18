namespace DiBK.Plankart.Application.Models.Map;

public class Envelope
{
    public Epsg Epsg { get; init; }
    public int Dimension { get; init; }
    public string [] LowerCorner { get; init; }
    public string [] UpperCorner { get; init; }
    public string AsString { get; init; }
}