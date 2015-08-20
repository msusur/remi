using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleasePlan;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.ReleaseCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.DataEntities.ReleasePlan;
using DataReleaseJob = ReMi.DataEntities.ReleasePlan.ReleaseJob;
using ReleaseJob = ReMi.BusinessEntities.DeploymentTool.ReleaseJob;

namespace ReMi.DataAccess.Tests.ReleasePlan
{
    [TestFixture]
    public class ReleaseJobGatewayTests : TestClassFor<ReleaseJobGateway>
    {
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;
        private Mock<IRepository<DataReleaseJob>> _releaseJobRepositoryMock;
        private Mock<IRepository<ReleaseDeploymentMeasurement>> _releaseDeploymentMeasurementRepositoryMock;
        private Mock<IMappingEngine> _mappingEngineMock;

        protected override ReleaseJobGateway ConstructSystemUnderTest()
        {
            return new ReleaseJobGateway
            {
                ReleaseJobRepository = _releaseJobRepositoryMock.Object,
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object,
                ReleaseDeploymentMeasurementRepository = _releaseDeploymentMeasurementRepositoryMock.Object,
                MappingEngine = _mappingEngineMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>(MockBehavior.Strict);
            _releaseJobRepositoryMock = new Mock<IRepository<DataReleaseJob>>(MockBehavior.Strict);
            _releaseDeploymentMeasurementRepositoryMock = new Mock<IRepository<ReleaseDeploymentMeasurement>>(MockBehavior.Strict);
            _mappingEngineMock = new Mock<IMappingEngine>(MockBehavior.Strict);

            base.TestInitialize();
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void AddJobToRelease_ShouldRaiseException_WhenReleaseWindowNotFound()
        {
            _releaseWindowRepositoryMock.SetupEntities(Enumerable.Empty<ReleaseWindow>());

            Sut.AddJobToRelease(null, Guid.NewGuid());
        }

        [Test]
        [ExpectedException(typeof(ReleaseJobDuplicationException))]
        public void AddJobToRelease_ShouldRaiseException_WhenReleasePipelineAlreadyExist()
        {
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ReleaseWindowId = RandomData.RandomInt(int.MaxValue)
            };
            var jobs = Builder<DataReleaseJob>.CreateListOfSize(5).Build();
            jobs[0].ReleaseWindowId = releaseWindow.ReleaseWindowId;
            var job = new ReleaseJob { JobId = jobs[0].JobId };

            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _releaseJobRepositoryMock.SetupEntities(jobs);

            Sut.AddJobToRelease(job, releaseWindow.ExternalId);
        }

        [Test]
        public void AddJobToRelease_ShouldInsertReleasePipeline__WhenInvoked()
        {
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ReleaseWindowId = RandomData.RandomInt(int.MaxValue)
            };
            var job = new ReleaseJob { JobId = Guid.NewGuid() };
            var dataJob = new DataReleaseJob { JobId = job.JobId };
            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _releaseJobRepositoryMock.SetupEntities(Enumerable.Empty<DataReleaseJob>());
            _mappingEngineMock.Setup(x => x.Map<ReleaseJob, DataReleaseJob>(job))
                .Returns(dataJob);
            _releaseJobRepositoryMock.Setup(x => x.Insert(dataJob));

            Sut.AddJobToRelease(job, releaseWindow.ExternalId);

            _releaseJobRepositoryMock.Verify(o => o.Insert(It.IsAny<DataReleaseJob>()), Times.Once);
        }

        [Test]
        public void GetReleaseJob_ShouldReturnNull_WhenRequestedJobNotFound()
        {
            _releaseJobRepositoryMock.SetupEntities(Enumerable.Empty<DataReleaseJob>());

            var result = Sut.GetReleaseJob(Guid.NewGuid(), Guid.NewGuid());

            Assert.IsNull(result);
        }

        [Test]
        public void GetReleaseJob_ShouldReturnReleaseJob_WhenInvoked()
        {
            var jobs = Builder<DataReleaseJob>.CreateListOfSize(5)
                .All()
                .Do(x => x.ReleaseWindow = new ReleaseWindow { ExternalId = Guid.NewGuid() })
                .Build();
            var job = new ReleaseJob { JobId = jobs[0].JobId };

            _releaseJobRepositoryMock.SetupEntities(jobs);
            _mappingEngineMock.Setup(x => x.Map<DataReleaseJob, ReleaseJob>(jobs[0]))
                .Returns(job);

            var result = Sut.GetReleaseJob(jobs[0].ReleaseWindow.ExternalId, jobs[0].JobId);

            Assert.AreEqual(job, result);
            _mappingEngineMock.Verify(x => x.Map<DataReleaseJob, ReleaseJob>(It.IsAny<DataReleaseJob>()), Times.Once);
        }

        [Test]
        public void GetReleaseJobs_ShouldReturnReleaseJobsWithoutLastBuildNumbers_WhenBuildNumbersAreNotRequested()
        {
            var releaseWindowId = Guid.NewGuid();
            var jobs = Builder<DataReleaseJob>.CreateListOfSize(5)
                .All()
                .With(x => x.ReleaseWindow, new ReleaseWindow { ExternalId = releaseWindowId })
                .Build();
            var resultJobs = Builder<ReleaseJob>.CreateListOfSize(5).Build();
            _releaseJobRepositoryMock.SetupEntities(jobs);
            _mappingEngineMock.Setup(x => x.Map<IEnumerable<DataReleaseJob>, IEnumerable<ReleaseJob>>(jobs))
                .Returns(resultJobs);

            var result = Sut.GetReleaseJobs(releaseWindowId).ToArray();

            Assert.AreEqual(resultJobs, result);

            _mappingEngineMock.Verify(x => x.Map<IEnumerable<DataReleaseJob>, IEnumerable<ReleaseJob>>(It.IsAny<IEnumerable<DataReleaseJob>>()), Times.Once);
        }

        [Test]
        public void GetReleaseJobs_ShouldReturnReleaseJobsWithLastBuildNumbers_WhenBuildNumbersAreRequested()
        {
            var releaseWindowId = Guid.NewGuid();
            var jobs = Builder<DataReleaseJob>.CreateListOfSize(5)
                .All()
                .With(x => x.ReleaseWindow, new ReleaseWindow { ExternalId = releaseWindowId })
                .Build();
            var resultJobs = Builder<ReleaseJob>.CreateListOfSize(5).Build();
            var measurements = resultJobs.Skip(1).Take(3).Select(x => new ReleaseDeploymentMeasurement
            {
                BuildNumber = RandomData.RandomInt(1000),
                StepName = x.Name
            }).ToList();
            measurements.Add(new ReleaseDeploymentMeasurement { StepName = resultJobs[1].Name, BuildNumber = RandomData.RandomInt(1001, 2000) });


            _releaseJobRepositoryMock.SetupEntities(jobs);
            _mappingEngineMock.Setup(x => x.Map<IEnumerable<DataReleaseJob>, IEnumerable<ReleaseJob>>(jobs))
                .Returns(resultJobs);
            _releaseDeploymentMeasurementRepositoryMock.SetupEntities(measurements);

            var result = Sut.GetReleaseJobs(releaseWindowId, true).ToArray();

            Assert.AreEqual(resultJobs, result);
            Assert.AreEqual(measurements[3].BuildNumber, resultJobs[1].LastBuildNumber);
            Assert.AreEqual(measurements[1].BuildNumber, resultJobs[2].LastBuildNumber);
            Assert.AreEqual(measurements[2].BuildNumber, resultJobs[3].LastBuildNumber);

            _mappingEngineMock.Verify(x => x.Map<IEnumerable<DataReleaseJob>, IEnumerable<ReleaseJob>>(It.IsAny<IEnumerable<DataReleaseJob>>()), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void RemoveJobFromRelease_ShouldRaiseException_WhenReleaseWindowNotFound()
        {
            _releaseWindowRepositoryMock.SetupEntities(Enumerable.Empty<ReleaseWindow>());

            Sut.RemoveJobFromRelease(Guid.NewGuid(), Guid.NewGuid());
        }

        [Test]
        public void RemoveJobFromRelease_ShouldRaiseException_WhenReleaseJobDoesNotExist()
        {
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ReleaseWindowId = RandomData.RandomInt(int.MaxValue)
            };
            var jobId = Guid.NewGuid();
            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _releaseJobRepositoryMock.SetupEntities(Enumerable.Empty<DataReleaseJob>());

            var ex = Assert.Throws<EntityNotFoundException>(() => Sut.RemoveJobFromRelease(jobId, releaseWindow.ExternalId));

            Assert.IsTrue(ex.Message.Contains(jobId.ToString()));
            Assert.IsTrue(ex.Message.Contains(releaseWindow.ExternalId.ToString()));
        }

        [Test]
        public void RemoveJobFromRelease_ShouldCallDeleteInGateway_WhenInvoked()
        {
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ReleaseWindowId = RandomData.RandomInt(int.MaxValue)
            };
            var jobs = Builder<DataReleaseJob>.CreateListOfSize(5).Build();
            jobs[0].ReleaseWindowId = releaseWindow.ReleaseWindowId;

            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _releaseJobRepositoryMock.SetupEntities(jobs);
            _releaseJobRepositoryMock.Setup(x => x.Delete(jobs[0]));

            Sut.RemoveJobFromRelease(jobs[0].JobId, releaseWindow.ExternalId);

            _releaseJobRepositoryMock.Verify(o => o.Delete(It.IsAny<DataReleaseJob>()), Times.Once);
        }

        [Test]
        public void Dispose_ShouldCallDisposeForAllRepositories_WhenInvoked()
        {
            _releaseJobRepositoryMock.Setup(o => o.Dispose());
            _releaseWindowRepositoryMock.Setup(o => o.Dispose());

            Sut.Dispose();

            _releaseJobRepositoryMock.Verify(o => o.Dispose(), Times.Once);
            _releaseWindowRepositoryMock.Verify(o => o.Dispose(), Times.Once);
        }
    }
}
