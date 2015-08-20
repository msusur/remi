using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.BusinessRules
{
    [Table("BusinessRuleParameter", Schema = Constants.BusinessRulesSchemaName)]
    public class BusinessRuleParameter
    {
        [Key]
        public int BusinessRuleParameterId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        [Required, Index("IX_BusinessRuleAndName", 1)]
        public int BusinessRuleId { get; set; }

        [Required, Index("IX_BusinessRuleAndName", 2), StringLength(50)]
        public string Name { get; set; }

        [Required, StringLength(256)]
        public string Type { get; set; }

        public virtual BusinessRuleTestData TestData { get; set; }

        public virtual BusinessRuleDescription BusinessRule { get; set; }
    }
}
