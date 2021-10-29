using System;
using System.Collections.Generic;
using System.Linq;

namespace DiBK.Plankart.Application.Models.Validation
{
    public class ValidationResult
    {
        public Guid Id { get; } = Guid.NewGuid();
        public IEnumerable<ValidationRule> Rules { get; set; } = Array.Empty<ValidationRule>();
        public IEnumerable<string> XsdValidationMessages { get; set; } = Array.Empty<string>();
        public bool XsdValidated => !XsdValidationMessages.Any();
    }
}
