using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dependencies;
using AutoMapper;
using ReMi.Common.Utils;
using ReMi.Plugin.Common.PluginsConfiguration;

namespace ReMi.Api
{
    public static class AutoMapperConfiguration
    {
        public static void Configure(HttpConfiguration configuration)
        {
            //NOTE: Can only call Mapper.Initialize once... 
            //Using reflection to find all the Profile implementations on all the assemblies of the AppDomain

            var profiles = GetAllProfileTypes()
                .Select(configuration.DependencyResolver.GetService)
                .Cast<Profile>()
                .ToList();

            AddPluginProfiles(profiles, configuration.DependencyResolver);

            // Initialize AutoMapper with each instance of the profiles found.
            Mapper.Initialize(m => profiles.ForEach(m.AddProfile));
        }

        private static void AddPluginProfiles(List<Profile> profiles, IDependencyScope dependencyResolver)
        {
            var pluginConfiguration = (IPluginConfiguration)dependencyResolver.GetService(typeof(IPluginConfiguration));
            profiles.AddRange(pluginConfiguration.GetAutoMapperProfiles());
        }

        private static IEnumerable<Type> GetAllProfileTypes()
        {
            var assemblies = RemiAssembliesHelper.GetReMiAssemblies();

            var profileType = typeof(Profile);

            var allProfileTypes = new List<Type>();
            foreach (var assembly in assemblies)
            {
                allProfileTypes.AddRange(
                    assembly.GetTypes().Where(
                        t =>
                            profileType.IsAssignableFrom(t) &&
                            t.GetConstructor(Type.EmptyTypes) != null));
            }

            return allProfileTypes;
        }
    }
}
