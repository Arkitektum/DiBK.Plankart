using DiBK.Plankart.Application.Extensions;
using DiBK.Plankart.Application.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wmhelp.XPath2;
using OGRGeometry = OSGeo.OGR.Geometry;

namespace DiBK.Plankart.Application.Services
{
    public class GmlToGeoJsonService : IGmlToGeoJsonService
    {
        private static readonly Regex _epsgRegex = new(@"^(http:\/\/www\.opengis\.net\/def\/crs\/EPSG\/0\/|^urn:ogc:def:crs:EPSG::)(?<epsg>\d+)$", RegexOptions.Compiled);

        public async Task<GeoJsonDocument> CreateGeoJsonDocument(IFormFile gmlFile, Dictionary<string, string> geoFieldMappings = null)
        {
            var document = await LoadXDocument(gmlFile);

            if (document == null)
                return null;

            var featureDocument = new GeoJsonDocument(gmlFile.FileName, GetEpsg(document));
            var featureMembers = document.GetElements("//*:featureMember/* | //*:featureMembers/*");

            foreach (var featureMember in featureMembers)
            {
                var geoElement = GetGeometryElement(featureMember, geoFieldMappings);

                if (geoElement == null)
                    continue;

                using var geometry = GetOGRGeometry(geoElement);

                if (geometry == null)
                    continue;

                var feature = new GeoJsonFeature { Geometry = GetGeometry(geometry) };

                geoElement.Remove();
                
                feature.Properties = CreateProperties(featureMember, featureMember.GetName(), featureMember.GetAttribute("gml:id"));

                featureDocument.FeatureCollection.Features.Add(feature);
            }

            return featureDocument;
        }

        private static async Task<XDocument> LoadXDocument(IFormFile gmlFile)
        {
            try
            {
                return await XDocument.LoadAsync(gmlFile.OpenReadStream(), LoadOptions.None, new CancellationToken());
            }
            catch
            {
                return null;
            }
        }

        private static XElement GetGeometryElement(XElement featureMember, Dictionary<string, string> geoFieldMappings)
        {
            if (geoFieldMappings != null && geoFieldMappings.TryGetValue(featureMember.GetName(), out var xPath))
                xPath = $"*:{xPath}/*";
            else
                xPath = "*/gml:*";

            return featureMember.GetElement(xPath);
        }

        private static string GetEpsg(XDocument document)
        {
            var srsName = document.XPath2SelectOne<XAttribute>("(//*[@srsName]/@srsName)[1]")?.Value;

            if (srsName == null)
                return null;

            var match = _epsgRegex.Match(srsName);

            return match.Success ? $"EPSG:{match.Groups["epsg"].Value}" : null;
        }

        private static OGRGeometry GetOGRGeometry(XElement geoElement)
        {
            if (!TryCreateGeometry(geoElement, out var tempGeometry))
                return null;

            if (!geoElement.Has("//gml:Arc"))
                return tempGeometry;

            OGRGeometry geometry = null;
            var geometryType = tempGeometry.GetGeometryType();

            switch (geometryType)
            {
                case wkbGeometryType.wkbCircularString:
                    geometry = Ogr.ForceToLineString(tempGeometry);
                    break;
                case wkbGeometryType.wkbSurface:
                    geometry = Ogr.ForceToPolygon(tempGeometry);
                    break;
                case wkbGeometryType.wkbMultiSurface:
                    geometry = Ogr.ForceToMultiPolygon(tempGeometry);
                    break;
                default:
                    break;
            }

            tempGeometry.Dispose();
            return geometry;
        }

        private static bool TryCreateGeometry(XElement geoElement, out OGRGeometry geometry)
        {
            try
            {
                geometry = OGRGeometry.CreateFromGML(geoElement.ToString());
                return true;
            }
            catch
            {
                geometry = null;
                return false;
            }
        }

        private static GeoJsonGeometry GetGeometry(OGRGeometry geometry)
        {
            var json = geometry.ExportToJson(Array.Empty<string>());

            if (json == null)
                return null;

            var jObject = JObject.Parse(json);
            var type = jObject["type"].ToString();
            var coordinates = jObject["coordinates"].ToString();

            if (type == GeometryType.Point)
            {
                return new Point(type, JsonConvert.DeserializeObject<double[]>(coordinates));
            }

            if (type == GeometryType.MultiPoint)
            {
                return new MultiPoint(type, JsonConvert.DeserializeObject<double[][]>(coordinates));
            }

            if (type == GeometryType.LineString)
            {
                return new LineString(type, JsonConvert.DeserializeObject<double[][]>(coordinates));
            }

            if (type == GeometryType.MultiLineString)
            {
                return new MultiLineString(type, JsonConvert.DeserializeObject<double[][][]>(coordinates));
            }

            if (type == GeometryType.Polygon)
            {
                return new Polygon(type, JsonConvert.DeserializeObject<double[][][]>(coordinates));
            }

            if (type == GeometryType.MultiPolygon)
            {
                return new MultiPolygon(type, JsonConvert.DeserializeObject<double[][][][]>(coordinates));
            }

            return null;
        }

        private static JObject CreateProperties(XElement featureMember, string featureName, string gmlId)
        {
            featureMember.Name = "values";

            var builder = new StringBuilder();
            using var writer = new StringWriter(builder);

            var serializer = JsonSerializer.Create();
            serializer.Serialize(new GmlToJsonWriter(writer), featureMember);

            var jObject = JObject.Parse(builder.ToString());
            var values = jObject["values"] as JObject;

            values.Add(new JProperty("name", featureName));
            values.Add(new JProperty("label", $"{featureName} '{gmlId}'"));

            return jObject["values"] as JObject;
        }
    }
}
