using System.Collections.Generic;
using ReMi.Plugin.Go.Entities;

namespace ReMi.Plugin.Go.BusinessLogic
{
    public interface IGitCommitsCollector
    {
        IEnumerable<GitCommit> Collect(IEnumerable<string> liveAndStagingPipelines);
    }
}
