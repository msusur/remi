using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.DataAccess.BusinessEntityGateways.Plugins;
using ReMi.Plugin.Composites.Entities;

namespace ReMi.Plugin.Composites
{
    public abstract class BaseComposit<T> where T : class, IPluginService 
    {
        public Func<IPluginGateway> PluginGatewayFactory { get; set; }
        public IContainer Container { get; set; }

        protected T GetPluginService(PluginType pluginType)
        {
            using (var gateway = PluginGatewayFactory())
            {
                var packageConfiguration = gateway.GetGlobalPluginConfiguration()
                    .FirstOrDefault(x => x.PluginType.HasFlag(pluginType));
                return packageConfiguration == null || !packageConfiguration.PluginId.HasValue
                    ? null
                    : Container.ResolveNamed<T>(packageConfiguration.PluginId.ToString().ToUpper());
            }
        }

        protected T GetPluginService(Guid packageId, PluginType pluginType)
        {
            using (var gateway = PluginGatewayFactory())
            {
                var packageConfiguration = gateway.GetPackagePluginConfiguration(packageId, pluginType);
                return !packageConfiguration.PluginId.HasValue
                    ? null
                    : Container.ResolveNamed<T>(packageConfiguration.PluginId.ToString().ToUpper());
            }
        }

        protected IEnumerable<PluginServiceWithCofiguration<T>> GetPackageServicesWithConfiguration(IEnumerable<Guid> packageIds, PluginType pluginType)
        {
            using (var gateway = PluginGatewayFactory())
            {
                return packageIds
                    .Select(x => gateway.GetPackagePluginConfiguration(x, pluginType))
                    .Where(x => x.PluginId.HasValue)
                    .GroupBy(x => x.PluginId)
                    .Select(x => new PluginServiceWithCofiguration<T>{
                        Service = Container.ResolveNamed<T>(x.Key.ToString().ToUpper()),
                        Configurations = x.ToArray()
                    })
                    .ToArray();
            }
        }
    }
}
