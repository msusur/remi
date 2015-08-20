using ReMi.Contracts.Cqrs.Events;

namespace ReMi.Common.WebApi.Notifications
{
    public interface IFrontendNotificator
    {
        void Register(string token);

        void UnRegister(string token);

        void Subscribe(string eventName, string filterJson);

        void Unsubscribe(string eventName);

        void Unsubscribe(string eventName, string filterJson);

        void NotifyFiltered<T>(T evnt) where T : IEvent;

        void NotifyGlobal<T>(T evnt) where T : IEvent;
    }
}
