using ReMi.Plugin.Gerrit.DataAccess.Setup;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.Plugin.Gerrit.DataAccess.DataEntities
{
    [Table("PackageConfiguration", Schema = Constants.Schema)]
    public class PackageConfiguration
    {
        [Key]
        public int PackageConfigurationId { get; set; }

        [Index(IsUnique = true)]
        public Guid PackageId { get; set; }

        public virtual ICollection<Repository> Repositories { get; set; }
    }
}
