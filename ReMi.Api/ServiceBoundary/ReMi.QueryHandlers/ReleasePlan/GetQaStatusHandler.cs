using System;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Services.QaStats;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryHandlers.ReleasePlan
{
    public class GetQaStatusHandler:IHandleQuery<GetQaStatusRequest,GetQaStatusResponse>
    {
        public ICheckQaStatus CheckQaStatus { get; set; }
        public Func<IProductGateway> ProductGatewayFactory { get; set; }

        public GetQaStatusResponse Handle(GetQaStatusRequest request)
        {
            using(var gateway = ProductGatewayFactory())
            {
                var package = gateway.GetProduct(request.PackageName);
                var qaStatusCheck = CheckQaStatus.GetQaStatusCheckItems(new [] { package.ExternalId });
                return new GetQaStatusResponse
                {
                    Content = qaStatusCheck
                };
            }
        }
    }
}
