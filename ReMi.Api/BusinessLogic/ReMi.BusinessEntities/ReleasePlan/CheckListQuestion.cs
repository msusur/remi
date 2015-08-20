using System;

namespace ReMi.BusinessEntities.ReleasePlan
{
    public class CheckListQuestion
    {
        public Guid ExternalId { get; set; }
        public Guid CheckListId { get; set; }
        public string Question { get; set; }

        public override string ToString()
        {
            return
                String.Format("[ExternalId={0}, CheckListId={1}, Question={2}]",
                    ExternalId, CheckListId, Question);
        }
    }
}
