using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DiBK.Plankart.Application.Models.Map;
using DiBK.Plankart.Application.Utils;
using MaxRev.Gdal.Core;
using OSGeo.OSR;

[assembly: InternalsVisibleTo("DiBK.Plankart.Application.Tests")]
namespace DiBK.Plankart.Application.Services
{
    internal class CoordinateTransformer
    {
        private readonly CoordinateTransformation _coordinateTransformation;
        private readonly double _heightOffset;

        public CoordinateTransformer(int sourceEpsgCode, int targetEpsgCode, IReadOnlyList<string> heightOffsetReferencePoint=null)
        {
            GdalBase.ConfigureAll();

            var sourceCoordinateSystem = new SpatialReference(string.Empty);
            var targetCoordinateSystem = new SpatialReference(string.Empty);

            sourceCoordinateSystem.ImportFromEPSG(sourceEpsgCode);
            targetCoordinateSystem.ImportFromEPSG(targetEpsgCode);

            _coordinateTransformation = new CoordinateTransformation(sourceCoordinateSystem, targetCoordinateSystem);
            if (_coordinateTransformation == null)
                throw new ArgumentException("Invalid EPSG code(s) or unsupported transformation");

            if (heightOffsetReferencePoint != null)
                _heightOffset = GetHeightOffset(heightOffsetReferencePoint, sourceEpsgCode, targetEpsgCode).Result;
        }

        public Coordinate Transform(double x, double y, double z=0, bool transformHeight = false)
        {
            var projected = new double[3];
            _coordinateTransformation.TransformPoint(projected, x, y, z);

            return new Coordinate { X = projected[0], Y = projected[1], Z = z + (transformHeight ? _heightOffset : 0) };
        }

        public IEnumerable<Coordinate> Transform(List<double> xyz, bool transformHeight=false)
        {
            var length = xyz.Count;
            if (length % 3 != 0)
                throw new ArgumentException("Coordinate array must be dividable by 3");

            var (x, y, z) = SplitCoordinates(xyz);
            var nPoints = length / 3;
            var xProj = x.ToArray();
            var yProj = y.ToArray();
            var zProj = z.ToArray();

            _coordinateTransformation.TransformPoints(nPoints, xProj, yProj, zProj);

            var result = new List<Coordinate>();

            for (var i = 0; i < nPoints; i++)
            {
                result.Add(new Coordinate { X = yProj[i], Y = xProj[i], Z = zProj[i] + (transformHeight ? _heightOffset : 0) });
            }
            return result;
        }

        private static (IEnumerable<double> x, IEnumerable<double> y, IEnumerable<double> z) SplitCoordinates(IEnumerable<double> xyz)
        {
            var x = new List<double>();
            var y = new List<double>();
            var z = new List<double>();
            var counter = 1;

            foreach (var coordinate in xyz)
            {
                switch (counter)
                {
                    case 1:
                        x.Add(coordinate);
                        break;
                    case 2:
                        y.Add(coordinate);
                        break;
                    case 3:
                        z.Add(coordinate);
                        counter = 0;
                        break;
                }

                counter++;
            }

            return (x, y, z);
        }

        private static async Task<double> GetHeightOffset(IReadOnlyList<string> referencePoint, int fromEpsg, int toEpsg)
        {
            if (toEpsg == Epsg.CesiumCoordinateSystemCode)
                toEpsg = Epsg.CesiumCoordinateSystemCode2D;

            var x = referencePoint[0];
            var y = referencePoint[1];
            var h = referencePoint[2];

            var getUri = new Uri($"https://ws.geonorge.no/transformering/v1/transformer?x={x}&y={y}&z={h}&fra={fromEpsg}&til={toEpsg}");

            using var client = new HttpClient();
            var coordinate = await client.GetFromJsonAsync<Coordinate>(getUri);

            return coordinate?.Z - double.Parse(h, ApplicationConfig.DoubleFormatInfo) ?? 0;
        }
    }
}
