using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Auth
{
    [Command("Check Account", CommandGroup.AccessControl)]
    public class CheckAccountsCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid ReleaseWindowId { get; set; }

        public IEnumerable<Account> Accounts { get; set; }

        public override string ToString()
        {
            return string.Format("[ReleaseWindowId={0}, Approvers={1}]", ReleaseWindowId, Accounts.FormatElements());
        }
    }
}
