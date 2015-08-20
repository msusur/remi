using ReMi.Common.Constants;
using System.Collections.Generic;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Enums;

namespace ReMi.Queries.Common
{
    public class GetEnumsResponse
    {
        public IDictionary<string, IEnumerable<EnumDescription>> Enums { get; set; }

        public override string ToString()
        {
            return string.Format("[Enums={0}]", Enums.FormatElements());
        }
    }
}
