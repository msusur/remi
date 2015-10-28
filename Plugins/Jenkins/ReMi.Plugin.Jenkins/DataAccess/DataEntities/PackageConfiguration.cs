using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Jenkins.DataAccess.Setup;

namespace ReMi.Plugin.Jenkins.DataAccess.DataEntities
{
    [Table("PackageConfiguration", Schema = Constants.Schema)]
    public class PackageConfiguration
    {
        [Key]
        public int PackageConfigurationId { get; set; }

        [Index(IsUnique = true)]
        public Guid PackageId { get; set; }

        public TimeZone TimeZone { get; set; }

        public bool? AllowGettingDeployTime { get; set; }

        public int? JenkinsServerConfigurationId { get; set; }
        public virtual JenkinsServerConfiguration JenkinsServerConfiguration { get; set; }

        public virtual ICollection<PackageJenkinsJobConfiguration> PackageJenkinsJobConfiguration { get; set; }
    }
}
