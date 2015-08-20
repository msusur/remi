using System;

namespace ReMi.DataAccess.Exceptions
{
    public class RoleAlreadyExistsException : EntityAlreadyExistsException
    {
        public RoleAlreadyExistsException(Guid roleId)
            : base("Role", roleId)
        {
        }
        public RoleAlreadyExistsException(string roleName)
            : base("Role", roleName)
        {
        }

        public RoleAlreadyExistsException(Guid roleId, Exception innerException)
            : base("Role", roleId, innerException)
        {
        }

        public RoleAlreadyExistsException(string roleName, Exception innerException)
            : base("Role", roleName, innerException)
        {
        }
    }
}
