using System.Collections.Generic;
using System.Linq;

namespace DiBK.Plankart.Application.Models.Validation
{
    public class ValidationResult
    {
        public string Id { get; set; }
        public List<ValidationRule> Rules { get; set; } = new();
        public List<string> XsdValidationMessages { get; set; } = new();
        public List<string> EpsgValidationMessages { get; set; } = new();
        public bool XsdValidated => !XsdValidationMessages.Any();
        public bool EpsgValidated => !EpsgValidationMessages.Any();
    }
}
