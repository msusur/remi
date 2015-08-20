using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Plugins
{
    [Query("Get packages plugin configuration", QueryGroup.Plugins)]
    public class GetPackagePluginConfigurationRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public override string ToString()
        {
            return String.Format("[Context={0}]", Context);
        }
    }
}
