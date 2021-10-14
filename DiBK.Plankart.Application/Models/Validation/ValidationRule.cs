using System;
using System.Collections.Generic;
using System.Linq;

namespace DiBK.Plankart.Application.Models.Validation
{
    public class ValidationRule
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Documentation { get; set; }
        public string MessageType { get; set; }
        public string Status { get; set; }
        public IEnumerable<ValidationRuleMessage> Messages { get; set; } = Array.Empty<ValidationRuleMessage>();
        public bool Passed => !Messages.Any();
    }
}

