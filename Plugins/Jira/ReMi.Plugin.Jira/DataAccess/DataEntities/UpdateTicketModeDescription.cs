using ReMi.Common.Utils.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Plugin.Jira.DataAccess.Setup;

namespace ReMi.Plugin.Jira.DataAccess.DataEntities
{
    [Table("UpdateTicketModes", Schema = Constants.Schema)]
    public class UpdateTicketModeDescription : EnumDescription
    { }
}
