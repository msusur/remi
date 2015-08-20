using AutoMapper;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.Contracts.Plugins.Services.SourceControl;
using ReMi.Plugin.Go.DataAccess.Gateways;
using ReMi.Plugin.Go.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.Plugin.Go.BusinessLogic
{
    public class GoSourceControl : ISourceControl
    {
        public IGoRequest GoRequest { get; set; }
        public Func<IPackageConfigurationGateway> PackageConfigurationGatewayFactory { get; set; }
        public IMappingEngine Mapper { get; set; }

        public IEnumerable<ReleaseRepository> GetRepositories(IEnumerable<Guid> packageIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SourceControlChange> GetChanges(IEnumerable<Guid> packageIds)
        {
            return GetChangesByReleaseJobs(packageIds, null);
        }

        public IEnumerable<SourceControlChange> GetChangesByReleaseJobs(IEnumerable<Guid> packageIds, IEnumerable<Guid> jobIds)
        {
            IEnumerable<PluginPackageConfigurationEntity> configurations;
            using (var gateway = PackageConfigurationGatewayFactory())
            {
                configurations = gateway.GetPackagesConfiguration(packageIds).ToArray();
            }

            var changes = configurations
                .Select(x =>
                {
                    GoRequest.ChangeBaseUrl(x.GoServerUrl);
                    var pipelines = x.GoPipelines
                        .Where(p => !p.IsDisabled && (jobIds == null || jobIds.Any(j => j == p.ExternalId)))
                        .Select(p => p.Name)
                        .ToArray();

                    return GoRequest.GetReadyToReleaseCommits(pipelines);
                })
                .SelectMany(x => x)
                .Where(c => !c.Comment.Contains("Merge") && !c.Comment.Contains("Merging"))
                .ToArray();

            changes.Each(c =>
            {
                if (c.Comment.Contains("Conflicts:"))
                {
                    c.Comment = c.Comment.Substring(0, c.Comment.IndexOf("Conflicts:", StringComparison.Ordinal));
                }
            });

            return Mapper.Map<IEnumerable<GitCommit>, IEnumerable<SourceControlChange>>(changes);
        }

        public IEnumerable<SourceControlChange> GetChangesByRepository(IEnumerable<Guid> packageIds, IEnumerable<ReleaseRepository> repositories)
        {
            throw new NotImplementedException();
        }

        public IDictionary<Guid, SourceControlRetrieveMode> GetSourceControlRetrieveMode(IEnumerable<Guid> packageIds)
        {
            return packageIds.ToDictionary(x => x, x => SourceControlRetrieveMode.DeploymentJobs);
        }
    }
}
