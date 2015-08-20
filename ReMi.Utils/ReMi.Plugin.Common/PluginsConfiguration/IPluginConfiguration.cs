using System;
using System.Collections.Generic;
using Autofac;
using AutoMapper;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;

namespace ReMi.Plugin.Common.PluginsConfiguration
{
    public interface IPluginConfiguration
    {
        IDictionary<PluginType, IPluginInitializer[]> PluginInitializersGrouped { get; }
        IEnumerable<IPluginInitializer> PluginInitializers { get; }

        IPluginInitializer GetPluginInitializer(Guid pluginId);
        void InitializeDatabase();
        void InitializeDependencies(ContainerBuilder builder);
        IEnumerable<Profile> GetAutoMapperProfiles();
    }
}
