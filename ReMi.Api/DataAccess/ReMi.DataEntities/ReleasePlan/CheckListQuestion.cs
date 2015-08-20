using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReMi.DataEntities.ReleasePlan
{
    public class CheckListQuestion
    {
        public int CheckListQuestionId { get; set; }

        [Index(IsUnique = true)]
        public Guid ExternalId { get; set; }

        public string Content { get; set; }


        public virtual ICollection<CheckListQuestionToProduct> CheckListQuestionsToProducts { get; set; }

        public virtual ICollection<CheckList> CheckLists { get; set; }
    }
}
