using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Contracts.Plugins.Data;
using ReMi.DataEntities.Products;

namespace ReMi.DataEntities.Plugins
{
    [Table("PluginPackageConfiguration", Schema = Constants.PluginsSchemaName)]
    public class PluginPackageConfiguration
    {
        [Key]
        public int PluginPackageConfigurationId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        [Index("IX_PluginType_Package", IsUnique = true, Order = 1)]
        public PluginType PluginType { get; set; }

        [Index("IX_PluginType_Package", IsUnique = true, Order = 2)]
        public int PackageId { get; set; }
        [ForeignKey("PackageId")]
        public virtual Product Package { get; set; }

        public int? PluginId { get; set; }
        public virtual Plugin Plugin { get; set; }
    }
}
