using System;
using ReMi.BusinessEntities.ExecPoll;
using ReMi.DataAccess.BusinessEntityGateways;
using ReMi.DataAccess.Exceptions;

namespace ReMi.BusinessLogic.ExecPoll
{
    public class CommandPollBusinessLogic : ICommandPollBusinessLogic
    {
        public Func<ICommandPollGateway> CommandPollGatewayFactory { get; set; }

        public CommandState GetCommandExecutionState(Guid externalId)
        {
            using (ICommandPollGateway commandPollGateway = CommandPollGatewayFactory())
            {
                //check if Guid is unique
                var commandStage = commandPollGateway.GetByExternalId(externalId);
                if (commandStage == null)
                {
                    return new CommandState { StateType = CommandStateType.NotRegistered };
                }

                return new CommandState { StateType = commandStage.State, Details = commandStage.Details };
            }
        }

        public void StartCommandExecution(Guid externalId, string description)
        {
            using (ICommandPollGateway commandPollGateway = CommandPollGatewayFactory())
            {
                //check if Guid is unique
                var command = commandPollGateway.GetByExternalId(externalId);
                if (command != null)
                {
                    throw new CommandDuplicationException(externalId);
                }

                commandPollGateway.Create(externalId, description);
            }
        }

        public void SetCommandExecutionState(Guid externalId, CommandStateType state, string details)
        {
            using (ICommandPollGateway commandPollGateway = CommandPollGatewayFactory())
            {
                //check if Guid is unique
                var command = commandPollGateway.GetByExternalId(externalId);
                if (command == null)
                {
                    throw new CommandExecutionNotFoundException(externalId);
                }

                commandPollGateway.SetState(externalId, state, details);
            }
        }
    }
}
