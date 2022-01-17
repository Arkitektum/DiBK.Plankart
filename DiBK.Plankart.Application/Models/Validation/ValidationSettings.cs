namespace DiBK.Plankart.Application.Models.Validation
{
    public class ValidationSettings
    {
        public static readonly string SectionName = "Validation";
        public string ApiUrl { get; set; }
        public string XsdRuleId { get; set; }
        public string Epsg2dRuleId { get; set; }
        public string Epsg3dRuleId { get; set; }
    }
}
