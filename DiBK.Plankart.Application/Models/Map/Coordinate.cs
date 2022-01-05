using System.Globalization;

namespace DiBK.Plankart.Application.Models.Map
{
    internal class Coordinate
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double Z { get; init; }

        public override string ToString()
        {
            return $"{X.ToString(NumberFormatInfo.InvariantInfo)},{Y.ToString(NumberFormatInfo.InvariantInfo)},{Z.ToString(NumberFormatInfo.InvariantInfo)}";
        }
    }
}
