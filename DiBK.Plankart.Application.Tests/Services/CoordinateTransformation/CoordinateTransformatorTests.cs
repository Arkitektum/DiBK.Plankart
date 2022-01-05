using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using DiBK.Plankart.Application.Exceptions;
using DiBK.Plankart.Application.Extensions;
using DiBK.Plankart.Application.Models.Map;
using DiBK.Plankart.Application.Models.Map.Cesium;
using DiBK.Plankart.Application.Services.CoordinateTransformation;
using Wmhelp.XPath2;
using Xunit;

namespace DiBK.Plankart.Application.Tests.Services
{
    public class CoordinateTransformatorTests
    {
        private CoordinateTransformator? _coordinateTransformator;

        [Fact, Trait("Category", "GDAL_Proj_Transformation")]
        public void TransformationTest()
        {
            const int sourceEpsgCode = 5972;
            const int targetEpsgCode = 4326;

            var sourceCoordinate = new List<double>{299416.02, 6695529.56, 0};

            var transformedCoordinate = new CoordinateTransformator(sourceEpsgCode, targetEpsgCode).Transform(sourceCoordinate);

            var coordinate = transformedCoordinate.First();
            Assert.Equal(60.34645969, coordinate.X, 8);
            Assert.Equal(5.36455553, coordinate.Y, 8);
            Assert.Equal(0, coordinate.Z);
        }

        [Fact, Trait("Integration", "GML data extraction")]
        public void Load3dGmlStuff()
        {
            var filePath = "C:\\Users\\LeifHalvorSunde\\FTP\\3d-plankart\\nattlandsfjellet_3d.gml";

            var xDoc = LoadXDocument(filePath).Result;

            var sourceEpsgCode = int.Parse(GetEpsg(xDoc).Code.Remove(0, 5));

            _coordinateTransformator = new CoordinateTransformator(sourceEpsgCode, Epsg.CesiumCoordinateSystemCode);

            var rpSpatialElements = GetRpSpatialElements(xDoc);

            var czml = "";

            foreach ((var rpSpatialElementName, var cesiumGraphicCollections)  in rpSpatialElements)
            {
                foreach (var cesiumGraphicCollection in cesiumGraphicCollections)
                {
                    czml = cesiumGraphicCollection.CzmlRepresentation;
                }
            }

            Assert.True(rpSpatialElements.ContainsKey(RpSpatialElement.RpBestemmelseRegTerreng.ToString()));
            Assert.True(czml != null && czml.Contains("id"));
        }

        private static async Task<XDocument> LoadXDocument(string filePath)
        {
            await using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            try
            {
                return await XDocument.LoadAsync(file, LoadOptions.None, new CancellationToken());
            }
            catch (Exception exception)
            {
                throw new CouldNotLoadXDocumentException("Kunne ikke laste plankartet.", exception);
            }
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
                Enum? surfaceType = null;
                if (spatialElement.Name.LocalName.Equals(RpSpatialElement.RpBestemmelseRegTerreng.ToString()))
                    surfaceType = Enum.Parse<RegTerrengOverflateType>(spatialElement.GetValue("//*:overflateType"));

                return new CesiumGraphicCollection
                {
                    Id = spatialElement.GetElement("//*:lokalId").Value,
                    Type = surfaceType,
                    CesiumGraphics = ConvertGmlToCesiumGraphics(spatialElement.GetElements("//*:posList"), surfaceType)
                };
            });
        }

        private List<CesiumGraphic> ConvertGmlToCesiumGraphics(IEnumerable<XElement> positionLists, Enum? surfaceType)
        {
            var cesiumGraphics = new List<CesiumGraphic>();

            foreach (var positionList in positionLists)
            {
                var sourceCoordinates = positionList.Value.Split(' ')
                    .Select(v => double.Parse(v, NumberFormatInfo.InvariantInfo)).ToList();
                var transformedCoordinates = _coordinateTransformator.Transform(sourceCoordinates);

                cesiumGraphics.Add(new PolygonGraphic(transformedCoordinates, surfaceType));
            }

            return cesiumGraphics;
        }

        private static Epsg GetEpsg(XDocument document)
        {
            var srsName = document.XPath2SelectOne<XAttribute>("(//*[@srsName]/@srsName)[1]")?.Value;

            if (srsName == null)
                return null;

            return Epsg.Create(srsName);
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