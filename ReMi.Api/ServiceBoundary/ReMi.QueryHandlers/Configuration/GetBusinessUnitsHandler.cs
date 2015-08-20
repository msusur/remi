using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ReMi.BusinessEntities.Products;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Queries.Configuration;

namespace ReMi.QueryHandlers.Configuration
{
    public class GetBusinessUnitsHandler : IHandleQuery<GetBusinessUnitsRequest, GetBusinessUnitsResponse>
    {
        public Func<IBusinessUnitsGateway> BusinessUnitGatewayFactory { get; set; }
        public IMappingEngine MappingEngine { get; set; }

        public GetBusinessUnitsResponse Handle(GetBusinessUnitsRequest request)
        {
            if (request.Context == null
                || request.Context.UserId == Guid.Empty
                || request.Context.UserRole == "NotAuthenticated")
            {
                return new GetBusinessUnitsResponse
                {
                    BusinessUnits = Enumerable.Empty<BusinessUnitView>()
                };
            }
            using (var gateway = BusinessUnitGatewayFactory())
            {
                var defaultPackage = gateway.GetDefaultPackage(request.Context.UserId);
                var packages = gateway.GetPackages(request.Context.UserId, request.IncludeAll);
                var emptyBusinessUnits = gateway.GetEmptyBusinessUnits();
                var emptyBusinessUnitsView = emptyBusinessUnits == null
                    ? Enumerable.Empty<BusinessUnitView>()
                    : MappingEngine.Map<IEnumerable<BusinessUnit>, IEnumerable<BusinessUnitView>>(emptyBusinessUnits);

                var businessUnits = packages
                    .OrderBy(x => x.Description)
                    .GroupBy(x => x.BusinessUnit)
                    .Select(x =>
                    {
                        x.Key.Packages = x;
                        return MappingEngine.Map<BusinessUnit, BusinessUnitView>(x.Key);
                    })
                    .Union(emptyBusinessUnitsView)
                    .OrderBy(x => x.Description)
                    .ToArray();

                if (defaultPackage == null)
                    return new GetBusinessUnitsResponse { BusinessUnits = businessUnits };

                businessUnits.First(x => x.Packages.Any(p => p.ExternalId == defaultPackage.ExternalId))
                    .Packages.First(p => p.ExternalId == defaultPackage.ExternalId).IsDefault = true;

                return new GetBusinessUnitsResponse
                {
                    BusinessUnits = businessUnits
                };
            }
        }
    }
}
