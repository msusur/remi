using System;
using AutoMapper;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data.SourceControl;
using ReMi.DataAccess.AutoMapper;

namespace ReMi.DataAccess.Tests.AutoMapper
{
    [TestFixture]
    public class ContractEntityToDataEntityMappingProfileTests : TestClassFor<IMappingEngine>
    {
        protected override IMappingEngine ConstructSystemUnderTest()
        {
            Mapper.Initialize(
                c => c.AddProfile<ContractEntityToDataEntityMappingProfile>());

            return Mapper.Engine;
        }

        [Test]
        public void SourceControlChange_ShouldReturnDataSourceControlChange_WhenMapFromContractEntity()
        {
            var sourceControlChange = new SourceControlChange
            {
                Description = RandomData.RandomString(2500),
                Identifier = RandomData.RandomString(10),
                Owner = RandomData.RandomString(10),
                Repository = RandomData.RandomString(10),
                Date = RandomData.RandomDateTime()
            };

            var result = Sut.Map<SourceControlChange, DataEntities.SourceControl.SourceControlChange>(sourceControlChange);

            Assert.AreEqual(sourceControlChange.Description.Substring(0, 2048), result.Description);
            Assert.AreEqual(sourceControlChange.Identifier, result.Identifier);
            Assert.AreEqual(sourceControlChange.Owner, result.Owner);
            Assert.AreEqual(sourceControlChange.Repository, result.Repository);
        }

        [Test]
        public void ReleaseRepository_ShouldReturnDataReleaseRepository_WhenMapFromContractEntity()
        {
            var releaseRepository = new ReleaseRepository
            {
                ChangesFrom = RandomData.RandomString(10),
                ChangesTo = RandomData.RandomString(10),
                ExternalId = Guid.NewGuid(),
                IsDisabled = RandomData.RandomBool(),
                IsIncluded = RandomData.RandomBool(),
                LatestChange = RandomData.RandomBool(),
                Repository = RandomData.RandomString(10)
            };

            var result = Sut.Map<ReleaseRepository, DataEntities.ReleasePlan.ReleaseRepository>(releaseRepository);

            Assert.AreEqual(releaseRepository.ChangesFrom, result.ChangesFrom);
            Assert.AreEqual(releaseRepository.ChangesTo, result.ChangesTo);
            Assert.AreEqual(releaseRepository.ExternalId, result.RepositoryId);
            Assert.AreEqual(releaseRepository.Repository, result.Name);
            Assert.AreEqual(releaseRepository.IsIncluded, result.IsIncluded);
            Assert.AreEqual(releaseRepository.LatestChange, result.LatestChange);
        }
    }
}
