namespace ReMi.Contracts.Cqrs.Events
{
    public interface IEvent : IMessage
    {
        EventContext Context { get; set; }
    }
}
