using System;
using Autofac;
using AutoMapper;
using ReMi.Contracts.Plugins.Services;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using Autofac.Core;
using ReMi.Common.Utils.Repository;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services.QaStats;
using ReMi.Plugin.QaStats.AutoMapper;
using ReMi.Plugin.QaStats.DataAccess;
using ReMi.Plugin.QaStats.Migrations;

namespace ReMi.Plugin.QaStats
{
    public class PluginInitializer : IPluginInitializer
    {
        private const string QaStatsKey = "ReMi.Plugin.QaStats";
        private const PluginType QaStatsPluginType = PluginType.QaStats;
        public const string QaStatsId = "ED228003-1120-41B0-8E58-D5AB83BD1F55";

        public string Key { get { return QaStatsKey; } }
        public Guid Id { get; private set; }
        public PluginType PluginType { get { return QaStatsPluginType; } }
        public bool IsGlobalConfigurationReadonly { get { return false; } }
        public bool IsPackageConfigurationReadonly { get { return false; } }

        public PluginInitializer()
        {
            Id = new Guid(QaStatsId);
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
                .Named<IPluginInitializer>(QaStatsId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginConfiguration>()
                .As<IPluginConfiguration>()
                .As<IPluginConfiguration<PluginConfigurationEntity>>()
                .Named<IPluginConfiguration>(QaStatsId)
                .Named<IPluginConfiguration<PluginConfigurationEntity>>(QaStatsId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginPackageConfiguration>()
                .As<IPluginPackageConfiguration<PluginPackageConfigurationEntity>>()
                .As<IPluginPackageConfiguration>()
                .Named<IPluginPackageConfiguration>(QaStatsId)
                .Named<IPluginPackageConfiguration<PluginPackageConfigurationEntity>>(QaStatsId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            var propertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Namespace != null && p.ParameterType.Namespace.StartsWith("ReMi.Plugin.QaStats"),
                (p, c) => c.ResolveNamed(QaStatsId, p.ParameterType)), 2);
            builder.RegisterType<QaStatsRequest>()
                .As<ICheckQaStatus>()
                .Named<ICheckQaStatus>(QaStatsId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope()
                .WithProperties(propertyResolver);

            builder.RegisterType<EntityFrameworkUnitOfWork<QaStatsContext>>()
                .As<IUnitOfWork>()
                .Named<IUnitOfWork>(QaStatsId)
                .InstancePerDependency()
                .PreserveExistingDefaults()
                .PropertiesAutowired();

            builder.RegisterGeneric(typeof(EntityFrameworkRepository<>))
                .As(typeof(IRepository<>))
                .Named(QaStatsId, typeof(IRepository<>))
                .InstancePerDependency()
                .PropertiesAutowired()
                .WithParameter(new ResolvedParameter(
                    (p, c) => p.ParameterType.Name == typeof(IUnitOfWork).Name,
                    (p, c) => c.ResolveNamed<IUnitOfWork>(QaStatsId)));

            var repositoryPropertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Name == typeof(IRepository<>).Name,
                (p, c) => c.ResolveNamed(QaStatsId, p.ParameterType)), 5);
            builder.RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => !string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("ReMi.Plugin.QaStats.DataAccess.Gateways"))
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
                new QaStatsDataEntitiesToBusinessEnities(),
                new QaStatsBusinessEnitiesToDataEntities()
            };
        }
    }
}
