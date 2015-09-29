using System;
using NUnit.Framework;
using ReMi.BusinessEntities.Metrics;
using ReMi.Common.Constants.Metrics;
using ReMi.Common.Utils.Enums;
using ReMi.TestUtils.UnitTests;

namespace ReMi.BusinessLogic.Tests.Entities
{
    public class MeasurementTimeByTypeTests : TestClassFor<MeasurementTimeByType>
    {
        protected override MeasurementTimeByType ConstructSystemUnderTest()
        {
            return new MeasurementTimeByType();
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Name_ShouldThrowException_WhenInvokedWithInvalidName()
        {
            Sut.Name = RandomData.RandomString(1, 20);
        }

        [Test]
        public void Name_ShouldUpdateType_WhenInvokedWithValidName()
        {
            var type = RandomData.RandomEnum<MeasurementType>();

            Sut.Name = type.ToString();

            Assert.AreEqual(type.ToString(), Sut.Type.ToString());
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Name_ShouldThrowException_WhenNamePassedWithInvalidLetterCase()
        {
            var type = RandomData.RandomEnum<MeasurementType>();

            Sut.Name = type.ToString().ToLower();

            Assert.AreEqual(type.ToString(), Sut.Type.ToString());
        }

        [Test]
        public void Name_ShouldPreserveName_WhenConverted()
        {
            var type = RandomData.RandomEnum<MeasurementType>();

            Sut.Type = type;

            var result = (MeasurementTime)Sut;

            Assert.AreEqual(type.ToDescriptionString(), result.Name);
        }
    }
}
