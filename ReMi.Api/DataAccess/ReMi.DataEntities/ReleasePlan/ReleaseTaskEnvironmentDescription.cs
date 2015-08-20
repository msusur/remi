using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants;
using ReMi.Common.Utils.Enums;

namespace ReMi.DataEntities.ReleasePlan
{
    [Table("ReleaseTaskEnvironments", Schema = Constants.SchemaName)]
    public class ReleaseTaskEnvironmentDescription : EnumDescription
    {
    }
}
