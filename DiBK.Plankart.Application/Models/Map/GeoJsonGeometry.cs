namespace DiBK.Plankart.Application.Models.Map
{
    public abstract class GeoJsonGeometry
    {
        public string Type { get; set; }
    }

    public class Point : GeoJsonGeometry
    {
        public double[] Coordinates { get; set; }

        public Point(string type, double[] coordinates)
        {
            Type = type;
            Coordinates = coordinates;
        }
    }

    public class MultiPoint : GeoJsonGeometry
    {
        public double[][] Coordinates { get; set; }

        public MultiPoint(string type, double[][] coordinates)
        {
            Type = type;
            Coordinates = coordinates;
        }
    }

    public class LineString : GeoJsonGeometry
    {
        public double[][] Coordinates { get; set; }

        public LineString(string type, double[][] coordinates)
        {
            Type = type;
            Coordinates = coordinates;
        }
    }

    public class MultiLineString : GeoJsonGeometry
    {
        public double[][][] Coordinates { get; set; }

        public MultiLineString(string type, double[][][] coordinates)
        {
            Type = type;
            Coordinates = coordinates;
        }
    }

    public class Polygon : GeoJsonGeometry
    {
        public double[][][] Coordinates { get; set; }

        public Polygon(string type, double[][][] coordinates)
        {
            Type = type;
            Coordinates = coordinates;
        }
    }

    public class MultiPolygon : GeoJsonGeometry
    {
        public double[][][][] Coordinates { get; set; }

        public MultiPolygon(string type, double[][][][] coordinates)
        {
            Type = type;
            Coordinates = coordinates;
        }
    }

    public class GeometryType
    {
        public static readonly string Point = "Point";
        public static readonly string MultiPoint = "MultiPoint";
        public static readonly string LineString = "LineString";
        public static readonly string MultiLineString = "MultiLineString";
        public static readonly string Polygon = "Polygon";
        public static readonly string MultiPolygon = "MultiPolygon";
    }
}
