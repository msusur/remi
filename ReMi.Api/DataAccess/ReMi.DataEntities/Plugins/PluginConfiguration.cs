using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Contracts.Plugins.Data;

namespace ReMi.DataEntities.Plugins
{
    [Table("PluginConfiguration", Schema = Constants.PluginsSchemaName)]
    public class PluginConfiguration
    {
        [Key]
        public int PluginConfigurationId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        [Index(IsUnique = true)]
        public PluginType PluginType { get; set; }

        public int? PluginId { get; set; }
        public virtual Plugin Plugin { get; set; }
    }
}
