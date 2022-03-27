using DiBK.Plankart.Application.Extensions;
using DiBK.Plankart.Application.Models.Map;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace DiBK.Plankart.Application.Services
{
    public class GmlToGeoJsonService : IGmlToGeoJsonService
    {
        public GeoJsonFeatureCollection CreateGeoJsonDocument(XDocument document, Dictionary<string, string> geoElementMappings = null)
        {
            if (document == null)
                return null;

            var featureMembers = document.GetElements("//*:featureMember/* | //*:featureMembers/*");
            var featureCollection = new GeoJsonFeatureCollection();

            foreach (var featureMember in featureMembers)
            {
                var geoElement = GetGeometryElement(featureMember, geoElementMappings);

                if (geoElement == null)
                    continue;

                using var geometry = GetOGRGeometry(geoElement);

                if (geometry == null)
                    continue;

                var feature = new GeoJsonFeature { Geometry = GetGeometry(geometry) };

                geoElement.Remove();

                feature.Properties = CreateProperties(featureMember, featureMember.GetName(), featureMember.GetAttribute("gml:id"));

                featureCollection.Features.Add(feature);
            }

            return featureCollection;
        }

        private static XElement GetGeometryElement(XElement featureMember, Dictionary<string, string> geoElementMappings)
        {
            if (geoElementMappings != null && geoElementMappings.TryGetValue(featureMember.GetName(), out var xPath))
                xPath = $"*:{xPath}/*";
            else
                xPath = "*/gml:*";

            return featureMember.GetElement(xPath);
        }

        private static Geometry GetOGRGeometry(XElement geoElement)
        {
            if (!TryCreateGeometry(geoElement, out var geometry))
                return null;

            var linearGeometry = geometry.GetLinearGeometry(0, Array.Empty<string>());
            geometry.Dispose();

            return linearGeometry;
        }

        private static bool TryCreateGeometry(XElement geoElement, out Geometry geometry)
        {
            try
            {
                geometry = Geometry.CreateFromGML(geoElement.ToString());
                return true;
            }
            catch
            {
                geometry = null;
                return false;
            }
        }

        private static GeoJsonGeometry GetGeometry(Geometry geometry)
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
