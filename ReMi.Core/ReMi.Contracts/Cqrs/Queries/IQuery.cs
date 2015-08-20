namespace ReMi.Contracts.Cqrs.Queries
{
    public interface IQuery : IMessage
    {
        QueryContext Context { get; set; }
    }
}
