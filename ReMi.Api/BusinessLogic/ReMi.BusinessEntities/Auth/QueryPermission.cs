using ReMi.BusinessEntities.Api;

namespace ReMi.BusinessEntities.Auth
{
    public class QueryPermission
    {
        public int QueryPermissionId { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public int QueryId { get; set; }
        public Query Query { get; set; }
    }
}
