using System;
using ReMi.BusinessEntities.ExecPoll;

namespace ReMi.BusinessLogic.ExecPoll
{
    public interface ICommandPollBusinessLogic
    {
        CommandState GetCommandExecutionState(Guid externalId);

        void StartCommandExecution(Guid externalId, string description);
        void SetCommandExecutionState(Guid externalId, CommandStateType state, string details);
    }
}
