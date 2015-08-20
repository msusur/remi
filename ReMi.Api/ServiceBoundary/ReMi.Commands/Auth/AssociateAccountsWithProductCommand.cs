using System;
using System.Collections.Generic;
using ReMi.BusinessEntities.Auth;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Auth
{
    [Command("Assign Account to Package", CommandGroup.AccessControl)]
    public class AssociateAccountsWithProductCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public IEnumerable<Account> Accounts { get; set; }
        public Guid ReleaseWindowId { get; set; }

        public override string ToString()
        {
            return String.Format("[Approvers={0},ReleaseWindowId={1},CommandContext]={2}", Accounts.FormatElements(), ReleaseWindowId,
                CommandContext);
        }
    }
}
