using System;
using System.Text.RegularExpressions;

namespace ReMi.Plugin.Go.Entities
{
    public class MaterialRevision
    {
        public string Revision { get; set; }

        public DateTime Date { get; set; }

        public string MaterialName { get; set; }

        public int Counter
        {
            get
            {
                var match = Regex.Match(Revision, "^[a-zA-Z]+/(\\d+)/");
                return match.Success && match.Groups.Count == 2 ? int.Parse(match.Groups[1].Value) : -1;
            }
        }

        public string PipelineName
        {
            get
            {
                var match = Regex.Match(Revision, "^([a-zA-Z]+)/\\d+/");
                return match.Success && match.Groups.Count == 2 ? match.Groups[1].Value : null;
            }
        }
    }
}
