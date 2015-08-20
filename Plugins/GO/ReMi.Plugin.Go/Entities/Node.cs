using System.Collections.Generic;

namespace ReMi.Plugin.Go.Entities
{
    public class Node
    {
        public string Name { get; set; }
        public NoteType Node_Type { get; set; }
        public short Depth { get; set; }
        public string Id { get; set; }
        public string[] Dependants { get; set; }
        public string[] Parents { get; set; }

        public List<Instance> Instances { get; set; }
    }
}
