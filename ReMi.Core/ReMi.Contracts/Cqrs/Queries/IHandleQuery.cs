
namespace ReMi.Contracts.Cqrs.Queries
{
	public interface IHandleQuery<in TRequest, out TResponse> where TRequest : IQuery
	{
		TResponse Handle(TRequest request);
	}
}
