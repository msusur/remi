using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Logging;
using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data.DeploymentTool;
using ReMi.Plugin.Common;
using ReMi.Plugin.Jenkins.DataAccess.Gateways;
using ReMi.Plugin.Jenkins.JenkinsApi.Model;
using RestSharp;

namespace ReMi.Plugin.Jenkins.JenkinsApi
{
    public class JenkinsRequest : RestApiRequest, IJenkinsRequest
    {
        public Func<IGlobalConfigurationGateway> GlobalConfigurationGatewayFactory { get; set; }
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        private string _password;
        protected override string Password
        {
            get
            {
                if (!string.IsNullOrEmpty(_password)) return _password;

                using (var gateway = GlobalConfigurationGatewayFactory())
                {
                    var config = gateway.GetGlobalConfiguration();
                    _password = config != null ? config.JenkinsPassword : null;
                }
                return _password;
            }
            set { _password = value; }
        }

        private string _userName;
        protected override string UserName
        {
            get
            {
                if (!string.IsNullOrEmpty(_userName)) return _userName;

                using (var gateway = GlobalConfigurationGatewayFactory())
                {
                    var config = gateway.GetGlobalConfiguration();
                    _userName = config != null ? config.JenkinsUser : null;
                }
                return _userName;
            }
            set { _userName = value; }
        }

        private string _baseUrl;
        public override string BaseUrl
        {
            get { return _baseUrl; }
        }

        public JobMetric GetJobMetrics(string jobName, int? lastBuildNumber, TimeZone timeZone)
        {
            var jobInfo = GetJobInfo(jobName);
            if (jobInfo == null || jobInfo.Builds.IsNullOrEmpty()) return null;

            var latestBuild = jobInfo.Builds.OrderByDescending(x => x.Number).First();
            var builds = lastBuildNumber.HasValue && latestBuild.Number > lastBuildNumber.Value
                ? jobInfo.Builds.Where(x => x.Number > lastBuildNumber.Value).OrderByDescending(x => x.Number).ToArray()
                : new[] { latestBuild };
            var buildInfos = new ConcurrentBag<BuildInfo>();
            Parallel.ForEach(builds, b =>
            {
                var buildInfo = GetBuildInfo(jobName, b.Number);
                if (buildInfo == null) return;
                buildInfos.Add(buildInfo);
            });
            var orderedBuildInfos = buildInfos.OrderByDescending(x => x.Number).ToArray();
            var releaseBuild = builds.First();
            var result = new JobMetric
            {
                JobId = orderedBuildInfos.First().Id,
                Name = jobName,
                BuildNumber = builds.First().Number,
                NumberOfTries = builds.Count(),
                StartTime = orderedBuildInfos.Last().GetStartTime(timeZone),
                EndTime = orderedBuildInfos.First().GetEndTime(timeZone),
                Url = orderedBuildInfos.First().Url,
                ChildMetrics = null
            };
            if (releaseBuild.SubBuilds.IsNullOrEmpty())
                return result;

            var jobBuildInfos = new ConcurrentDictionary<string, BuildInfo>();
            Parallel.ForEach(builds.First().SubBuilds, b =>
            {
                var buildInfo = GetBuildInfo(b.JobName, b.BuildNumber);
                if (buildInfo == null) return;
                if (!jobBuildInfos.TryAdd(b.JobName, buildInfo))
                    Logger.ErrorFormat("Cannot get job={0}, BuildNumber={1}",
                        b.JobName, b.BuildNumber);
            });
            var otherBuilds = builds.Skip(1);
            var jobsNumberOfTries = releaseBuild.SubBuilds.ToDictionary(x => x.JobName, x => 1);
            if (!otherBuilds.IsNullOrEmpty())
                otherBuilds.SelectMany(x => x.SubBuilds)
                    .Where(x => x.Result != "SUCCESS")
                    .Each(x => jobsNumberOfTries[x.JobName]++);
            result.ChildMetrics = releaseBuild.SubBuilds.GroupBy(x => x.PhaseName)
                .Select(x => new { Phase = x.Key, Jobs = x.Select(j =>
                    new { Job = j, JobInfo = jobBuildInfos[j.JobName], NumberOfTries = jobsNumberOfTries[j.JobName] }) })
                .Select(x => new JobMetric
                {
                    JobId = string.Format("{0}_{1}", result.JobId, x.Phase),
                    Name = x.Phase,
                    BuildNumber = result.BuildNumber,
                    StartTime = x.Jobs.Min(j => j.JobInfo.GetStartTime(timeZone)),
                    EndTime = x.Jobs.Max(j => j.JobInfo.GetEndTime(timeZone)),
                    NumberOfTries = x.Jobs.Max(j => j.NumberOfTries),
                    ChildMetrics = x.Jobs.Select(j => new JobMetric
                    {
                        JobId = j.JobInfo.Id,
                        NumberOfTries = j.NumberOfTries,
                        BuildNumber = j.Job.BuildNumber,
                        Name = j.Job.JobName,
                        EndTime = j.JobInfo.GetEndTime(timeZone),
                        StartTime = j.JobInfo.GetStartTime(timeZone),
                        Url = j.JobInfo.Url
                    }).ToArray()
                }).ToArray();
            return result;
        }

        public void ChangeBaseUrl(string url)
        {
            _baseUrl = url;
        }

        public JobInfo GetJobInfo(string jobName)
        {
            var result = Get<JobInfo>(string.Format("job/{0}/api/json", jobName));

            return GetResult(result);
        }

        public BuildInfo GetBuildInfo(string jobName, int buildNumber)
        {
            var response = Get<BuildInfo>(string.Format("job/{0}/{1}/api/json", jobName, buildNumber));

            return GetResult(response);
        }

        private T GetResult<T>(IRestResponse<T> response)
            where T : class
        {
            if (response == null) return null;
            if (response.ErrorException != null)
                throw new ApplicationException("Error occured while getting data from Jenkins API", response.ErrorException);
            if (!string.IsNullOrEmpty(response.ErrorMessage))
                throw new ApplicationException(response.ErrorMessage);

            return response.Data;
        }
    }
}
