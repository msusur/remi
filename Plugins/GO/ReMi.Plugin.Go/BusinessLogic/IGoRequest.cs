using System.Collections.Generic;
using ReMi.Plugin.Go.Entities;

namespace ReMi.Plugin.Go.BusinessLogic
{
    public interface IGoRequest
    {
        PipelineList GetPipelinesList();

        IEnumerable<GitCommit> GetReadyToReleaseCommits(IEnumerable<string> pipelines);

        PipelineInfo GetPipelineInfo(string pipeline);

        ValueStreamMap GetPipelineValueStreamMap(string pipeline, string label);

        IEnumerable<string> GetJobsForStage(string stageId);

        StepTiming GetPipelineTiming(string pipeline);

        JobInfo GetJobInfo(string pipeline, string stageName, string jobId);

        void ChangeBaseUrl(string url);

        string BaseUrl { get; }
    }
}
