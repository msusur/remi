using System;
using AutoMapper;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.DataAccess.AutoMapper;

namespace ReMi.DataAccess.Tests.AutoMapper
{
    [TestFixture]
    public class DataEntityToContractEntityMappingProfileTests : TestClassFor<IMappingEngine>
    {
        protected override IMappingEngine ConstructSystemUnderTest()
        {
            Mapper.Initialize(
                c => c.AddProfile<DataEntityToContractEntityMappingProfile>());

            return Mapper.Engine;
        }

        [Test]
        public void SourceControlChange_ShouldReturnContractSourceControlChange_WhenMapFromDataEntity()
        {
            var sourceControlChange = new DataEntities.SourceControl.SourceControlChange
            {
                Description = RandomData.RandomString(10),
                Identifier = RandomData.RandomString(10),
                Owner = RandomData.RandomString(10),
                Repository = RandomData.RandomString(10),
            };

            var result = Sut.Map<DataEntities.SourceControl.SourceControlChange, SourceControlChange>(sourceControlChange);

            Assert.AreEqual(sourceControlChange.Description, result.Description);
            Assert.AreEqual(sourceControlChange.Identifier, result.Identifier);
            Assert.AreEqual(sourceControlChange.Owner, result.Owner);
            Assert.AreEqual(sourceControlChange.Repository, result.Repository);
        }

        [Test]
        public void ReleaseRepository_ShouldReturnContractReleaseRepository_WhenMapFromDataEntity()
        {
            var releaseRepository = new DataEntities.ReleasePlan.ReleaseRepository
            {
                ChangesFrom = RandomData.RandomString(10),
                ChangesTo = RandomData.RandomString(10),
                RepositoryId = Guid.NewGuid(),
                IsIncluded = RandomData.RandomBool(),
                LatestChange = RandomData.RandomBool(),
                Name = RandomData.RandomString(10)
            };

            var result = Sut.Map<DataEntities.ReleasePlan.ReleaseRepository, ReleaseRepository>(releaseRepository);

            Assert.AreEqual(releaseRepository.ChangesFrom, result.ChangesFrom);
            Assert.AreEqual(releaseRepository.ChangesTo, result.ChangesTo);
            Assert.AreEqual(releaseRepository.RepositoryId, result.ExternalId);
            Assert.AreEqual(releaseRepository.Name, result.Repository);
            Assert.AreEqual(releaseRepository.IsIncluded, result.IsIncluded);
            Assert.AreEqual(releaseRepository.LatestChange, result.LatestChange);
        }
    }
}
