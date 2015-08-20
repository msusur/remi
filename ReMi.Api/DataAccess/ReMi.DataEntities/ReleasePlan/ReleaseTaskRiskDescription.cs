using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants;
using ReMi.Common.Utils.Enums;

namespace ReMi.DataEntities.ReleasePlan
{
    [Table("ReleaseTaskRisks", Schema = Constants.SchemaName)]
    public class ReleaseTaskRiskDescription : EnumDescription
    {
    }
}
