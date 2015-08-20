using System;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Common.WebApi.Tracking
{
    public interface IEventTracker : ITracker<IEvent>
	{
        void CreateTracker(Guid externalId, string description, IEvent evnt, string handler);
	}
}
