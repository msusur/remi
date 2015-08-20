using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants;
using ReMi.Common.Utils.Enums;

namespace ReMi.DataEntities.ReleaseCalendar
{
    [Table("ReleaseTrack", Schema = Constants.SchemaName)]
    public class ReleaseTrackDescription : EnumDescription
    {
    }
}
