using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants;
using ReMi.Common.Utils.Enums;

namespace ReMi.DataEntities.ReleaseCalendar
{
    [Table("ReleaseTypes", Schema = Constants.SchemaName)]
	public class ReleaseTypeDescription : EnumDescription
	{
        public virtual bool? IsMaintenance
        {
            get { return string.Equals(Annotation, "IsMaintenance"); }
            set { Annotation = value.HasValue && value.Value ? "IsMaintenance" : null; }
        }
	}
}
