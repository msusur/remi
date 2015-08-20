using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataEntities.Metrics
{
    [Table("Metrics", Schema = Constants.SchemaName)]
    public class Metric
    {
        [Key]
        public int MetricId { get; set; }
        
        [Required, Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        public DateTime? ExecutedOn { get; set; }
        
        [Required, Index("IX_ReleaseWindowId_MetricType", 1, IsUnique = true)]
        public int ReleaseWindowId { get; set; }

        public virtual ReleaseWindow ReleaseWindow { get; set; }

        [Required, Index("IX_ReleaseWindowId_MetricType", 2, IsUnique = true)]
        public MetricType MetricType { get; set; }

        public override string ToString()
        {
            return
                String.Format(
                    "[MetricId={0}, ExternalId={1}, ExecutedOn={2}, MetricType={3}, ReleaseWindowId={4}]",
                    MetricId, ExternalId, ExecutedOn, MetricType, ReleaseWindowId);
        }
    }
}
