using System.Collections.Generic;
using ReMi.BusinessEntities;
using ReMi.Common.Utils;

namespace ReMi.Queries.ReleasePlan
{
    public class GetTicketRiskResponse
    {
        public IEnumerable<EnumEntry> TicketRisk { get; set; }

        public override string ToString()
        {
            return string.Format("[TicketRisk = {0}]", TicketRisk.FormatElements());
        }
    }
}
