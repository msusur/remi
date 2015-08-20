using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleaseExecution
{
    [Query("Get history of measurements for deployment jobs", QueryGroup.Metrics)]
    public class GetDeploymentJobMeasurementsByProductRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public string Product { get; set; }

        public override string ToString()
        {
            return string.Format("[Product={0}, Context={1}]", 
                Product, Context);
        }
    }
}
