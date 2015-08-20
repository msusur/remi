using ReMi.BusinessEntities.Api;

namespace ReMi.BusinessEntities.Auth
{
    public class CommandPermission
    {
        public int CommandPermissionId { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public int CommandId { get; set; }
        public Command Command { get; set; }
    }
}
