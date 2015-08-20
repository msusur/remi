using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using FizzWare.NBuilder;
using NUnit.Framework;
using ReMi.BusinessEntities.Api;
using ReMi.BusinessEntities.DeploymentTool;
using ReMi.BusinessEntities.Metrics;
using ReMi.BusinessEntities.Products;
using ReMi.BusinessLogic.AutoMapper;
using ReMi.Common.Constants.ReleaseCalendar;
using ReMi.Common.Utils.UnitTests;

namespace ReMi.BusinessLogic.Tests.AutoMapper
{
    [TestFixture]
    public class BusinessEntityToBusinessEntityMappingProfileTests : TestClassFor<IMappingEngine>
    {
        protected override IMappingEngine ConstructSystemUnderTest()
        {
            Mapper.Initialize(
                c => c.AddProfile<BusinessEntityToBusinessEntityMappingProfile>());

            return Mapper.Engine;
        }

        [Test]
        public void Product_ShouldReturnProductView_WhenMapInvoked()
        {
            var product = new Product
            {
                BusinessUnit = new BusinessUnit { Description = RandomData.RandomString(10) },
                ChooseTicketsByDefault = RandomData.RandomBool(),
                ExternalId = Guid.NewGuid(),
                Description = RandomData.RandomString(10),
                ReleaseTrack = RandomData.RandomEnum<ReleaseTrack>()
            };

            var productView = Sut.Map<Product, ProductView>(product);

            Assert.AreEqual(product.Description, productView.Name);
            Assert.AreEqual(product.ExternalId, productView.ExternalId);
            Assert.AreEqual(product.ChooseTicketsByDefault, productView.ChooseTicketsByDefault);
            Assert.AreEqual(product.ReleaseTrack, productView.ReleaseTrack);
            Assert.AreEqual(product.BusinessUnit.Description, productView.BusinessUnit);
            Assert.AreEqual(false, productView.IsDefault);
        }

        [Test]
        public void BusinessUnit_ShouldReturnBusinessUnitView_WhenMapInvoked()
        {
            var businessUnit = new BusinessUnit
            {
                Description = RandomData.RandomString(10),
                ExternalId = Guid.NewGuid(),
                Name = RandomData.RandomString(10),
                Packages = new[]
                {
                    new Product { ExternalId = Guid.NewGuid() },
                    new Product { ExternalId = Guid.NewGuid() }
                }
            };

            var businessUnitView = Sut.Map<BusinessUnit, BusinessUnitView>(businessUnit);

            Assert.AreEqual(businessUnit.Description, businessUnitView.Description);
            Assert.AreEqual(businessUnit.ExternalId, businessUnitView.ExternalId);
            Assert.AreEqual(businessUnit.Name, businessUnitView.Name);
            foreach (var package in businessUnit.Packages.Zip(businessUnitView.Packages, (p, pv) => new { Product = p, ProductView = pv }))
            {
                Assert.AreEqual(package.Product.ExternalId, package.ProductView.ExternalId);
            }
        }

        [Test]
        public void StepMeasurement_ShouldReturnMeasurementTime_WhenMapInvoked()
        {
            var startTime = RandomData.RandomDateTime();
            var durationSec = RandomData.RandomInt(1, 600);

            var stepMeasurement = Builder<JobMeasurement>.CreateNew()
                .With(x => x.StepId, RandomData.RandomString(10))
                .With(x => x.StepName, RandomData.RandomString(10))
                .With(x => x.StartTime, startTime)
                .With(x => x.FinishTime, startTime.AddSeconds(durationSec))
                .Build();

            var expected = Builder<MeasurementTime>.CreateNew()
                .With(x => x.Name, stepMeasurement.StepName)
                .With(x => x.Value, Convert.ToInt32(Convert.ToDouble(durationSec) / 60))
                .Build();

            var actual = Sut.Map<JobMeasurement, MeasurementTime>(stepMeasurement);

            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Value, actual.Value);
        }

        [Test]
        public void ApiDescriptionMapping_ShouldReturnApiDescriptionFull_WhenMapInvoked()
        {
            var description = Builder<ApiDescription>.CreateNew()
                .With(x => x.Description, RandomData.RandomString(10))
                .With(x => x.InputFormat, RandomData.RandomString(10))
                .With(x => x.Method, RandomData.RandomString(10))
                .With(x => x.OutputFormat, RandomData.RandomString(10))
                .With(x => x.Url, RandomData.RandomString(10))
                .Build();

            var expected = Builder<ApiDescriptionFull>.CreateNew()
                .With(x => x.Description, description.Description)
                .With(x => x.InputFormat, description.InputFormat)
                .With(x => x.Method, description.Method)
                .With(x => x.OutputFormat, description.OutputFormat)
                .With(x => x.Url, description.Url)
                .Build();

            var actual = Sut.Map<ApiDescription, ApiDescriptionFull>(description);

            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.InputFormat, actual.InputFormat);
            Assert.AreEqual(expected.Method, actual.Method);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.OutputFormat, actual.OutputFormat);
            Assert.AreEqual(expected.Url, actual.Url);

            Assert.IsNull(actual.Roles);
            Assert.IsNull(actual.DescriptionShort);
            Assert.IsNull(actual.Group);
        }
    }
}
