using ReMi.BusinessEntities.ExecPoll;
using ReMi.BusinessLogic.ExecPoll;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.Queries.ExecPoll;

namespace ReMi.QueryHandlers.ExecPoll
{
    public class GetCommandStateHandler : IHandleQuery<GetCommandStateRequest, GetCommandStateResponse>
    {
        public ICommandPollBusinessLogic CommandPollBusinessLogic { get; set; }

        public GetCommandStateResponse Handle(GetCommandStateRequest request)
        {
            CommandState state = CommandPollBusinessLogic.GetCommandExecutionState(request.ExternalId);

            return new GetCommandStateResponse { State = state.StateType, Details = state.Details };
        }
    }
}
