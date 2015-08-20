using ReMi.Common.Cqrs;
using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Api.Insfrastructure.Notifications
{
    public interface ISubscriptionManager
    {
        void Register(string connectionId, string token);
        
        void Subscribe(string connectionId, string eventName, string filter);
        void Unsubscribe(string connectionId, string eventName);
        void Unsubscribe(string connectionId, string eventName, string filter);

        void UnRegisterByToken(string token);
        void UnRegisterByConnectionId(string connectionId);

        bool IsRegistered(string token);
        bool IsRegisteredConnectionId(string connectionId);

        string[] FilterSubscribers(IEvent evnt);
    }
}
