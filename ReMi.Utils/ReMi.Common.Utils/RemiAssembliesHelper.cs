using System;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using Common.Logging;

namespace ReMi.Common.Utils
{
    public static class RemiAssembliesHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RemiAssembliesHelper));

        public static Assembly[] GetReMiAssemblies()
        {
            return GetAssemblies("remi", "remi.plugin");
        }

        public static Assembly[] GetReMiPluginAssemblies()
        {
            return GetAssemblies("remi.plugin", "remi.plugin.common");
        }

        private static Assembly[] GetAssemblies(string filterIn, string filterOut)
        {
            var assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>()
                .Where(o =>
                {
                    var name = o.GetName().Name;
                    return (string.IsNullOrEmpty(filterIn) || name.StartsWith(filterIn, StringComparison.InvariantCultureIgnoreCase))
                        && (string.IsNullOrEmpty(filterOut) || !name.StartsWith(filterOut, StringComparison.InvariantCultureIgnoreCase));
                })
                .ToArray();

            Logger.InfoFormat("Loading assemblies:\n{0}",
                string.Join("\n", assemblies.Select(assembly => assembly.FullName).ToArray()));

            return assemblies;
        }
    }
}
