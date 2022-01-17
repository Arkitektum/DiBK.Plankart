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
                    $"\"id\": \"{_id}\"," +
                    $"\"name\": \"{_name}\"," +
                    "\"polygon\": {" +
                        "\"positions\": {" +
                            "\"cartographicDegrees\": [" +
                                Coordinates.Aggregate("", (s, c) => s + $"{c},").TrimEnd(',') +
                            "]" +
                        "}," +
                        "\"material\": {" +
                            "\"solidColor\": {" +
                                "\"color\": {" +
                                    $"\"rgba\": [{SetColor()}]" +
                                "}" +
                            "}" +
                        "}," +
                        "\"perPositionHeight\": true" +
                    "}" +
                "},";
        }

        private string SetColor()
        {
            string rgb;
            string alpha;

            if (_type is GmlToCzmlService.RpSpatialElement rpSpatialElementType)
            {
                return rpSpatialElementType switch
                {
                    GmlToCzmlService.RpSpatialElement.RpBestemmelseRegTerreng => "200,100,0,170",
                    GmlToCzmlService.RpSpatialElement.RpHandlingRom => "255,255,51,170",
                    GmlToCzmlService.RpSpatialElement.RpBestemmelseRom => "0,0,0,0",
                    GmlToCzmlService.RpSpatialElement.RpHensynRom => "255,255,255,50", //Hensynrom må bruke tegneregel fra hensynområdet det gjelder for
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            if (_type is GmlToCzmlService.RegTerrengOverflateType type)
            {
                rgb = "200,100,0";
                alpha = "," + type switch
                {
                    GmlToCzmlService.RegTerrengOverflateType.høyeste
                        or GmlToCzmlService.RegTerrengOverflateType.laveste => "150",
                    GmlToCzmlService.RegTerrengOverflateType.planlagt => "200",
                    _ => "255"
                };
            }
            else
            {
                rgb = "255,204,0";
                alpha = ",170";
            }

            return $"{rgb}{alpha}";
        }
    }
}
