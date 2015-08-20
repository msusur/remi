using ReMi.Common.Constants;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Utils.Enums;

namespace ReMi.DataEntities.ProductRequests
{
    [Table("RemovingReasons", Schema = Constants.SchemaName)]
    public class RemovingReasonDescription : EnumDescription
    {
    }
}
