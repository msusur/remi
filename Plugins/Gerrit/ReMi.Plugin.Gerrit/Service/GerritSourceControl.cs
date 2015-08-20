using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.Contracts.Plugins.Services.SourceControl;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Common.Logging;
using ReMi.Plugin.Gerrit.DataAccess.Gateways;
using ReMi.Plugin.Gerrit.GerritApi;
using ReMi.Plugin.Gerrit.GerritApi.Model;
using ReMi.Plugin.Gerrit.Service.Model;

namespace ReMi.Plugin.Gerrit.Service
{
    public class GerritSourceControl : ISourceControl
    {
        public Func<IPackageConfigurationGateway> PackageConfigurationGatewayFactory { get; set; }
        public IGerritRequest GerritRequest { get; set; }
        public IMappingEngine Mapper { get; set; }
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public IEnumerable<ReleaseRepository> GetRepositories(IEnumerable<Guid> packageIds)
        {
            using (var gateway = PackageConfigurationGatewayFactory())
            {
                return Mapper.Map<IEnumerable<Repository>, IEnumerable<ReleaseRepository>>(
                    gateway.GetPackagesConfiguration(packageIds)
                        .SelectMany(x => x.Repositories));
            }
        }

        public IEnumerable<SourceControlChange> GetChanges(IEnumerable<Guid> packageIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SourceControlChange> GetChangesByReleaseJobs(IEnumerable<Guid> packageIds, IEnumerable<Guid> jobIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SourceControlChange> GetChangesByRepository(IEnumerable<Guid> packageIds, IEnumerable<ReleaseRepository> repositories)
        {
            Log.DebugFormat("Start getting git logs");
            var result = GerritRequest.GetGitLog(repositories)
                .SelectMany(x =>
            {
                var changes = Mapper.Map<IEnumerable<GitLogEntity>, IEnumerable<SourceControlChange>>(x.Value).ToArray();
                changes.Each(c => c.Repository = x.Key.Repository);
                return changes;
            }).ToArray();
            Log.DebugFormat("Finish getting git logs");
            return result;
        }

        public IDictionary<Guid, SourceControlRetrieveMode> GetSourceControlRetrieveMode(IEnumerable<Guid> packageIds)
        {
            return packageIds.ToDictionary(x => x, x => SourceControlRetrieveMode.RepositoryIdentifier);
        }
    }
}
