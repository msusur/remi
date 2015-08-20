
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.Plugin.Gerrit.GerritApi.Model;

namespace ReMi.Plugin.Gerrit.GerritApi
{
    public interface IGerritRequest
    {
        IDictionary<ReleaseRepository, IEnumerable<GitLogEntity>> GetGitLog(IEnumerable<ReleaseRepository> repositories);
    }
}
