using ReMi.BusinessEntities.Plugins;
using ReMi.Common.Utils;
using System;
using System.Collections.Generic;

namespace ReMi.Queries.Plugins
{
    public class GetPluginsResponse
    {
        public IEnumerable<PluginView2> Plugins { get; set; }

        public override string ToString()
        {
            return String.Format("[Plugins={0}]",
                Plugins.FormatElements());
        }
    }
}
