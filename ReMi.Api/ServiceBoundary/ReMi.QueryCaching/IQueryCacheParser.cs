using ReMi.Contracts.Cqrs.Queries;

namespace ReMi.QueryCaching
{
    public interface IQueryCacheParser
    {
        TResponse Parse<TRequest, TResponse>(TResponse queryRequest, byte[] value)
            where TRequest : IQuery
            where TResponse : class;

        object Parse(IQuery queryRequest, byte[] value);
    }
}
