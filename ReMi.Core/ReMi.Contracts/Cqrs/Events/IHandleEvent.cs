namespace ReMi.Contracts.Cqrs.Events
{
    public interface IHandleEvent<in TEvent> where TEvent : IEvent
    {
        void Handle(TEvent evnt);
    }
}
