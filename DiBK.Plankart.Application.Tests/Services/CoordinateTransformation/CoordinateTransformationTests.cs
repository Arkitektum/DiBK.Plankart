using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using DiBK.Plankart.Application.Exceptions;
using DiBK.Plankart.Application.Models.Map;
using DiBK.Plankart.Application.Services;
using DiBK.Plankart.Application.Services.CoordinateTransformation;
using Wmhelp.XPath2;
using Xunit;

namespace DiBK.Plankart.Application.Tests.Services
{
    public class CoordinateTransformationTests
    {
        [Fact, Trait("Category", "GDAL_Proj_Transformation")]
        public void TransformationTest()
        {
            const int sourceEpsgCode = 5972;
            //const int targetEpsgCode = 4326;
            const int targetEpsgCode = 4979;

            var sourceCoordinate = new List<double>{299416.02, 6695529.56, 138};
            var sourceCoordinate2 = new List<double>{299439.62, 6695534.72, 149.66};

            var coordinateTransformer = new CoordinateTransformer(sourceEpsgCode, targetEpsgCode);

            var transformedCoordinate = coordinateTransformer.Transform(sourceCoordinate);
            var transformedCoordinateAndHeight = coordinateTransformer.Transform(sourceCoordinate, true);
            var transformedCoordinate2 = coordinateTransformer.Transform(sourceCoordinate2);

            var coordinate = transformedCoordinate.First();
            var coordinateAndHeight = transformedCoordinateAndHeight.First();
            var coordinate2 = transformedCoordinate2.First();

            Assert.Equal(5.36455553, coordinate.X, 8);
            Assert.Equal(60.34645969, coordinate.Y, 8);
            Assert.Equal(138, coordinate.Z);

            Assert.Equal(5.36455553, coordinateAndHeight.X, 8);
            Assert.Equal(60.34645969, coordinateAndHeight.Y, 8);
            Assert.Equal(188, coordinateAndHeight.Z);

            Assert.Equal(5.364977108609242, coordinate2.X);
            Assert.Equal(60.34651760036854, coordinate2.Y);
            Assert.Equal(199.66, coordinate2.Z);
        }

        [Fact, Trait("Integration", "GML data extraction")]
        public void Load3dGmlStuff()
        {
            var filePath = "C:\\Users\\LeifHalvorSunde\\FTP\\3d-plankart\\nattlandsfjellet_3d.gml";

            var xDoc = LoadXDocument(filePath).Result;

            var dataCollection = new GmlToCzmlService().CreateCzmlCollection(xDoc, GetEpsg(xDoc).Code, null);

            Assert.True(dataCollection.CzmlStrings.Any());
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

        private static Epsg GetEpsg(XDocument document)
        {
            var srsName = document.XPath2SelectOne<XAttribute>("(//*[@srsName]/@srsName)[1]")?.Value;

            if (srsName == null)
                return null;

            return Epsg.Create(srsName);
        }
    }
}