using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.BusinessRules;

namespace ReMi.DataEntities.Api
{
    [Table("Queries", Schema = Constants.ApiSchemaName)]
    public class Query
    {
        [Key]
        public int QueryId { get; set; }

        [Index(IsUnique = true)]
        [StringLength(256), Required]
        public string Name { get; set; }

        [StringLength(256), Required]
        public string Group { get; set; }

        [StringLength(256), Required]
        public string Description { get; set; }

        [StringLength(256), Required]
        public string Namespace { get; set; }

        [Required]
        public bool IsStatic { get; set; }

        public int? RuleId { get; set; }

        public virtual ICollection<QueryPermission> QueryPermissions { get; set; }

        [ForeignKey("RuleId")]
        public virtual BusinessRuleDescription Rule { get; set; }
    }
}
