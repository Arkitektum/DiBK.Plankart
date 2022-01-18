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
        private CoordinateTransformer _coordinateTransformer;

        public CzmlDataCollection CreateCzmlCollection(XDocument document, string epsgCode, Dictionary<string, string> geoElementMappings)
        {
            if (document == null)
                return null;

            var sourceEpsgCode = int.Parse(epsgCode.Remove(0, 5));

            _coordinateTransformer = new CoordinateTransformer(sourceEpsgCode, Epsg.CesiumCoordinateSystemCode);

            var rpSpatialElements = GetRpSpatialElements(document);
            var czmlDataCollection = new CzmlDataCollection();

            foreach (var (_, cesiumGraphicCollections) in rpSpatialElements)
            {
                foreach (var cesiumGraphicCollection in cesiumGraphicCollections)
                {
                    czmlDataCollection.CzmlStrings.Add(cesiumGraphicCollection.CzmlRepresentation);
                }
            }

            return czmlDataCollection;
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
            var cesiumGraphicsCollection = new List<CesiumGraphicCollection>();

            foreach (var spatialElement in spatialElements)
            {
                Enum? surfaceType = null;

                if (spatialElement.Name.LocalName.Equals(RpSpatialElement.RpBestemmelseRegTerreng.ToString()))
                    surfaceType = Enum.Parse<RegTerrengOverflateType>(spatialElement.GetValue("//*:overflateType"));

                surfaceType ??= Enum.Parse<RpSpatialElement>(spatialElement.Name.LocalName);

                cesiumGraphicsCollection.Add(new CesiumGraphicCollection
                {
                    Id = spatialElement.GetElement("//*:lokalId").Value,
                    Type = surfaceType,
                    CesiumGraphics = ConvertGmlToCesiumGraphics(spatialElement.GetElements("//*:posList"), surfaceType)
                });
            }

            return cesiumGraphicsCollection;
        }

        private List<CesiumGraphic> ConvertGmlToCesiumGraphics(IEnumerable<XElement> positionLists, Enum? surfaceType)
        {
            var cesiumGraphics = new List<CesiumGraphic>();

            foreach (var positionList in positionLists)
            {
                var sourceCoordinates = positionList.Value.Split(' ')
                    .Select(v => double.Parse(v, NumberFormatInfo.InvariantInfo)).ToList();

                var transformedCoordinates = _coordinateTransformer?.Transform(sourceCoordinates);

                cesiumGraphics.Add(new PolygonGraphic(transformedCoordinates, surfaceType));
            }

            return cesiumGraphics;
        }

        public enum RpSpatialElement
        {
            RpHandlingRom,
            RpBestemmelseRom,
            RpHensynRom,
            RpBestemmelseRegTerreng,
        }

        public enum RegTerrengOverflateType
        {
            planlagt,
            høyeste,
            laveste,
        }
    }
}
