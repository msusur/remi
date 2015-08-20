using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Jira.DataAccess.Setup;

namespace ReMi.Plugin.Jira.DataAccess.DataEntities
{
    [Table("PackageConfiguration", Schema = Constants.Schema)]
    public class PackageConfiguration
    {
        [Key]
        public int PackageConfigurationId { get; set; }

        [Index(IsUnique = true)]
        public Guid PackageId { get; set; }

        public virtual ICollection<PackageJqlFilter> PackageJqlFilters { get; set; }
        public virtual ICollection<PackageDefectJqlFilter> PackageDefectJqlFilters { get; set; }
        public UpdateTicketMode UpdateTicketMode { get; set; }
        public string Label { get; set; }
    }
}
