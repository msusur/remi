using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.BusinessRules
{
    [Table("BusinessRuleAccountTestData", Schema = Constants.BusinessRulesSchemaName)]
    public class BusinessRuleAccountTestData
    {
        [Key]
        public int BusinessRuleAccountTestDataId { get; set; }
        
        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        public string JsonData { get; set; }

        public virtual BusinessRuleDescription Rule { get; set; }
    }
}
