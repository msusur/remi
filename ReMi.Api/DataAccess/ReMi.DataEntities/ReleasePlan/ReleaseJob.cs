using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataEntities.ReleasePlan
{
    [Table("ReleaseJobs", Schema = Constants.SchemaName)]
    public class ReleaseJob
    {
        [Key]
        public int ReleaseJobId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        public string Name { get; set; }

        [Index("IX_JobId_ReleaseWindowId", IsUnique = true, Order = 1)]
        public Guid JobId { get; set; }

        public int Order { get; set; }

        public bool IsIncluded { get; set; }


        [Index("IX_JobId_ReleaseWindowId", IsUnique = true, Order = 2)]
        public int ReleaseWindowId { get; set; }

        public virtual ReleaseWindow ReleaseWindow { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseJobId={0}, ExternalId={1}, Name={2}, JobId={3}, Order={4}, IsIncluded={5}, ReleaseWindowId={6}]",
                ReleaseJobId, ExternalId, Name, JobId, Order, IsIncluded, ReleaseWindowId);
        }
    }
}
