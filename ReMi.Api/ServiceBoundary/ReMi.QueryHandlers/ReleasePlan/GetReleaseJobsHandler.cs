using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.Queries.ReleasePlan;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using EqualityComparer = ReMi.Common.Utils.EqualityComparer<ReMi.BusinessEntities.DeploymentTool.ReleaseJob>;

namespace ReMi.QueryHandlers.ReleasePlan
{
    public class GetReleaseJobsHandler : IHandleQuery<GetReleaseJobsRequest, GetReleaseJobsResponse>
    {
        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IProductGateway> PackageGatewayFactory { get; set; }
        public Func<IReleaseJobGateway> ReleaseJobGatewayFactory { get; set; }
        public IDeploymentTool DeploymentToolService { get; set; }
        public IMappingEngine Mapper { get; set; }

        public GetReleaseJobsResponse Handle(GetReleaseJobsRequest request)
        {
            ReleaseWindow releaseWindow;
            using (var gateway = ReleaseWindowGatewayFactory())
            {
                releaseWindow = gateway.GetByExternalId(request.ReleaseWindowId, true);
            }
            IEnumerable<Guid> packageIds;
            using (var gateway = PackageGatewayFactory())
            {
                packageIds = gateway.GetProducts(request.ReleaseWindowId)
                    .Select(x => x.ExternalId)
                    .ToArray();
            }
            using (var gateway = ReleaseJobGatewayFactory())
            {
                var releaseJobs = gateway.GetReleaseJobs(request.ReleaseWindowId).ToArray();
                var deploymentJobs = releaseWindow.ClosedOn.HasValue
                    ? Enumerable.Empty<ReleaseJob>()
                    : Mapper.Map<IEnumerable<Contracts.Plugins.Data.DeploymentTool.ReleaseJob>, IEnumerable<ReleaseJob>>(
                        DeploymentToolService.GetReleaseJobs(packageIds)
                            .Where(x => !x.IsDisabled)
                            .ToArray());

                deploymentJobs.Each(x => x.IsIncluded = false);

                return new GetReleaseJobsResponse
                {
                    ReleaseJobs = releaseJobs.Union(deploymentJobs, EqualityComparer.Compare((x, y) => x.JobId == y.JobId))
                        .OrderBy(x => x.Order)
                        .ToArray()
                };
            }
        }
    }
}
