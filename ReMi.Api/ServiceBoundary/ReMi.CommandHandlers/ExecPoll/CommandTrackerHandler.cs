using System;
using ReMi.BusinessEntities.ExecPoll;
using ReMi.BusinessLogic.ExecPoll;
using ReMi.Common.WebApi.Tracking;

namespace ReMi.CommandHandlers.ExecPoll
{
    public class CommandTrackerHandler : ICommandTracker
    {
        private readonly ICommandPollBusinessLogic _pollLogic;

        #region .ctor

        public CommandTrackerHandler(ICommandPollBusinessLogic pollLogic)
        {
            _pollLogic = pollLogic;
        }

        #endregion

        public void CreateTracker(Guid commandId, string description)
        {
            ICommandPollBusinessLogic pollLogic = _pollLogic;

            pollLogic.StartCommandExecution(commandId, description);
        }

        public void Started(Guid commandId)
        {
            ICommandPollBusinessLogic pollLogic = _pollLogic;

            pollLogic.SetCommandExecutionState(commandId, CommandStateType.Running, null);
        }

        public void Finished(Guid commandId, string error = null)
        {
            ICommandPollBusinessLogic pollLogic = _pollLogic;

            pollLogic.SetCommandExecutionState(
                commandId,
                !string.IsNullOrWhiteSpace(error) ? CommandStateType.Failed : CommandStateType.Success,
                error
            );
        }
    }
}
