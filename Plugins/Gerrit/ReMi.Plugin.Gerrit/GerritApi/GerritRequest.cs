using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.Plugin.Gerrit.GerritApi.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;

namespace ReMi.Plugin.Gerrit.GerritApi
{
    public class GerritRequest : IGerritRequest
    {
        private const string GitLogCommandTemplate
            = "gitcommand log --project {0} {1}..{2} --pretty format:\"%H<sep>%p<sep>%an<sep>%ai<sep>%d<sep>%s<sep>%b<end>\"";
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public Func<ISshClient> SshClientFactory { get; set; }

        public IDictionary<ReleaseRepository, IEnumerable<GitLogEntity>> GetGitLog(IEnumerable<ReleaseRepository> repositories)
        {
            if (repositories.IsNullOrEmpty())
                return null;

            Log.DebugFormat("Starting connecting to Gerrit throught SSH");
            using (var sshClient = SshClientFactory())
            {
                sshClient.Connect();
                Log.DebugFormat("Connection to Gerrit established, starting executing command");

                var results = new ConcurrentDictionary<ReleaseRepository, IEnumerable<GitLogEntity>>();
                Parallel.ForEach(repositories, repository =>
                {
                    if (sshClient == null) return;

                    var command = string.Format(GitLogCommandTemplate, repository.Repository,
                        repository.ChangesFrom, repository.ChangesTo);
                    Log.DebugFormat("Executing command  [{0}]", command);

                    var result = sshClient.ExecuteCommand(command);

                    if (!string.IsNullOrWhiteSpace(result)
                        && !results.TryAdd(repository, GitLogParser.Parse(result)
                        .Where(x => x.CommitType == CommitType.Typical).ToArray())) { }

                    Log.DebugFormat("Finished executing command  [{0}]", command);
                });

                return results;
            }
        }
    }
}
