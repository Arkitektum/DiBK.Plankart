namespace DiBK.Plankart.Application.Models.Map.Cesium
{
    public abstract class CesiumGraphic
    {
        public double[] Coordinates { get; set; }
    }

    // https://cesium.com/learn/cesiumjs/ref-doc/PolygonGraphics.html
    public class PolygonGraphic : CesiumGraphic
    {
        public PolygonGraphic(double[] coordinates)
        {
            Coordinates = coordinates;
        }
    }
}
