using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Data.Authentication;
using ReMi.Contracts.Plugins.Services.Authentication;
using System.Collections.Generic;

namespace ReMi.Plugin.Composites.Services
{
    public class AuthenticationServiceComposite : BaseComposit<IAuthenticationService>, IAuthenticationService
    {
        public Account GetAccount(string userName, string password)
        {
            var service = GetPluginService(PluginType.Authentication);
            return service == null ? null : service.GetAccount(userName, password);
        }

        public List<Account> Search(object criteria)
        {
            var service = GetPluginService(PluginType.Authentication);
            return service == null ? null : service.Search(criteria);
        }
    }
}
