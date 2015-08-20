using System;

namespace ReMi.Contracts.Plugins.Data.SourceControl
{
    public class SourceControlChange
    {
        public string Identifier { get; set; }
        public string Owner { get; set; }
        public DateTime? Date { get; set; }
        public string Repository { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return string.Format("[Identifier={0}, Owner={1}, Date={2}, Repository={3}, Description={4}]",
                Identifier, Owner, Date, Repository, Description);
        }
    }
}
