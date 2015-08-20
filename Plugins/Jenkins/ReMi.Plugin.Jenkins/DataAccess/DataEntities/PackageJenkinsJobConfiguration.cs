using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Jenkins.DataAccess.Setup;

namespace ReMi.Plugin.Jenkins.DataAccess.DataEntities
{
    [Table("PackageJenkinsJobConfiguration", Schema = Constants.Schema)]
    public class PackageJenkinsJobConfiguration
    {
        [Key]
        public int PackageJenkinsJobConfigurationId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        [Index("IX_JenkinsJobName_PackageConfigurationId", IsUnique = true, Order = 1), StringLength(256)]
        public string Name { get; set; }

        public bool IsIncludedByDefault { get; set; }
        public bool IsDisabled { get; set; }

        [Index("IX_JenkinsJobName_PackageConfigurationId", IsUnique = true, Order = 2)]
        public int PackageConfigurationId { get; set; }
        public virtual PackageConfiguration PackageConfiguration { get; set; }
    }
}
