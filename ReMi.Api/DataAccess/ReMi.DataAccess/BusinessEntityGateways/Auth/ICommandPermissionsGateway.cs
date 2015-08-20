using ReMi.BusinessEntities.Api;
using System;
using System.Collections.Generic;

namespace ReMi.DataAccess.BusinessEntityGateways.Auth
{
    public interface ICommandPermissionsGateway : IDisposable
    {
        IEnumerable<Command> GetCommands(bool includeBackground = false);
        IEnumerable<String> GetAllowedCommands(Guid roleId);

        void AddCommandPermission(int commandId, Guid roleExternalId);
        void RemoveCommandPermission(int commandId, Guid roleExternalId);
    }
}
