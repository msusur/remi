using System;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Constants.Auth;

namespace ReMi.Api.Insfrastructure.Security
{
    public interface IPermissionChecker
    {
        PermissionStatus CheckCommandPermission(Type commandType, Account account);
        PermissionStatus CheckQueryPermission(Type queryType, Account account);

        PermissionStatus CheckRule(Account account, object data);
    }
}
