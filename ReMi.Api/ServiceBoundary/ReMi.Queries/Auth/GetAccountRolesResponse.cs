using System.Collections.Generic;
using ReMi.BusinessEntities;
using ReMi.Common.Utils;

namespace ReMi.Queries.Auth
{
    public class GetAccountRolesResponse
    {
        public IEnumerable<EnumEntry> Roles { get; set; }

        public override string ToString()
        {
            return string.Format("[Roles={0}]", Roles.FormatElements());
        }
    }
}
