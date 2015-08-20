using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.Api;

namespace ReMi.DataEntities.Auth
{
    [Table("CommandPermissions", Schema = Constants.AuthSchemaName)]
    public class CommandPermission
    {
        [Key]
        public int CommandPermissionId { get; set; }

        [Required]
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }

        [Required]
        public int CommandId { get; set; }
        public virtual Command Command { get; set; }
    }
}
