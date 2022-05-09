using System;
using System.Collections.Generic;
using System.Linq;
using DiBK.Plankart.Application.Services;
using Newtonsoft.Json.Linq;

namespace DiBK.Plankart.Application.Models.Map
{
    public abstract class CesiumGraphic
    {
        protected abstract JObject CreateCzmlRepresentation();
        public JObject CzmlRepresentation => CreateCzmlRepresentation();
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

        protected override JObject CreateCzmlRepresentation()
        {
            var result = Coordinates.Aggregate(new List<double>(),
                (current, coordinate) => current.Concat(new List<double>(coordinate.ToEnumerable())).ToList());

            var polygonCzml = new JObject
            {
                new JProperty("id", _id),
                new JProperty("name", _name),
                { "polygon", new JObject
                    {
                        { "positions", new JObject { new JProperty("cartographicDegrees", result) } },
                        { "material", new JObject
                            {{
                                "solidColor", new JObject
                                {{
                                    "color", new JObject
                                    {
                                        new JProperty("rgba", SetColor())
                                    }
                                }}
                            }}
                        },
                        new JProperty("perPositionHeight", true),
                        //new JProperty("outline", true)
                    }
                }
            };

            return polygonCzml;
        }

        private int[] SetColor()
        {
            // todo: Bruk tegneregel som gjelder for området som er linket til rommet
            if (_type is GmlToCzmlService.RpSpatialElement rpSpatialElementType)
            {
                return rpSpatialElementType switch
                {
                    GmlToCzmlService.RpSpatialElement.RpBestemmelseRegTerreng => new [] {200,100,0,75},
                    GmlToCzmlService.RpSpatialElement.RpHandlingRom => new[] {150,150,150,75},
                    GmlToCzmlService.RpSpatialElement.RpBestemmelseRom => new[] {0,0,0,0},
                    GmlToCzmlService.RpSpatialElement.RpHensynRom => new[] { 150,150,150,75},
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (_type is GmlToCzmlService.RegTerrengOverflateType type)
            {
                return type switch
                {
                    GmlToCzmlService.RegTerrengOverflateType.høyeste
                        or GmlToCzmlService.RegTerrengOverflateType.laveste => new[] {200,100,0,150},
                    GmlToCzmlService.RegTerrengOverflateType.planlagt => new[] {200,100,0,200},
                    _ => new[] {200,100,0,255}
                };
            }
            
            return new[] { 255,204,0,170};
        }
    }
}
