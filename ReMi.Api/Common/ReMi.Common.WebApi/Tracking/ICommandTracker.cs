using ReMi.Contracts.Cqrs.Commands;

namespace ReMi.Common.WebApi.Tracking
{
    public interface ICommandTracker : ITracker<ICommand>
	{
	}
}
