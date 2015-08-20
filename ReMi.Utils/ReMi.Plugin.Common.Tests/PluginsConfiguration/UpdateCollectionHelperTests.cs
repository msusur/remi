using System;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Common.Utils.Repository;
using ReMi.Plugin.Common.PluginsConfiguration;

namespace ReMi.Plugin.Common.Tests.PluginsConfiguration
{
    [TestFixture]
    public class UpdateCollectionHelperTests
    {
        public class TestDataClass
        {
            public int Id { get; set; }
            public int ExternalId { get; set; }
        }
        public class TestBusinessClass
        {
            public int ExternalId { get; set; }
        }
        [Test]
        public void UpdateCollection_ShouldInsertUpdateAndRemoveItems_WhenCalled()
        {
            var dataCollection = Builder<TestDataClass>.CreateListOfSize(5).Build();
            var businessCollection = Builder<TestBusinessClass>.CreateListOfSize(7).Build();
            dataCollection[0].ExternalId = 100;
            var repositoryMock = new Mock<IRepository<TestDataClass>>(MockBehavior.Strict);
            var mapperMock = new Mock<IMappingEngine>(MockBehavior.Strict);
            Func<TestDataClass, TestBusinessClass, bool> comparer = (td, tb) => td.ExternalId == tb.ExternalId;
            Action<TestDataClass> assingId = t => t.Id = 666;


            mapperMock.Setup(x => x.Map(businessCollection[1], dataCollection[1])).Returns((TestDataClass)null);
            mapperMock.Setup(x => x.Map(businessCollection[2], dataCollection[2])).Returns((TestDataClass)null);
            mapperMock.Setup(x => x.Map(businessCollection[3], dataCollection[3])).Returns((TestDataClass)null);
            mapperMock.Setup(x => x.Map(businessCollection[4], dataCollection[4])).Returns((TestDataClass)null);
            repositoryMock.Setup(x => x.Update(dataCollection[1])).Returns((ChangedFields<TestDataClass>)null);
            repositoryMock.Setup(x => x.Update(dataCollection[2])).Returns((ChangedFields<TestDataClass>)null);
            repositoryMock.Setup(x => x.Update(dataCollection[3])).Returns((ChangedFields<TestDataClass>)null);
            repositoryMock.Setup(x => x.Update(dataCollection[4])).Returns((ChangedFields<TestDataClass>)null);

            repositoryMock.Setup(x => x.Delete(dataCollection[0]));

            mapperMock.Setup(x => x.Map<TestBusinessClass, TestDataClass>(businessCollection[0]))
                .Returns((TestBusinessClass x) => new TestDataClass { ExternalId = x.ExternalId });
            mapperMock.Setup(x => x.Map<TestBusinessClass, TestDataClass>(businessCollection[5]))
                .Returns((TestBusinessClass x) => new TestDataClass { ExternalId = x.ExternalId });
            mapperMock.Setup(x => x.Map<TestBusinessClass, TestDataClass>(businessCollection[6]))
                .Returns((TestBusinessClass x) => new TestDataClass { ExternalId = x.ExternalId });
            repositoryMock.Setup(x => x.Insert(It.Is<TestDataClass>(t => t.ExternalId == businessCollection[0].ExternalId && t.Id == 666)));
            repositoryMock.Setup(x => x.Insert(It.Is<TestDataClass>(t => t.ExternalId == businessCollection[5].ExternalId && t.Id == 666)));
            repositoryMock.Setup(x => x.Insert(It.Is<TestDataClass>(t => t.ExternalId == businessCollection[6].ExternalId && t.Id == 666)));

            UpdateCollectionHelper.UpdateCollection(dataCollection, businessCollection, repositoryMock.Object, mapperMock.Object, comparer, assingId);

            mapperMock.Verify(x => x.Map(It.IsAny<TestBusinessClass>(), It.IsAny<TestDataClass>()), Times.Exactly(4));
            repositoryMock.Verify(x => x.Update(It.IsAny<TestDataClass>()), Times.Exactly(4));
            repositoryMock.Verify(x => x.Delete(It.IsAny<TestDataClass>()), Times.Once);
            mapperMock.Verify(x => x.Map<TestBusinessClass, TestDataClass>(It.IsAny<TestBusinessClass>()), Times.Exactly(3));
            repositoryMock.Verify(x => x.Insert(It.IsAny<TestDataClass>()), Times.Exactly(3));
        }
    }
}
