using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Metrics
{
    [Query("Get measurements for releases", QueryGroup.Metrics)]
    public class GetMeasurementsRequest : IQuery
    {
        public QueryContext Context { get; set; }
        public String Product { get; set; }

        public override string ToString()
        {
            return String.Format("[Product={0}, Context={1}]", Product, Context);
        }
    }
}
