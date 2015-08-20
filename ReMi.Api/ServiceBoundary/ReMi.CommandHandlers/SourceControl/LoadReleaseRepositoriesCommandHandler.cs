using ReMi.Commands.SourceControl;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.Contracts.Plugins.Services.SourceControl;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.CommandHandlers.SourceControl
{
    public class LoadReleaseRepositoriesCommandHandler : IHandleCommand<LoadReleaseRepositoriesCommand>
    {
        public Func<IReleaseRepositoryGateway> ReleaseRepositoryGatewayFactory { get; set; }
        public Func<IProductGateway> PackageGatewayFactory { get; set; }
        public ISourceControl SourceControlService { get; set; }

        public void Handle(LoadReleaseRepositoriesCommand command)
        {
            IEnumerable<Guid> packageIds;
            using (var gateway = PackageGatewayFactory())
                packageIds = gateway.GetProducts(command.ReleaseWindowId).Select(x => x.ExternalId).ToArray();

            var repositories = SourceControlService.GetSourceControlRetrieveMode(packageIds)
                .GroupBy(x => x.Value)
                .Where(x => x.Key == SourceControlRetrieveMode.RepositoryIdentifier)
                .SelectMany(x => SourceControlService.GetRepositories(x.Select(y => y.Key)) ?? Enumerable.Empty<ReleaseRepository>())
                .Where(x => x != null && !x.IsDisabled)
                .ToArray();

            if (repositories.IsNullOrEmpty()) return;

            using (var gateway = ReleaseRepositoryGatewayFactory())
            {
                gateway.RemoveRepositoriesFromRelease(command.ReleaseWindowId);

                foreach (var repository in repositories)
                {
                    gateway.AddRepositoryToRelease(repository, command.ReleaseWindowId);
                }
            }
        }
    }
}
