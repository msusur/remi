using System.Collections.Generic;
using System.Linq;
using Moq;
using ReMi.Common.Utils.Repository;

namespace ReMi.TestUtils.UnitTests
{
    public static class RepositoryMockExtensions
    {
		public static void SetupEntities<TEntity>(this Mock<IRepository<TEntity>> repositoryMock, IEnumerable<TEntity> objects) where TEntity : class
		{
			repositoryMock.SetupGet(repository => repository.Entities)
				.Returns(objects.AsQueryable());
		}
    }
}
