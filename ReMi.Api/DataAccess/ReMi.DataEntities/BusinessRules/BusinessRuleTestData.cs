using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.BusinessRules
{
    [Table("BusinessRuleTestData", Schema = Constants.BusinessRulesSchemaName)]
    public class BusinessRuleTestData
    {
        [Key]
        public int BusinessRuleTestDataId { get; set; }
        
        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        public string JsonData { get; set; }

        public virtual BusinessRuleParameter Parameter { get; set; }
    }
}
