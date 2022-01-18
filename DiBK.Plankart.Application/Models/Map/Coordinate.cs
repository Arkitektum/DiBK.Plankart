using System.Collections.Generic;
using System.Globalization;

namespace DiBK.Plankart.Application.Models.Map
{
    internal class Coordinate
    {
        private NumberFormatInfo _nfi = new NumberFormatInfo
        {
            NumberDecimalSeparator = "."
        };

        public double X { get; init; }
        public double Y { get; init; }
        public double Z { get; init; }

        public override string ToString()
        {
            return $"{X.ToString(_nfi)}, {Y.ToString(_nfi)}, {Z.ToString(_nfi)}";
        }

        public IEnumerable<double> ToEnumerable()
        {
            return new List<double> { X, Y, Z };
        }
    }
}
