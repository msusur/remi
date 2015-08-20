using System;
using ReMi.BusinessEntities.Evt;

namespace ReMi.DataAccess.BusinessEntityGateways.Evt
{
    public interface IEventGateway : IDisposable
	{
		Event GetByExternalId(Guid externalId);

        void Create(Guid externalId, string description, string data, string handler);
        void SetState(Guid externalId, EventStateType state, string details);
	}
}
