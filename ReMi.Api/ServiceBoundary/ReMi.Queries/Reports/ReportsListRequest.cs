using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Reports
{
    [Query("Returns the list of available reports", QueryGroup.Metrics)]
    public class ReportsListRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return String.Format("Context={0}", Context);
        }
    }
}
