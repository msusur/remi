using System.Collections.Generic;

namespace ReMi.Plugin.Go.Entities
{
    public class Instance
    {
        // GIT
        public string Modified_Time { get; set; }
        public string User { get; set; }
        public string Comment { get; set; }
        public string Revision { get; set; }

        // PIPELINE
        public string Locator { get; set; }
        public int Counter { get; set; }
        public string Label { get; set; }
        public List<StageInfo> Stages { get; set; }
    }
}
