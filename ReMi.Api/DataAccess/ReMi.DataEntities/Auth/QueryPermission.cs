using ReMi.DataEntities.Api;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.Auth
{
    [Table("QueryPermissions", Schema = Constants.AuthSchemaName)]
    public class QueryPermission
    {
        [Key]
        public int QueryPermissionId { get; set; }

        [Required]
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }

        [Required]
        public int QueryId { get; set; }
        public virtual Query Query { get; set; }
    }
}
