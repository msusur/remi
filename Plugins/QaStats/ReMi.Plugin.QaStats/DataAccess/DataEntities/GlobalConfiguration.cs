using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.Plugin.QaStats.DataAccess.DataEntities
{
    [Table("GlobalConfiguration", Schema = Constants.Schema)]
    public class GlobalConfiguration
    {
        [Key]
        public int GlobalConfigurationId { get; set; }

        [StringLength(1024)]
        public string QaServiceUrl { get; set; }
    }
}
