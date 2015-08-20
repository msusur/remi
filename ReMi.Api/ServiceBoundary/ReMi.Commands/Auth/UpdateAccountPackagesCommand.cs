using System;
using System.Collections.Generic;
using ReMi.Common.Utils;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Auth
{
    [Command("Update Account Packages", CommandGroup.AccessControl)]
    public class UpdateAccountPackagesCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }

        public Guid AccountId { get; set; }
        public IEnumerable<Guid> PackageIds { get; set; }
        public Guid DefaultPackageId { get; set; }

        public override string ToString()
        {
            return string.Format("[AccountId = {0}, PackageIds={1}, DefaultPackageId={2}]",
                AccountId, PackageIds.FormatElements(), DefaultPackageId);
        }
    }
}
