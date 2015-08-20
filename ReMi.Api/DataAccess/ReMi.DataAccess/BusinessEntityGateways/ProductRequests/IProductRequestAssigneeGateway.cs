using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;

namespace ReMi.DataAccess.BusinessEntityGateways.ProductRequests
{
    public interface IProductRequestAssigneeGateway : IDisposable
    {
        IEnumerable<Account> GetAssignees(Guid requestGroupId);

        void AppendAssignee(Guid requestGroupId, Guid assigneeExternalId);
        void RemoveAssignee(Guid requestGroupId, Guid assigneeExternalId);
    }
}
