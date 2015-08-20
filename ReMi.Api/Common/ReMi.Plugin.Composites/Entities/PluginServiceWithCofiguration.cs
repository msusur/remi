using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Plugins;
using ReMi.Contracts.Plugins.Services;

namespace ReMi.Plugin.Composites.Entities
{
    public class PluginServiceWithCofiguration<T>
        where T : class, IPluginService
    {
        public T Service { get; set; }
        public IEnumerable<PackagePluginConfiguration> Configurations { get; set; }
        public IEnumerable<Guid> PackageIds { get; set; } 
    }
}
