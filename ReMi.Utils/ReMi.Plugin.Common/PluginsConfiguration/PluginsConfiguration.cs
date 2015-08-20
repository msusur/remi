using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using AutoMapper;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;

namespace ReMi.Plugin.Common.PluginsConfiguration
{
    public class PluginConfiguration : IPluginConfiguration
    {
        private readonly IDictionary<PluginType, IPluginInitializer[]> _pluginInitializersGrouped;
        private readonly IEnumerable<IPluginInitializer> _pluginInitializers;

        public PluginConfiguration(IEnumerable<Assembly> pluginAssemblies)
        {
            _pluginInitializers = GetPluginInitializers(pluginAssemblies);
            _pluginInitializersGrouped = GetPluginInitializersGrouped(_pluginInitializers);
        }

        public PluginConfiguration()
        {
            var pluginAssemblies = RemiAssembliesHelper.GetReMiPluginAssemblies();
            _pluginInitializers = GetPluginInitializers(pluginAssemblies);
            _pluginInitializersGrouped = GetPluginInitializersGrouped(_pluginInitializers);
        }

        public IDictionary<PluginType, IPluginInitializer[]> PluginInitializersGrouped
        {
            get { return _pluginInitializersGrouped; }
        }

        public IEnumerable<IPluginInitializer> PluginInitializers
        {
            get { return _pluginInitializers; }
        }

        public IPluginInitializer GetPluginInitializer(Guid pluginId)
        {
            return PluginInitializers.FirstOrDefault(x => x.Id == pluginId);
        }

        public void InitializeDatabase()
        {
            foreach (var pluginInitializer in _pluginInitializers)
            {
                pluginInitializer.InitializeDatabase();
            }
        }

        public void InitializeDependencies(ContainerBuilder builder)
        {
            foreach (var pluginInitializer in _pluginInitializers)
            {
                pluginInitializer.InitializeDependencies(builder);
            }
        }

        public IEnumerable<Profile> GetAutoMapperProfiles()
        {
            return _pluginInitializers
                .SelectMany(x => x.GetAutoMapperProfiles());
        }

        private static IEnumerable<IPluginInitializer> GetPluginInitializers(IEnumerable<Assembly> pluginAssemblies)
        {
            return GetAllTypes<IPluginInitializer>(pluginAssemblies);
        }

        private static IDictionary<PluginType, IPluginInitializer[]> GetPluginInitializersGrouped(IEnumerable<IPluginInitializer> pluginInitializers)
        {
            var result = new Dictionary<PluginType, IList<IPluginInitializer>>();

            foreach (var pluginInitializer in pluginInitializers)
            {
                foreach (var pluginType in pluginInitializer.PluginType.ToFlagList())
                {
                    if (!result.ContainsKey(pluginType))
                    {
                        result.Add(pluginType, new List<IPluginInitializer>());
                    } 
                    result[pluginType].Add(pluginInitializer);
                }
            }

            return result.ToDictionary(k => k.Key, v => v.Value.ToArray());
        }

        private static IEnumerable<T> GetAllTypes<T>(IEnumerable<Assembly> pluginAssemblies)
            where T : class
        {
            var type = typeof (T);

            return pluginAssemblies
                .SelectMany(x => x.GetTypes()
                    .Where(t => type.IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null))
                .Select(x => (T) Activator.CreateInstance(x));
        }
    }
}
