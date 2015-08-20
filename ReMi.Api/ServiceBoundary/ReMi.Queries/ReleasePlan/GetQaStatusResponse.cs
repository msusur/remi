
using ReMi.Common.Utils;
using ReMi.Contracts.Plugins.Data.QaStats;
using System;
using System.Collections.Generic;

namespace ReMi.Queries.ReleasePlan
{
    public class GetQaStatusResponse
    {
        public IEnumerable<QaStatusCheckItem> Content { get; set; }

        public override string ToString()
        {
            return String.Format("[Content={0}]", Content.FormatElements());
        }
    }
}
