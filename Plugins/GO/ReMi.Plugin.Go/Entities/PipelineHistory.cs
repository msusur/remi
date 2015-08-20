
using System.Collections.Generic;

namespace ReMi.Plugin.Go.Entities
{
    public class PipelineHistory
    {
        public string PipelineId { get; set; }

        public string Label { get; set; }
        public string CounterOrLabel { get; set; }
        public string Scheduled_Date { get; set; }
        public string BuildCausedBy { get; set; }
        public string Modification_Date { get; set; }
        public List<MaterialRevision> MaterialRevisions { get; set; }
        public List<Stage> Stages { get; set; }

    }
}
