using ReMi.BusinessEntities.ReleasePlan;
using System;
using System.Collections.Generic;
using ReMi.Common.Utils;

namespace ReMi.Queries.ReleasePlan
{
    public class GetReleaseContentInformationResponse
    {
        public IEnumerable<ReleaseContentTicket> Content { get; set; }

        public override string ToString()
        {
            return String.Format("[Content={0}]", Content.FormatElements());
        }
    }
}
