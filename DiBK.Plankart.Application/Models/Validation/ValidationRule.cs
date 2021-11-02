using System.Collections.Generic;

namespace DiBK.Plankart.Application.Models.Validation
{
    public class ValidationRule
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<ValidationRuleMessage> Messages { get; set; } = new();
        public string Status { get; set; }
        public string PreCondition { get; set; }
        public string ChecklistReference { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string Documentation { get; set; }
        public string MessageType { get; set; }
        public int MessageCount => Messages?.Count ?? 0;
    }
}
