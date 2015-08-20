using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Jira.DataAccess.Setup;

namespace ReMi.Plugin.Jira.DataAccess.DataEntities
{
    [Table("PackageDefectJqlFilters", Schema = Constants.Schema)]
    public class PackageDefectJqlFilter
    {
        [Key]
        public int PackageDefectJqlFilterId { get; set; }

        [Index("IX_DefectJqlName_PackageConfigurationId", IsUnique = true, Order = 1), StringLength(256)]
        public string Name { get; set; }

        public string Value { get; set; }

        [Index("IX_DefectJqlName_PackageConfigurationId", IsUnique = true, Order = 2)]
        public int PackageConfigurationId { get; set; }
        public virtual PackageConfiguration PackageConfiguration { get; set; }
    }
}
