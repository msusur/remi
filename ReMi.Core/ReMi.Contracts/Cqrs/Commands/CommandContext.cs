namespace ReMi.Contracts.Cqrs.Commands
{
    public class CommandContext : BaseContext
    {
        public bool IsSynchronous { get; set; }
    }
}
