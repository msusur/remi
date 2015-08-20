using AutoMapper;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.Contracts.Plugins.Services.SourceControl;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.BusinessEntityGateways.SourceControl;
using ReMi.DataAccess.Exceptions;
using ReMi.Queries.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;

namespace ReMi.QueryHandlers.ReleasePlan
{
    public class GetReleaseChangesHandler : IHandleQuery<GetReleaseChangesRequest, GetReleaseChangesResponse>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IProductGateway> ProductGatewayFactory { get; set; }
        public Func<ISourceControlChangeGateway> SourceControlChangeGatewayFactory { get; set; }
        public Func<IReleaseJobGateway> ReleaseJobGatewayFactory { get; set; }
        public Func<IReleaseRepositoryGateway> ReleaseRepositoryGatewayFactory { get; set; }
        public ISourceControl SourceControlService { get; set; }
        public IMappingEngine MappingEngine { get; set; }
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public GetReleaseChangesResponse Handle(GetReleaseChangesRequest request)
        {
            Log.DebugFormat("Starting getting release changes");
            ReleaseWindow releaseWindow;
            using (var windowGateway = ReleaseWindowGatewayFactory())
            {
                releaseWindow = windowGateway.GetByExternalId(request.ReleaseWindowId, true);
            }

            if (releaseWindow.Products.IsNullOrEmpty())
                throw new ProductShouldBeAssignedException(releaseWindow.ExternalId);

            if (releaseWindow.ApprovedOn.HasValue && !request.IsBackground)
            {
                var result = new GetReleaseChangesResponse { Changes = GetChangesFromDatabase(releaseWindow.ExternalId) };
                Log.DebugFormat("Finish getting release changes");
                return result;
            }
            IEnumerable<Guid> packageIds;
            using (var gateway = ProductGatewayFactory())
            {
                packageIds = gateway.GetProducts(releaseWindow.ExternalId)
                    .Select(x => x.ExternalId)
                    .ToArray();
            }
            var retrieveMode = SourceControlService.GetSourceControlRetrieveMode(packageIds)
                .GroupBy(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Select(t => t.Key));
            var repositories = retrieveMode.ContainsKey(SourceControlRetrieveMode.RepositoryIdentifier)
                ? GetRepositories(releaseWindow.ExternalId)
                : null;

            Log.DebugFormat("Starting getting data from plugin");
            var sourceControlChanges = GetChangesFromSourceControl(releaseWindow.ExternalId, retrieveMode, repositories);
            Log.DebugFormat("Finish getting data from plugin");

            Log.DebugFormat("Filtering out existing changes");
            var response = new GetReleaseChangesResponse
            {
                Changes = request.IsBackground
                    ? sourceControlChanges
                    : FilterExistingChanges(sourceControlChanges, packageIds),
                Repositories = repositories
            };

            Log.DebugFormat("Finish getting release changes");
            return response;
        }

        private IEnumerable<ReleaseRepository> GetRepositories(Guid releaseWindowId)
        {
            using (var gateway = ReleaseRepositoryGatewayFactory())
            {
                return gateway.GetReleaseRepositories(releaseWindowId);
            }
        }

        private IEnumerable<SourceControlChange> GetChangesFromDatabase(Guid releaseWindowId)
        {
            using (var gateway = SourceControlChangeGatewayFactory())
            {
                return gateway.GetChanges(releaseWindowId).ToList();
            }
        }

        private IEnumerable<SourceControlChange> GetChangesFromSourceControl(Guid releaseWindowId,
            IDictionary<SourceControlRetrieveMode, IEnumerable<Guid>> retrieveMode,
            IEnumerable<ReleaseRepository> repositories = null)
        {
            var result = new List<SourceControlChange>();
            foreach (var mode in retrieveMode)
            {
                if (mode.Key == SourceControlRetrieveMode.DeploymentJobs)
                {
                    IEnumerable<Guid> jobIds;
                    using (var gateway = ReleaseJobGatewayFactory())
                    {
                        jobIds = gateway.GetReleaseJobs(releaseWindowId)
                            .Select(x => x.JobId)
                            .ToArray();
                    }
                    result.AddRange(SourceControlService.GetChangesByReleaseJobs(mode.Value, jobIds).ToArray());
                }
                else if (mode.Key == SourceControlRetrieveMode.RepositoryIdentifier
                    && repositories != null && repositories.Any(x => x.IsIncluded))
                {
                    result.AddRange(SourceControlService.GetChangesByRepository(mode.Value,
                        repositories.Where(x => x.IsIncluded).ToArray()).ToArray());
                }
                else if (mode.Key == SourceControlRetrieveMode.None)
                {
                    result.AddRange(SourceControlService.GetChanges(mode.Value).ToArray());
                }
            }
            return result;
        }


        private IEnumerable<SourceControlChange> FilterExistingChanges(IEnumerable<SourceControlChange> changes, IEnumerable<Guid> productIds)
        {
            using (var gateway = SourceControlChangeGatewayFactory())
            {
                var ids = gateway.FilterExistingChangesByProduct(changes.Select(c => c.Identifier).ToArray(), productIds);
                return changes.Where(c => ids.All(h => h != c.Identifier)).ToArray();
            }
        }
    }
}
