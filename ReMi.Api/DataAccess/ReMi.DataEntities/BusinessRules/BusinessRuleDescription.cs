using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants.BusinessRules;

namespace ReMi.DataEntities.BusinessRules
{
    [Table("BusinessRule", Schema = Constants.BusinessRulesSchemaName)]
    public class BusinessRuleDescription
    {
        [Key]
        public int BusinessRuleId { get; set; }

        [Required]
        [StringLength(50)]
        [Index("IX_NameGroupUnique", 1, IsUnique = true)]
        public string Name { get; set; }

        [Index("IX_NameGroupUnique", 2, IsUnique = true)]
        public BusinessRuleGroup Group { get; set; }

        [Required]
        [StringLength(256)]
        public string Description { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        public virtual ICollection<BusinessRuleParameter> Parameters { get; set; }

        public virtual BusinessRuleAccountTestData AccountTestData { get; set; }

        public string Script { get; set; }
    }
}
