using System;
using ReMi.BusinessEntities.ExecPoll;

namespace ReMi.DataAccess.BusinessEntityGateways
{
    public interface ICommandPollGateway : IDisposable
	{
		CommandExecution GetByExternalId(Guid externalId);

        void Create(Guid externalId, string description);
		void SetState(Guid externalId, CommandStateType state, string description);
	}
}
