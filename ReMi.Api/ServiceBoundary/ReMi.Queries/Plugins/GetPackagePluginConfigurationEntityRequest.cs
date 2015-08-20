using System;
using ReMi.Contracts.Cqrs.Commands;
using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.Queries.Plugins
{
    [Query("Get package plugins configuration entity", QueryGroup.Plugins)]
    public class GetPackagePluginConfigurationEntityRequest : IQuery
    {
        public QueryContext Context { get; set; }

        public Guid PluginId { get; set; }
        public Guid PackageId { get; set; }

        public override string ToString()
        {
            return String.Format("[Context={0}, PluginId={1}, PackageId={2}]",
                Context, PluginId, PackageId);
        }
    }
}
