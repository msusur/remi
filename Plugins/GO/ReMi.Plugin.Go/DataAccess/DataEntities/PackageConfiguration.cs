using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Go.DataAccess.Setup;

namespace ReMi.Plugin.Go.DataAccess.DataEntities
{
    [Table("PackageConfiguration", Schema = Constants.Schema)]
    public class PackageConfiguration
    {
        [Key]
        public int PackageConfigurationId { get; set; }

        [Index(IsUnique = true)]
        public Guid PackageId { get; set; }

        public int? GoServerConfigurationId { get; set; }
        public virtual GoServerConfiguration GoServerConfiguration { get; set; }

        public virtual ICollection<PackageGoPipelineConfiguration> PackageGoPipelineConfiguration { get; set; }
    }
}
