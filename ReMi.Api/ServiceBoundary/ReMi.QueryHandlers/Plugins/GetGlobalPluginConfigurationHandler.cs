using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using ReMi.Queries.Plugins;
using System;
using System.Linq;

namespace ReMi.QueryHandlers.Plugins
{
    public class GetGlobalPluginConfigurationHandler : IHandleQuery<GetGlobalPluginConfigurationRequest, GetGlobalPluginConfigurationResponse>
    {
        public Func<IPluginGateway> PluginGatewayFactory { get; set; }

        public GetGlobalPluginConfigurationResponse Handle(GetGlobalPluginConfigurationRequest request)
        {
            var response = new GetGlobalPluginConfigurationResponse();
            using (var gateway = PluginGatewayFactory())
            {
                response.GlobalPluginConfiguration = gateway.GetGlobalPluginConfiguration();
                response.GlobalPlugins = gateway.GetPlugins()
                    .Where(x => response.GlobalPluginConfiguration.Any(c => x.PluginTypes.Contains(c.PluginType)));
            }

            return response;
        }
    }
}
