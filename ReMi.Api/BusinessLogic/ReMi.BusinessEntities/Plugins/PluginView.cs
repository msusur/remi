using System;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.BusinessEntities.Plugins
{
    public class PluginView
    {
        public Guid PluginId { get; set; }
        public string PluginKey { get; set; }
        public PluginType PluginType { get; set; }

        public override string ToString()
        {
            return string.Format("[PluginId={0}, PluginKey={1}]",
                PluginId, PluginKey);
        }
    }
}
