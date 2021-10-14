using DiBK.RuleValidator.Config;
using System;
using System.Collections.Generic;

namespace DiBK.Plankart.Config
{
    public class Validator
    {
        public object DataType { get; init; }
        public Type SchemaRuleType { get; init; }
        public IEnumerable<Type> RuleTypes { get; init; }
        public Action<ValidationOptions> ValidationOptions { get; init; }
    }
}
