using Autofac;
using Autofac.Core;
using AutoMapper;
using ReMi.Common.Utils.Repository;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Contracts.Plugins.Services.HelpDesk;
using ReMi.Plugin.ZenDesk.AutoMapper;
using ReMi.Plugin.ZenDesk.DataAccess;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using ReMi.Plugin.ZenDesk.BusinessLogic;
using ReMi.Plugin.ZenDesk.Migrations;

namespace ReMi.Plugin.ZenDesk
{
    public class PluginInitializer : IPluginInitializer
    {
        private const string ZenDeskKey = "ReMi.Plugin.ZenDesk";
        private const PluginType ZenDeskPluginType = PluginType.HelpDesk;

        public const string ZenDeskId = "552D791C-2033-4F9F-BEC3-8ADE3E819D5A";

        public string Key { get { return ZenDeskKey; } }
        public Guid Id { get; private set; }
        public PluginType PluginType { get { return ZenDeskPluginType; } }
        public bool IsGlobalConfigurationReadonly { get { return false; } }
        public bool IsPackageConfigurationReadonly { get { return false; } }

        public PluginInitializer()
        {
            Id = new Guid(ZenDeskId);
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
                .Named<IPluginInitializer>(ZenDeskId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginConfiguration>()
                .As<IPluginConfiguration>()
                .As<IPluginConfiguration<PluginConfigurationEntity>>()
                .Named<IPluginConfiguration>(ZenDeskId)
                .Named<IPluginConfiguration<PluginConfigurationEntity>>(ZenDeskId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<ZenDeskService>()
                .As<IHelpDeskService>()
                .Named<IHelpDeskService>(ZenDeskId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();
            builder.RegisterType<ZenDeskRequest>()
                .As<IZenDeskRequest>()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();


            builder.RegisterType<EntityFrameworkUnitOfWork<ZenDeskContext>>()
                .As<IUnitOfWork>()
                .Named<IUnitOfWork>(ZenDeskId)
                .InstancePerDependency()
                .PreserveExistingDefaults()
                .PropertiesAutowired();

            builder.RegisterGeneric(typeof(EntityFrameworkRepository<>))
                .As(typeof(IRepository<>))
                .Named(ZenDeskId, typeof(IRepository<>))
                .InstancePerDependency()
                .PropertiesAutowired()
                .WithParameter(new ResolvedParameter(
                    (p, c) => p.ParameterType.Name == typeof(IUnitOfWork).Name,
                    (p, c) => c.ResolveNamed<IUnitOfWork>(ZenDeskId)));

            var repositoryPropertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Name == typeof(IRepository<>).Name,
                (p, c) => c.ResolveNamed(ZenDeskId, p.ParameterType)), 20);
            builder.RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => !string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("ReMi.Plugin.ZenDesk.DataAccess.Gateways"))
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
                new ZenDeskModelToContractContract(),
                new ContractModelToZenDeskModel(),
                new ZenDeskBusinessEnitiesToDataEntities(),
                new ZenDeskDataEntitiesToBusinessEnities()
            };
        }
    }
}
