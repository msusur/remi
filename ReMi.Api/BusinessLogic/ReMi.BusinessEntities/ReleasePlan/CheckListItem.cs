using System;

namespace ReMi.BusinessEntities.ReleasePlan
{
    public class CheckListItem
    {
        public Guid ExternalId { get; set; }
        public string Comment { get; set; }
        public bool Checked { get; set; }
        public string LastChangedBy { get; set; }
    }
}
