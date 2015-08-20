
namespace ReMi.Contracts.Cqrs.Commands
{
	public interface IHandleCommand<TRequest> where TRequest : ICommand
	{
		void Handle(TRequest command);
	}
}
