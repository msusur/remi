using System;
using System.Collections.Generic;
using Common.Logging;
using ReMi.Plugin.Go.Entities;
using ReMi.Plugin.Go.Helpers;

namespace ReMi.Plugin.Go.BusinessLogic
{
    public class GoJobTimingCollector : IGoJobTimingCollector
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IGoRequest _goRequest;

        public GoJobTimingCollector(IGoRequest goRequest)
        {
            _goRequest = goRequest;
        }


        public StepTiming GetPipelineTiming(string pipeline)
        {
            var pipelineInfo = _goRequest.GetPipelineInfo(pipeline);
            if (pipelineInfo.Groups.Count == 0 || pipelineInfo.Groups[0].History.Count == 0)
                return null;

            var lastHistroy = pipelineInfo.Groups[0].History[0];

            var stageTimings = new List<StepTiming>();

            var pipelinePeriodCalc = new TimePeriodCaluclator();

            foreach (var stageItem in lastHistroy.Stages)
            {
                var stageTiming = GetStageTiming(pipeline, stageItem);
                if (stageTiming == null) continue;

                pipelinePeriodCalc.AddPeriod(stageTiming.StartTime, stageTiming.FinishTime);

                stageTimings.Add(stageTiming);
            }

            int buildNumber;
            var hasBuildNumber = int.TryParse(lastHistroy.CounterOrLabel, out buildNumber);
            return new StepTiming
            {
                StepName = pipeline,
                Locator = null,
                StepId = lastHistroy.PipelineId,
                StartTime = pipelinePeriodCalc.StartTime.ToUniversalTime(),
                FinishTime = pipelinePeriodCalc.FinishTime.ToUniversalTime(),
                ChildSteps = stageTimings,
                BuildNumber = hasBuildNumber ? buildNumber : (int?)null
            };
        }

        private StepTiming GetStageTiming(string pipeline, Stage stage)
        {
            if (stage.StageId == "0")
            {
                Log.WarnFormat("Stage doesn't has actual state. Stage={0}", stage);
                return null;
            }

            var jobIds = _goRequest.GetJobsForStage(stage.StageId);

            var jobTimings = new List<StepTiming>();

            var stagePeriodCalc = new TimePeriodCaluclator();

            foreach (var jobId in jobIds)
            {
                var jobTiming = GetJobTiming(pipeline, stage.StageName, jobId);
                if (jobTiming == null) continue;

                stagePeriodCalc.AddPeriod(jobTiming.StartTime, jobTiming.FinishTime);

                jobTimings.Add(jobTiming);
            }

            return new StepTiming
            {
                StepName = stage.StageName,
                Locator = string.Format("{0}pipelines/{1}", _goRequest.BaseUrl, stage.StageLocator),
                StepId = stage.StageId,
                StartTime = stagePeriodCalc.StartTime.ToUniversalTime(),
                FinishTime = stagePeriodCalc.FinishTime.ToUniversalTime(),
                ChildSteps = jobTimings
            };
        }

        private StepTiming GetJobTiming(string pipelineName, string stageName, string jobId)
        {
            var jobInfo = _goRequest.GetJobInfo(pipelineName, stageName, jobId);
            if (jobInfo == null)
            {
                Log.WarnFormat("Fail to get infromation about Go jobId={0}", jobId);
                return null;
            }

            DateTime completeTime, scheduleTime;

            if (DateTime.TryParse(jobInfo.Build_Completed_Date, out completeTime)
                && DateTime.TryParse(jobInfo.Build_Scheduled_Date, out scheduleTime))
            {
                return new StepTiming
                {
                    StepName = jobInfo.Name,
                    Locator = string.Format("{0}tab/build/detail/{1}", _goRequest.BaseUrl, jobInfo.BuildLocator),
                    StepId = jobId,
                    StartTime = scheduleTime.ToUniversalTime(),
                    FinishTime = completeTime.ToUniversalTime()
                };
            }

            Log.WarnFormat("GO job has incorrect format. Job={0}", jobInfo);
            return null;
        }
    }
}
