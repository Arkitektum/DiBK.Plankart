using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DiBK.Plankart.Application.Models.Map;

[assembly: InternalsVisibleTo("DiBK.Plankart.Application.Tests")]
namespace DiBK.Plankart.Application.Services.CoordinateTransformation
{
    internal class CoordinateTransformator
    {
        private static int _sourceEpsgCode;
        private static int _targetEpsgCode;

        public CoordinateTransformator(int sourceEpsgCode, int targetEpsgCode)
        {
            _sourceEpsgCode = sourceEpsgCode;
            _targetEpsgCode = targetEpsgCode;
        }

        public Coordinate Transform(double x, double y, double z=0)
        {
            return TransformAsync(x, y, z).Result;
        }

        public Coordinate Transform(double[] xyz)
        {
            if (xyz.Length % 3 != 0)
                return null;

            return TransformAsync(xyz[0], xyz[1], xyz[2]).Result;
        }

        private static async Task<Coordinate> TransformAsync(double x, double y, double z)
        {
            var nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = "."
            };

            var getUri = $@"trans?x={x.ToString(nfi)}&y={y.ToString(nfi)}&z={z.ToString(nfi)}&s_srs={_sourceEpsgCode}&t_srs={_targetEpsgCode}";
            using var client = new HttpClient { BaseAddress = new Uri(@"http://epsg.io/") };
            
            return await client.GetFromJsonAsync<Coordinate>(getUri);
        }
    }
}
