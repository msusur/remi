using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ReMi.Common.Utils;
using ReMi.Plugin.Go.Entities;

namespace ReMi.Plugin.Go.BusinessLogic
{
    public class GitCommitsCollector : IGitCommitsCollector
    {
        private readonly IGoRequest _goRequest;

        private static readonly object ParentsSync = new object();
        private static readonly object ResultSync = new object();

        public GitCommitsCollector(IGoRequest goRequest)
        {
            _goRequest = goRequest;
        }

        public IEnumerable<GitCommit> Collect(IEnumerable<string> livePipelines)
        {
            if (livePipelines == null)
                return null;

            var liveParents = new Dictionary<string, int>();
            Parallel.ForEach(livePipelines, livePipeline =>
            {
                var tempLiveParents = FindLatestParentsPipelinesWithBuildNumbers(livePipeline);
                if (tempLiveParents != null)
                    FillParents(tempLiveParents, liveParents);
            });

            var currentParents = new ConcurrentDictionary<string, int>();
            Parallel.ForEach(liveParents, liveParent =>
            {
                var lastBuildNumber = FindLatestBuildNumer(liveParent.Key);
                if (lastBuildNumber > 0)
                    currentParents.TryAdd(liveParent.Key, lastBuildNumber);
            });
            var lastGitCommits = FetchLastGitCommitsHistory(liveParents).Values.SelectMany(x => x.Values);
            var gitCommits = FetchGitCommitsHistory(liveParents, currentParents).Values.SelectMany(x => x.Values);

            return gitCommits
                .Where(x => lastGitCommits.All(l => l.Revision != x.Revision))
                .ToList();
        }

        private IDictionary<string, IDictionary<string, GitCommit>> FetchGitCommitsHistory(
            IDictionary<string, int> liveParents, IEnumerable<KeyValuePair<string, int>> stagingParents)
        {
            var result = new Dictionary<string, IDictionary<string, GitCommit>>();

            foreach (var stagingParent in stagingParents)
            {
                var stagingLastBuild = stagingParent.Value;
                var liveLastBuild = liveParents.ContainsKey(stagingParent.Key)
                    ? liveParents[stagingParent.Key] + 1 // we don't won't to take something which already gone live
                    : 1;

                var parent = stagingParent;
                Parallel.For(liveLastBuild, stagingLastBuild + 1, number =>
                {
                    var valueStreamMap = _goRequest.GetPipelineValueStreamMap(parent.Key,
                        number.ToString(CultureInfo.InvariantCulture));

                    if (valueStreamMap == null || valueStreamMap.Levels.IsNullOrEmpty())
                        return;
                    if (valueStreamMap.Levels.First(x => x.Nodes.Any(n => n.Name == parent.Key))
                        .Nodes.First().Instances.Any(x => x.Stages.Any(s => s.Status != "Passed")))
                        return;

                    var levels = valueStreamMap.Levels;

                    foreach (var node in levels
                        .Where(level => !level.Nodes.IsNullOrEmpty())
                        .SelectMany(level => level.Nodes.Where(node => node.Node_Type == NoteType.Git)))
                    {
                        lock (ResultSync)
                        {
                            if (!result.ContainsKey(node.Name))
                                result.Add(node.Name, new Dictionary<string, GitCommit>());
                            var repo = result[node.Name];
                            foreach (var instance in node.Instances.Where(instance => !repo.ContainsKey(instance.Revision)))
                            {
                                repo.Add(instance.Revision, new GitCommit
                                {
                                    Comment = instance.Comment,
                                    User = instance.User,
                                    ModifiedTime = instance.Modified_Time,
                                    Revision = instance.Revision,
                                    Repository = node.Name,
                                    Name = RepositoryNameResolver.Resolve(node.Name)
                                });
                            }
                        }
                    }
                });
            }

            return result;
        }
        private IDictionary<string, IDictionary<string, GitCommit>> FetchLastGitCommitsHistory(
            IEnumerable<KeyValuePair<string, int>> liveParents)
        {
            var result = new Dictionary<string, IDictionary<string, GitCommit>>();

            Parallel.ForEach(liveParents, liveParent =>
            {
                var valueStreamMap = _goRequest.GetPipelineValueStreamMap(liveParent.Key,
                    liveParent.Value.ToString(CultureInfo.InvariantCulture));

                if (valueStreamMap == null || valueStreamMap.Levels.IsNullOrEmpty())
                    return;

                var levels = valueStreamMap.Levels;

                foreach (var node in levels
                    .Where(level => !level.Nodes.IsNullOrEmpty())
                    .SelectMany(level => level.Nodes.Where(node => node.Node_Type == NoteType.Git)))
                {
                    lock (ResultSync)
                    {
                        if (!result.ContainsKey(node.Name))
                            result.Add(node.Name, new Dictionary<string, GitCommit>());
                        var repo = result[node.Name];
                        foreach (var instance in node.Instances.Where(instance => !repo.ContainsKey(instance.Revision)))
                        {
                            repo.Add(instance.Revision, new GitCommit
                            {
                                Comment = instance.Comment,
                                User = instance.User,
                                ModifiedTime = instance.Modified_Time,
                                Revision = instance.Revision,
                                Repository = node.Name,
                                Name = RepositoryNameResolver.Resolve(node.Name)
                            });
                        }
                    }
                }
            });

            return result;
        }

        private static void FillParents(IEnumerable<KeyValuePair<string, int>> tempParents, IDictionary<string, int> parents)
        {
            foreach (var parent in tempParents)
            {
                lock (ParentsSync)
                {
                    if (!parents.ContainsKey(parent.Key))
                    {
                        parents.Add(parent.Key, parent.Value);
                    }
                    else if (parents[parent.Key] > parent.Value)
                    {
                        parents[parent.Key] = parent.Value;
                    }
                }
            }
        }

        private int FindLatestBuildNumer(string pipelineName)
        {
            var history = FindLatestHistory(pipelineName);

            if (history == null) return -1;

            return int.Parse(history.CounterOrLabel);
        }

        private Dictionary<string, int> FindLatestParentsPipelinesWithBuildNumbers(string pipelineName)
        {
            var history = FindLatestHistory(pipelineName);

            if (history == null) return null;

            if (history.MaterialRevisions == null) return null;

            var result = history.MaterialRevisions
                .Where(x => x.Counter > 0 && x.PipelineName != null)
                .ToDictionary(materialRevision => materialRevision.PipelineName, materialRevision => materialRevision.Counter);

            return result;
        }

        private PipelineHistory FindLatestHistory(string pipelineName)
        {
            var pipelineInfo = _goRequest.GetPipelineInfo(pipelineName);

            if (pipelineInfo == null || pipelineInfo.Groups.IsNullOrEmpty())
                return null;

            var group = pipelineInfo.Groups.First(x => !x.History.IsNullOrEmpty());

            return group.History.IsNullOrEmpty() ? null : group.History.First();
        }
    }
}
