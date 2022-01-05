using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DiBK.Plankart.Application.Models.Map
{
    public class CesiumMapDocument : MapDocument
    {
        public Rectangle Rectangle { get; set; }
        public CesiumDataCollection CesiumData { get; set; } = new();
    }

    public class Rectangle
    {
        public double West { get; set; }
        public double South { get; set; }
        public double East { get; set; }
        public double North { get; set; }
    }

    public class CesiumDataCollection
    {
        public string Type { get; } = "CzmlDataCollection";
        public List<CzmlDataObject> CzmlDataObjects { get; set; }
    }

    public class CzmlDataObject
    {
        public string Type { get; } = "CzmlDataObject";

        public JObject Properties { get; set; }
    }
}
