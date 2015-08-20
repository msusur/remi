using ReMi.Plugin.Gerrit.DataAccess.Setup;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.Plugin.Gerrit.DataAccess.DataEntities
{
    [Table("Repositories", Schema = Constants.Schema)]
    public class Repository
    {
        [Key]
        public int RepositoryId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        [Index("IX_Name_PackageConfigurationId", IsUnique = true, Order = 1)]
        [StringLength(256)]
        public string Name { get; set; }
        public string DefaultFrom { get; set; }
        public string DefaultTo { get; set; }
        public bool StartFromLatest { get; set; }
        public bool IsIncludedByDefault { get; set; }
        public bool IsDisabled { get; set; }

        [Index("IX_Name_PackageConfigurationId", IsUnique = true, Order = 2)]
        public int PackageConfigurationId { get; set; }
        public virtual PackageConfiguration PackageConfiguration { get; set; }
    }
}
