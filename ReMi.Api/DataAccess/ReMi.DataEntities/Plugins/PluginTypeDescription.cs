using System;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Utils.Enums;

namespace ReMi.DataEntities.Plugins
{
    [Table("PluginTypes", Schema = Constants.PluginsSchemaName)]
    public class PluginTypeDescription : EnumDescription
    {
        public virtual bool? IsGlobal
        {
            get { return string.Equals(Annotation, "global"); }
            // ReSharper disable once ValueParameterNotUsed
            set { Annotation = value.HasValue && value.Value ? "global" : null; }
        }

        public override string ToString()
        {
            return String.Format("[Id={0}, Name={1}, Description={2}, IsGlobal={3}]",
                Id, Name, Description, IsGlobal);
        }
    }
}
