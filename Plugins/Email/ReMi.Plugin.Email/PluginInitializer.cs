using Autofac;
using Autofac.Core;
using AutoMapper;
using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Services;
using ReMi.Contracts.Plugins.Services.Email;
using ReMi.Plugin.Email.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.Plugin.Email
{
    public class PluginInitializer : IPluginInitializer
    {
        private const string EmailKey = "ReMi.Plugin.Email";
        private const PluginType EmailPluginType = PluginType.Email;
        public const string EmailId = "C5014D5B-0425-4A25-B76D-9A5F5F2F8357";

        public string Key { get { return EmailKey; } }
        public Guid Id { get; private set; }
        public PluginType PluginType { get { return EmailPluginType; } }
        public bool IsGlobalConfigurationReadonly { get { return true; } }
        public bool IsPackageConfigurationReadonly { get { return true; } }

        public PluginInitializer()
        {
            Id = new Guid(EmailId);
        }

        public void InitializeDatabase()
        {

        }

        public void InitializeDependencies(ContainerBuilder builder)
        {

            builder.RegisterType<PluginInitializer>()
                .As<IPluginInitializer>()
                .Named<IPluginInitializer>(EmailId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();

            builder.RegisterType<PluginConfiguration>()
                .As<IPluginConfiguration>()
                .As<IPluginConfiguration<PluginConfigurationEntity>>()
                .Named<IPluginConfiguration>(EmailId)
                .Named<IPluginConfiguration<PluginConfigurationEntity>>(EmailId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope();


            var propertyResolver = Enumerable.Repeat(new ResolvedParameter(
                (p, c) => p.ParameterType.Namespace != null && p.ParameterType.Namespace.StartsWith("ReMi.Plugin.Email"),
                (p, c) => c.ResolveNamed(EmailId, p.ParameterType)), 2);
            builder.RegisterType<EmailService>()
                .As<IEmailService>()
                .Named<IEmailService>(EmailId)
                .PreserveExistingDefaults()
                .PropertiesAutowired()
                .InstancePerLifetimeScope()
                .WithProperties(propertyResolver);
        }

        public IEnumerable<Profile> GetAutoMapperProfiles()
        {
            return new Profile[]
            {
                new EmailContractEntitiesToBusinessEnities(),
                new EmailBusinessEnitiesToContractEntities()
            };
        }
    }
}
