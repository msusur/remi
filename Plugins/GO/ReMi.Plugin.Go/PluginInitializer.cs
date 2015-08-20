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
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using ReMi.Contracts.Plugins.Services.SourceControl;
using ReMi.Plugin.Go.AutoMapper;
using ReMi.Plugin.Go.BusinessLogic;
using ReMi.Plugin.Go.DataAccess;
using ReMi.Plugin.Go.Migrations;

namespace ReMi.Plugin.Go
{
    public class PluginInitializer : IPluginInitializer
    {
        private const string GoKey = "ReMi.Plugin.Go";
        private const PluginType GoPluginType = PluginType.SourceControl | PluginType.DeploymentTool;

        public const string GoId = "D4BC4086-FAC4-4C47-BE7D-2CA2B4DEC16C";

        public string Key { get { return GoKey; } }
        public Guid Id { get; private set; }
        public PluginType PluginType { get { return GoPluginType; } }
        public bool IsGlobalConfigurationReadonly { get { return false; } }
        public bool IsPackageConfigurationReadonly { get { return false; } }

        public PluginInitializer()
        {
            Id = new Guid(GoId);
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
                .Named<IPluginInitializer>(GoId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginConfiguration>()
                .As<IPluginConfiguration>()
                .As<IPluginConfiguration<PluginConfigurationEntity>>()
                .Named<IPluginConfiguration>(GoId)
                .Named<IPluginConfiguration<PluginConfigurationEntity>>(GoId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginPackageConfiguration>()
                .As<IPluginPackageConfiguration<PluginPackageConfigurationEntity>>()
                .As<IPluginPackageConfiguration>()
                .Named<IPluginPackageConfiguration>(GoId)
                .Named<IPluginPackageConfiguration<PluginPackageConfigurationEntity>>(GoId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            var propertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Namespace != null && p.ParameterType.Namespace.StartsWith("ReMi.Plugin.Go"),
                (p, c) => c.ResolveNamed(GoId, p.ParameterType)), 20);
            builder.RegisterType<GoRequest>()
                .As<IGoRequest>()
                .Named<IGoRequest>(GoId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope()
                .WithProperties(propertyResolver);
            propertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Namespace != null && p.ParameterType.Namespace.StartsWith("ReMi.Plugin.Go"),
                (p, c) => c.ResolveNamed(GoId, p.ParameterType)), 20);
            builder.RegisterType<GoSourceControl>()
                .As<ISourceControl>()
                .Named<ISourceControl>(GoId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope()
                .WithProperties(propertyResolver);
            propertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Namespace != null && p.ParameterType.Namespace.StartsWith("ReMi.Plugin.Go"),
                (p, c) => c.ResolveNamed(GoId, p.ParameterType)), 20);
            builder.RegisterType<GoDeploymentTool>()
                .As<IDeploymentTool>()
                .Named<IDeploymentTool>(GoId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope()
                .WithProperties(propertyResolver);

            builder.RegisterType<EntityFrameworkUnitOfWork<GoContext>>()
                .As<IUnitOfWork>()
                .Named<IUnitOfWork>(GoId)
                .InstancePerDependency()
                .PreserveExistingDefaults()
                .PropertiesAutowired();

            builder.RegisterGeneric(typeof(EntityFrameworkRepository<>))
                .As(typeof(IRepository<>))
                .Named(GoId, typeof(IRepository<>))
                .InstancePerDependency()
                .PropertiesAutowired()
                .WithParameter(new ResolvedParameter(
                    (p, c) => p.ParameterType.Name == typeof(IUnitOfWork).Name,
                    (p, c) => c.ResolveNamed<IUnitOfWork>(GoId)));

            var repositoryPropertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Name == typeof(IRepository<>).Name,
                (p, c) => c.ResolveNamed(GoId, p.ParameterType)), 20);
            builder.RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => !string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("ReMi.Plugin.Go.DataAccess.Gateways"))
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
                new GoDataEntitiesToBusinessEnities(),
                new GoBusinessEntitiesToDataEnities(),
                new GoBusinessEntitiesToContractEntity() 
            };
        }
    }
}
