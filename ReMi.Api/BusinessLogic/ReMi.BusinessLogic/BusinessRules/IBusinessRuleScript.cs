using ReMi.BusinessEntities.Auth;
using System.Collections.Generic;

namespace ReMi.BusinessLogic.BusinessRules
{
    public interface IBusinessRuleScript
    {
        object Execute(Account account, IDictionary<string, object> parameters);
    }
}
