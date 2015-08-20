using ReMi.Common.Utils;
using System.Collections.Generic;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ContinuousDelivery
{
    [Query("Get status before release", QueryGroup.ContinuousDelivery)]
    public class GetContinuousDeliveryStatusRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public IEnumerable<string> Products { get; set; }

        public override string ToString()
        {
            return string.Format("[Products={0}, Context={1}]",
                Products.FormatElements(), Context);
        }
    }
}
