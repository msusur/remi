using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.DataEntities.Plugins
{
    [Table("Plugins", Schema = Constants.PluginsSchemaName)]
    public class Plugin
    {
        [Key]
        public int PluginId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        [StringLength(256), Index(IsUnique = true)]
        public string Key { get; set; }

        public PluginType PluginType { get; set; }
    }
}
