
namespace ReMi.Api.Insfrastructure
{
    public interface IClientRequestInfoRetriever
    {
        string UserHostAddress { get; }
        string UserHostName { get; }
    }
}
