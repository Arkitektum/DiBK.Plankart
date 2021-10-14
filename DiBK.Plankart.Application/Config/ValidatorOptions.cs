using DiBK.RuleValidator.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiBK.Plankart.Config
{
    public class ValidatorOptions
    {
        public List<Validator> Validators { get; } = new();
        public Type GetSchemaRuleType(object dataType) => GetValidator(dataType)?.SchemaRuleType;
        public IEnumerable<Type> GetRuleTypes(object dataType) => GetValidator(dataType)?.RuleTypes;
        public Action<ValidationOptions> GetValidationOptions(object dataType) => GetValidator(dataType)?.ValidationOptions;
        public Validator GetValidator(object dataType) => Validators.SingleOrDefault(validator => validator.DataType.ToString() == dataType.ToString());

        public void AddValidator(object dataType, Type schemaRuleType, IEnumerable<Type> ruleTypes, Action<ValidationOptions> options = null)
        {
            var allRuleTypes = new List<Type> { schemaRuleType };
            allRuleTypes.AddRange(ruleTypes);

            Validators.Add(new Validator
            {
                DataType = dataType,
                SchemaRuleType = schemaRuleType,
                RuleTypes = allRuleTypes,
                ValidationOptions = options
            });
        }
    }
}
