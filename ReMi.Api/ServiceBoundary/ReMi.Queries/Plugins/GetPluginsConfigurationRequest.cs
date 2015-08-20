using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Plugins
{
    [Query("Get plugins configuration", QueryGroup.Plugins)]
    public class GetPluginsConfigurationRequest : IQuery
    {
        public QueryContext Context { get; set; }
        public Guid PluginId { get; set; }

        public override string ToString()
        {
            return String.Format("[Context={0}]", Context);
        }
    }
}
