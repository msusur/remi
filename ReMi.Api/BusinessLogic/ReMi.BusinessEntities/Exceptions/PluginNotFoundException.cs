using System;

namespace ReMi.BusinessEntities.Exceptions
{
    public class PluginNotFoundException : ApplicationException
    {
        public PluginNotFoundException(Guid pluginId)
            : base(FormatMessage(pluginId))
        {
        }

        public PluginNotFoundException(Guid pluginId, Exception innerException)
            : base(FormatMessage(pluginId), innerException)
        {
        }

        private static string FormatMessage(Guid pluginId)
        {
            return String.Format("Plugin cound not be found. PluginId: {0}", pluginId);
        }
    }
}
