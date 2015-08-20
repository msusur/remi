using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleaseExecution;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution
{
    public interface ISignOffGateway : IDisposable
    {
        void SignOff(Guid accountId, Guid releaseWindowId, String description);
        void RemoveSigner(Guid signOffId);
        void AddSigners(List<SignOff> signOffs, Guid releaseWindowId);

        List<SignOff> GetSignOffs(Guid releaseWindowId);
        Boolean CheckSigningOff(Guid releaseWindowId);
    }
}
