using Common.Logging;
using ReMi.Api.Insfrastructure.Security;
using ReMi.Common.Utils;
using ReMi.Common.WebApi.Exceptions;
using ReMi.Common.WebApi.Tracking;
using ReMi.Contracts.Cqrs.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;

namespace ReMi.Api.Insfrastructure.Commands
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

        public IDependencyResolver DependencyResolver { get; set; }
        public ICommandTracker CommandTracker { get; set; }
        public ISerialization Serialization { get; set; }
        public IApplicationSettings ApplicationSettings { get; set; }

        public Task Send<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            if (command == null)
                throw new ArgumentNullException("command", "Cannot send empty command");

            if (command.CommandContext == null)
            {
                var principal = Thread.CurrentPrincipal as RequestPrincipal;
                if (principal == null)
                    throw new Exception("Couldn't populate empty command context");

                command.CommandContext = new CommandContext
                {
                    Id = Guid.NewGuid(),
                    UserEmail = principal.Account != null ? principal.Account.Email : string.Empty,
                    UserName = principal.Account != null ? principal.Account.FullName : string.Empty,
                    UserRole = principal.Account != null ? principal.Account.Role.Name : string.Empty,
                    UserId = principal.Account != null ? principal.Account.ExternalId : Guid.Empty
                };
            }

            var commandHandler = (IHandleCommand<TCommand>)DependencyResolver
                .GetService(typeof(IHandleCommand<>).MakeGenericType(command.GetType()));

            Logger.InfoFormat("Command '{0}' was handled: {1}", typeof(TCommand).Name, FormatLogHelper.FormatEntry(Serialization, ApplicationSettings, command));


            if (CommandTracker == null)
                throw new TrackerNotImplementedException<ICommandTracker>();
            CommandTracker.CreateTracker(command.CommandContext.Id, typeof(TCommand).Name);

            if (!command.CommandContext.IsSynchronous)
                return new TaskFactory().StartNew(() => InvokeHandle(command, commandHandler));

            InvokeHandle(command, commandHandler);
            return null;
        }

        private void InvokeHandle<TCommand>(TCommand command, IHandleCommand<TCommand> handler) where TCommand : class, ICommand
        {
            if (handler == null)
                throw new CommandHandlerNotImplementedException<TCommand>();

            try
            {
                CommandTracker.Started(command.CommandContext.Id);

                handler.Handle(command);

                CommandTracker.Finished(command.CommandContext.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                if (ex is ApplicationException)
                    CommandTracker.Finished(command.CommandContext.Id, (ex as ApplicationException).Message);
                else
                    CommandTracker.Finished(command.CommandContext.Id, "Request failed!");

                if (command.CommandContext.IsSynchronous)
                {
                    throw;
                }
            }
        }
    }
}
