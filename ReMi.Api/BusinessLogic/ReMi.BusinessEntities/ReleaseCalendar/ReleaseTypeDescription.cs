using System.Runtime.Serialization;
using ReMi.Common.Constants;
using ReMi.Common.Utils.Enums;

namespace ReMi.BusinessEntities.ReleaseCalendar
{
    public class ReleaseTypeDescription : EnumDescription
    {
        [DataMember]
        public virtual bool IsMaintenance
        {
            get { return string.Equals(Annotation, "IsMaintenance"); }
            set { ; }
        }
    }
}
