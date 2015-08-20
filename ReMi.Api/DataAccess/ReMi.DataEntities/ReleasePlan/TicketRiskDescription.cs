using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Utils.Enums;

namespace ReMi.DataEntities.ReleasePlan
{
    [Table("TicketRisk", Schema = Constants.SchemaName)]
    public class TicketRiskDescription : EnumDescription
    {
    }
}
