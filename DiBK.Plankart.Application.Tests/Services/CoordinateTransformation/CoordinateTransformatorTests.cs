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
using DiBK.Plankart.Application.Models.Map.Cesium;
using DiBK.Plankart.Application.Services.CoordinateTransformation;
using Xunit;

namespace DiBK.Plankart.Application.Tests.Services
{
    public class CoordinateTransformatorTests
    {
        [Fact, Trait("Integration", "web-API")]
        public void TransformationTest()
        {
            const int sourceEpsgCode = 5972;
            const int targetEpsgCode = 4326;

            var sourceCoordinate = new[] { 299416.02, 6695529.56, 0};

            var transformedCoordinate = new CoordinateTransformator(sourceEpsgCode, targetEpsgCode).Transform(sourceCoordinate);

            Assert.Equal(new[] { 5.36455553, 60.34645969 }, new[] { transformedCoordinate.x, transformedCoordinate.y });
        }

        [Fact]
        public void Load3dGmlStuff()
        {
            var filePath = "C:\\Users\\LeifHalvorSunde\\FTP\\3d-plankart\\nattlandsfjellet_3d.gml";

            var xDoc = LoadXDocument(filePath).Result;

            var rpSpatialElements = GetRpSpatialElements(xDoc);

            Assert.True(rpSpatialElements.ContainsKey(RpSpatialElement.RpBestemmelseRegTerreng.ToString()));
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
                Enum noko = null;
                if (spatialElement.Name.LocalName.Equals(RpSpatialElement.RpBestemmelseRegTerreng.ToString()))
                    noko = Enum.Parse<RegTerrengOverflateType>(spatialElement.GetValue("//*:overflateType"));

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