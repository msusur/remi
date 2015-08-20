using System.Threading.Tasks;

namespace ReMi.Contracts.Cqrs.Commands
{
    public interface ICommandDispatcher
    {
        Task Send<TCommand>(TCommand command) where TCommand : class, ICommand;
    }
}
