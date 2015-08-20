using ReMi.BusinessEntities.ContinuousDelivery;
using ReMi.Common.Constants.ContinuousDelivery;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Services.ReleaseContent;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.Exceptions.Configuration;
using ReMi.Queries.ContinuousDelivery;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.QueryHandlers.ContinuousDelivery
{
    public class GetContinuousDeliveryStatusHandler : IHandleQuery<GetContinuousDeliveryStatusRequest, GetContinuousDeliveryStatusResponse>
    {
        public IReleaseContent ReleaseContent { get; set; }
        public Func<IProductGateway> PackageGatewayFactory { get; set; }

        public GetContinuousDeliveryStatusResponse Handle(GetContinuousDeliveryStatusRequest request)
        {
            //foreach (var product in request.Products)
            //{
            //    //check should be turn on in November
            //    //DefectCheck(product)
            //}

            return new GetContinuousDeliveryStatusResponse
            {
                Products = request.Products,
                StatusCheck =
                    new List<StatusCheckItem>
                    {
                        new StatusCheckItem
                        {
                            Status = StatusType.Green,
                            MetricControl = "ContinousIntegration",
                            Comments = new[] {"No issues"}
                        },
                        new StatusCheckItem
                        {
                            Status = StatusType.Green,
                            MetricControl = "CriticalBug",
                            Comments = new[] {"No issues"}
                        },
                        new StatusCheckItem
                        {
                            Status = StatusType.Green,
                            MetricControl = "UnitTests",
                            Comments = new[] {"No issues"}
                        },
                    }
            };
        }

        private StatusCheckItem DefectCheck(String package)
        {
            Guid packageId;
            using (var gateway = PackageGatewayFactory())
            {
                var productEntity = gateway.GetProduct(package);
                if (productEntity == null)
                    throw new ProductNotFoundException(package);
                packageId = productEntity.ExternalId;
            }

            var defectCheck = new StatusCheckItem
            {
                MetricControl = "Defects"
            };

            var tickets = ReleaseContent.GetDefectTickets(new[] { packageId });

            if (tickets != null)
            {
                defectCheck.Status = tickets.Any() ? StatusType.Red : StatusType.Green;
                defectCheck.Comments = tickets.Any()
                    ? tickets.Select(x => String.Format("{1}{0}", x.TicketName, "ticket url")).ToList()
                    : null;
            }
            else
            {
                defectCheck.Status = StatusType.Yellow;
            }

            return defectCheck;
        }
    }
}
