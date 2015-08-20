using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.ReleaseExecution;
using ReMi.Common.Utils;

namespace ReMi.Queries.ReleaseExecution
{
    public class GetSignOffsResponse
    {
        public List<SignOff> SignOffs { get; set; }

        public override string ToString()
        {
            return String.Format("[SignOffs={0}]", SignOffs.FormatElements());
        }
    }
}
