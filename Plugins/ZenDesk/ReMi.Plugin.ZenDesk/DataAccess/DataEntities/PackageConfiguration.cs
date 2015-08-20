using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.ZenDesk.DataAccess.Setup;

namespace ReMi.Plugin.ZenDesk.DataAccess.DataEntities
{
    [Table("PackageConfiguration", Schema = Constants.Schema)]
    public class PackageConfiguration
    {
        [Key]
        public int PackageConfigurationId { get; set; }

        [Index(IsUnique = true)]
        public Guid PackageId { get; set; }
    }
}
