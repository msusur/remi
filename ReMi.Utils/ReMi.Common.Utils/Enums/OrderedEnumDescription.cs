using System;
using System.ComponentModel.DataAnnotations;

namespace ReMi.Common.Utils.Enums
{
    public abstract class OrderedEnumDescription : EnumDescription
    {
        [Required]
        public virtual int Order { get; set; }

        public override string ToString()
        {
            return String.Format("[Id={0}, Name={1}, Description={2}, Order={3}]", Id, Name, Description, Order);
        }
    }
}
