
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils;

namespace ReMi.Queries.Auth
{
    public class GetAccountsByRoleResponse
    {
        public List<Account> Accounts { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}]", Accounts.FormatElements());
        }
    }
} 
