using System.Text.RegularExpressions;

namespace ReMi.Plugin.Go.BusinessLogic
{
    public static class RepositoryNameResolver
    {
        private const string GerritPattern = "^ssh://\\w+@gerrit\\..+:\\d+/(.*)$";
        private const string GitlabPattern = "^git2@git\\..+:(.*).git$";
        private const string GithubPattern = "^git@github.com:(.*).git$";

        private static readonly Regex GerritRegex = new Regex(GerritPattern);
        private static readonly Regex GitlabRegex = new Regex(GitlabPattern);
        private static readonly Regex GithubRegex = new Regex(GithubPattern);

        public static string Resolve(string repository)
        {
            if (GerritRegex.IsMatch(repository))
            {
                return GetMatchValue(GerritRegex.Match(repository));
            }
            if (GitlabRegex.IsMatch(repository))
            {
                return GetMatchValue(GitlabRegex.Match(repository));
            }
            if (GithubRegex.IsMatch(repository))
            {
                return GetMatchValue(GithubRegex.Match(repository));
            }

            return null;
        }

        private static string GetMatchValue(Match match)
        {
            return match.Groups.Count == 2 ? match.Groups[1].Value : null;
        }
    }
}
