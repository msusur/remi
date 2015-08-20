using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.AutoMapper;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.ReleaseCalendar;
using ReMi.DataEntities.ReleasePlan;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMi.DataAccess.Tests.ReleaseExecution
{
    public class ReleaseDeploymentJobMeasurementGatewayTests : TestClassFor<ReleaseDeploymentMeasurementGateway>
    {
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;
        private Mock<IRepository<Account>> _accountRepositoryMock;
        private Mock<IRepository<ReleaseDeploymentMeasurement>> _releaseJobMeasurementRepositoryMock;

        protected override ReleaseDeploymentMeasurementGateway ConstructSystemUnderTest()
        {
            return new ReleaseDeploymentMeasurementGateway
            {
                AccountRepository = _accountRepositoryMock.Object,
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object,
                ReleaseDeploymentMeasurementRepository = _releaseJobMeasurementRepositoryMock.Object,
                MappingEngine = Mapper.Engine
            };
        }

        [TestFixtureSetUp]
        public static void AutoMapperInitialize()
        {
            Mapper.Initialize(
                c =>
                {
                    c.AddProfile<BusinessEntityToDataEntityMappingProfile>();
                    c.AddProfile(new DataEntityToBusinessEntityMappingProfile());
                });
        }

        protected override void TestInitialize()
        {
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>();
            _accountRepositoryMock = new Mock<IRepository<Account>>();
            _releaseJobMeasurementRepositoryMock = new Mock<IRepository<ReleaseDeploymentMeasurement>>();

            base.TestInitialize();
        }

        [Test]
        public void GetMeasurements_ShouldReturnFlatData_Wheninvoked()
        {
            var releaseWindowId = Guid.NewGuid();

            var measurements = SetupMeasurements(releaseWindowId).ToArray();

            var result = Sut.GetDeploymentMeasurements(releaseWindowId).ToArray();

            Assert.NotNull(result);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(measurements[0].StepId, result[0].StepId);
            Assert.AreEqual(measurements[1].StepId, result[1].StepId);
        }

        [Test]
        public void GetMeasurements_ShouldReturnTreeData_WhenInvoked()
        {
            var releaseWindowId = Guid.NewGuid();

            var measurements = SetupMeasurements(releaseWindowId, false).ToArray();

            var result = Sut.GetDeploymentMeasurements(releaseWindowId).ToArray();

            Assert.NotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.IsNotNull(result[0].ChildSteps);
            Assert.AreEqual(2, result[0].ChildSteps.Count);
            Assert.AreEqual(measurements[1].StepId, result[0].ChildSteps[0].StepId);
            Assert.AreEqual(measurements[2].StepId, result[0].ChildSteps[1].StepId);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StoreDeploymentMeasurements_ShouldRaiseException_WhenNullPassed()
        {
            var releaseWindowId = Guid.NewGuid();
            var accountId = Guid.NewGuid();

            Sut.StoreDeploymentMeasurements(null, releaseWindowId, accountId);
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void StoreDeploymentMeasurements_ShouldRaiseException_WhenReleaseWindowNotFound()
        {
            var accountId = Guid.NewGuid();

            var steps = GetStepMeasurementsFlat();

            Sut.StoreDeploymentMeasurements(steps, Guid.NewGuid(), accountId);
        }

        [Test]
        [ExpectedException(typeof(AccountNotFoundException))]
        public void StoreDeploymentMeasurements_ShouldRaiseException_WhenAccountNotFound()
        {
            var releaseWindow = SetupReleaseWindow(Guid.NewGuid());

            var steps = GetStepMeasurementsFlat();

            Sut.StoreDeploymentMeasurements(steps, releaseWindow.ExternalId, Guid.NewGuid());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StoreDeploymentMeasurements_ShouldRaiseException_WhenStepsArgumentIsNull()
        {
            var releaseWindow = SetupReleaseWindow(Guid.NewGuid());

            Sut.StoreDeploymentMeasurements(null, releaseWindow.ExternalId, Guid.NewGuid());
        }

        [Test]
        public void StoreDeploymentMeasurements_ShouldInsertRepository_WhenInvokedWithFlatSteps()
        {
            var releaseWindow = SetupReleaseWindow(Guid.NewGuid());
            var account = SetupAccount(Guid.NewGuid());

            var steps = GetStepMeasurementsFlat();

            Sut.StoreDeploymentMeasurements(steps, releaseWindow.ExternalId, account.ExternalId);

            _releaseJobMeasurementRepositoryMock
                .Verify(o => o.Insert(It.IsAny<ReleaseDeploymentMeasurement>()), Times.Exactly(2));

            _releaseJobMeasurementRepositoryMock
                .Verify(o => o.Insert(It.Is<ReleaseDeploymentMeasurement>(x => x.StepId == steps[0].StepId)), Times.Once);
            _releaseJobMeasurementRepositoryMock
                .Verify(o => o.Insert(It.Is<ReleaseDeploymentMeasurement>(x => x.StepId == steps[1].StepId)), Times.Once);
        }

        [Test]
        public void StoreDeploymentMeasurements_ShouldInsertRepository_WhenInvokedWithTreeSteps()
        {
            var releaseWindow = SetupReleaseWindow(Guid.NewGuid());
            var account = SetupAccount(Guid.NewGuid());

            var steps = GetStepMeasurementsTree();

            Sut.StoreDeploymentMeasurements(steps, releaseWindow.ExternalId, account.ExternalId);

            _releaseJobMeasurementRepositoryMock
                .Verify(o => o.Insert(It.IsAny<ReleaseDeploymentMeasurement>()), Times.Exactly(5));

            _releaseJobMeasurementRepositoryMock
                .Verify(o => o.Insert(It.Is<ReleaseDeploymentMeasurement>(
                    x => x.StepId == steps[0].StepId
                    && x.BuildNumber == steps[0].BuildNumber
                    && x.NumberOfTries == steps[0].NumberOfTries)), Times.Once);
            _releaseJobMeasurementRepositoryMock
                .Verify(o => o.Insert(It.Is<ReleaseDeploymentMeasurement>(x => x.StepId == steps[0].ChildSteps[0].StepId)), Times.Once);
            _releaseJobMeasurementRepositoryMock
                .Verify(o => o.Insert(It.Is<ReleaseDeploymentMeasurement>(x => x.StepId == steps[0].ChildSteps[1].StepId)), Times.Once);
            _releaseJobMeasurementRepositoryMock
                .Verify(o => o.Insert(It.Is<ReleaseDeploymentMeasurement>(x => x.StepId == steps[0].ChildSteps[1].ChildSteps[0].StepId)), Times.Once);
            _releaseJobMeasurementRepositoryMock
                .Verify(o => o.Insert(It.Is<ReleaseDeploymentMeasurement>(x => x.StepId == steps[0].ChildSteps[1].ChildSteps[1].StepId)), Times.Once);
        }

        [Test]
        public void StoreDeploymentMeasurements_ShouldDoNothing_WhenNoDeploymentJobsAreConfigured()
        {

            Sut.StoreDeploymentMeasurements(new List<JobMeasurement>(), Guid.NewGuid(), Guid.NewGuid());

        }
        [Test]
        public void Dispose_ShouldDisposeAllRepositories_WhenInvoked()
        {
            Sut.Dispose();

            _releaseWindowRepositoryMock.Verify(o => o.Dispose());
            _releaseJobMeasurementRepositoryMock.Verify(o => o.Dispose());
            _accountRepositoryMock.Verify(o => o.Dispose());
        }

        [Test]
        public void RemoveDeploymentMeasurements_ShouldThrowException_WhenReleaseWindowNotFound()
        {
            _releaseWindowRepositoryMock.SetupEntities(Enumerable.Empty<ReleaseWindow>());
            var releaseWindowId = Guid.NewGuid();

            var ex = Assert.Throws<ReleaseWindowNotFoundException>(() => Sut.RemoveDeploymentMeasurements(releaseWindowId));

            Assert.IsTrue(ex.Message.Contains(releaseWindowId.ToString()));
        }

        [Test]
        public void RemoveDeploymentMeasurements_ShouldDeleteFoundedMeasurements_WhenCalled()
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ReleaseWindowId, RandomData.RandomInt(100, int.MaxValue))
                .Build();
            var measurements = Builder<ReleaseDeploymentMeasurement>.CreateListOfSize(20)
                .Random(10)
                .With(x => x.ReleaseWindowId, releaseWindow.ReleaseWindowId)
                .Build();
            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _releaseJobMeasurementRepositoryMock.SetupEntities(measurements);
            _releaseJobMeasurementRepositoryMock.Setup(x => x.Delete(
                It.Is<IEnumerable<ReleaseDeploymentMeasurement>>(m => m.SequenceEqual(
                    measurements.Where(m2 => m2.ReleaseWindowId == releaseWindow.ReleaseWindowId)))));

            Sut.RemoveDeploymentMeasurements(releaseWindow.ExternalId);

            _releaseJobMeasurementRepositoryMock.Verify(
                x => x.Delete(It.IsAny<IEnumerable<ReleaseDeploymentMeasurement>>()), Times.Once());
        }

        #region Helpers

        private ReleaseWindow SetupReleaseWindow(Guid releaseWindowId, ReleaseType? type = null)
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
               .With(o => o.ExternalId, releaseWindowId)
               .With(o => o.ReleaseType, type ?? ReleaseType.Scheduled)
               .Build();

            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });

            return releaseWindow;
        }

        private Account SetupAccount(Guid accountId)
        {
            var account = Builder<Account>.CreateNew()
               .With(o => o.ExternalId, accountId)
               .Build();

            _accountRepositoryMock.SetupEntities(new[] { account });

            return account;
        }

        private IEnumerable<ReleaseDeploymentMeasurement> SetupMeasurements(Guid releaseWindowId, bool flatData = true, DateTime? finishTime = null)
        {
            var releaseWindow = Builder<ReleaseWindow>.CreateNew()
                .With(o => o.ExternalId, releaseWindowId)
                .Build();

            ReleaseDeploymentMeasurement[] items = flatData ?
                GetMasurementsFlat(releaseWindow, finishTime) :
                GetMasurementsTree(releaseWindow);

            _releaseJobMeasurementRepositoryMock.SetupEntities(items);

            return items;
        }

        private ReleaseDeploymentMeasurement[] GetMasurementsFlat(ReleaseWindow releaseWindow, DateTime? finishTime = null)
        {
            return new[]
            {
                Builder<ReleaseDeploymentMeasurement>.CreateNew()
                    .With(o => o.ReleaseDeploymentMeasurementId, 1)
                    .With(o => o.ReleaseWindow, releaseWindow)
                    .With(o => o.ParentMeasurementId, null)
                    .With(o => o.StepId, RandomData.RandomString(10))
                    .With(o => o.FinishTime, finishTime ?? RandomData.RandomDateTime(DateTimeKind.Utc))
                    .Build(),
                Builder<ReleaseDeploymentMeasurement>.CreateNew()
                    .With(o => o.ReleaseDeploymentMeasurementId, 2)
                    .With(o => o.ReleaseWindow, releaseWindow)
                    .With(o => o.ParentMeasurementId, null)
                    .With(o => o.StepId, RandomData.RandomString(10))
                    .With(o => o.FinishTime, finishTime ?? RandomData.RandomDateTime(DateTimeKind.Utc))
                    .Build()
            };
        }

        private ReleaseDeploymentMeasurement[] GetMasurementsTree(ReleaseWindow releaseWindow)
        {
            var parent = Builder<ReleaseDeploymentMeasurement>.CreateNew()
                .With(o => o.ReleaseDeploymentMeasurementId, 1)
                .With(o => o.ParentMeasurementId, null)
                .With(o => o.ReleaseWindow, releaseWindow)
                .With(o => o.StepId, RandomData.RandomString(10))
                .Build();

            return new[]
            {
                parent,
                Builder<ReleaseDeploymentMeasurement>.CreateNew()
                    .With(o => o.ReleaseDeploymentMeasurementId, 2)
                    .With(o => o.ParentMeasurementId, 1)
                    .With(o => o.ReleaseWindow, releaseWindow)
                    .With(o => o.StepId, RandomData.RandomString(10))
                    .Build(),
                Builder<ReleaseDeploymentMeasurement>.CreateNew()
                    .With(o => o.ReleaseDeploymentMeasurementId, 3)
                    .With(o => o.ParentMeasurementId, 1)
                    .With(o => o.ReleaseWindow, releaseWindow)
                    .With(o => o.StepId, RandomData.RandomString(10))
                    .Build()
            };
        }

        private JobMeasurement[] GetStepMeasurementsFlat()
        {
            return new[]
            {
                Builder<JobMeasurement>.CreateNew()
                    .With(o => o.StepId, RandomData.RandomString(10))
                    .Build(),
                Builder<JobMeasurement>.CreateNew()
                    .With(o => o.StepId, RandomData.RandomString(10))
                    .Build()
            };
        }

        private JobMeasurement[] GetStepMeasurementsTree()
        {
            return new[]
            {
                Builder<JobMeasurement>.CreateNew()
                    .With(o => o.StepId, RandomData.RandomString(10))
                    .With(o => o.BuildNumber, RandomData.RandomInt(int.MaxValue))
                    .With(o => o.NumberOfTries, RandomData.RandomInt(int.MaxValue))
                    .With(o => o.ChildSteps, new[]
                    {
                        Builder<JobMeasurement>.CreateNew()
                            .With(o => o.StepId, RandomData.RandomString(10))
                            .Build(),
                        Builder<JobMeasurement>.CreateNew()
                            .With(o => o.StepId, RandomData.RandomString(10))
                            .With(o => o.ChildSteps, new[]
                            {
                                Builder<JobMeasurement>.CreateNew()
                                    .With(o => o.StepId, RandomData.RandomString(10))
                                    .Build(),
                                Builder<JobMeasurement>.CreateNew()
                                    .With(o => o.StepId, RandomData.RandomString(10))
                                    .Build()

                            }.ToList())
                            .Build()

                    }.ToList())
                    .Build()
            };
        }

        #endregion
    }
}
