using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data.Authentication;

namespace ReMi.Contracts.Plugins.Services.Authentication
{
    public interface IAuthenticationService : IPluginService
    {
        Account GetAccount(string userName, string password);
        List<Account> Search(object criteria);
    }
}
