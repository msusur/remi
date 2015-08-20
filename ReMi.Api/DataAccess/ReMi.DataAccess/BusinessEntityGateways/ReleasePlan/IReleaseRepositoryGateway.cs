using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data.SourceControl;

namespace ReMi.DataAccess.BusinessEntityGateways.ReleasePlan
{
    public interface IReleaseRepositoryGateway : IDisposable
    {
        ReleaseRepository GetReleaseRepository(Guid releaseWindowId, Guid repositoryId);
        IEnumerable<ReleaseRepository> GetReleaseRepositories(Guid releaseWindowId);
        void AddRepositoryToRelease(ReleaseRepository repository, Guid releaseWindowId);
        void UpdateRepository(ReleaseRepository repository, Guid releaseWindowId);
        void RemoveRepositoriesFromRelease(Guid releaseWindowId);
    }
}
