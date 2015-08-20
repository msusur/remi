using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Api;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Api;
using ReMi.DataEntities.Api;

namespace ReMi.DataAccess.Tests.Api
{
    public class ApiDescriptionGatewayTests : TestClassFor<ApiDescriptionGateway>
    {
        private Mock<IRepository<Description>> _descriptionRepositoryMock;

        protected override ApiDescriptionGateway ConstructSystemUnderTest()
        {
            return new ApiDescriptionGateway
            {
                DescriptionRepository = _descriptionRepositoryMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _descriptionRepositoryMock = new Mock<IRepository<Description>>();

            base.TestInitialize();
        }

        [Test]
        public void GetApiDescription_ShouldReturnCorrectValue()
        {
            var description = new Description
            {
                Url = RandomData.RandomString(14),
                HttpMethod = RandomData.RandomString(4)
            };
            _descriptionRepositoryMock.SetupEntities(new[] {description});

            var result = Sut.GetApiDescriptions();

            Assert.AreEqual(1, result.Count, "list size");
            Assert.AreEqual(description.Url, result[0].Url, "url");
            Assert.AreEqual(description.HttpMethod, result[0].Method, "http method");
        }

        [Test]
        public void RemoveApiDescription_ShouldWorkCorrectly()
        {
            var description = new Description
            {
                Url = RandomData.RandomString(14),
                HttpMethod = RandomData.RandomString(4)
            };
            _descriptionRepositoryMock.SetupEntities(new[] { description });

            Sut.RemoveApiDescriptions(new List<ApiDescription>
            {
                new ApiDescription {Url = description.Url, Method = description.HttpMethod}
            });

            _descriptionRepositoryMock.Verify(
                x =>
                    x.Delete(It.Is<Description>(d => d.Url == description.Url && d.HttpMethod == description.HttpMethod)));
        }

        [Test]
        public void UpdateApiDescription_ShouldWorkCorrectly()
        {
            var description = new Description
            {
                Url = RandomData.RandomString(14),
                HttpMethod = RandomData.RandomString(4)
            };
            _descriptionRepositoryMock.SetupEntities(new[] { description });

            Sut.UpdateApiDescription(
                new ApiDescription
                {
                    Url = description.Url,
                    Method = description.HttpMethod,
                    Description = "desc"
                });

            _descriptionRepositoryMock.Verify(
                x =>
                    x.Update(
                        It.Is<Description>(
                            d =>
                                d.Url == description.Url && d.HttpMethod == description.HttpMethod &&
                                d.DescriptionText == "desc")));
        }

        [Test]
        public void CreateApiDescription_ShouldWorkCorrectly()
        {
            var description = new Description
            {
                Url = RandomData.RandomString(14),
                HttpMethod = RandomData.RandomString(4)
            };

            Sut.CreateApiDescriptions(new List<ApiDescription>
            {
                new ApiDescription {Url = description.Url, Method = description.HttpMethod}
            });

            _descriptionRepositoryMock.Verify(
                x =>
                    x.Insert(
                        It.Is<IEnumerable<Description>>(
                            d => d.Any(f => f.HttpMethod == description.HttpMethod && f.Url == description.Url))));
        }
    }
}
