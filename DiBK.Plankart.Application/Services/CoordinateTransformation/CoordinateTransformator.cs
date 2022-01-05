using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DiBK.Plankart.Application.Models.Map;
using MaxRev.Gdal.Core;
using OSGeo.OSR;

[assembly: InternalsVisibleTo("DiBK.Plankart.Application.Tests")]
namespace DiBK.Plankart.Application.Services.CoordinateTransformation
{
    internal class CoordinateTransformator
    {
        private readonly OSGeo.OSR.CoordinateTransformation _coordinateTransformation;

        public CoordinateTransformator(int sourceEpsgCode, int targetEpsgCode)
        {
            GdalBase.ConfigureAll();

            var sourceCoordinateSystem = new SpatialReference(string.Empty);
            var targetCoordinateSystem = new SpatialReference(string.Empty);

            sourceCoordinateSystem.ImportFromEPSG(sourceEpsgCode);
            targetCoordinateSystem.ImportFromEPSG(targetEpsgCode);

            _coordinateTransformation = new OSGeo.OSR.CoordinateTransformation(sourceCoordinateSystem, targetCoordinateSystem);
            if (_coordinateTransformation == null)
                throw new ArgumentException("Invalid EPSG code(s) or unsupported transformation");
        }

        public Coordinate Transform(double x, double y, double z=0)
        {
            var projected = new double[3];
            _coordinateTransformation.TransformPoint(projected, x, y, z);

            return new Coordinate { X = projected[0], Y = projected[1], Z = projected[2] };
        }

        public IEnumerable<Coordinate> Transform(List<double> xyz)
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
                result.Add(new Coordinate { X = yProj[i], Y = xProj[i], Z = zProj[i] + 55 });
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
    }
}
