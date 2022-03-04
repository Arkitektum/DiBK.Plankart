using System.Collections.Generic;
using DiBK.Plankart.Application.Utils;

namespace DiBK.Plankart.Application.Models.Map
{
    internal class Coordinate
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double Z { get; init; }

        public override string ToString()
        {
            return $"{X.ToString(ApplicationConfig.DoubleFormatInfo)}, {Y.ToString(ApplicationConfig.DoubleFormatInfo)}, {Z.ToString(ApplicationConfig.DoubleFormatInfo)}";
        }

        public IEnumerable<double> ToEnumerable()
        {
            return new List<double> { X, Y, Z };
        }
    }
}
