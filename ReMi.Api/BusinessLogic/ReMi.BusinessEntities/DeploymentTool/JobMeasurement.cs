using System;
using System.Collections.Generic;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;

namespace ReMi.BusinessEntities.DeploymentTool
{
    public class JobMeasurement
    {
        public string StepName { get; set; }
        public string Locator { get; set; }
        public string StepId { get; set; }
        public int? BuildNumber { get; set; }
        public int NumberOfTries { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }

        public JobStage JobStage { get; set; }

        private TimeSpan GetDuration()
        {
            if (!StartTime.HasValue || !FinishTime.HasValue) return new TimeSpan();
            if (FinishTime.Value <= StartTime.Value) return new TimeSpan();

            return FinishTime.Value.Subtract(StartTime.Value);
        }

        public double Duration
        {
            get
            {
                return GetDuration().TotalMilliseconds;
            }
        }

        public string DurationLabel
        {
            get
            {
                return GetDuration().ToDurationString();
            }
        }

        public List<JobMeasurement> ChildSteps { get; set; }

        public override string ToString()
        {
            return string.Format("[StepName={0}, Locator={1}, StepId={2}, StartTime={3}, FinishTime={4}," +
                                 " DurationLabel={5}, JobStage={6}, ChildSteps={7}, BuildNumber={8}, NumberOfTries={9}]",
                StepName, Locator, StepId, StartTime, FinishTime,
                DurationLabel, JobStage.ToDescriptionString(), ChildSteps.FormatElements(), BuildNumber, NumberOfTries);
        }
    }
}
