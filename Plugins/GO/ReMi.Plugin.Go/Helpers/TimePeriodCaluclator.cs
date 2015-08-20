using System;

namespace ReMi.Plugin.Go.Helpers
{
    public class TimePeriodCaluclator
    {
        private bool IsEmpty { get; set; }

        public DateTime StartTime { get; private set; }
        public DateTime FinishTime { get; private set; }

        public TimePeriodCaluclator()
        {
            IsEmpty = true;
        }

        public void AddPeriod(DateTime startTime, DateTime finishTime)
        {
            if (finishTime < startTime)
                throw new ArgumentException("FinishTime should be grater then StartTime");

            if (IsEmpty)
            {
                IsEmpty = false;
                StartTime = startTime;
                FinishTime = finishTime;
                return;
            }

            if (StartTime > startTime)
                StartTime = startTime;
            if (FinishTime < finishTime)
                FinishTime = finishTime;
        }
    }
}
