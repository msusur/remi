using System;

namespace ReMi.BusinessEntities.ReleasePlan
{
    public class CheckListItemView
    {
        public Guid ExternalId { get; set; }
        public Guid ReleaseWindowId { get; set; }
        public string CheckListQuestion { get; set; }
        public bool Checked { get; set; }
        public string Comment { get; set; }
        public string LastChangedBy { get; set; }
    }
}
