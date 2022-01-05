using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DiBK.Plankart.Application.Extensions;
using DiBK.Plankart.Application.Models.Map;
using DiBK.Plankart.Application.Models.Map.Cesium;
using DiBK.Plankart.Application.Services.CoordinateTransformation;

namespace DiBK.Plankart.Application.Services
{
    public class GmlToCzmlService : IGmlToCzmlService
    {
        private CesiumMapDocument _mapDocument = new();

        public CesiumDataCollection CreateCzmlObject(XDocument document, string epsgCode, Dictionary<string, string> geoElementMappings)
        {
            if (document == null)
                return null;

            var rpSpatialElements = GetRpSpatialElements(document);
            var cesiumDataCollection = new CesiumDataCollection();

            foreach ((var rpSpatialElementName, var cesiumGraphicCollections) in rpSpatialElements)
            {
                foreach (var cesiumGraphicCollection in cesiumGraphicCollections)
                {
                    foreach (var graphic in cesiumGraphicCollection.CesiumGraphics)
                    {
                        var transformedCoordinates = new CoordinateTransformator(int.Parse(epsgCode.Remove(0,5)), Epsg.CesiumCoordinateSystemCode).Transform(graphic.Coordinates);
                    }
                }
            }

            return cesiumDataCollection;
        }

        private Dictionary<string, IEnumerable<CesiumGraphicCollection>> GetRpSpatialElements(XDocument xDoc)
        {
            var rpSpatialObjectsMappedByName = new Dictionary<string, IEnumerable<CesiumGraphicCollection>>();

            foreach (var rpSpatialElementName in Enum.GetNames<RpSpatialElement>())
            {
                var rpSpatialElements = xDoc.GetElements($"//*:featureMember/*:{rpSpatialElementName}");
                if (rpSpatialElements?.FirstOrDefault() == null)
                    continue;

                rpSpatialObjectsMappedByName.Add(rpSpatialElementName, CreateCesiumGraphicsCollection(rpSpatialElements));
            }

            return rpSpatialObjectsMappedByName;
        }

        private IEnumerable<CesiumGraphicCollection> CreateCesiumGraphicsCollection(IEnumerable<XElement> spatialElements)
        {
            return spatialElements.Select(spatialElement =>
            {
                Enum noko = null;
                if (spatialElement.Name.LocalName.Equals(RpSpatialElement.RpBestemmelseRegTerreng.ToString()))
                    noko = Enum.Parse<RegTerrengOverflateType>(spatialElement.GetValue("//*:overflatetype"));

                return new CesiumGraphicCollection
                {
                    Id = spatialElement.GetElement("//*:lokalId").Value,
                    Type = noko,
                    CesiumGraphics = ConvertGmlToCesiumGraphics(spatialElement.GetElements("//*:posList"))
                };
            });
        }

        private List<CesiumGraphic> ConvertGmlToCesiumGraphics(IEnumerable<XElement> positionLists)
        {
            var cesiumGraphics = new List<CesiumGraphic>();

            foreach (var positionList in positionLists)
            {
                cesiumGraphics.Add(
                    new PolygonGraphic(positionList.Value.Split(' ')
                        .Select(p => double.Parse(p, NumberFormatInfo.InvariantInfo))
                        .ToArray()
                    ));
            }

            return cesiumGraphics;
        }

        private enum RpSpatialElement
        {
            RpHandlingRom,
            RpBestemmelseRom,
            RpHensynRom,
            RpBestemmelseRegTerreng,
        }

        private enum RegTerrengOverflateType
        {
            planlagt,
            høyeste,
            laveste,
        }
    }
}
