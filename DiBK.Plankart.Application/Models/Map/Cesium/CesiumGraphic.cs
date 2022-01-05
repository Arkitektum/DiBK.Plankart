using System;
using System.Collections.Generic;
using System.Linq;
using DiBK.Plankart.Application.Services;

namespace DiBK.Plankart.Application.Models.Map.Cesium
{
    public abstract class CesiumGraphic
    {
        protected abstract string CreateCzmlRepresentation();
        public string CzmlRepresentation => CreateCzmlRepresentation();
        internal IEnumerable<Coordinate> Coordinates { get; init; }

    }

    // https://cesium.com/learn/cesiumjs/ref-doc/PolygonGraphics.html
    internal class PolygonGraphic : CesiumGraphic
    {
        private readonly string _id;
        private readonly string _name;
        private readonly Enum _type;

        public PolygonGraphic(IEnumerable<Coordinate> coordinates, Enum type)
        {
            var guid = Guid.NewGuid().ToString();
            _id = guid;
            _name = "polygon_" + guid;
            _type = type;
            Coordinates = coordinates;
        }

        public PolygonGraphic(string id, string name, Enum type, IEnumerable<Coordinate> coordinates)
        {
            _id = id;
            _name = name;
            _type = type;
            Coordinates = coordinates;
        }

        protected override string CreateCzmlRepresentation()
        {
            return
                "{" +
                    $"id: '{_id}'," +
                    $"name: '{_name}'," +
                    "polygon: {" +
                        "positions: {" +
                            "cartographicDegrees: [" +
                                Coordinates.Aggregate("", (s, c) => s + $"{c},") +
                            "]" +
                        "}," +
                        "material: {" +
                            "solidColor: {" +
                                "color: {" +
                                    $"rgba: [{SetColor()}]" +
                                "}" +
                            "}" +
                        "}," +
                        "perPositionHeight: true," +
                    "}" +
                "},";
        }

        private string SetColor()
        {
            string rgb;
            string alpha;

            if (_type is GmlToCzmlService.RegTerrengOverflateType type)
            {
                rgb = "200,100,0";
                alpha = type switch
                {
                    GmlToCzmlService.RegTerrengOverflateType.høyeste
                        or GmlToCzmlService.RegTerrengOverflateType.laveste => "150",
                    GmlToCzmlService.RegTerrengOverflateType.planlagt => "200",
                    _ => "255"
                };
            }
            else
            {
                rgb = "255,255,51";
                alpha = "170";
            }

            return $"{rgb},{alpha}";
        }
    }
}
