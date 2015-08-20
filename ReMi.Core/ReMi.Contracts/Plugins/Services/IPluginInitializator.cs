using System.Collections.Generic;
using Autofac;
using AutoMapper;
using System;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.Contracts.Plugins.Services
{
    public interface IPluginInitializer
    {
        string Key { get; }
        Guid Id { get; }
        PluginType PluginType { get; }
        bool IsGlobalConfigurationReadonly { get; }
        bool IsPackageConfigurationReadonly { get; }

        void InitializeDatabase();
        void InitializeDependencies(ContainerBuilder builder);
        IEnumerable<Profile> GetAutoMapperProfiles();
    }
}
