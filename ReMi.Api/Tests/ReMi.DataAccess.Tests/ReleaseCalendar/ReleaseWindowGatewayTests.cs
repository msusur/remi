using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.Contracts.Plugins.Data;
using ReMi.DataAccess.BusinessEntityGateways.ReleaseCalendar;
using ReMi.DataAccess.Exceptions;
using ReMi.DataAccess.Exceptions.Configuration;
using ReMi.DataEntities.Metrics;
using ReMi.DataEntities.Plugins;
using ReMi.DataEntities.Products;
using ReMi.DataEntities.ReleaseCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using ReMi.DataEntities.Auth;
using BusinessReleaseWindow = ReMi.BusinessEntities.ReleaseCalendar.ReleaseWindow;

namespace ReMi.DataAccess.Tests.ReleaseCalendar
{
    public class ReleaseWindowGatewayTests : TestClassFor<ReleaseWindowGateway>
    {
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;
        private Mock<IRepository<Product>> _productRepositoryMock;
        private Mock<IRepository<Metric>> _metricsRepositoryMock;
        private Mock<IRepository<Account>> _accountRepositoryMock;
        private Mock<IMappingEngine> _mappingEngineMock;
        private Mock<IReleaseProductGateway> _releaseProductGatewayMock;

        protected override ReleaseWindowGateway ConstructSystemUnderTest()
        {
            return new ReleaseWindowGateway
            {
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object,
                ProductRepository = _productRepositoryMock.Object,
                MetricRepository = _metricsRepositoryMock.Object,
                MappingEngine = _mappingEngineMock.Object,
                AccountRepository = _accountRepositoryMock.Object,
                ReleaseProductGatewayFactory = () => _releaseProductGatewayMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>();
            _productRepositoryMock = new Mock<IRepository<Product>>();
            _metricsRepositoryMock = new Mock<IRepository<Metric>>();
            _mappingEngineMock = new Mock<IMappingEngine>();
            _releaseProductGatewayMock = new Mock<IReleaseProductGateway>();
            _accountRepositoryMock = new Mock<IRepository<Account>>();

            base.TestInitialize();
        }

        protected override void TestCleanup()
        {
            SystemTime.Reset();
            base.TestCleanup();
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetAllStartingInTimeRange_ShouldThrowException_WhenDateRangeIsInvalid()
        {
            Sut.GetAllStartingInTimeRange(new DateTime(2014, 1, 2), new DateTime(2014, 1, 1));
        }

        [Test]
        public void GetAllStartingInTimeRange_ShouldLookInReleaseWindowRepository_WhenDateRangeIsValid()
        {
            Sut.GetAllStartingInTimeRange(new DateTime(2014, 1, 2), new DateTime(2014, 1, 3));

            _releaseWindowRepositoryMock.VerifyGet(r => r.Entities);
        }

        [Test]
        public void GetAllStartingInTimeRange_ShouldNotLookInProductRepository_WhenDateRangeIsValid()
        {
            Sut.GetAllStartingInTimeRange(new DateTime(2014, 1, 2), new DateTime(2014, 1, 3));

            _productRepositoryMock.VerifyGet(r => r.Entities, Times.Never());
        }

        [Test]
        public void GetAllStartingInTimeRange_ShouldReturnOnlyWindowReleasesInRangeFromRepository_WhenDateRangeIsValid()
        {
            var rangeDays = RandomData.RandomInt(5, 10);

            var startDate = new DateTime(2014, 1, 2);

            //create twice as much (excess will be out of the range)
            SetupRelaseWindowsWithProduct(rangeDays * 2, startDate);

            Sut.GetAllStartingInTimeRange(startDate, startDate.AddDays(rangeDays));

            _mappingEngineMock.Verify(
                me => me.Map<IEnumerable<ReleaseWindow>, IEnumerable<BusinessEntities.ReleaseCalendar.ReleaseWindowView>>(
                    It.Is<IEnumerable<ReleaseWindow>>(
                        list => list.Count() == rangeDays + 1)));
        }

        [Test]
        public void FindFirstOverlappedRelease_ShouldNotReturnReleases_WhenFinishTimeOfSpecifiedPeriodEqualsToExistingReleaseStartTime()
        {
            var product = RandomData.RandomString(1, 10);

            var startDate = RandomData.RandomDateTime(DateTimeKind.Utc);

            SetupRelaseWindowsWithProduct(1, startDate, product);

            Sut.FindFirstOverlappedRelease(product, startDate.AddHours(-5), startDate);

            _mappingEngineMock.Verify(o =>
                o.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(null));
        }

        [Test]
        public void FindFirstOverlappedRelease_ShouldReturnReleases_WhenFinishTimeOfSpecifiedPeriodGraterThenExistingReleaseStartTimeOneMinute()
        {
            var product = RandomData.RandomString(1, 10);

            var startDate = RandomData.RandomDateTime(DateTimeKind.Utc);
            var releases = SetupRelaseWindowsWithProduct(1, startDate, product);

            Sut.FindFirstOverlappedRelease(product, startDate.AddHours(-2), startDate.AddMinutes(1));

            _mappingEngineMock.Verify(o => o.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(
                    It.Is<ReleaseWindow>(x => x.ExternalId == releases.First().ExternalId)));
        }

        [Test]
        public void FindFirstOverlappedRelease_ShouldReturnReleases_WhenEndTimeOfRequestedPeriodGraterThenReleaseStartTime()
        {
            var product = RandomData.RandomString(1, 10);

            var startDate = RandomData.RandomDateTime(DateTimeKind.Utc);
            var releases = SetupRelaseWindowsWithProduct(1, startDate, product).ToList();

            Sut.FindFirstOverlappedRelease(product, startDate.AddDays(-2).AddMinutes(1), startDate.AddDays(releases.Count()).AddHours(2));

            _mappingEngineMock.Verify(o => o.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(
                    It.Is<ReleaseWindow>(x => x.ExternalId == releases.First().ExternalId)), Times.Exactly(releases.Count));
        }

        [Test]
        public void FindFirstOverlappedRelease_ShouldReturnReleases_WhenStartTimeOfRequestedPeriodLessThenReleaseEndTime()
        {
            var product = RandomData.RandomString(1, 10);

            var startDate = RandomData.RandomDateTime(DateTimeKind.Utc);
            var releases = SetupRelaseWindowsWithProduct(1, startDate, product).ToList();

            Sut.FindFirstOverlappedRelease(product, startDate.AddHours(1), startDate.AddHours(4));

            _mappingEngineMock.Verify(o => o.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(
                    It.Is<ReleaseWindow>(x => x.ExternalId == releases.First().ExternalId)), Times.Exactly(releases.Count));
        }

        [Test]
        public void FindFirstOverlappedRelease_ShouldReturnReleases_WhenRequestedPeriodGraterThenReleasePeriod()
        {
            var product = RandomData.RandomString(1, 10);

            var startDate = RandomData.RandomDateTime(DateTimeKind.Utc);
            var releases = SetupRelaseWindowsWithProduct(1, startDate, product).ToList();

            Sut.FindFirstOverlappedRelease(product, startDate.AddHours(-4), startDate.AddHours(4));

            _mappingEngineMock.Verify(o => o.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(
                    It.Is<ReleaseWindow>(x => x.ExternalId == releases.First().ExternalId)), Times.Exactly(releases.Count));
        }

        [Test]
        public void FindFirstOverlappedRelease_ShouldNotConsiderExistingReleases_WhenProductIsDifferent()
        {
            var product = RandomData.RandomString(1, 10);

            var startDate = RandomData.RandomDateTime(DateTimeKind.Utc);
            SetupRelaseWindowsWithProduct(1, startDate, RandomData.RandomString(1, 10));

            Sut.FindFirstOverlappedRelease(product, startDate, startDate.AddHours(2));

            _mappingEngineMock.Verify(o => o.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(null));
        }

        [Test]
        public void FindFirstOverlappedRelease_ShouldNotConsiderClosedExistingReleases_WhenInvoked()
        {
            var product = RandomData.RandomString(1, 10);

            var startDate = RandomData.RandomDateTime(DateTimeKind.Utc);
            SetupRelaseWindowsWithProduct(1, startDate, product,
                r =>
                {
                    r.Metrics = new List<Metric>
                    {
                        new Metric { ExecutedOn = RandomData.RandomDateTime(), MetricType = MetricType.Close }
                    };
                    return r;
                });

            Sut.FindFirstOverlappedRelease(product, startDate, startDate.AddHours(2));

            _mappingEngineMock.Verify(o => o.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(null));
        }

        [Test]
        public void FindFirstOverlappedRelease_ShouldNotConsiderReleasesForAutomatedProducts_WhenInvoked()
        {
            var product = RandomData.RandomString(1, 10);

            var startDate = RandomData.RandomDateTime(DateTimeKind.Utc);
            SetupRelaseWindowsWithProduct(1, startDate, product,
                r =>
                {
                    r.ReleaseProducts.First().Product.ReleaseTrack = ReleaseTrack.Automated;

                    return r;
                });

            Sut.FindFirstOverlappedRelease(product, startDate, startDate.AddHours(2));

            _mappingEngineMock.Verify(o => o.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(null));
        }

        [Test]
        public void FindFirstOverlappedRelease_ShouldNotConsiderMaintenanceReleases_WhenInvoked()
        {
            var product = RandomData.RandomString(1, 10);

            var startDate = RandomData.RandomDateTime(DateTimeKind.Utc);
            SetupRelaseWindowsWithProduct(1, startDate, product,
                r =>
                {
                    r.ReleaseType = ReleaseType.SystemMaintenance;

                    return r;
                });

            var result = Sut.FindFirstOverlappedRelease(product, startDate, startDate.AddHours(2));

            Assert.IsNull(result);

            _mappingEngineMock.Verify(o => o.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(null));
        }

        [Test]
        [ExpectedException(typeof(ProductNotFoundException))]
        public void GetNearReleases_ShouldRaiseProductNotFoundException_WhenProductInvalid()
        {
            Sut.GetNearReleases(string.Empty);
        }

        [Test]
        public void GetNearReleases_ShouldReturnTwoClosedReleases_WhenInvoked()
        {
            var product = RandomData.RandomString(5);

            var rows = Builder<ReleaseWindow>.CreateListOfSize(RandomData.RandomInt(5, 10))
                .All()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.ReleaseProducts, new[] { SetupReleaseProduct(product) })
                .With(o => o.Metrics, new List<Metric>
                    {
                        new Metric { ExecutedOn = RandomData.RandomDateTime(), MetricType = MetricType.Close }
                    })
                .Build();

            var requiredRows = rows.OrderByDescending(o => o.StartTime).Take(2).ToList();

            _releaseWindowRepositoryMock.SetupEntities(rows);

            Sut.GetNearReleases(product);

            _mappingEngineMock.Verify(
                me => me.Map<IEnumerable<ReleaseWindow>, IEnumerable<BusinessEntities.ReleaseCalendar.ReleaseWindow>>(
                    It.Is<IEnumerable<ReleaseWindow>>(
                        list => list.Count() == requiredRows.Count
                            && list.Select(o => o.ExternalId).ToArray().All(x => requiredRows.Any(y => y.ExternalId == x)))));
        }

        [Test]
        public void GetNearReleases_ShouldReturnThreeOpenReleases_WhenInvoked()
        {
            var product = RandomData.RandomString(5);

            var rows = Builder<ReleaseWindow>.CreateListOfSize(RandomData.RandomInt(5, 10))
                .All()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.ReleaseProducts, new[] { SetupReleaseProduct(product) })
                .With(o => o.Metrics, new List<Metric>())
                .Build();

            var requiredRows = rows.OrderBy(o => o.StartTime).Take(3).ToList();

            _releaseWindowRepositoryMock.SetupEntities(rows);

            Sut.GetNearReleases(product);

            _mappingEngineMock.Verify(
                me => me.Map<IEnumerable<ReleaseWindow>, IEnumerable<BusinessEntities.ReleaseCalendar.ReleaseWindow>>(
                    It.Is<IEnumerable<ReleaseWindow>>(
                        list => list.Count() == requiredRows.Count
                            && list.Select(o => o.ExternalId).ToArray().All(x => requiredRows.Any(y => y.ExternalId == x)))));
        }

        [Test]
        public void GetNearReleases_ShouldReturnMixedReleases_WhenInvoked()
        {
            var product = RandomData.RandomString(5);

            var rows = Builder<ReleaseWindow>.CreateListOfSize(20)
                .All()
                .With(o => o.ExternalId, Guid.NewGuid())
                .With(o => o.ReleaseProducts, new[] { SetupReleaseProduct(product) })
                .TheFirst(10)
                .With(o => o.Metrics, new List<Metric>())
                .TheLast(10)
                .With(o => o.Metrics, new List<Metric>
                    {
                        new Metric { ExecutedOn = RandomData.RandomDateTime(), MetricType = MetricType.Close }
                    })
                .Build();

            _releaseWindowRepositoryMock.SetupEntities(rows);

            Sut.GetNearReleases(product);

            _mappingEngineMock.Verify(
                me => me.Map<IEnumerable<ReleaseWindow>, IEnumerable<BusinessEntities.ReleaseCalendar.ReleaseWindow>>(
                    It.Is<IEnumerable<ReleaseWindow>>(
                        list => list.Count() == 5
                            && list.Count(o => !o.Metrics.Any(m => m.MetricType == MetricType.Close && m.ExecutedOn.HasValue)) == 3
                            && list.Count(o => o.Metrics.Any(m => m.MetricType == MetricType.Close && m.ExecutedOn.HasValue)) == 2)));
        }

        [Test]
        public void GetCurrentRelease_ShouldReturnRelease_WhenReleaseWillStartInFifteenMinutes()
        {
            var startTime = new DateTime(2015, 1, 1, 20, 0, 0, DateTimeKind.Utc);
            var endTime = new DateTime(2015, 1, 1, 22, 0, 0, DateTimeKind.Utc);
            var nowTime = new DateTime(2015, 1, 1, 19, 45, 0, DateTimeKind.Utc);
            var product = new Product { Description = RandomData.RandomString(10) };
            var release = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.ReleaseProducts, new[] { new ReleaseProduct { Product = product } })
                .With(x => x.Metrics, new[]
                {
                    new Metric { MetricType = MetricType.StartTime, ExecutedOn = startTime },
                    new Metric { MetricType = MetricType.EndTime, ExecutedOn = endTime }
                }).Build();
            SystemTime.Mock(nowTime);

            _releaseWindowRepositoryMock.SetupEntities(new[] { release });

            Sut.GetCurrentRelease(product.Description);

            _mappingEngineMock.Verify(x => x.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(release), Times.Once);
        }

        [Test]
        public void GetCurrentRelease_ShouldReturnRelease_WhenReleaseWhenNowIsOnStartTime()
        {
            var startTime = new DateTime(2015, 1, 1, 20, 0, 0, DateTimeKind.Utc);
            var endTime = new DateTime(2015, 1, 1, 22, 0, 0, DateTimeKind.Utc);
            var nowTime = startTime;
            var product = new Product { Description = RandomData.RandomString(10) };
            var release = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.ReleaseProducts, new[] { new ReleaseProduct { Product = product } })
                .With(x => x.Metrics, new[]
                {
                    new Metric { MetricType = MetricType.StartTime, ExecutedOn = startTime },
                    new Metric { MetricType = MetricType.EndTime, ExecutedOn = endTime }
                }).Build();
            SystemTime.Mock(nowTime);

            _releaseWindowRepositoryMock.SetupEntities(new[] { release });

            Sut.GetCurrentRelease(product.Description);

            _mappingEngineMock.Verify(x => x.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(release), Times.Once);
        }

        [Test]
        public void GetCurrentRelease_ShouldReturnRelease_WhenReleaseWhenNowIsOnEndTime()
        {
            var startTime = new DateTime(2015, 1, 1, 20, 0, 0, DateTimeKind.Utc);
            var endTime = new DateTime(2015, 1, 1, 22, 0, 0, DateTimeKind.Utc);
            var nowTime = startTime;
            var product = new Product { Description = RandomData.RandomString(10) };
            var release = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.ReleaseProducts, new[] { new ReleaseProduct { Product = product } })
                .With(x => x.Metrics, new[]
                {
                    new Metric { MetricType = MetricType.StartTime, ExecutedOn = startTime },
                    new Metric { MetricType = MetricType.EndTime, ExecutedOn = endTime }
                }).Build();
            SystemTime.Mock(nowTime);

            _releaseWindowRepositoryMock.SetupEntities(new[] { release });

            Sut.GetCurrentRelease(product.Description);

            _mappingEngineMock.Verify(x => x.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(release), Times.Once);
        }

        [Test]
        public void GetCurrentRelease_ShouldNotReturnRelease_WhenReleaseWillStartInSixteenMinutes()
        {
            var startTime = new DateTime(2015, 1, 1, 20, 0, 0, DateTimeKind.Utc);
            var endTime = new DateTime(2015, 1, 1, 22, 0, 0, DateTimeKind.Utc);
            var nowTime = new DateTime(2015, 1, 1, 19, 44, 0, DateTimeKind.Utc);
            var product = new Product { Description = RandomData.RandomString(10) };
            var release = Builder<ReleaseWindow>.CreateNew()
                .With(x => x.ExternalId, Guid.NewGuid())
                .With(x => x.ReleaseProducts, new[] { new ReleaseProduct { Product = product } })
                .With(x => x.Metrics, new[]
                {
                    new Metric { MetricType = MetricType.StartTime, ExecutedOn = startTime },
                    new Metric { MetricType = MetricType.EndTime, ExecutedOn = endTime }
                }).Build();
            SystemTime.Mock(nowTime);

            _releaseWindowRepositoryMock.SetupEntities(new[] { release });

            Sut.GetCurrentRelease(product.Description);

            _mappingEngineMock.Verify(x => x.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(null), Times.Once);
        }

        [Test]
        public void CloseRelease_ShouldFillOutClosedOnAndReleaseNotesAndInserMetric_WhenUpdatingReleaseWindowAndMetricNotExists()
        {
            var rows = Builder<ReleaseWindow>.CreateListOfSize(20)
                .All()
                .Do(o => o.ExternalId = Guid.NewGuid())
                .Do(o => o.Metrics = new List<Metric>())
                .Do(o => o.ReleaseWindowId = RandomData.RandomInt(int.MaxValue))
                .Build();

            _releaseWindowRepositoryMock.SetupEntities(rows);
            _metricsRepositoryMock.Setup(x => x.Insert(It.Is<Metric>(m => m.ReleaseWindowId == rows[5].ReleaseWindowId)))
                .Callback((Metric m) => rows[5].Metrics.Add(m));

            var releaseNotes = RandomData.RandomString(10);
            var now = DateTime.UtcNow;
            SystemTime.Mock(now);

            Sut.CloseRelease(releaseNotes, rows[5].ExternalId);

            Assert.AreEqual(releaseNotes, rows[5].ReleaseNotes.ReleaseNotes);
            Assert.AreEqual(now, rows[5].Metrics.First(m => m.MetricType == MetricType.Close).ExecutedOn);
            _releaseWindowRepositoryMock.Verify(x => x.Update(rows[5]));
            _metricsRepositoryMock.Verify(x => x.Insert(It.IsAny<Metric>()), Times.Once);
            _metricsRepositoryMock.Verify(x => x.Update(It.IsAny<Metric>()), Times.Never);
        }

        [Test]
        public void CloseRelease_ShouldFillOutClosedOnAndReleaseNotesAndUpdateMetric_WhenUpdatingReleaseWindowAndMetricExists()
        {
            var rows = Builder<ReleaseWindow>.CreateListOfSize(20)
                .All()
                .Do(o => o.ExternalId = Guid.NewGuid())
                .Do(o => o.Metrics = new List<Metric>())
                .Do(o => o.ReleaseWindowId = RandomData.RandomInt(int.MaxValue))
                .Build();
            var metric = new Metric
            {
                ExternalId = Guid.NewGuid(),
                MetricType = MetricType.Close,
                ReleaseWindowId = rows[5].ReleaseWindowId
            };

            _releaseWindowRepositoryMock.SetupEntities(rows);
            _metricsRepositoryMock.SetupEntities(new[] { metric });
            rows[5].Metrics.Add(metric);

            var releaseNotes = RandomData.RandomString(10);
            var now = DateTime.UtcNow;
            SystemTime.Mock(now);

            Sut.CloseRelease(releaseNotes, rows[5].ExternalId);

            Assert.AreEqual(releaseNotes, rows[5].ReleaseNotes.ReleaseNotes);
            Assert.AreEqual(now, rows[5].Metrics.First(m => m.MetricType == MetricType.Close).ExecutedOn);
            _releaseWindowRepositoryMock.Verify(x => x.Update(rows[5]));
            _metricsRepositoryMock.Verify(x => x.Insert(It.IsAny<Metric>()), Times.Never);
            _metricsRepositoryMock.Verify(x => x.Update(It.IsAny<Metric>()), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void CloseRelease_ShouldThrowException_WhenReleaseWindowNotFound()
        {
            _releaseWindowRepositoryMock.SetupEntities(new ReleaseWindow[] { });

            Sut.CloseRelease(string.Empty, Guid.NewGuid());
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void GetByExternalId_ShouldRaiseExceptionNonExistingId_WhenCheckForExistanceForce()
        {
            Sut.GetByExternalId(Guid.Empty, true);
        }

        [Test]
        public void GetByExternalId_ShouldReturnNullForNonExistingId_WhenCheckForExistanceNotForced()
        {
            var result = Sut.GetByExternalId(Guid.Empty);

            Assert.IsNull(result);
        }

        [Test]
        public void GetByExternalId_ShouldReturnReleaseWindowWithReleaseNotesAndPlugins_WhenReleaseExists()
        {
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ReleaseNotes = new ReleaseNote { ReleaseNotes = RandomData.RandomString(10), Issues = RandomData.RandomString(10) },
                ReleaseProducts = new List<ReleaseProduct>
                {
                    new ReleaseProduct { Product = new Product { PluginPackageConfiguration = new List<PluginPackageConfiguration>
                    {
                        new PluginPackageConfiguration
                        {
                            PluginId = RandomData.RandomInt(int.MaxValue),
                            Plugin = new DataEntities.Plugins.Plugin { ExternalId = Guid.NewGuid(), Key = RandomData.RandomString(10), PluginType = RandomData.RandomEnum<PluginType>() }
                        }
                    }}}
                }
            };
            _releaseWindowRepositoryMock.SetupEntities(new[] { releaseWindow });
            _mappingEngineMock.Setup(x => x.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(releaseWindow))
                .Returns((ReleaseWindow r) => new BusinessEntities.ReleaseCalendar.ReleaseWindow { ExternalId = releaseWindow.ExternalId });

            var result = Sut.GetByExternalId(releaseWindow.ExternalId, true, true);

            Assert.AreEqual(releaseWindow.ExternalId, result.ExternalId);
            Assert.AreEqual(releaseWindow.ReleaseNotes.ReleaseNotes, result.ReleaseNotes);
            Assert.AreEqual(releaseWindow.ReleaseNotes.Issues, result.Issues);
            Assert.AreEqual(releaseWindow.ReleaseProducts.First().Product.PluginPackageConfiguration.First().Plugin.ExternalId, result.Plugins.First().PluginId);
            Assert.AreEqual(releaseWindow.ReleaseProducts.First().Product.PluginPackageConfiguration.First().Plugin.Key, result.Plugins.First().PluginKey);
            Assert.AreEqual(releaseWindow.ReleaseProducts.First().Product.PluginPackageConfiguration.First().Plugin.PluginType, result.Plugins.First().PluginType);
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void SaveIssues_ShouldRaiseExceptionNonExistingId_WhenCheckForExistanceForce()
        {
            Sut.SaveIssues(new BusinessEntities.ReleaseCalendar.ReleaseWindow { ExternalId = Guid.NewGuid() });
        }

        [Test]
        public void SaveIssues_ShouldCreateReleaseNotes_WhenTheyDoesNotExist()
        {
            var releaseWindow = new ReleaseWindow { ExternalId = Guid.NewGuid() };
            var issues = RandomData.RandomString(44, 66);
            _releaseWindowRepositoryMock.SetupEntities(new List<ReleaseWindow> { releaseWindow });

            Sut.SaveIssues(new BusinessEntities.ReleaseCalendar.ReleaseWindow { ExternalId = releaseWindow.ExternalId, Issues = issues });

            _releaseWindowRepositoryMock.Verify(
                x =>
                    x.Update(
                        It.Is<ReleaseWindow>(
                            w =>
                                w.ExternalId == releaseWindow.ExternalId && w.ReleaseNotes != null &&
                                w.ReleaseNotes.Issues == issues)));
        }

        [Test]
        public void SaveIssues_ShouldJustSaveIssues_WhenReleaseNotesExist()
        {
            var releaseWindow = new ReleaseWindow
            {
                ExternalId = Guid.NewGuid(),
                ReleaseNotes = new ReleaseNote { Issues = RandomData.RandomString(33, 55) }
            };
            var issues = RandomData.RandomString(44, 66);
            _releaseWindowRepositoryMock.SetupEntities(new List<ReleaseWindow> { releaseWindow });

            Sut.SaveIssues(new BusinessEntities.ReleaseCalendar.ReleaseWindow { ExternalId = releaseWindow.ExternalId, Issues = issues });

            _releaseWindowRepositoryMock.Verify(
                x =>
                    x.Update(
                        It.Is<ReleaseWindow>(
                            w =>
                                w.ExternalId == releaseWindow.ExternalId && w.ReleaseNotes != null &&
                                w.ReleaseNotes.Issues == issues)));
        }

        [Test]
        public void UpdateReleaseDecision_ShouldUpdateReleaseDecisionField_WhenInvoked()
        {
            var rows = Builder<ReleaseWindow>.CreateListOfSize(20)
                .All()
                .Do(o => o.ExternalId = Guid.NewGuid())
                .Build();

            _releaseWindowRepositoryMock.SetupEntities(rows);

            var releaseDecision = RandomData.RandomEnum<ReleaseDecision>();

            Sut.UpdateReleaseDecision(rows[5].ExternalId, releaseDecision);

            Assert.AreEqual(releaseDecision, rows[5].ReleaseDecision);
            _releaseWindowRepositoryMock.Verify(x => x.Update(rows[5]));
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void UpdateReleaseDecision_ShouldThrowException_WhenReleaseWindowNotFound()
        {
            _releaseWindowRepositoryMock.SetupEntities(new ReleaseWindow[] { });

            Sut.UpdateReleaseDecision(Guid.NewGuid(), ReleaseDecision.Undetermined);
        }

        [Test]
        public void ApproveRelease_ShouldInserMetric_WhenMetricNotExists()
        {
            var rows = Builder<ReleaseWindow>.CreateListOfSize(20)
                .All()
                .Do(o => o.ExternalId = Guid.NewGuid())
                .Do(o => o.Metrics = new List<Metric>())
                .Do(o => o.ReleaseWindowId = RandomData.RandomInt(int.MaxValue))
                .Build();

            _releaseWindowRepositoryMock.SetupEntities(rows);
            _metricsRepositoryMock.Setup(x => x.Insert(It.Is<Metric>(m => m.ReleaseWindowId == rows[5].ReleaseWindowId)))
                .Callback((Metric m) => rows[5].Metrics.Add(m));

            var now = DateTime.UtcNow;
            SystemTime.Mock(now);

            Sut.ApproveRelease(rows[5].ExternalId);

            Assert.AreEqual(now, rows[5].Metrics.First(m => m.MetricType == MetricType.Approve).ExecutedOn);
            _metricsRepositoryMock.Verify(x => x.Insert(It.IsAny<Metric>()), Times.Once);
            _metricsRepositoryMock.Verify(x => x.Update(It.IsAny<Metric>()), Times.Never);
        }

        [Test]
        public void ApproveRelease_ShouldUpdateMetric_WhenMetricExists()
        {
            var rows = Builder<ReleaseWindow>.CreateListOfSize(20)
                .All()
                .Do(o => o.ExternalId = Guid.NewGuid())
                .Do(o => o.Metrics = new List<Metric>())
                .Do(o => o.ReleaseWindowId = RandomData.RandomInt(int.MaxValue))
                .Build();
            var metric = new Metric
            {
                ExternalId = Guid.NewGuid(),
                MetricType = MetricType.Approve,
                ReleaseWindowId = rows[5].ReleaseWindowId
            };

            _releaseWindowRepositoryMock.SetupEntities(rows);
            _metricsRepositoryMock.SetupEntities(new[] { metric });
            rows[5].Metrics.Add(metric);

            var now = DateTime.UtcNow;
            SystemTime.Mock(now);

            Sut.ApproveRelease(rows[5].ExternalId);

            Assert.AreEqual(now, rows[5].Metrics.First(m => m.MetricType == MetricType.Approve).ExecutedOn);
            _metricsRepositoryMock.Verify(x => x.Insert(It.IsAny<Metric>()), Times.Never);
            _metricsRepositoryMock.Verify(x => x.Update(It.IsAny<Metric>()), Times.Once);
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void ApproveRelease_ShouldThrowException_WhenReleaseWindowNotFound()
        {
            _releaseWindowRepositoryMock.SetupEntities(new ReleaseWindow[] { });

            Sut.ApproveRelease(Guid.NewGuid());
        }

        [Test]
        public void GetExpiredReleases_ShouldReturnTwoClosedReleases_WhenInvoked()
        {
            var product = RandomData.RandomString(5);

            var rows = new[]
            {
                Builder<ReleaseWindow>.CreateNew()
                    .With(o => o.ExternalId, Guid.NewGuid())
                    .With(o => o.ReleaseProducts, new[] {SetupReleaseProduct(product)})
                    .With(o => o.Metrics, new List<Metric>
                        {
                            new Metric { ExecutedOn = RandomData.RandomDateTime(), MetricType = MetricType.Close }
                        })
                    .Build(),

                Builder<ReleaseWindow>.CreateNew()
                    .With(o => o.ExternalId, Guid.NewGuid())
                    .With(o => o.ReleaseProducts, new[] {SetupReleaseProduct(product)})
                    .With(o => o.Metrics, new List<Metric>
                        {
                            new Metric { ExecutedOn = RandomData.RandomDateTime(), MetricType = MetricType.Close },
                            new Metric { ExecutedOn = DateTime.UtcNow.AddHours(-1), MetricType = MetricType.EndTime }
                        })
                    .Build(),

                Builder<ReleaseWindow>.CreateNew()
                    .With(o => o.ExternalId, Guid.NewGuid())
                    .With(o => o.ReleaseProducts, new[] {SetupReleaseProduct(product)})
                    .With(o => o.Metrics, new List<Metric>
                    {
                        new Metric { ExecutedOn = null, MetricType = MetricType.Close },
                        new Metric { ExecutedOn = DateTime.UtcNow.AddHours(-1), MetricType = MetricType.EndTime }
                    })
                    .Build()
            };

            var requiredRow = rows[2];

            _releaseWindowRepositoryMock.SetupEntities(rows);

            Sut.GetExpiredReleases();

            _mappingEngineMock.Verify(
                x => x.Map<IEnumerable<ReleaseWindow>, IEnumerable<BusinessEntities.ReleaseCalendar.ReleaseWindow>>(
                    It.Is<IEnumerable<ReleaseWindow>>(
                        list => list.Count() == 1
                            && list.First().ExternalId == requiredRow.ExternalId)));
        }

        [Test]
        [ExpectedException(typeof(AccountNotFoundException))]
        public void Create_ShouldThrowException_WhenAccountNotFound()
        {
            _accountRepositoryMock.SetupEntities(new[] { new Account() });

            Sut.Create(new BusinessReleaseWindow(), Guid.NewGuid());
        }

        [Test]
        [ExpectedException(typeof(ProductNotFoundException))]
        public void Create_ShouldThrowException_WhenInvalidProductUsed()
        {
            var product = SetupProduct("P1");
            var account = new Account { ExternalId = Guid.NewGuid() };

            var release = Builder<BusinessReleaseWindow>.CreateNew()
                .With(x => x.ExternalId)
                .With(x => x.Products = new[] { "PX" })
                .Build();

            var dataRelease = BuildReleaseWindows(product, 1, DateTime.UtcNow.AddDays(1)).First();

            _mappingEngineMock.Setup(x => x.Map<BusinessReleaseWindow, ReleaseWindow>(release))
                .Returns(dataRelease);
            _accountRepositoryMock.SetupEntities(new[] { account });

            Sut.Create(release, account.ExternalId);
        }

        [Test]
        [ExpectedException(typeof(ProductNotFoundException))]
        public void Create_ShouldThrowException_WhenNotAllProductNamesResolved()
        {
            var products = SetupProducts(new[] { "P1", "P2" });
            var account = new Account { ExternalId = Guid.NewGuid() };

            var release = Builder<BusinessReleaseWindow>.CreateNew()
                .With(x => x.ExternalId)
                .With(x => x.Products = new[] { "P1", "PX" })
                .Build();

            var dataRelease = BuildReleaseWindows(products.First(), 1, DateTime.UtcNow.AddDays(1)).First();

            _mappingEngineMock.Setup(x => x.Map<BusinessReleaseWindow, ReleaseWindow>(release))
                .Returns(dataRelease);
            _accountRepositoryMock.SetupEntities(new[] { account });

            Sut.Create(release, account.ExternalId);
        }

        [Test]
        public void Create_ShouldAssignReleaseWithProduct_WhenInvoked()
        {
            var product = SetupProduct("P1");
            var account = new Account { ExternalId = Guid.NewGuid() };

            var release = Builder<BusinessReleaseWindow>.CreateNew()
                .With(x => x.ExternalId)
                .With(x => x.Products = new[] { product.Description })
                .Build();

            var dataRelease = BuildReleaseWindows(product, 1, DateTime.UtcNow.AddDays(1)).First();

            _mappingEngineMock.Setup(x => x.Map<BusinessReleaseWindow, ReleaseWindow>(release))
                .Returns(dataRelease);
            _accountRepositoryMock.SetupEntities(new[] { account });

            Sut.Create(release, account.ExternalId);

            _releaseProductGatewayMock.Verify(x => x.AssignProductsToRelease(release.ExternalId, It.Is<IEnumerable<string>>(p => p.Count() == 1 && p.First() == "P1")));
        }

        [Test]
        [ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void CloseFailedRelease_ShouldThrowException_WhenReleaseNotFound()
        {
            var product = SetupProduct("P1");

            var release = Builder<BusinessReleaseWindow>.CreateNew()
                .With(x => x.ExternalId)
                .With(x => x.Products = new[] { product.Description })
                .Build();

            Sut.CloseFailedRelease(release.ExternalId, "issues");
        }

        [Test]
        public void CloseFailedRelease_ShouldCreateReleaseNotes_WhenReleaseNotesNotCreated()
        {
            var releases = SetupRelaseWindowsWithProduct(1, DateTime.UtcNow, "P1").ToList();

            Sut.CloseFailedRelease(releases.First().ExternalId, "issues");

            _releaseWindowRepositoryMock.Verify(x =>
                x.Update(It.Is<ReleaseWindow>(r =>
                    r.ReleaseNotes != null
                    && r.ReleaseNotes.Issues == "issues"
                )));
        }

        [Test]
        public void CloseFailedRelease_ShouldUpdateIssues_WhenReleaseNotesAlreadyExists()
        {
            var releases = SetupRelaseWindowsWithProduct(1, DateTime.UtcNow, "P1", r =>
            {
                r.ReleaseNotes = new ReleaseNote { Issues = "old issues" };
                return r;
            }).ToList();

            Sut.CloseFailedRelease(releases.First().ExternalId, "issues");

            _releaseWindowRepositoryMock.Verify(x =>
                x.Update(It.Is<ReleaseWindow>(r =>
                    r.ReleaseNotes != null
                    && r.ReleaseNotes.Issues == "old issues" + Environment.NewLine + Environment.NewLine + "issues"
                )));
        }

        [Test]
        public void CloseFailedRelease_ShouldMarkReleaseAsFailed_WhenInvoked()
        {
            var releases = SetupRelaseWindowsWithProduct(1, DateTime.UtcNow, "P1").ToList();

            Sut.CloseFailedRelease(releases.First().ExternalId, "issues");

            _releaseWindowRepositoryMock.Verify(x =>
                x.Update(It.Is<ReleaseWindow>(r => r.IsFailed)));
        }

        [Test]
        public void CloseFailedRelease_ShouldMarkMetricsAsCompleted_WhenInvoked()
        {
            var releases = SetupRelaseWindowsWithProduct(1, DateTime.UtcNow, "P1").ToList();

            _metricsRepositoryMock.SetupEntities(new[]
            {
                new Metric{ExecutedOn = DateTime.UtcNow, MetricId = 1, MetricType = MetricType.StartRun, ReleaseWindow = releases.First()},
                new Metric{ExecutedOn = DateTime.UtcNow, MetricId = 2, MetricType = MetricType.SiteDown, ReleaseWindow = releases.First()},
                new Metric{ExecutedOn = DateTime.UtcNow, MetricId = 3, MetricType = MetricType.SiteUp, ReleaseWindow = releases.First()},
                new Metric{ExecutedOn = null, MetricId = 4, MetricType = MetricType.SignOff, ReleaseWindow = releases.First()},
                new Metric{ExecutedOn = null, MetricId = 5, MetricType = MetricType.FinishRun, ReleaseWindow = releases.First()}
            });


            Sut.CloseFailedRelease(releases.First().ExternalId, "issues");

            _metricsRepositoryMock.Verify(x =>
                x.Update(It.IsAny<Metric>()), Times.Exactly(2));
            _metricsRepositoryMock.Verify(x =>
                x.Update(It.Is<Metric>(m => m.ExecutedOn.HasValue && m.MetricId == 4)));
            _metricsRepositoryMock.Verify(x =>
                x.Update(It.Is<Metric>(m => m.ExecutedOn.HasValue && m.MetricId == 5)));
        }


        private Product SetupProduct(string name = null, int? id = null)
        {
            var product = Builder<Product>.CreateNew()
                .With(o => o.ProductId, id ?? RandomData.RandomInt(int.MaxValue))
                .With(o => o.Description, name ?? RandomData.RandomString(50))
                .With(o => o.ReleaseTrack, ReleaseTrack.Manual)
                .Build();

            _productRepositoryMock.SetupEntities(new[] { product });

            return product;
        }

        private IEnumerable<Product> SetupProducts(IEnumerable<string> names = null)
        {
            var products = names.Select(x => Builder<Product>.CreateNew()
                .With(o => o.ProductId, RandomData.RandomInt(int.MaxValue))
                .With(o => o.Description, x ?? RandomData.RandomString(50))
                .With(o => o.ReleaseTrack, ReleaseTrack.Manual)
                .Build()).ToList();

            _productRepositoryMock.SetupEntities(products);

            return products;
        }

        private ReleaseProduct SetupReleaseProduct(string name = null, int? id = null)
        {
            var product = SetupProduct(name, id);

            return new ReleaseProduct { Product = product };
        }

        private IEnumerable<ReleaseWindow> SetupRelaseWindowsWithProduct(int releaseWindowCount, DateTime startDate, string product = null, Func<ReleaseWindow, ReleaseWindow> testDataCustomiser = null)
        {
            var dbProduct = SetupProduct(product);

            IEnumerable<ReleaseWindow> releaseWindows = BuildReleaseWindows(dbProduct, releaseWindowCount, startDate).ToList();

            if (testDataCustomiser != null)
                releaseWindows = releaseWindows.Select(testDataCustomiser).ToList();

            _releaseWindowRepositoryMock.SetupEntities(releaseWindows);

            return releaseWindows;
        }

        private static IEnumerable<ReleaseWindow> BuildReleaseWindows(Product product, int releaseWindowCount, DateTime startDate)
        {
            //generator is used to increment daily the release window
            var dayGenerator = new SequentialGenerator<int> { Direction = GeneratorDirection.Ascending, Increment = 1 };
            dayGenerator.StartingWith(0);

            IList<ReleaseWindow> releaseWindows = Builder<ReleaseWindow>
                .CreateListOfSize(releaseWindowCount)
                .All()
                .With(rw => rw.ReleaseProducts = new[] { new ReleaseProduct { Product = product } })
                .And(rw => rw.StartTime = startDate.AddDays(dayGenerator.Generate()))
                .And(rw => rw.ExternalId = Guid.NewGuid())
                .Build();

            foreach (var releaseWindow in releaseWindows)
            {
                releaseWindow.Metrics = new List<Metric>
                {
                    new Metric
                    {
                        ExecutedOn = releaseWindow.StartTime,
                        MetricType = MetricType.StartTime,
                        ExternalId = Guid.NewGuid()
                    },
                    new Metric
                    {
                        ExecutedOn = releaseWindow.StartTime.AddHours(2),
                        MetricType = MetricType.EndTime,
                        ExternalId = Guid.NewGuid()
                    },
                };
            }

            return releaseWindows;
        }

    }
}
