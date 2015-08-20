using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Moq;
using NUnit.Framework;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseExecution;
using ReMi.DataAccess.Exceptions;
using ReMi.DataAccess.Exceptions.ReleaseExecution;
using ReMi.DataEntities.Auth;
using ReMi.DataEntities.Metrics;
using ReMi.DataEntities.ReleaseCalendar;
using SignOff = ReMi.DataEntities.ReleaseExecution.SignOff;

namespace ReMi.DataAccess.Tests.ReleaseExecution
{
    public class SignOffGatewayTests : TestClassFor<SignOffGateway>
    {
        private Mock<IRepository<SignOff>> _signOffRepositoryMock;
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;
        private Mock<IRepository<Account>> _accountRepositoryMock;
        private Mock<IRepository<Metric>> _metricsRepositoryMock;
        private Mock<IMappingEngine> _mapperMock;
        private List<SignOff> _signOffs;
        private List<ReleaseWindow> _releaseWindows;
        private List<Account> _accounts;

        protected override SignOffGateway ConstructSystemUnderTest()
        {
            return new SignOffGateway
            {
                SignOffRepository = _signOffRepositoryMock.Object,
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object,
                AccountRepository = _accountRepositoryMock.Object,
                MetricRepository = _metricsRepositoryMock.Object,
                Mapper = _mapperMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _signOffRepositoryMock = new Mock<IRepository<SignOff>>();
            _metricsRepositoryMock = new Mock<IRepository<Metric>>();
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>();
            _accountRepositoryMock = new Mock<IRepository<Account>>();
            _mapperMock = new Mock<IMappingEngine>();

            _accounts = new List<Account>
            {
                new Account {AccountId = RandomData.RandomInt(30, 89), ExternalId = Guid.NewGuid()},
                new Account {AccountId = RandomData.RandomInt(90, 189), ExternalId = Guid.NewGuid()},
                new Account {AccountId = RandomData.RandomInt(200, 289), ExternalId = Guid.NewGuid()}
            };

            _accountRepositoryMock.SetupEntities(_accounts);

            _releaseWindows = new List<ReleaseWindow>
            {
                new ReleaseWindow {ReleaseWindowId = RandomData.RandomInt(66, 99), ExternalId = Guid.NewGuid(), Metrics = new List<Metric>()}
            };

            _releaseWindowRepositoryMock.SetupEntities(_releaseWindows);

            _signOffs = new List<SignOff>
            {
                new SignOff
                {
                    AccountId = _accounts[0].AccountId,
                    Account = new Account{ExternalId = Guid.NewGuid()},
                    ReleaseWindowId = _releaseWindows[0].ReleaseWindowId,
                    SignedOff = SystemTime.Now.AddHours(-1),
                    ExternalId = Guid.NewGuid(),
                    ReleaseWindow = _releaseWindows[0]
                },
                new SignOff
                {
                    AccountId = _accounts[1].AccountId,
                    Account = new Account{ExternalId = Guid.NewGuid()},
                    ReleaseWindowId = _releaseWindows[0].ReleaseWindowId,
                    ExternalId = Guid.NewGuid(),
                    ReleaseWindow = _releaseWindows[0]
                }
            };

            _signOffRepositoryMock.SetupEntities(_signOffs);


            base.TestInitialize();
        }

        [Test, ExpectedException(typeof(SignOffNotFoundException))]
        public void SignOff_ShouldThrowException_WhenSignOffNotFound()
        {
            Sut.SignOff(Guid.NewGuid(), Guid.NewGuid(), String.Empty);
        }

        [Test]
        public void SignOff_ShouldUpdateSignersRepository()
        {
            Sut.SignOff(_signOffs[1].Account.ExternalId, _releaseWindows[0].ExternalId, "this is description");

            _signOffRepositoryMock.Verify(
                s =>
                    s.Update(
                        It.Is<SignOff>(
                            q =>
                                q.ExternalId == _signOffs[1].ExternalId && q.SignedOff != null &&
                                q.Comment == "this is description")));
        }

        [Test, ExpectedException(typeof(SignOffNotFoundException))]
        public void RemoveSigner_ShouldThrowException_WhenRemovedSignerNotFound()
        {
            Sut.RemoveSigner(Guid.NewGuid());
        }

        [Test]
        public void RemoveSigner_ShouldRemoveSigner()
        {
            Sut.RemoveSigner(_signOffs[1].ExternalId);

            _signOffRepositoryMock.Verify(
                s => s.Delete(_signOffs[1]));
        }

        [Test, ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void AddSigners_ShouldThrowException_WhenWindowNotFound()
        {
            Sut.AddSigners(new List<BusinessEntities.ReleaseExecution.SignOff>(), Guid.NewGuid());
        }

        [Test]
        public void AddSigners_ShouldAddSigners()
        {
            Sut.AddSigners(new List<BusinessEntities.ReleaseExecution.SignOff>
            {
                new BusinessEntities.ReleaseExecution.SignOff
                {
                    ExternalId = Guid.NewGuid(),
                    Signer = new BusinessEntities.Auth.Account
                    {
                        ExternalId = _accounts[2].ExternalId
                    }
                }
            }, _releaseWindows[0].ExternalId);

            _signOffRepositoryMock.Verify(
                s =>
                    s.Insert(
                        It.Is<List<SignOff>>(sg => sg.Any(i => i.AccountId == _accounts[2].AccountId) && sg.Count == 1)));
        }

        [Test, ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void GetSigners_ShouldThrowException_WhenWindowNotFound()
        {
            Sut.GetSignOffs(Guid.NewGuid());
        }

        [Test]
        public void GetSigners_ShouldWorkCorrectly()
        {
            Sut.GetSignOffs(_releaseWindows[0].ExternalId);

            _mapperMock.Verify(
                m =>
                    m.Map<List<SignOff>, List<BusinessEntities.ReleaseExecution.SignOff>>(
                        It.Is<List<SignOff>>(
                            s =>
                                s.Count == 2 && s.Any(i => i.ExternalId == _signOffs[0].ExternalId) &&
                                s.Any(i => i.ExternalId == _signOffs[1].ExternalId))));
        }

        [Test, ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void Check_ShouldThrowException_WhenWindowNotFound()
        {
            Sut.CheckSigningOff(Guid.NewGuid());
        }

        [Test]
        public void Check_ShouldReturnFalse_WhenOneUserHasNotSignedOff()
        {
            var result = Sut.CheckSigningOff(_releaseWindows[0].ExternalId);

            Assert.IsFalse(result);
            _releaseWindowRepositoryMock.Verify(x => x.Update(It.IsAny<ReleaseWindow>()), Times.Never);
        }

        [Test]
        public void Check_ShouldReturnTrueAndInsertMetric_WhenReleaseIsFullySignedOff()
        {
            _signOffs = new List<SignOff>
            {
                new SignOff
                {
                    AccountId = _accounts[0].AccountId,
                    ReleaseWindowId = _releaseWindows[0].ReleaseWindowId,
                    SignedOff = SystemTime.Now.AddHours(-1),
                    ExternalId = Guid.NewGuid(),
                    ReleaseWindow = _releaseWindows[0]
                },
                new SignOff
                {
                    AccountId = _accounts[1].AccountId,
                    ReleaseWindowId = _releaseWindows[0].ReleaseWindowId,
                    ExternalId = Guid.NewGuid(),
                    ReleaseWindow = _releaseWindows[0],
                    SignedOff = SystemTime.Now.AddMinutes(-50)
                }
            };
            _signOffRepositoryMock.SetupEntities(_signOffs);
            _metricsRepositoryMock.Setup(x => x.Insert(It.Is<Metric>(m => m.ReleaseWindowId == _releaseWindows[0].ReleaseWindowId)))
                .Callback((Metric m) => _releaseWindows[0].Metrics.Add(m));

            var result = Sut.CheckSigningOff(_releaseWindows[0].ExternalId);

            Assert.IsTrue(result);
            _metricsRepositoryMock.Verify(x => x.Insert(It.IsAny<Metric>()), Times.Once);
            _metricsRepositoryMock.Verify(x => x.Update(It.IsAny<Metric>()), Times.Never);
        }

        [Test]
        public void Check_ShouldReturnTrueAndUpdateMetric_WhenReleaseIsFullySignedOffAndMetricExists()
        {
            _signOffs = new List<SignOff>
            {
                new SignOff
                {
                    AccountId = _accounts[0].AccountId,
                    ReleaseWindowId = _releaseWindows[0].ReleaseWindowId,
                    SignedOff = SystemTime.Now.AddHours(-1),
                    ExternalId = Guid.NewGuid(),
                    ReleaseWindow = _releaseWindows[0]
                },
                new SignOff
                {
                    AccountId = _accounts[1].AccountId,
                    ReleaseWindowId = _releaseWindows[0].ReleaseWindowId,
                    ExternalId = Guid.NewGuid(),
                    ReleaseWindow = _releaseWindows[0],
                    SignedOff = SystemTime.Now.AddMinutes(-50)
                }
            };
            var metric = new Metric
            {
                ExternalId = Guid.NewGuid(),
                MetricType = MetricType.SignOff,
                ReleaseWindowId = _releaseWindows[0].ReleaseWindowId,
                ReleaseWindow = new ReleaseWindow { ExternalId = _releaseWindows[0].ExternalId }
            };
            _signOffRepositoryMock.SetupEntities(_signOffs);
            _metricsRepositoryMock.SetupEntities(new[] { metric });
            _releaseWindows[0].Metrics.Add(metric);

            var result = Sut.CheckSigningOff(_releaseWindows[0].ExternalId);

            Assert.IsTrue(result);
            _metricsRepositoryMock.Verify(x => x.Insert(It.IsAny<Metric>()), Times.Never);
            _metricsRepositoryMock.Verify(x => x.Update(It.IsAny<Metric>()), Times.Once);
            _metricsRepositoryMock.Verify(x => x.Update(It.Is<Metric>(m => m.MetricType == MetricType.SignOff && m.ExecutedOn.HasValue)));
        }
    }
}
