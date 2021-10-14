using DiBK.Plankart.Application.Models.Validation;
using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace DiBK.Plankart.Application.Helpers
{
    public class ValidationHelper
    {
        public static IEnumerable<ValidationRule> CreateValidationRules(IEnumerable<Rule> rules)
        {
            return rules
                .Select(rule =>
                {
                    return new ValidationRule
                    {
                        Id = rule.Id,
                        Name = rule.Name,
                        Messages = rule.Messages
                            .Select(message =>
                            {
                                return message switch
                                {
                                    GmlRuleMessage msg => new ValidationRuleMessage(msg.Message, msg.FileName, msg.XPath, msg.GmlIds, msg.ZoomTo),
                                    XmlRuleMessage msg => new ValidationRuleMessage(msg.Message, msg.FileName, msg.XPath, null, null),
                                    RuleMessage msg => new ValidationRuleMessage(msg.Message, msg.FileName, null, null, null),
                                    _ => null
                                };
                            })
                            .Where(message => message != null),
                        MessageType = rule.MessageType.ToString(),
                        Status = rule.Status.ToString(),
                        Description = rule.Description,
                        Documentation = rule.Documentation
                    };
                });
        }
    }
}
