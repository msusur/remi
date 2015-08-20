using ReMi.Contracts.Plugins.Data.DeploymentTool;
using ReMi.Plugin.Jenkins.JenkinsApi.Model;

namespace ReMi.Plugin.Jenkins.JenkinsApi
{
    public interface IJenkinsRequest
    {
        JobInfo GetJobInfo(string jobName);

        BuildInfo GetBuildInfo(string jobName, int buildNumber);

        JobMetric GetJobMetrics(string jobName, int? lastBuildNumber, TimeZone timeZone);

        void ChangeBaseUrl(string url);
    }
}
