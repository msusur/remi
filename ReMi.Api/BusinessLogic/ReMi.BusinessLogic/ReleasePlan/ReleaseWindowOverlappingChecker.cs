using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.Exceptions;
using ReMi.DataAccess.Exceptions.Configuration;

namespace ReMi.BusinessLogic.ReleasePlan
{
    public class ReleaseWindowOverlappingChecker : IReleaseWindowOverlappingChecker
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public Func<IReleaseWindowGateway> ReleaseWindowGatewayFactory { get; set; }
        public Func<IProductGateway> ProductGatewayFactory { get; set; }
        public IReleaseWindowHelper ReleaseWindowHelper { get; set; }

        public ReleaseWindow FindOverlappedWindow(ReleaseWindow releaseWindow)
        {
            if (ReleaseWindowHelper.IsMaintenance(releaseWindow))
                return null;

            IList<Product> products;

            using (var gateway = ProductGatewayFactory())
            {
                products = gateway.GetProducts(releaseWindow.Products).ToList();

                if (products.IsNullOrEmpty())
                    throw new ProductShouldBeAssignedException(releaseWindow.ExternalId);

                if (products.Count != releaseWindow.Products.Count())
                    throw new ProductNotFoundException(releaseWindow.Products.Where(x => products.All(p => p.Description != x)));

                if (products.All(x => x.ReleaseTrack == ReleaseTrack.Automated))
                    return null;
            }

            using (var gateway = ReleaseWindowGatewayFactory())
            {
                foreach (var product in products.Where(x => x.ReleaseTrack != ReleaseTrack.Automated))
                {
                    var conflictingRelease = gateway.FindFirstOverlappedRelease(
                        product.Description,
                        releaseWindow.StartTime,
                        releaseWindow.EndTime,
                        releaseWindow.ExternalId);

                    if (conflictingRelease == null)
                        Logger.DebugFormat("There are no booked releases for product={2} and time period={0} - {1}",
                            releaseWindow.StartTime,
                            releaseWindow.EndTime,
                            product.Description);
                    else
                    {
                        Logger.DebugFormat(
                            "Conflicting release with Id={3} found for product={2} and time period={0} - {1}",
                            releaseWindow.StartTime,
                            releaseWindow.EndTime,
                            product.Description,
                            conflictingRelease.ExternalId);

                        return conflictingRelease;
                    }
                }

                return null;
            }
        }
    }
}
