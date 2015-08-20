using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data.SourceControl;

namespace ReMi.Contracts.Plugins.Services.SourceControl
{
    public interface ISourceControl : IPluginService
    {
        IEnumerable<ReleaseRepository> GetRepositories(IEnumerable<Guid> packageIds);

        IEnumerable<SourceControlChange> GetChanges(IEnumerable<Guid> packageIds);
        IEnumerable<SourceControlChange> GetChangesByReleaseJobs(IEnumerable<Guid> packageIds, IEnumerable<Guid> jobIds);
        IEnumerable<SourceControlChange> GetChangesByRepository(IEnumerable<Guid> packageIds, IEnumerable<ReleaseRepository> repositories);

        IDictionary<Guid, SourceControlRetrieveMode> GetSourceControlRetrieveMode(IEnumerable<Guid> packageIds);
    }
}
