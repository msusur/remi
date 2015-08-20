using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.ReleasePlan
{
    [Query("Get release QA Status", QueryGroup.ReleasePlan)]
    public class GetQaStatusRequest:IQuery
    {
        public QueryContext Context { get; set; }
        public string PackageName { get; set; }

        public override string ToString()
        {
            return String.Format("[Comntext={0}, PackageId={1}]",Context,PackageName);
        }

    }
}
