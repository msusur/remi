using System.Linq;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleasePlan;

namespace ReMi.Queries.ReleasePlan
{
    public class GetCheckListResponse
    {
        public IEnumerable<CheckListItemView> CheckList { get; set; }

        public override string ToString()
        {
            return string.Format("[CheckList lenght={0}]", CheckList.Count());
        }
    }
}
