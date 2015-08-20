using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Autofac.Extras.Attributed;
using AutoMapper;
using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data.ReleaseContent;
using ReMi.Contracts.Plugins.Services;
using ReMi.Contracts.Plugins.Services.ReleaseContent;
using ReMi.Plugin.Common;
using ReMi.Plugin.Jira.Exceptions;
using ReMi.Plugin.Jira.Models;
using RestSharp;
using Version = ReMi.Plugin.Jira.Models.Version;

namespace ReMi.Plugin.Jira
{
    /// <summary>
    /// More info: https://docs.atlassian.com/jira/REST/latest/
    /// </summary>
    public class JiraRequest : RestApiRequest, IReleaseContent
    {
        public IMappingEngine MappingEngine { get; set; }

        private readonly IPluginConfiguration<PluginConfigurationEntity> _configuration;
        private readonly IPluginPackageConfiguration<PluginPackageConfigurationEntity> _packageConfigurations;


        private readonly string _baseUrl;

        public override string BaseUrl
        {
            get { return _baseUrl; }
        }

        public JiraRequest(
            [WithKey(PluginInitializer.JiraId)]
            IPluginConfiguration<PluginConfigurationEntity> configuration,
            [WithKey(PluginInitializer.JiraId)]
            IPluginPackageConfiguration<PluginPackageConfigurationEntity> packageConfigurations)
        {
            _configuration = configuration;
            _packageConfigurations = packageConfigurations;
            var config = _configuration.GetPluginConfiguration();
            UserName = config.JiraUser;
            Password = config.JiraPassword;
            _baseUrl = config.JiraUrl;
        }

        private T GetWrapper<T>(string resource, Dictionary<string, string> parameters = null)
            where T : class, new()
        {
            var response = Get<T>(resource, parameters);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                //if (!string.IsNullOrWhiteSpace(response.Content))
                //Log.Debug("Returned response:\n" + response.Content);

                //if (response.ErrorException != null)
                //Log.Error(response.ErrorException);

                return null;
            }

            return response.Data;
        }

        public User UserInfo(string userName)
        {
            return GetWrapper<User>("user", new Dictionary<string, string> { { "username", userName } });
        }

        public ServerInfo ServerInfo()
        {
            return GetWrapper<ServerInfo>("serverInfo");
        }

        public Project Project(string projectKey)
        {
            return GetWrapper<Project>("project/" + projectKey);
        }

        public Issue Issue(string issueKey)
        {
            return GetWrapper<Issue>("issue/" + issueKey);
        }

        public IEnumerable<Issue> SearchIssues(IEnumerable<string> filter)
        {
            var filterExpression = string.Format("({0})", string.Join(") OR (", filter));

            return SearchIssues(filterExpression);
        }
        public IEnumerable<Issue> SearchIssues(IDictionary<string, string> filter)
        {
            var filterExpression = string.Join(" AND ",
                                               filter.Select(o => string.Format("{0}={1}", o.Key, SanitizeFilterValue(o.Value)))
                                               .ToArray());

            return SearchIssues(filterExpression);
        }

        private IEnumerable<Issue> SearchIssues(string filterExpression)
        {
            var config = _configuration.GetPluginConfiguration();
            var searchObj = new
            {
                jql = filterExpression,
                startAt = 0,
                maxResults = config.JiraIssuesMaxCount <= 0 ? 300 : config.JiraIssuesMaxCount
            };

            IRestResponse<IssuesList> response = Post<IssuesList>("search", searchObj);

            if (response.StatusCode != HttpStatusCode.OK)
                return Enumerable.Empty<Issue>();

            if (response.Data == null)
                throw new Exception("Jira didn't return any data");

            return response.Data.Issues;
        }

        public Issue CreateIssue(Issue issue)
        {
            dynamic newIssue = new
            {
                update = new { },
                fields = new
                {
                    project = new { id = issue.Fields.Project.Id.ToString(CultureInfo.InvariantCulture) },
                    summary = issue.Fields.Summary,
                    issuetype = new { id = issue.Fields.IssueType.Id.ToString(CultureInfo.InvariantCulture) },
                    assignee = new { name = issue.Fields.Assignee.Name },
                    description = issue.Fields.Description
                }
            };

            IRestResponse<CreateIssueStatus> response = Post<CreateIssueStatus>("issue", newIssue);
            if (response.StatusCode != HttpStatusCode.Created || response.Data == null)
            {
                //if (response.ErrorException != null)
                //Log.Error(response.ErrorException);
                //else
                //Log.Error("Empty response returned");

                return issue;
            }

            issue.Id = response.Data.Id;
            issue.Key = response.Data.Key;

            return issue;
        }

        public bool SetAssignee(string issueKey, string assigneeName)
        {
            dynamic update = new
            {
                name = assigneeName
            };

            IRestResponse response = Put<CreateIssueStatus>("issue/" + issueKey + "/assignee", update);
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                //if (response.ErrorException != null)
                //Log.Error(response.ErrorException);

                return false;
            }

            return true;
        }

        public Comment AddComment(string issueKey, string text)
        {
            dynamic update = new
            {
                body = text,
            };

            IRestResponse<Comment> response = Post<Comment>("issue/" + issueKey + "/comment", update);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                //if (response.ErrorException != null)
                //Log.Error(response.ErrorException);

                return null;
            }

            return response.Data;
        }

        public bool AddLabel(IEnumerable<string> jiraTickets, string labelText)
        {
            bool updateResultSuccessful = true;

            var issuesFilter = string.Format("key in ({0})", string.Join(", ", jiraTickets));
            var issues = SearchIssues(issuesFilter);


            foreach (Issue jiraTicketToAddLabel in issues)
            {
                var issueLabels = jiraTicketToAddLabel.Fields.Labels.ToList();
                issueLabels.Add(labelText);

                var update = new
                {
                    fields = new
                    {
                        labels = issueLabels.ToArray()
                    }
                };

                IRestResponse response = Put<CreateIssueStatus>("issue/" + jiraTicketToAddLabel.Key, update);
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    updateResultSuccessful = false;
                }
            }

            return updateResultSuccessful;
        }

        public IEnumerable<Version> Versions(string project)
        {
            return GetWrapper<List<Version>>("project/" + project + "/versions");
        }

        public SprintInfo SprintIssues(string project, DateTime releaseDate)
        {
            var versions = Versions(project);
            var version = versions.FirstOrDefault(x => x.ReleaseDate == releaseDate);
            var issues =
                SearchIssues(new Dictionary<string, string>
                {
                    {"project", project},
                    {"cf[10730]", version.Name}
                });

            return new SprintInfo { IsReleased = version.Released, Issues = issues, Sprint = version.Name };
        }

        #region Helpers

        private string SanitizeFilterValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Replace(".", "\\u002e");
        }

        #endregion

        public IEnumerable<ReleaseContentTicket> GetTickets(IEnumerable<Guid> packageId)
        {
            var jqlScripts = _packageConfigurations.GetPluginPackageConfiguration()
                .Where(x => packageId.Any(p => p == x.PackageId) && !x.JqlFilter.IsNullOrEmpty())
                .SelectMany(x => x.JqlFilter)
                .ToList();
            if (jqlScripts.IsNullOrEmpty())
                throw new MissingJqlConfigurationException(packageId);
            var result = SearchIssues(jqlScripts.Select(x => x.Value));

            return MappingEngine.Map<IEnumerable<Issue>, IEnumerable<ReleaseContentTicket>>(result)
                .Select(x =>
                {
                    x.TicketUrl = _configuration.GetPluginConfiguration().JiraBrowseUrl + x.TicketName;
                    return x;
                });
        }

        public IEnumerable<ReleaseContentTicket> GetDefectTickets(IEnumerable<Guid> packageId)
        {
            var jqlScripts = _packageConfigurations.GetPluginPackageConfiguration()
                .Where(x => packageId.Any(p => p == x.PackageId) && !x.DefectFilter.IsNullOrEmpty())
                .SelectMany(x => x.DefectFilter);

            var result = SearchIssues(jqlScripts.Select(x => x.Value));

            return MappingEngine.Map<IEnumerable<Issue>, IEnumerable<ReleaseContentTicket>>(result)
                .Select(x =>
                {
                    x.TicketUrl = _configuration.GetPluginConfiguration().JiraBrowseUrl + x.TicketName;
                    return x;
                });
        }

        public void UpdateTicket(IEnumerable<ReleaseContentTicket> tickets, Guid packageId)
        {
            var config = _packageConfigurations.GetPluginPackageConfigurationEntity(packageId);
            if (config == null) return;

            if (config.UpdateTicketMode == UpdateTicketMode.AddLabel)
            {
                AddLabel(tickets.Select(x => x.TicketName), config.Label);
            }
        }
    }
}
