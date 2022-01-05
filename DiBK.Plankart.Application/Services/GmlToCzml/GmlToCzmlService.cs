using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        private int _sourceEpsgCode;

        public CesiumDataCollection CreateCzmlObject(XDocument document, string epsgCode, Dictionary<string, string> geoElementMappings)
        {
            if (document == null)
                return null;

            _sourceEpsgCode = int.Parse(epsgCode.Remove(0, 5));

            var rpSpatialElements = GetRpSpatialElements(document);
            var cesiumDataCollection = new CesiumDataCollection();

            foreach ((var rpSpatialElementName, var cesiumGraphicCollections) in rpSpatialElements)
            {
                foreach (var cesiumGraphicCollection in cesiumGraphicCollections)
                {
                    foreach (var graphic in cesiumGraphicCollection.CesiumGraphics)
                    {
                        
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
                Enum surfaceType = null;
                if (spatialElement.Name.LocalName.Equals(RpSpatialElement.RpBestemmelseRegTerreng.ToString()))
                    surfaceType = Enum.Parse<RegTerrengOverflateType>(spatialElement.GetValue("//*:overflatetype"));

                return new CesiumGraphicCollection
                {
                    Id = spatialElement.GetElement("//*:lokalId").Value,
                    Type = surfaceType,
                    CesiumGraphics = ConvertGmlToCesiumGraphics(spatialElement.GetElements("//*:posList"), surfaceType)
                };
            });
        }

        private List<CesiumGraphic> ConvertGmlToCesiumGraphics(IEnumerable<XElement> positionLists, Enum surfaceType)
        {
            var cesiumGraphics = new List<CesiumGraphic>();

            foreach (var positionList in positionLists)
            {
                var sourceCoordinates = positionList.Value.Split(' ').Select(double.Parse).ToList();

                var transformedCoordinates = new CoordinateTransformator(_sourceEpsgCode, Epsg.CesiumCoordinateSystemCode).Transform(sourceCoordinates);

                cesiumGraphics.Add(new PolygonGraphic(transformedCoordinates, surfaceType));
            }

            return cesiumGraphics;
        }

        private string ConvertPositionListToApiFriendlyString(string positionList)
        {
            var apiFriendlyString = "";
            long whitespaceCounter = 0;

            foreach (var character in positionList)
            {
                if (character == ' ')
                {
                    whitespaceCounter++;
                    if (whitespaceCounter % 3 == 0)
                        apiFriendlyString += ';';
                    else
                        apiFriendlyString += ',';
                }
                else
                    apiFriendlyString += character;
            }

            if (whitespaceCounter % 3 != 0)
                throw new ArgumentOutOfRangeException("positionList", "Length of coordinate array must be dividable by 3");

            return apiFriendlyString;
        }

        private enum RpSpatialElement
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
