using System;
using ReMi.Plugin.Gerrit.GerritApi.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;

namespace ReMi.Plugin.Gerrit.GerritApi
{
    public static class GitLogParser
    {
        private const string SingleLogRegex =
            "([0-9a-f]*)<sep>" +
            "([\\s0-9a-f]*)<sep>" +
            "([a-zA-Z_@\\-\\s\\.]*)<sep>" +
            "(\\d{4}-\\d{2}-\\d{2}\\s\\d{2}:\\d{2}:\\d{2}\\s\\+\\d{4})<sep>" +
            "(.*)<sep>" +
            "(.*)<sep>" +
            "(.*)";
        private const string JiraTicketRegex = "([A-Z]+-[0-9]+)*";
        private const string ChangeIdRegex = "Change-Id:\\s([I0-9a-f]*)";

        public static IEnumerable<GitLogEntity> Parse(string logs)
        {
            if (string.IsNullOrEmpty(logs))
                return null;

            return logs.Split(new[] { "<end>" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => Regex.Matches(x, SingleLogRegex, RegexOptions.Singleline).Cast<Match>().ToArray())
                .Where(x => x.Any() && x.All(m => m.Success && m.Groups.Count == 8))
                .Select(x =>
                {
                    var match = x.First();
                    return new GitLogEntity
                    {
                        Hash = match.Groups[1].Value,
                        CommitType = match.Groups[2].Value.Contains(" ") ? CommitType.Merge : CommitType.Typical,
                        Author = match.Groups[3].Value,
                        Date = DateTime.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture),
                        Reference = match.Groups[5].Value,
                        Subject = match.Groups[6].Value,
                        JiraTickets = Regex.Matches(match.Groups[6].Value, JiraTicketRegex).Cast<Match>()
                            .Where(m => m.Success && m.Groups.Count >= 2 && !string.IsNullOrWhiteSpace(m.Groups[1].Value))
                            .Select(m => m.Groups[1].Value).ToArray(),
                        ChangeId = Regex.Matches(match.Groups[7].Value, ChangeIdRegex, RegexOptions.Singleline).Cast<Match>()
                            .Where(m => m.Success && m.Groups.Count >= 2 && !string.IsNullOrWhiteSpace(m.Groups[1].Value))
                            .Select(m => m.Groups[1].Value).FirstOrDefault()
                    };
                })
                .ToArray();
        }
    }
}
