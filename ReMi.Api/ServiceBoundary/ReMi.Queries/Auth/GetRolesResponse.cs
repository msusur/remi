using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils;

namespace ReMi.Queries.Auth
{
    public class GetRolesResponse
    {
        public IEnumerable<Role> Roles { get; set; }

        public override string ToString()
        {
            return string.Format("[Roles={0}]", Roles.FormatElements());
        }
    }
}
