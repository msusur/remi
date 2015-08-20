using Autofac;
using Autofac.Core;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Contracts.Plugins.Services.ReleaseContent;
using ReMi.Plugin.Jira.AutoMapper;
using ReMi.Plugin.Jira.DataAccess;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using ReMi.Plugin.Jira.Migrations;

namespace ReMi.Plugin.Jira
{
    public class PluginInitializer : IPluginInitializer
    {
        private const string JiraKey = "ReMi.Plugin.Jira";
        private const PluginType JiraPluginType = PluginType.ReleaseContent;

        public const string JiraId = "529BCB2B-8ADD-401D-B4B5-1D539FD14A8E";

        public string Key { get { return JiraKey; } }
        public Guid Id { get; private set; }
        public PluginType PluginType { get { return JiraPluginType; } }
        public bool IsGlobalConfigurationReadonly { get { return false; } }
        public bool IsPackageConfigurationReadonly { get { return false; } }

        public PluginInitializer()
        {
            Id = new Guid(JiraId);
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
                .Named<IPluginInitializer>(JiraId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginConfiguration>()
                .As<IPluginConfiguration>()
                .As<IPluginConfiguration<PluginConfigurationEntity>>()
                .Named<IPluginConfiguration>(JiraId)
                .Named<IPluginConfiguration<PluginConfigurationEntity>>(JiraId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginPackageConfiguration>()
                .As<IPluginPackageConfiguration<PluginPackageConfigurationEntity>>()
                .As<IPluginPackageConfiguration>()
                .Named<IPluginPackageConfiguration>(JiraId)
                .Named<IPluginPackageConfiguration<PluginPackageConfigurationEntity>>(JiraId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();
            builder.RegisterType<JiraRequest>()
                .As<IReleaseContent>()
                .Named<IReleaseContent>(JiraId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();


            builder.RegisterType<EntityFrameworkUnitOfWork<JiraContext>>()
                .As<IUnitOfWork>()
                .Named<IUnitOfWork>(JiraId)
                .InstancePerDependency()
                .PreserveExistingDefaults()
                .PropertiesAutowired();

            builder.RegisterGeneric(typeof(EntityFrameworkRepository<>))
                .As(typeof(IRepository<>))
                .Named(JiraId, typeof(IRepository<>))
                .InstancePerDependency()
                .PropertiesAutowired()
                .WithParameter(new ResolvedParameter(
                    (p, c) => p.ParameterType.Name == typeof(IUnitOfWork).Name,
                    (p, c) => c.ResolveNamed<IUnitOfWork>(JiraId)));

            var repositoryPropertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Name == typeof(IRepository<>).Name,
                (p, c) => c.ResolveNamed(JiraId, p.ParameterType)), 20);
            builder.RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => !string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("ReMi.Plugin.Jira.DataAccess.Gateways"))
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
                new JiraModelToContractContract(),
                new ContractModelToJiraModel(),
                new JiraBusinessEnitiesToDataEntities(),
                new JiraDataEntitiesToBusinessEnities()
            };
        }
    }
}
