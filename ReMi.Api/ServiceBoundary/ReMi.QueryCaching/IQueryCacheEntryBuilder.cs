using ReMi.Contracts.Cqrs.Queries;
using ReMi.Contracts.Plugins.Data.CacheService;

namespace ReMi.QueryCaching
{
    public interface IQueryCacheEntryBuilder
    {
        CacheEntry Build<TRequest, TResponse>(TRequest queryRequest, TResponse queryResponse)
            where TRequest : IQuery
            where TResponse : class;

        CacheEntry Build(IQuery queryRequest, object queryResponse);
    }
}
