using System.Collections.Generic;

namespace DiBK.Plankart.Application.Models.Validation
{
    public class ValidationRuleMessage
    {
        public string Message { get; set; }
        public string FileName { get; set; }
        public IEnumerable<string> XPaths { get; set; }
        public IEnumerable<string> GmlIds { get; set; }
        public string ZoomTo { get; set; }

        public ValidationRuleMessage(string message, string fileName, IEnumerable<string> xPaths, IEnumerable<string> gmlIds, string zoomTo)
        {
            Message = message;
            FileName = fileName;
            XPaths = xPaths;
            GmlIds = gmlIds;
            ZoomTo = zoomTo;
        }
    }
}
