using System.Threading.Tasks;

namespace ReMi.Contracts.Cqrs.Events
{
    public interface IPublishEvent
    {
        Task[] Publish<T>(T evnt) where T : IEvent;
    }
}
