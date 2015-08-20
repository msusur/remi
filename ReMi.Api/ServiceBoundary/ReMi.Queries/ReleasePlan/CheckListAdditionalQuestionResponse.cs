using ReMi.BusinessEntities.ReleasePlan;
using ReMi.Common.Utils;
using System;
using System.Collections.Generic;

namespace ReMi.Queries.ReleasePlan
{
    public class CheckListAdditionalQuestionResponse
    {
        public List<CheckListQuestion> Questions { get; set; }

        public override string ToString()
        {
            return String.Format("[Queations={0}]", Questions.FormatElements());
        }
    }
}
