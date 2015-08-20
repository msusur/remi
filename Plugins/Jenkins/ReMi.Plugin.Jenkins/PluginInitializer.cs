using Autofac;
using Autofac.Core;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Contracts.Plugins.Services.DeploymentTool;
using ReMi.Plugin.Jenkins.AutoMapper;
using ReMi.Plugin.Jenkins.BusinessLogic;
using ReMi.Plugin.Jenkins.DataAccess;
using ReMi.Plugin.Jenkins.JenkinsApi;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using ReMi.Plugin.Jenkins.Migrations;

namespace ReMi.Plugin.Jenkins
{
    public class PluginInitializer : IPluginInitializer
    {
        private const string JenkinsKey = "ReMi.Plugin.Jenkins";
        private const PluginType JenkinsPluginType = PluginType.DeploymentTool;

        private const string JenkinsId = "DF64367E-C805-423A-AF76-E01D3D569B99";

        public string Key { get { return JenkinsKey; } }
        public Guid Id { get; private set; }
        public PluginType PluginType { get { return JenkinsPluginType; } }
        public bool IsGlobalConfigurationReadonly { get { return false; } }
        public bool IsPackageConfigurationReadonly { get { return false; } }

        public PluginInitializer()
        {
            Id = new Guid(JenkinsId);
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
                .Named<IPluginInitializer>(JenkinsId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginConfiguration>()
                .As<IPluginConfiguration>()
                .As<IPluginConfiguration<PluginConfigurationEntity>>()
                .Named<IPluginConfiguration>(JenkinsId)
                .Named<IPluginConfiguration<PluginConfigurationEntity>>(JenkinsId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginPackageConfiguration>()
                .As<IPluginPackageConfiguration<PluginPackageConfigurationEntity>>()
                .As<IPluginPackageConfiguration>()
                .Named<IPluginPackageConfiguration>(JenkinsId)
                .Named<IPluginPackageConfiguration<PluginPackageConfigurationEntity>>(JenkinsId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            var propertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Namespace != null && p.ParameterType.Namespace.StartsWith("ReMi.Plugin.Jenkins"),
                (p, c) => c.ResolveNamed(JenkinsId, p.ParameterType)), 20);
            builder.RegisterType<JenkinsRequest>()
                .As<IJenkinsRequest>()
                .Named<IJenkinsRequest>(JenkinsId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope()
                .WithProperties(propertyResolver);

            propertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Namespace != null && p.ParameterType.Namespace.StartsWith("ReMi.Plugin.Jenkins"),
                (p, c) => c.ResolveNamed(JenkinsId, p.ParameterType)), 20);
            builder.RegisterType<JenkinsDeploymentTool>()
                .As<IDeploymentTool>()
                .Named<IDeploymentTool>(JenkinsId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope()
                .WithProperties(propertyResolver);

            builder.RegisterType<EntityFrameworkUnitOfWork<JenkinsContext>>()
                .As<IUnitOfWork>()
                .Named<IUnitOfWork>(JenkinsId)
                .InstancePerDependency()
                .PreserveExistingDefaults()
                .PropertiesAutowired();

            builder.RegisterGeneric(typeof(EntityFrameworkRepository<>))
                .As(typeof(IRepository<>))
                .Named(JenkinsId, typeof(IRepository<>))
                .InstancePerDependency()
                .PropertiesAutowired()
                .WithParameter(new ResolvedParameter(
                    (p, c) => p.ParameterType.Name == typeof(IUnitOfWork).Name,
                    (p, c) => c.ResolveNamed<IUnitOfWork>(JenkinsId)));

            var repositoryPropertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Name == typeof(IRepository<>).Name,
                (p, c) => c.ResolveNamed(JenkinsId, p.ParameterType)), 20);
            builder.RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => !string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("ReMi.Plugin.Jenkins.DataAccess.Gateways"))
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
                new JenkinsDataEntitiesToBusinessEnities(),
                new JenkinsBusinessEntitiesToDataEnities(),
                new JenkinsBusinessEntitiesToContractEntity() 
            };
        }
    }
}
