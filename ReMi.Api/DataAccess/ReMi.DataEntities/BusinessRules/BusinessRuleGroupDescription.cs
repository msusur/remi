using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants;
using ReMi.Common.Utils.Enums;

namespace ReMi.DataEntities.BusinessRules
{
    [Table("BusinessRuleGroup", Schema = Constants.BusinessRulesSchemaName)]
    public class BusinessRuleGroupDescription : EnumDescription
    {
    }
}
