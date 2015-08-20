using System.Collections.Generic;

namespace ReMi.Plugin.Go.Entities
{
    public class StageDetails
    {
        public string Result { get; set; }
        public string State { get; set; }

        public List<JobRef> Jobs { get; set; }
    }
}
