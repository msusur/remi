using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Go.DataAccess.Setup;

namespace ReMi.Plugin.Go.DataAccess.DataEntities
{
    [Table("PackageGoPipelineConfiguration", Schema = Constants.Schema)]
    public class PackageGoPipelineConfiguration
    {
        [Key]
        public int PackageGoPipelineConfigurationId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        [Index("IX_GoPipelineName_PackageConfigurationId", IsUnique = true, Order = 1), StringLength(256)]
        public string Name { get; set; }

        public bool IsIncludedByDefault { get; set; }
        public bool IsDisabled { get; set; }

        [Index("IX_GoPipelineName_PackageConfigurationId", IsUnique = true, Order = 2)]
        public int PackageConfigurationId { get; set; }
        public virtual PackageConfiguration PackageConfiguration { get; set; }
    }
}
