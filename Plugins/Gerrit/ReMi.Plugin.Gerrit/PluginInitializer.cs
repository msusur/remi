using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using Autofac;
using Autofac.Core;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Contracts.Plugins.Services.SourceControl;
using ReMi.Plugin.Gerrit.AutoMapper;
using ReMi.Plugin.Gerrit.DataAccess;
using ReMi.Plugin.Gerrit.GerritApi;
using ReMi.Plugin.Gerrit.Migrations;
using ReMi.Plugin.Gerrit.Service;

namespace ReMi.Plugin.Gerrit
{
    public class PluginInitializer : IPluginInitializer
    {
        private const string GerritKey = "ReMi.Plugin.Gerrit";
        private const PluginType GerritPluginType = PluginType.SourceControl;

        private const string GerritId = "2FD33C53-2B53-418D-9B38-BE6BAF1FE2E9";

        public string Key { get { return GerritKey; } }
        public Guid Id { get; private set; }
        public PluginType PluginType { get { return GerritPluginType; } }
        public bool IsGlobalConfigurationReadonly { get { return false; } }
        public bool IsPackageConfigurationReadonly { get { return false; } }

        public PluginInitializer()
        {
            Id = new Guid(GerritId);
        }

        public void InitializeDatabase()
        {
            var configuration = new Configuration();

            var migrator = new DbMigrator(configuration);
            migrator.Update();
        }

        public void InitializeDependencies(ContainerBuilder builder)
        {
            builder.RegisterType<PluginInitializer>()
                .As<IPluginInitializer>()
                .Named<IPluginInitializer>(GerritId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginConfiguration>()
                .As<IPluginConfiguration>()
                .As<IPluginConfiguration<PluginConfigurationEntity>>()
                .Named<IPluginConfiguration>(GerritId)
                .Named<IPluginConfiguration<PluginConfigurationEntity>>(GerritId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginPackageConfiguration>()
                .As<IPluginPackageConfiguration<PluginPackageConfigurationEntity>>()
                .As<IPluginPackageConfiguration>()
                .Named<IPluginPackageConfiguration>(GerritId)
                .Named<IPluginPackageConfiguration<PluginPackageConfigurationEntity>>(GerritId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            var propertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Namespace != null && p.ParameterType.Namespace.StartsWith("ReMi.Plugin.Gerrit"),
                (p, c) => c.ResolveNamed(GerritId, p.ParameterType)), 20);
            builder.RegisterType<GerritRequest>()
                .As<IGerritRequest>()
                .Named<IGerritRequest>(GerritId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope()
                .WithProperties(propertyResolver);

            propertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Namespace != null && p.ParameterType.Namespace.StartsWith("ReMi.Plugin.Gerrit"),
                (p, c) => c.ResolveNamed(GerritId, p.ParameterType)), 20);
            builder.RegisterType<GerritSourceControl>()
                .As<ISourceControl>()
                .Named<ISourceControl>(GerritId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope()
                .WithProperties(propertyResolver);

            builder.RegisterType<SshClient>()
                .As<ISshClient>()
                .Named<ISshClient>(GerritId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerDependency()
                .WithProperty(new ResolvedParameter(
                    (p, c) => p.ParameterType.Namespace != null && p.ParameterType.Namespace.StartsWith("ReMi.Plugin.Gerrit"),
                    (p, c) => c.ResolveNamed(GerritId, p.ParameterType)));

            builder.RegisterType<EntityFrameworkUnitOfWork<GerritContext>>()
                .As<IUnitOfWork>()
                .Named<IUnitOfWork>(GerritId)
                .InstancePerDependency()
                .PreserveExistingDefaults()
                .PropertiesAutowired();

            builder.RegisterGeneric(typeof(EntityFrameworkRepository<>))
                .As(typeof(IRepository<>))
                .Named(GerritId, typeof(IRepository<>))
                .InstancePerDependency()
                .PropertiesAutowired()
                .WithParameter(new ResolvedParameter(
                    (p, c) => p.ParameterType.Name == typeof(IUnitOfWork).Name,
                    (p, c) => c.ResolveNamed<IUnitOfWork>(GerritId)));

            var repositoryPropertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Name == typeof(IRepository<>).Name,
                (p, c) => c.ResolveNamed(GerritId, p.ParameterType)), 20);
            builder.RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => !string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("ReMi.Plugin.Gerrit.DataAccess.Gateways"))
                .AsImplementedInterfaces()
                .InstancePerDependency()
                .PropertiesAutowired()
                .PreserveExistingDefaults()
                .WithProperties(repositoryPropertyResolver);
        }

        public IEnumerable<Profile> GetAutoMapperProfiles()
        {
            return new Profile[]
            {
                new GerritDataEntitiesToBusinessEnities(),
                new GerritBusinessEntitiesToDataEnities(),
                new GerritBusinessEntitiesToContractEntity() 
            };
        }
    }
}
