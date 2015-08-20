using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants;
using ReMi.Common.Utils.Enums;

namespace ReMi.DataEntities.ReleasePlan
{
    [Table("ReleaseTaskType", Schema = Constants.SchemaName)]
    public class ReleaseTaskTypeDescription : EnumDescription
    {
    }
}
