using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using ReMi.Queries.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.BusinessEntities.Plugins;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.QueryHandlers.Plugins
{
    public class GetPackagePluginConfigurationHandler : IHandleQuery<GetPackagePluginConfigurationRequest, GetPackagePluginConfigurationResponse>
    {
        public Func<IPluginGateway> PluginGatewayFactory { get; set; }

        public GetPackagePluginConfigurationResponse Handle(GetPackagePluginConfigurationRequest request)
        {
            var response = new GetPackagePluginConfigurationResponse();
            using (var gateway = PluginGatewayFactory())
            {
                var packagePluginConfiguration = gateway.GetPackagePluginConfiguration();
                response.PackagePluginConfiguration = packagePluginConfiguration
                    .GroupBy(x => x.PackageId)
                    .ToDictionary(x => x.Key, x =>
                        (IDictionary<PluginType, PackagePluginConfiguration>)x.GroupBy(p => p.PluginType)
                            .ToDictionary(p => p.Key, p => p.First()));
                response.PackagePlugins = gateway.GetPlugins()
                    .Where(x => packagePluginConfiguration.Any(c => x.PluginTypes.Contains(c.PluginType)));
            }


            return response;
        }
    }
}
