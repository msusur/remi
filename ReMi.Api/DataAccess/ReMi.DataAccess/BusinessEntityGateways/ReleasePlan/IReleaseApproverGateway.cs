using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleasePlan;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public interface IReleaseApproverGateway : IDisposable
    {
        void AddApprover(ReleaseApprover approver);

        void ApproveRelease(Guid accountId, Guid releaseWindowId, String description);

        void RemoveApprover(Guid approverId);

        void ClearApproverSignatures(Guid releaseWindowId);

        DateTime? WhenApproved(Guid accountId, Guid releaseWindowId);

        IEnumerable<ReleaseApprover> GetApprovers(Guid releaseWindowId);
    }
}
