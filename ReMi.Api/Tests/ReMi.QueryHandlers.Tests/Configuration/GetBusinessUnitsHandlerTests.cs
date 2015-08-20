using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.Products;
using ReMi.Common.Utils;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Cqrs.Queries;
using ReMi.DataAccess.BusinessEntityGateways.Products;
using ReMi.Queries.Configuration;
using ReMi.QueryHandlers.Configuration;

namespace ReMi.QueryHandlers.Tests.Configuration
{
    public class GetBusinessUnitsHandlerTests : TestClassFor<GetBusinessUnitsHandler>
    {
        private Mock<IBusinessUnitsGateway> _businessUnitGatewayMock;
        private Mock<IMappingEngine> _mappingEngineMock;

        protected override GetBusinessUnitsHandler ConstructSystemUnderTest()
        {
            return new GetBusinessUnitsHandler
            {
                BusinessUnitGatewayFactory = () => _businessUnitGatewayMock.Object,
                MappingEngine = _mappingEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _businessUnitGatewayMock = new Mock<IBusinessUnitsGateway>(MockBehavior.Strict);
            _mappingEngineMock = new Mock<IMappingEngine>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        public void Handle_ShouldReturnBusinessUnitsWithPackagesAndDefaultPackage_WhenDefaultExists()
        {
            var businessUnites = new List<BusinessUnit>
            {
                new BusinessUnit {ExternalId = Guid.NewGuid(), Description = "2"},
                new BusinessUnit {ExternalId = Guid.NewGuid(), Description = "1"},
                new BusinessUnit {ExternalId = Guid.NewGuid(), Description = "3"},
                new BusinessUnit {ExternalId = Guid.NewGuid(), Description = "4"}
            };
            var packages = new List<Product>
            {
                new Product { ExternalId = Guid.NewGuid(), Description = "2.5", BusinessUnit = businessUnites[0] },
                new Product { ExternalId = Guid.NewGuid(), Description = "1.2", BusinessUnit = businessUnites[1] },
                new Product { ExternalId = Guid.NewGuid(), Description = "2.3", BusinessUnit = businessUnites[0] },
                new Product { ExternalId = Guid.NewGuid(), Description = "1.1", BusinessUnit = businessUnites[1] },
                new Product { ExternalId = Guid.NewGuid(), Description = "2.4", BusinessUnit = businessUnites[0] },
                new Product { ExternalId = Guid.NewGuid(), Description = "3.2", BusinessUnit = businessUnites[2] },
                new Product { ExternalId = Guid.NewGuid(), Description = "2.1", BusinessUnit = businessUnites[0] },
                new Product { ExternalId = Guid.NewGuid(), Description = "2.2", BusinessUnit = businessUnites[0] },
                new Product { ExternalId = Guid.NewGuid(), Description = "3.1", BusinessUnit = businessUnites[2] },
                new Product { ExternalId = Guid.NewGuid(), Description = "3.3", BusinessUnit = businessUnites[2] }
            };
            var defaultPackage = packages.First();
            var request = new GetBusinessUnitsRequest
            {
                Context = new QueryContext
                {
                    UserId = Guid.NewGuid(),
                    UserRole = RandomData.RandomString(10)
                },
                IncludeAll = true
            };

            _businessUnitGatewayMock.Setup(x => x.GetDefaultPackage(request.Context.UserId))
                .Returns(defaultPackage);
            _businessUnitGatewayMock.Setup(x => x.GetPackages(request.Context.UserId, true))
                .Returns(packages);
            _businessUnitGatewayMock.Setup(x => x.GetEmptyBusinessUnits())
                .Returns(new[] { businessUnites[3] });
            _mappingEngineMock.Setup(
                x => x.Map<BusinessUnit, BusinessUnitView>(It.Is<BusinessUnit>(b => businessUnites.Contains(b))))
                .Returns(
                    (BusinessUnit b) =>
                        new BusinessUnitView
                        {
                            ExternalId = b.ExternalId,
                            Description = b.Description,
                            Packages = b.Packages.Select(p => new ProductView { ExternalId = p.ExternalId, Name = p.Description }).ToList()
                        });
            _mappingEngineMock.Setup(
                x => x.Map<IEnumerable<BusinessUnit>, IEnumerable<BusinessUnitView>>(It.IsAny<IEnumerable<BusinessUnit>>()))
                .Returns(
                    (IEnumerable<BusinessUnit> list) => list.Select(b =>
                        new BusinessUnitView
                        {
                            ExternalId = b.ExternalId,
                            Description = b.Description
                        }));
            _businessUnitGatewayMock.Setup(x => x.Dispose());

            var response = Sut.Handle(request);

            Assert.AreEqual("1", response.BusinessUnits.ElementAt(0).Description, "Wrong order of elements");
            Assert.AreEqual("2", response.BusinessUnits.ElementAt(1).Description, "Wrong order of elements");
            Assert.AreEqual("3", response.BusinessUnits.ElementAt(2).Description, "Wrong order of elements");
            Assert.AreEqual("4", response.BusinessUnits.ElementAt(3).Description, "Wrong order of elements");
            Assert.AreEqual(2, response.BusinessUnits.ElementAt(0).Packages.Count(), "Incorrect packages count");
            Assert.AreEqual(5, response.BusinessUnits.ElementAt(1).Packages.Count(), "Incorrect packages count");
            Assert.AreEqual(3, response.BusinessUnits.ElementAt(2).Packages.Count(), "Incorrect packages count");
            Assert.AreEqual("2.1", response.BusinessUnits.ElementAt(1).Packages.First().Name, "Incorrect packages count");
            Assert.IsTrue(response.BusinessUnits.All(x => businessUnites.Any(b => b.ExternalId == x.ExternalId)), "Incorrect ExternalId in business unit");
            Assert.IsTrue(response.BusinessUnits.Any(b => b.Packages.Any(p => p.IsDefault)), "Missing default account package");
        }

        [Test]
        public void Handle_ShouldReturnBusinessUnitsWithPackagesAndWithoutDefault_WhenDefaultNoneExists()
        {
            var businessUnites = Builder<BusinessUnit>.CreateListOfSize(3)
                .All()
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Do(x => x.Description = RandomData.RandomString(10))
                .Build();
            var packages = Builder<Product>.CreateListOfSize(10)
                .All()
                .Do(x => x.ExternalId = Guid.NewGuid())
                .Do(x => x.Description = RandomData.RandomString(10))
                .Do(x => x.BusinessUnit = businessUnites[RandomData.RandomInt(0, 3)])
                .Build();
            var request = new GetBusinessUnitsRequest
            {
                Context = new QueryContext
                {
                    UserId = Guid.NewGuid(),
                    UserRole = RandomData.RandomString(10)
                }
            };

            _businessUnitGatewayMock.Setup(x => x.GetDefaultPackage(request.Context.UserId))
                .Returns((Product)null);
            _businessUnitGatewayMock.Setup(x => x.GetPackages(request.Context.UserId, false))
                .Returns(packages);
            _businessUnitGatewayMock.Setup(x => x.GetEmptyBusinessUnits())
                .Returns((IEnumerable<BusinessUnit>)null);
            _mappingEngineMock.Setup(
                x => x.Map<BusinessUnit, BusinessUnitView>(It.Is<BusinessUnit>(b => businessUnites.Contains(b))))
                .Returns((BusinessUnit b) => new BusinessUnitView
                        {
                            ExternalId = b.ExternalId,
                            Packages = b.Packages.Select(p => new ProductView { ExternalId = p.ExternalId })
                        });
            _mappingEngineMock.Setup(
                x => x.Map<IEnumerable<BusinessUnit>, IEnumerable<BusinessUnitView>>(It.IsAny<IEnumerable<BusinessUnit>>()))
                .Returns(
                    (IEnumerable<BusinessUnit> list) => list.Select(b =>
                        new BusinessUnitView
                        {
                            ExternalId = b.ExternalId,
                            Description = b.Description
                        }));
            _businessUnitGatewayMock.Setup(x => x.Dispose());

            var response = Sut.Handle(request);

            Assert.IsTrue(response.BusinessUnits.All(x => businessUnites.Any(b => b.ExternalId == x.ExternalId)), "Incorrect ExternalId in business unit");
            Assert.AreEqual(10, response.BusinessUnits.Sum(x => x.Packages.Count()), "Incorrect count of total amount of packages");
            Assert.IsFalse(response.BusinessUnits.Any(b => b.Packages.Any(p => p.IsDefault)), "Default should not exists");
        }

        [Test]
        public void Handle_ShouldReturnEmptyArray_WhenUserIsNotLoggedIn()
        {
            var request = new GetBusinessUnitsRequest
            {
                Context = new QueryContext()
            };

            var response = Sut.Handle(request);

            Assert.IsTrue(response.BusinessUnits.IsNullOrEmpty());
        }

        [Test]
        public void Handle_ShouldReturnEmptyArray_WhenUserIsNotAuthenticated()
        {
            var request = new GetBusinessUnitsRequest
            {
                Context = new QueryContext
                {
                    UserId = Guid.NewGuid(),
                    UserRole = "NotAuthenticated"
                }
            };

            var response = Sut.Handle(request);

            Assert.IsTrue(response.BusinessUnits.IsNullOrEmpty());
        }
    }
}
