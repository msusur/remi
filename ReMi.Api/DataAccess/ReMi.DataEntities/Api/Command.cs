using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.BusinessRules;

namespace ReMi.DataEntities.Api
{
    [Table("Commands", Schema = Constants.ApiSchemaName)]
    public class Command
    {
        [Key]
        public int CommandId { get; set; }

        [Index(IsUnique = true)]
        [StringLength(256), Required]
        public string Name { get; set; }

        [StringLength(256), Required]
        public string Group { get; set; }

        [StringLength(256), Required]
        public string Description { get; set; }

        [StringLength(256)]
        public string Namespace { get; set; }

        [Required]
        public bool IsBackground { get; set; }

        public int? RuleId { get; set; }

        public virtual ICollection<CommandPermission> CommandPermissions { get; set; }

        [ForeignKey("RuleId")]
        public virtual BusinessRuleDescription Rule { get; set; }
    }
}
