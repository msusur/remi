using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;

namespace ReMi.DataEntities.ReleasePlan
{
    [Table("ReleaseDeploymentMeasurements", Schema = Constants.SchemaName)]
    public class ReleaseDeploymentMeasurement
    {
        [Key]
        public int ReleaseDeploymentMeasurementId { get; set; }

        public int ReleaseWindowId { get; set; }

        public int? ParentMeasurementId { get; set; }

        [Index, StringLength(256)]
        public string StepName { get; set; }

        public string Locator { get; set; }

        public string StepId { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? FinishTime { get; set; }

        public DateTime CreatedOn { get; set; }

        public int CreatedByAccountId { get; set; }

        [Index]
        public int? BuildNumber { get; set; }

        public int NumberOfTries { get; set; }


        // Relations
        public virtual ReleaseWindow ReleaseWindow { get; set; }

        public virtual Account CreatedBy { get; set; }

        public virtual ReleaseDeploymentMeasurement ParentMeasurement { get; set; }


        public override string ToString()
        {
            return string.Format("[ReleaseDeploymentMeasurementId={0}, ReleaseWindowId={1}, ParentMeasurementId={2}, " +
                "StepName={3}, Locator={4}, StepId={5}, StartTime={6}, FinishTime={7}, CreatedOn={8}, CreatedByAccountId={9}, NumberOfTries={10}]",
                ReleaseDeploymentMeasurementId, ReleaseWindowId, ParentMeasurementId,
                StepName, Locator, StepId, StartTime, FinishTime, CreatedOn, CreatedByAccountId, NumberOfTries);
        }
    }
}
