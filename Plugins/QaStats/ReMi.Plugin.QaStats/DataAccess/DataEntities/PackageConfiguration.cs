using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.Plugin.QaStats.DataAccess.DataEntities
{
    [Table("PackageConfiguration", Schema = Constants.Schema)]
    public class PackageConfiguration
    {
        [Key]
        public int PackageConfigurationId { get; set; }

        public Guid PackageId { get; set; }

        [StringLength(1024)]
        public string PackagePath { get; set; }
    }
}
