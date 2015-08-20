using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using ReMi.BusinessEntities.Metrics;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Utils;
using ReMi.Common.Utils.Repository;
using ReMi.TestUtils.UnitTests;
using ReMi.DataAccess.BusinessEntityGateways.Metrics;
using ReMi.DataAccess.Exceptions;
using ReMi.DataEntities.Products;
using ReMi.DataEntities.ReleaseCalendar;
using DataMetric = ReMi.DataEntities.Metrics.Metric;

namespace ReMi.DataAccess.Tests.Metrics
{
    public class MetricsGatewayTests : TestClassFor<MetricsGateway>
    {
        private Mock<IRepository<DataMetric>> _metricRepositoryMock;
        private Mock<IRepository<ReleaseWindow>> _releaseWindowRepositoryMock;
        private Mock<IMappingEngine> _mappingEngineMock;
        private Mock<IApplicationSettings> _applicationSettingsMock;
        private List<ReleaseWindow> _releaseWindows;
        private List<DataMetric> _metrics;
        private const int DefaultReleaseWindowDurationTime = 120;

        protected override MetricsGateway ConstructSystemUnderTest()
        {
            return new MetricsGateway
            {
                MetricsRepository = _metricRepositoryMock.Object,
                ReleaseWindowRepository = _releaseWindowRepositoryMock.Object,
                Mapper = _mappingEngineMock.Object,
                ApplicationSettings = _applicationSettingsMock.Object
            };
        }

        protected override void TestInitialize()
        {
            _metricRepositoryMock = new Mock<IRepository<DataMetric>>();
            _releaseWindowRepositoryMock = new Mock<IRepository<ReleaseWindow>>();

            _applicationSettingsMock = new Mock<IApplicationSettings>();
            _applicationSettingsMock.SetupGet(o => o.DefaultReleaseWindowDurationTime)
                .Returns(DefaultReleaseWindowDurationTime);

            _mappingEngineMock = new Mock<IMappingEngine>();
            _mappingEngineMock.Setup(
                m => m.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(It.IsAny<ReleaseWindow>()))
                    .Returns<ReleaseWindow>(r => new BusinessEntities.ReleaseCalendar.ReleaseWindow
                    {
                        ExternalId = r.ExternalId,
                        Products = r.ReleaseProducts != null ? r.ReleaseProducts.Select(p => p.Product.Description) : null
                    });

            base.TestInitialize();
        }

        [Test, ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void GetMetrics_ShouldThrowException_WhenReleaseWindowNotFound()
        {
            Sut.GetMetrics(Guid.NewGuid());
        }

        [Test]
        public void GetMetrics_ShouldReturnEmptyMetric_WhenReleaseWindowDoesNotContainIt()
        {
            var metrics = Sut.GetMetrics("test");

            Assert.AreEqual(0, metrics.Count());
        }

        [Test]
        public void GetMetrics_ShouldReturnBackgroundMetricsAndCallMapper_AfterRetrieveMetrics()
        {
            var releaseWindowId = RandomData.RandomInt(1, int.MaxValue);

            var metrics = SetupDataMetrics(releaseWindowId, new[] { MetricType.SiteDown, MetricType.SiteUp });

            var releaseWindows = SetupReleaseWindows(releaseWindowId, "test", metrics).ToList();
            var releaseWindow = releaseWindows.First(o => o.ReleaseWindowId == releaseWindowId);

            Sut.GetMetrics(releaseWindow.ExternalId, false);

            _mappingEngineMock.Verify(
                m =>
                    m.Map<IEnumerable<DataMetric>, IEnumerable<Metric>>(
                        It.Is<IEnumerable<DataMetric>>(
                            x => x.Count() == 2)));
        }

        [Test]
        public void GetMetrics_ShouldCallMapper_AfterRetrieveMetrics()
        {
            var releaseWindowId = RandomData.RandomInt(1, int.MaxValue);

            var metrics = SetupDataMetrics(releaseWindowId, new[] { MetricType.SiteDown, MetricType.SiteUp });

            var releaseWindows = SetupReleaseWindows(releaseWindowId, "test", metrics).ToList();
            var releaseWindow = releaseWindows.First(o => o.ReleaseWindowId == releaseWindowId);

            Sut.GetMetrics(releaseWindow.ExternalId);

            _mappingEngineMock.Verify(
                m =>
                    m.Map<IEnumerable<DataMetric>, IEnumerable<Metric>>(
                        It.Is<IEnumerable<DataMetric>>(
                            x => x.Count() == 2)));
        }

        [Test, ExpectedException(typeof(ReleaseWindowNotFoundException))]
        public void GetMetric_ShouldThrowException_WhenReleaseWindowNotFound()
        {
            Sut.GetMetric(Guid.NewGuid(), MetricType.SiteUp);
        }

        [Test]
        public void GetMetric_ShouldReturnNull_WhenReleaseWindowDoesNotContainMetricType()
        {
            var releaseWindowId = RandomData.RandomInt(1, int.MaxValue);

            var metrics = SetupDataMetrics(releaseWindowId, new[] { MetricType.SiteDown, MetricType.SiteUp });

            var releaseWindows = SetupReleaseWindows(releaseWindowId, "test", metrics).ToList();
            var releaseWindow = releaseWindows.First(o => o.ReleaseWindowId == releaseWindowId);


            var metric = Sut.GetMetric(releaseWindow.ExternalId, MetricType.SiteUp);

            Assert.IsNull(metric);
        }

        [Test]
        public void GetMetric_ShouldCallMapper_AfterRetrieveMetric()
        {
            var releaseWindowId = RandomData.RandomInt(1, int.MaxValue);

            var metrics = SetupDataMetrics(releaseWindowId, new[] { MetricType.SiteDown, MetricType.SiteUp }, 0).ToList();

            var releaseWindows = SetupReleaseWindows(releaseWindowId, "test", metrics).ToList();
            var releaseWindow = releaseWindows.First(o => o.ReleaseWindowId == releaseWindowId);


            Sut.GetMetric(releaseWindow.ExternalId, MetricType.SiteDown);

            _mappingEngineMock.Verify(
                m => m.Map<DataMetric, Metric>(It.Is<DataMetric>(x => x.ExternalId == metrics.ElementAt(0).ExternalId && x.ExecutedOn == metrics.ElementAt(0).ExecutedOn)));
        }

        [Test]
        public void UpdateMetrics_ShouldCallUpdate()
        {
            var metrics = SetupDataMetrics(1, new[] { MetricType.Close }, 0).ToArray();

            var metric = new Metric
            {
                ExecutedOn = RandomData.RandomDateTime(DateTimeKind.Utc),
                ExternalId = metrics[0].ExternalId
            };

            Sut.UpdateMetrics(metric);

            _metricRepositoryMock.Verify(
                m =>
                    m.Update(
                        It.Is<DataMetric>(
                            d =>
                                d.MetricId == metrics[0].MetricId
                                && d.ExecutedOn.Value == metric.ExecutedOn.Value
                                && d.ExecutedOn.Value.Kind == DateTimeKind.Utc
                                && d.ExternalId == metric.ExternalId)));
        }

        [Test]
        public void UpdateMetrics_ShouldCallUpdateAndConvertToUtc_WhenLocalTimeProvided()
        {
            var metrics = SetupDataMetrics(1, new[] { MetricType.Close }, 0).ToArray();

            var metric = new Metric
            {
                ExecutedOn = RandomData.RandomDateTime(),
                ExternalId = metrics[0].ExternalId
            };

            Sut.UpdateMetrics(metric);

            _metricRepositoryMock.Verify(
                m =>
                    m.Update(
                        It.Is<DataMetric>(
                            d =>
                                d.MetricId == metrics[0].MetricId
                                && d.ExecutedOn.Value == metric.ExecutedOn.Value.ToUniversalTime()
                                && d.ExecutedOn.Value.Kind == DateTimeKind.Utc
                                && d.ExternalId == metric.ExternalId)));
        }

        [Test]
        public void GetMetrics_ShouldReturnTwoReleaseWindowsWithMetrics_WhenInvoked()
        {
            var releaseWindowId1 = RandomData.RandomInt(1, 1000);
            var signOff1 = RandomData.RandomDateTime(DateTimeKind.Utc);

            var releaseWindowId2 = RandomData.RandomInt(1001, 2000);
            var signOff2 = RandomData.RandomDateTime(DateTimeKind.Utc);

            var metrics = new List<DataMetric>
            {
                new DataMetric
                {
                    MetricType = MetricType.SiteDown,
                    ExternalId = Guid.NewGuid(),
                    ExecutedOn = DateTime.UtcNow,
                    ReleaseWindowId = releaseWindowId1
                },
                new DataMetric
                {
                    MetricType = MetricType.StartDeploy,
                    ExternalId = Guid.NewGuid(),
                    ExecutedOn = DateTime.UtcNow.AddMinutes(1),
                    ReleaseWindowId = releaseWindowId1
                },
                new DataMetric
                {
                    MetricType = MetricType.FinishDeploy,
                    ExternalId = Guid.NewGuid(),
                    ExecutedOn = DateTime.UtcNow.AddMinutes(2),
                    ReleaseWindowId = releaseWindowId1
                },
                new DataMetric
                {
                    MetricType = MetricType.SiteUp,
                    ExternalId = Guid.NewGuid(),
                    ExecutedOn = DateTime.UtcNow.AddMinutes(3),
                    ReleaseWindowId = releaseWindowId1
                },
                new DataMetric
                {
                    MetricType = MetricType.SignOff,
                    ExternalId = Guid.NewGuid(),
                    ExecutedOn = signOff1,
                    ReleaseWindowId = releaseWindowId1
                },
                new DataMetric
                {
                    MetricType = MetricType.SignOff,
                    ExternalId = Guid.NewGuid(),
                    ExecutedOn = signOff2,
                    ReleaseWindowId = releaseWindowId2
                },
                new DataMetric
                {
                    MetricType = MetricType.SiteDown,
                    ExternalId = Guid.NewGuid(),
                    ExecutedOn = DateTime.UtcNow.AddDays(2),
                    ReleaseWindowId = releaseWindowId2
                }
            };

            var releaseWindows = new List<ReleaseWindow>
            {
                new ReleaseWindow
                {
                    ReleaseWindowId = releaseWindowId1,
                    ExternalId = Guid.NewGuid(),
                    ReleaseProducts = Builder<ReleaseProduct>
                        .CreateListOfSize(1)
                        .All()
                        .With(x => x.Product, new Product{Description = "test"})
                        .Build(),
                    StartTime = DateTime.UtcNow.AddHours(-1),
                    Metrics = metrics.Where(m => m.ReleaseWindowId == releaseWindowId1).ToList(),
                    ReleaseType = ReleaseType.Pci
                },
                new ReleaseWindow
                {
                    ReleaseWindowId = releaseWindowId2,
                    ExternalId = Guid.NewGuid(),
                    ReleaseProducts = Builder<ReleaseProduct>
                        .CreateListOfSize(1)
                        .All()
                        .With(x => x.Product, new Product{Description = "test"})
                        .Build(),
                    StartTime = DateTime.UtcNow.AddHours(-1),
                    Metrics = metrics.Where(m => m.ReleaseWindowId == releaseWindowId2).ToList(),
                    ReleaseType = ReleaseType.Hotfix
                },
                new ReleaseWindow
                {
                    ReleaseWindowId = RandomData.RandomInt(66, 77),
                    ExternalId = Guid.NewGuid(),
                    ReleaseProducts = Builder<ReleaseProduct>
                        .CreateListOfSize(1)
                        .All()
                        .With(x => x.Product, new Product{Description = "a"})
                        .Build(),
                }
            };

            _releaseWindowRepositoryMock.SetupEntities(releaseWindows);
            _metricRepositoryMock.SetupEntities(metrics);

            var result = Sut.GetMetrics("test");

            Assert.AreEqual(2, result.Count, "result size");
            Assert.AreEqual(releaseWindows[1].ExternalId, result.ElementAt(1).Key.ExternalId);
            Assert.AreEqual(releaseWindows[0].ExternalId, result.ElementAt(0).Key.ExternalId);

            _mappingEngineMock.Verify(
                m =>
                    m.Map<ReleaseWindow, BusinessEntities.ReleaseCalendar.ReleaseWindow>(
                       It.Is<ReleaseWindow>(r =>
                           r.ReleaseWindowId == releaseWindowId1
                           && r.Metrics.First(x => x.MetricType == MetricType.SignOff).ExecutedOn.Value == signOff1
                        )
                    ));

            _mappingEngineMock.Verify(
                m =>
                    m.Map<IEnumerable<DataMetric>, IEnumerable<Metric>>(
                       It.Is<IEnumerable<DataMetric>>(items =>
                           items.Count() == 5
                        )
                    ), Times.Once, "metrics count");

            _mappingEngineMock.Verify(
                m =>
                    m.Map<IEnumerable<DataMetric>, IEnumerable<Metric>>(
                       It.Is<IEnumerable<DataMetric>>(items =>
                           items.Count() == 2
                        )
                    ), Times.Once, "metrics count");
        }

        [Test]
        public void CreateMetrics_ShouldCreateDeployMetrics_WhenInvoked()
        {
            var releases = SetupReleaseWindows(1, RandomData.RandomString(5), null, ReleaseType.Scheduled, 0).ToList();

            Sut.CreateMetrics(releases.First().ExternalId, new[] { MetricType.FinishDeploy, MetricType.StartDeploy });

            _metricRepositoryMock.Verify(m => m.Insert(It.IsAny<DataMetric>()), Times.Exactly(2));

            _metricRepositoryMock.Verify(
                m => m.Insert(
                    It.Is<DataMetric>(x => x.MetricType == MetricType.StartDeploy
                        && x.ExecutedOn == null
                        && x.ReleaseWindowId == releases.First().ReleaseWindowId)
                    )
                );

            _metricRepositoryMock.Verify(
                m => m.Insert(
                    It.Is<DataMetric>(x => x.MetricType == MetricType.FinishDeploy
                        && x.ExecutedOn == null
                        && x.ReleaseWindowId == releases.First().ReleaseWindowId)
                    )
                );
        }

        [Test]
        public void CreateMetrics_ShouldCreateMetricAndConvertTimeToUtc_WhenInvoked()
        {
            var releases = SetupReleaseWindows(1, RandomData.RandomString(5), null, ReleaseType.Scheduled, 0).ToList();
            var time1 = RandomData.RandomDateTime();
            var time2 = RandomData.RandomDateTime(DateTimeKind.Utc);

            Sut.CreateMetrics(releases.First().ExternalId, new Dictionary<MetricType, DateTime?>
            {
                {MetricType.FinishDeploy, time1 },
                {MetricType.StartDeploy, time2 }
            });

            _metricRepositoryMock.Verify(m => m.Insert(It.IsAny<DataMetric>()), Times.Exactly(2));
            
            _metricRepositoryMock.Verify(
                m => m.Insert(
                    It.Is<DataMetric>(x => x.MetricType == MetricType.FinishDeploy
                        && x.ExecutedOn.Value == time1.ToUniversalTime()
                        && x.ExecutedOn.Value.Kind == DateTimeKind.Utc
                        && x.ReleaseWindowId == releases.First().ReleaseWindowId)
                    )
                );

            _metricRepositoryMock.Verify(
                m => m.Insert(
                    It.Is<DataMetric>(x => x.MetricType == MetricType.StartDeploy
                        && x.ExecutedOn.Value == time2
                        && x.ExecutedOn.Value.Kind == DateTimeKind.Utc
                        && x.ReleaseWindowId == releases.First().ReleaseWindowId)
                    )
                );
        }

        [Test]
        public void CreateMetrics_ShouldCreateSiteMetrics_WhenInvoked()
        {
            SetupDefaults();

            var window = new BusinessEntities.ReleaseCalendar.ReleaseWindow
            {
                ExternalId = _releaseWindows[1].ExternalId,
                ReleaseType = ReleaseType.Scheduled,
                RequiresDowntime = true
            };

            Sut.CreateMetrics(window.ExternalId, new[] { MetricType.SiteDown, MetricType.SiteUp });

            _metricRepositoryMock.Verify(m =>
                m.Insert(It.IsAny<DataMetric>()), Times.Exactly(2));

            _metricRepositoryMock.Verify(m =>
                    m.Insert(It.Is<DataMetric>(x =>
                        x.MetricType == MetricType.SiteDown && x.ExecutedOn == null)));

            _metricRepositoryMock.Verify(m =>
                    m.Insert(It.Is<DataMetric>(x =>
                        x.MetricType == MetricType.SiteUp && x.ExecutedOn == null)));
        }

        [Test]
        public void CreateMetrics_ShouldCreateStartEndMetrics_WhenInvoked()
        {
            SetupDefaults();

            _releaseWindows[1].StartTime = RandomData.RandomDateTime(DateTimeKind.Utc);

            Sut.CreateMetrics(_releaseWindows[1].ExternalId, new[] { MetricType.StartTime, MetricType.EndTime });

            _metricRepositoryMock.Verify(m =>
                m.Insert(It.IsAny<DataMetric>()), Times.Exactly(2));

            _metricRepositoryMock.Verify(m =>
                    m.Insert(It.Is<DataMetric>(x =>
                        x.MetricType == MetricType.StartTime && x.ExecutedOn == _releaseWindows[1].StartTime)));

            _metricRepositoryMock.Verify(m =>
                    m.Insert(It.Is<DataMetric>(x =>
                        x.MetricType == MetricType.EndTime && x.ExecutedOn == _releaseWindows[1].StartTime.AddMinutes(DefaultReleaseWindowDurationTime))));

        }

        [Test]
        public void CreateMetrics_ShouldCreateRunMetrics_WhenInvoked()
        {
            SetupDefaults();

            var window = new BusinessEntities.ReleaseCalendar.ReleaseWindow
            {
                ExternalId = _releaseWindows[1].ExternalId,
                ReleaseType = ReleaseType.Pci,
                RequiresDowntime = true,
            };

            Sut.CreateMetrics(window.ExternalId, new[] { MetricType.FinishRun, MetricType.StartRun });

            _metricRepositoryMock.Verify(m =>
                    m.Insert(It.Is<DataMetric>(x =>
                        x.MetricType == MetricType.FinishRun && x.ExecutedOn == null)));

            _metricRepositoryMock.Verify(m =>
                    m.Insert(It.Is<DataMetric>(x =>
                        x.MetricType == MetricType.StartRun && x.ExecutedOn == null)));
        }

        [Test]
        public void CreateMetrics_ShouldCreateDictionaryMetrics_WhenInvoked()
        {
            var release = SetupReleaseWindows(1).First(x => x.ReleaseWindowId == 1);
            var dictionary = new Dictionary<MetricType, DateTime?>
            {
                {MetricType.Approve, null},
                {MetricType.Close, RandomData.RandomDateTime(DateTimeKind.Utc)},
                {MetricType.SignOff, null},
                {MetricType.Complete, RandomData.RandomDateTime(DateTimeKind.Utc)},
            };

            Sut.CreateMetrics(release.ExternalId, dictionary);

            _metricRepositoryMock.Verify(m =>
                m.Insert(It.IsAny<DataMetric>()), Times.Exactly(4));

            _metricRepositoryMock.Verify(m =>
                    m.Insert(It.Is<DataMetric>(x =>
                        x.MetricType == MetricType.Approve && x.ExecutedOn == null)));

            _metricRepositoryMock.Verify(m =>
                    m.Insert(It.Is<DataMetric>(x =>
                        x.MetricType == MetricType.Close && x.ExecutedOn == dictionary[MetricType.Close])));

            _metricRepositoryMock.Verify(m =>
                    m.Insert(It.Is<DataMetric>(x =>
                        x.MetricType == MetricType.SignOff && x.ExecutedOn == null)));

            _metricRepositoryMock.Verify(m =>
                    m.Insert(It.Is<DataMetric>(x =>
                        x.MetricType == MetricType.Complete && x.ExecutedOn == dictionary[MetricType.Complete])));

        }

        [Test]
        public void DeleteMetrics_ShouldWorkCorrectly()
        {
            SetupDefaults();

            _metrics = new List<DataMetric>
            {
                new DataMetric
                {
                    ExternalId = Guid.NewGuid(),
                    MetricId = RandomData.RandomInt(24, 66),
                    ExecutedOn = DateTime.UtcNow,
                    ReleaseWindowId = _releaseWindows[0].ReleaseWindowId
                }
            };

            _metricRepositoryMock.SetupEntities(_metrics);

            Sut.DeleteMetrics(_releaseWindows[0].ExternalId);

            _metricRepositoryMock.Verify(m => m.Delete(_metrics[0]));
        }

        [Test]
        public void CreateOrUpdateMetric_ShouldCreateNewMetric_WhenMetricNotExists()
        {
            var releases = SetupReleaseWindows(1, 5);
            var release = releases.First();

            var metric = Builder<Metric>.CreateNew()
                .With(o => o.MetricType, RandomData.RandomEnum<MetricType>())
                .With(o => o.ExecutedOn, RandomData.RandomBool() ? (DateTime?)null : RandomData.RandomDateTime(DateTimeKind.Utc))
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();

            Sut.CreateOrUpdateMetric(release.ExternalId, metric);

            _metricRepositoryMock.Verify(o =>
                o.Insert(It.Is<DataMetric>(m =>
                    m.MetricType == metric.MetricType
                        && m.ExecutedOn == metric.ExecutedOn
                        && m.ExternalId == metric.ExternalId
                ))
            );
        }

        [Test]
        public void CreateOrUpdateMetric_ShouldCreateUpdateMetricAndConvertDateToUtc_WhenMetricExistsAndTimeIsInLocal()
        {
            var releases = SetupReleaseWindows(1, 5);
            var release = releases.First();

            var metric = Builder<Metric>.CreateNew()
                .With(o => o.MetricType, RandomData.RandomEnum<MetricType>())
                .With(o => o.ExecutedOn, RandomData.RandomDateTime())
                .With(o => o.ExternalId, Guid.NewGuid())
                .Build();
            var dataMetric = new DataMetric
            {
                ExternalId = metric.ExternalId,
                ExecutedOn = RandomData.RandomDateTime()
            };

            _metricRepositoryMock.SetupEntities(new[] { dataMetric });

            Sut.CreateOrUpdateMetric(release.ExternalId, metric);

            _metricRepositoryMock.Verify(o =>
                o.Update(It.Is<DataMetric>(m =>
                    m.ExecutedOn == metric.ExecutedOn.Value.ToUniversalTime()
                    && m.ExecutedOn.Value.Kind == DateTimeKind.Utc
                    && m.ExternalId == metric.ExternalId
                ))
            );
        }

        #region Helpers

        private void SetupMetrics()
        {
            _metrics = new List<DataMetric>
            {
                new DataMetric
                {
                    ExternalId = Guid.NewGuid(),
                    MetricId = RandomData.RandomInt(24, 66),
                    ExecutedOn = DateTime.UtcNow,
                    MetricType = MetricType.SiteDown
                }
            };
            _metricRepositoryMock.SetupEntities(_metrics);
        }

        private void SetupReleaseWindows()
        {
            _releaseWindows = new List<ReleaseWindow>
            {
                new ReleaseWindow
                {
                    ExternalId = Guid.NewGuid(),
                    ReleaseWindowId = RandomData.RandomInt(59, 78),
                    Metrics = _metrics
                },
                new ReleaseWindow
                {
                    ExternalId = Guid.NewGuid(),
                    ReleaseWindowId = RandomData.RandomInt(122, 154),
                    Metrics = new DataMetric[0] 
                }
            };
            _releaseWindowRepositoryMock.SetupEntities(_releaseWindows);
        }

        private void SetupDefaults()
        {
            SetupMetrics();

            SetupReleaseWindows();
        }

        private IEnumerable<DataMetric> SetupDataMetrics(int releaseWindowId, IEnumerable<MetricType> metricTypes, int fakeMetricsCount = -1)
        {
            var startTime = RandomData.RandomDateTime(DateTimeKind.Utc);

            var metrics = new List<DataMetric>();

            foreach (var metricType in metricTypes)
            {
                startTime = startTime.AddMinutes(RandomData.RandomInt(1, 10));

                metrics.Add(
                    Builder<DataMetric>.CreateNew()
                        .With(o => o.MetricType, metricType)
                        .With(o => o.ExternalId, Guid.NewGuid())
                        .With(o => o.ExecutedOn, startTime)
                        .With(o => o.ReleaseWindowId, releaseWindowId)
                        .Build()
                    );
            }
            fakeMetricsCount = fakeMetricsCount < 0 ? RandomData.RandomInt(1, 5) : fakeMetricsCount;

            for (int i = 0; i < fakeMetricsCount; i++)
            {
                metrics.Add(
                    Builder<DataMetric>.CreateNew()
                        .With(o => o.MetricType, RandomData.RandomEnum<MetricType>())
                        .With(o => o.ExternalId, Guid.NewGuid())
                        .With(o => o.ExecutedOn, RandomData.RandomDateTime(DateTimeKind.Utc))
                        .With(o => o.ReleaseWindowId, RandomData.RandomInt(releaseWindowId - 1))
                        .Build()
                    );
            }

            _metricRepositoryMock.SetupEntities(metrics);

            return metrics.Where(o => o.ReleaseWindowId == releaseWindowId).ToList();
        }


        private IEnumerable<ReleaseWindow> SetupReleaseWindows(int releaseWindowId, int fakeItemsCount = -1)
        {
            return SetupReleaseWindows(releaseWindowId, RandomData.RandomString(5), null, ReleaseType.Scheduled, fakeItemsCount);
        }

        private IEnumerable<ReleaseWindow> SetupReleaseWindows(
            int releaseWindowId, string product, IEnumerable<DataMetric> metrics,
            ReleaseType releaseType = ReleaseType.Scheduled, int fakeItemsCount = -1
        )
        {
            var count = fakeItemsCount < 0 ? RandomData.RandomInt(5) : fakeItemsCount;
            var metricsList = (metrics ?? Enumerable.Empty<DataMetric>()).ToList();

            var releaseWindows = Builder<ReleaseWindow>.CreateListOfSize(count + 1)
                .All()
                .With(o => o.ReleaseWindowId = RandomData.RandomInt(1, int.MaxValue))
                .With(o => o.ExternalId = Guid.NewGuid())
                .With(o => o.ReleaseProducts = Builder<ReleaseProduct>
                        .CreateListOfSize(1)
                        .All()
                        .With(x => x.Product, new Product { Description = RandomData.RandomString(5) })
                        .Build())
                .With(o => o.Metrics = new List<DataMetric>())
                .With(o => o.ReleaseType = RandomData.RandomEnum(releaseType))
                .Random(1)
                .With(o => o.ReleaseWindowId = releaseWindowId)
                .With(o => o.ExternalId = Guid.NewGuid())
                .With(o => o.ReleaseProducts = Builder<ReleaseProduct>
                        .CreateListOfSize(1)
                        .All()
                        .With(x => x.Product, new Product { Description = product })
                        .Build())
                .With(o => o.ReleaseType = releaseType)
                .With(o => o.Metrics = metricsList)
                .Build();

            _releaseWindowRepositoryMock.SetupEntities(releaseWindows);

            return releaseWindows;
        }

        #endregion
    }
}
