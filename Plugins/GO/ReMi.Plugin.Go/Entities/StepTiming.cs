using System;
using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.Plugin.Go.Entities
{
    public class StepTiming
    {
        public string StepId { get; set; }
        public string StepName { get; set; }
        public string Locator { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public int? BuildNumber { get; set; }

        public IEnumerable<StepTiming> ChildSteps { get; set; }

        public override string ToString()
        {
            return string.Format("[StepId={0}, StepName={1}, Locator={2}, StartTime={3}, FinishTime={4}, ChildSteps={5}, BuildNumber={6}]",
                StepId, StepName, Locator, StartTime, FinishTime, ChildSteps.FormatElements(), BuildNumber);
        }
    }
}
