using System;
using ReMi.BusinessEntities.Auth;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Auth
{
    [Command("Set default package for new user", CommandGroup.Configuration)]
    public class SetDefaultProductForNewlyRegisteredUserCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public Account Account { get; set; }

        public override string ToString()
        {
            return String.Format("[Account={0}, CommandContext={1}]", Account, CommandContext);
        }
    }
}
