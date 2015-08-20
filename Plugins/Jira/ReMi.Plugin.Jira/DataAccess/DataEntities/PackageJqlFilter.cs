using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Jira.DataAccess.Setup;

namespace ReMi.Plugin.Jira.DataAccess.DataEntities
{
    [Table("PackageJqlFilters", Schema = Constants.Schema)]
    public class PackageJqlFilter
    {
        [Key]
        public int PackageJqlFilterId { get; set; }

        [Index("IX_JqlFiltersName_PackageConfigurationId", IsUnique = true, Order = 1), StringLength(256)]
        public string Name { get; set; }

        public string Value { get; set; }

        [Index("IX_JqlFiltersName_PackageConfigurationId", IsUnique = true, Order = 2)]
        public int PackageConfigurationId { get; set; }
        public virtual PackageConfiguration PackageConfiguration { get; set; }
    }
}
