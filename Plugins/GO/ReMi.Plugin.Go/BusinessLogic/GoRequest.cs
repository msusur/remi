using ReMi.Plugin.Common;
using ReMi.Plugin.Common.Serialization;
using ReMi.Plugin.Go.Entities;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ReMi.Contracts.Plugins.Data.Exceptions;
using ReMi.Contracts.Plugins.Services;

namespace ReMi.Plugin.Go.BusinessLogic
{
    public class GoRequest : RestApiRequest, IGoRequest
    {
        public IPluginConfiguration<PluginConfigurationEntity> PluginConfiguration { get; set; }

        protected override RestClient Client
        {
            get
            {
                var client = base.Client;

                client.RemoveHandler("text/plain");
                client.AddHandler("text/plain", new JsonSerializationImpl());

                return client;
            }
        }

        protected override string UserName { get { return PluginConfiguration.GetPluginConfiguration().GoUser; } set { } }
        protected override string Password { get { return PluginConfiguration.GetPluginConfiguration().GoPassword; } set { } }

        private string _baseUrl;

        public override string BaseUrl
        {
            get { return _baseUrl; }
        }

        //List of all pipelines
        public PipelineList GetPipelinesList()
        {
            var response = Get<PipelineList>("api/pipelines.xml");

            return response.Data;
        }

        public ValueStreamMap GetPipelineValueStreamMap(string pipeline, string label)
        {
            var valueStreamMapServicePath = string.Format("/pipelines/value_stream_map/{0}/{1}.json",
                pipeline, label);
            var valueStreamMapResponse = Get<ValueStreamMap>(valueStreamMapServicePath);

            return valueStreamMapResponse != null ? valueStreamMapResponse.Data : null;
        }

        public IEnumerable<string> GetJobsForStage(string stageId)
        {
            var response = Get<StageDetails>(string.Format("api/stages/{0}.xml", stageId));
            if (response.Data.Jobs != null)
            {
                var regexp = new Regex(@"(?<=\/)\d+(?=\.xml)");
                var ids = response.Data.Jobs
                    .Select(o => regexp.Match(o.Href).Success ? regexp.Match(o.Href).Value : string.Empty)
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .ToList();

                return ids;
            }

            return new string[0];
        }

        public IEnumerable<GitCommit> GetReadyToReleaseCommits(IEnumerable<string> livePiplines)
        {
            try
            {
                IGitCommitsCollector gitCommitsCollector = new GitCommitsCollector(this);
                return gitCommitsCollector.Collect(livePiplines);
            }
            catch (Exception ex)
            {
                throw new FailedToRetrieveSourceControlChangesException("Failed to retrieve git commits", ex);
            }
        }

        public PipelineInfo GetPipelineInfo(string pipeline)
        {
            var pipelineInfoResponse = Get<PipelineInfo>("pipelineHistory.json", new Dictionary<string, string>
            {
                {"pipelineName", pipeline}
            });

            return pipelineInfoResponse == null || pipelineInfoResponse.Data == null ? null : pipelineInfoResponse.Data;
        }

        public JobInfo GetJobInfo(string pipeline, string stageName, string jobId)
        {
            var jobInfoResponse = Get<List<BuildingInfo>>("jobStatus.json", new Dictionary<string, string>
            {
                {"pipelineName", pipeline},
                {"stageName", stageName},
                {"jobId", jobId}
            });

            var data = jobInfoResponse.Data;
            if (data != null)
            {
                var buildingInfo = data.FirstOrDefault();
                if (buildingInfo != null)
                    return buildingInfo.Building_Info;
            }

            return null;
        }

        public StepTiming GetPipelineTiming(string pipeline)
        {
            var goJobsCollector = new GoJobTimingCollector(this);
            return goJobsCollector.GetPipelineTiming(pipeline);
        }

        public void ChangeBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
        }
    }
}
