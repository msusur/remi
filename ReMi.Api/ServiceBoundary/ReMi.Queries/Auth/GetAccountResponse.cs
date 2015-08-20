using System;
using ReMi.BusinessEntities.Auth;

namespace ReMi.Queries.Auth
{
    public class GetAccountResponse
    {
        public Account Account { get; set; }

        public override string ToString()
        {
            return String.Format("[Account={0}]", Account);
        }
    }
}
