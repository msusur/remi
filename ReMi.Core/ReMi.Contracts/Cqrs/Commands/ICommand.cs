namespace ReMi.Contracts.Cqrs.Commands
{
    public interface ICommand : IMessage
    {
        CommandContext CommandContext { get; set; }
    }
}
