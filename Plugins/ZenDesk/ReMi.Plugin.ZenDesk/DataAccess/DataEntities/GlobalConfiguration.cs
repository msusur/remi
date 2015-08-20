using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.ZenDesk.DataAccess.Setup;

namespace ReMi.Plugin.ZenDesk.DataAccess.DataEntities
{
    [Table("GlobalConfiguration", Schema = Constants.Schema)]
    public class GlobalConfiguration
    {
        [Key]
        public int GlobalConfigurationId { get; set; }

        public string ZenDeskUrl { get; set; }
        public string ZenDeskUser { get; set; }
        public string ZenDeskPassword { get; set; }
    }
}
