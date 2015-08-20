using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils;

namespace ReMi.Queries.Auth
{
    public class GetAccountsByProductResponse
    {
        public IEnumerable<Account> Accounts { get; set; }

        public override string ToString()
        {
            return string.Format("[Accounts={0}]", Accounts.FormatElements());
        }
    }
}
