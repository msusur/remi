using System;

namespace ReMi.DataAccess.Exceptions
{
    public class RoleNotFoundException : EntityNotFoundException
    {
        public RoleNotFoundException(Guid roleExternalId)
            : base("Command", roleExternalId)
        {
        }

        public RoleNotFoundException(Guid roleExternalId, Exception innerException)
            : base("Command", roleExternalId, innerException)
        {
        }
    }
}
