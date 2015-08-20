using System;
using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Commands.Auth
{
    [Command("Start new user session", CommandGroup.AccessControl)]
    public class StartSessionCommand : ICommand
    {
        public CommandContext CommandContext { get; set; }
        public Guid SessionId { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public override string ToString()
        {
            return string.Format("Login = {0}, Password = {1}, SessionId={2}",
                Login, new string('*', (Password ?? string.Empty).Length), SessionId);
        }
    }
}
