using System;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants;
using ReMi.Common.Utils.Enums;

namespace ReMi.DataEntities.Metrics
{
    [Table("MetricTypes", Schema = Constants.SchemaName)]
    public class MetricTypeDescription : OrderedEnumDescription
    {
        public virtual bool? IsBackground
        {
            get { return string.Equals(Annotation, "IsBackground"); }
            // ReSharper disable once ValueParameterNotUsed
            set { Annotation = value.HasValue && value.Value ? "IsBackground" : null; }
        }

        public override string ToString()
        {
            return String.Format("[Id={0}, Name={1}, Description={2}, Order={3}, IsBackground={4}]",
                Id, Name, Description, Order, IsBackground);
        }
    }
}
