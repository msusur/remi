using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants;
using ReMi.Common.Utils.Enums;

namespace ReMi.DataEntities.ReleaseCalendar
{
    [Table("ReleaseDecision", Schema = Constants.SchemaName)]
    public class ReleaseDecisionDescription : EnumDescription
	{
	}
}
