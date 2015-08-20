using System;
using System.Linq;
using FizzWare.NBuilder;
using NUnit.Framework;
using ReMi.BusinessEntities.Metrics;
using ReMi.BusinessEntities.ReleaseCalendar;
using ReMi.BusinessLogic.Metrics;
using ReMi.Common.Constants.Metrics;
using ReMi.Common.Constants.ReleaseExecution;
using ReMi.Common.Utils.UnitTests;
using ReMi.Contracts;

namespace ReMi.BusinessLogic.Tests.Metrics
{
    public class MeasurementsCalculatorTests : TestClassFor<MeasurementsCalculator>
    {
        protected override MeasurementsCalculator ConstructSystemUnderTest()
        {
            return new MeasurementsCalculator();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Calculate_ShouldRaiseException_WhenReleaseNull()
        {
            Sut.Calculate(MeasurementType.OverallTime, null, null);
        }

        [Test]
        public void OverallTime_ShouldRetrunNull_WhenReleaseNotSignedOff()
        {
            var releaseWindow = SetupReleaseWindow();

            var result = Sut.Calculate(MeasurementType.OverallTime, releaseWindow, null);

            Assert.IsNull(result);
        }

        [Test]
        public void OverallTime_ShouldReturnCorrectValue_WhenInvoked()
        {
            var startTime = RandomData.RandomDateTime();
            var shift = RandomData.RandomInt(1, 100);

            var releaseWindow = SetupReleaseWindow(r =>
            {
                r.StartTime = startTime;
                r.SignedOff = startTime.AddMinutes(shift);
            });

            var result = Sut.Calculate(MeasurementType.OverallTime, releaseWindow, null);

            Assert.IsNotNull(result);
            Assert.AreEqual(shift, result.Value);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DownTime_ShouldRaiseException_WhenMetricsAreEmpty()
        {
            var releaseWindow = SetupReleaseWindow();

            Sut.Calculate(MeasurementType.DownTime, releaseWindow, Enumerable.Empty<Metric>());
        }

        [Test]
        public void DownTime_ShouldRetrunNull_WhenRequiredMetricAbsent()
        {
            var releaseWindow = SetupReleaseWindow();

            var startTime = RandomData.RandomDateTime();

            var metric1 = Builder<Metric>.CreateNew()
                .With(o => o.ExecutedOn, startTime)
                .With(o => o.MetricType, MetricType.SiteDown)
                .Build();

            var result = Sut.Calculate(MeasurementType.DownTime, releaseWindow, new[] { metric1 });

            Assert.IsNull(result);
        }

        [Test]
        public void DownTime_ShouldRetrunNull_WhenMetricDoesntHasExecutedOnTime()
        {
            var releaseWindow = SetupReleaseWindow();

            var startTime = RandomData.RandomDateTime();

            var metric1 = Builder<Metric>.CreateNew()
                .With(o => o.ExecutedOn, startTime)
                .With(o => o.MetricType, MetricType.SiteDown)
                .Build();

            var metric2 = Builder<Metric>.CreateNew()
                .With(o => o.ExecutedOn, null)
                .With(o => o.MetricType, MetricType.SiteUp)
                .Build();

            var result = Sut.Calculate(MeasurementType.DownTime, releaseWindow, new[] { metric1, metric2 });

            Assert.IsNull(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void DownTime_ShouldRaiseException_WhenFinishTimeLessThenStartTime()
        {
            var releaseWindow = SetupReleaseWindow();

            var startTime = RandomData.RandomDateTime();
            var shift = RandomData.RandomInt(1, 100);

            var metric1 = Builder<Metric>.CreateNew()
                .With(o => o.ExecutedOn, startTime)
                .With(o => o.MetricType, MetricType.SiteDown)
                .Build();

            var metric2 = Builder<Metric>.CreateNew()
                .With(o => o.ExecutedOn, startTime.AddMinutes(-shift))
                .With(o => o.MetricType, MetricType.SiteUp)
                .Build();

            Sut.Calculate(MeasurementType.DownTime, releaseWindow, new[] { metric1, metric2 });
        }

        [Test]
        public void DownTime_ShouldReturnCorrectTime_WhenInvokedWithValidData()
        {
            var releaseWindow = SetupReleaseWindow();

            var startTime = RandomData.RandomDateTime();
            var shift = RandomData.RandomInt(1, 100);

            var metric1 = Builder<Metric>.CreateNew()
                .With(o => o.ExecutedOn, startTime)
                .With(o => o.MetricType, MetricType.SiteDown)
                .Build();

            var metric2 = Builder<Metric>.CreateNew()
                .With(o => o.ExecutedOn, startTime.AddMinutes(shift))
                .With(o => o.MetricType, MetricType.SiteUp)
                .Build();

            var result = Sut.Calculate(MeasurementType.DownTime, releaseWindow, new[] { metric1, metric2 });

            Assert.IsNotNull(result);
            Assert.AreEqual(shift, result.Value);
        }

        [Test]
        public void PreDownTime_ShouldRetrunNull_WhenSiteDownMetricMissing()
        {
            var startTime = RandomData.RandomDateTime();

            var releaseWindow = SetupReleaseWindow(r =>
            {
                r.StartTime = startTime;
            });

            var metric = Builder<Metric>.CreateNew()
               .With(o => o.ExecutedOn, null)
               .With(o => o.MetricType, MetricType.SiteDown)
               .Build();

            var result = Sut.Calculate(MeasurementType.PreDownTime, releaseWindow, new[] { metric });

            Assert.IsNull(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void PreDownTime_ShouldRaiseException_WhenSiteDownTimeLessThenReleaseStartTime()
        {
            var startTime = RandomData.RandomDateTime();
            var shift = RandomData.RandomInt(1, 100);

            var releaseWindow = SetupReleaseWindow(r =>
            {
                r.StartTime = startTime;
            });

            var metric = Builder<Metric>.CreateNew()
               .With(o => o.ExecutedOn, startTime.AddMinutes(-shift))
               .With(o => o.MetricType, MetricType.SiteDown)
               .Build();

            var result = Sut.Calculate(MeasurementType.PreDownTime, releaseWindow, new[] { metric });

            Assert.IsNotNull(result);
            Assert.AreEqual(shift, result.Value);
        }

        [Test]
        public void PreDownTime_ShouldReturnCorrectValue_WhenInvoked()
        {
            var startTime = RandomData.RandomDateTime();
            var shift = RandomData.RandomInt(1, 100);

            var releaseWindow = SetupReleaseWindow(r =>
            {
                r.StartTime = startTime;
            });

            var metric = Builder<Metric>.CreateNew()
               .With(o => o.ExecutedOn, startTime.AddMinutes(shift))
               .With(o => o.MetricType, MetricType.SiteDown)
               .Build();

            var result = Sut.Calculate(MeasurementType.PreDownTime, releaseWindow, new[] { metric });

            Assert.IsNotNull(result);
            Assert.AreEqual(shift, result.Value);
        }

        [Test]
        public void PostDownTime_ShouldRetrunNull_WhenSiteDownMetricMissing()
        {
            var startTime = RandomData.RandomDateTime();

            var releaseWindow = SetupReleaseWindow(r =>
            {
                r.SignedOff = startTime;
            });

            var metric = Builder<Metric>.CreateNew()
               .With(o => o.ExecutedOn, null)
               .With(o => o.MetricType, MetricType.SiteUp)
               .Build();

            var result = Sut.Calculate(MeasurementType.PostDownTime, releaseWindow, new[] { metric });

            Assert.IsNull(result);
        }

        [Test]
        public void PostDownTime_ShouldRetrunNull_WhenReleaseNotSignedOff()
        {
            var startTime = RandomData.RandomDateTime();

            var releaseWindow = SetupReleaseWindow();

            var metric = Builder<Metric>.CreateNew()
               .With(o => o.ExecutedOn, startTime)
               .With(o => o.MetricType, MetricType.SiteUp)
               .Build();

            var result = Sut.Calculate(MeasurementType.PostDownTime, releaseWindow, new[] { metric });

            Assert.IsNull(result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void PostDownTime_ShouldRaiseException_WhenSiteDownTimeLessThenReleaseStartTime()
        {
            var startTime = RandomData.RandomDateTime();
            var shift = RandomData.RandomInt(1, 100);

            var releaseWindow = SetupReleaseWindow(r =>
            {
                r.SignedOff = startTime.AddMinutes(-shift);
            });

            var metric = Builder<Metric>.CreateNew()
               .With(o => o.ExecutedOn, startTime)
               .With(o => o.MetricType, MetricType.SiteUp)
               .Build();

            Sut.Calculate(MeasurementType.PostDownTime, releaseWindow, new[] { metric });
        }

        [Test]
        public void PostDownTime_ShouldReturnCorrectValue_WhenInvoked()
        {
            var startTime = RandomData.RandomDateTime();
            var shift = RandomData.RandomInt(1, 100);

            var releaseWindow = SetupReleaseWindow(r =>
            {
                r.SignedOff = startTime.AddMinutes(shift);
            });

            var metric = Builder<Metric>.CreateNew()
               .With(o => o.ExecutedOn, startTime)
               .With(o => o.MetricType, MetricType.SiteUp)
               .Build();

            var result = Sut.Calculate(MeasurementType.PostDownTime, releaseWindow, new[] { metric });

            Assert.IsNotNull(result);
            Assert.AreEqual(shift, result.Value);
        }

        private ReleaseWindow SetupReleaseWindow(Action<ReleaseWindow> customSetup = null)
        {
            var releaseWindow = new ReleaseWindow();
            releaseWindow.ExternalId = Guid.NewGuid();

            if (customSetup != null)
                customSetup(releaseWindow);

            return releaseWindow;
        }
    }
}
