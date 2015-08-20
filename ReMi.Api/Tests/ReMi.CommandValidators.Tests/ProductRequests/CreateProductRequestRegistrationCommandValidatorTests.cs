using System;
using System.Linq;
using NUnit.Framework;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.Commands.ProductRequests;
using ReMi.CommandValidators.ProductRequests;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.ProductRequests
{
    [TestFixture]
    public class CreateProductRequestRegistrationCommandValidatorTests : TestClassFor<CreateProductRequestRegistrationCommandValidator>
    {
        protected override CreateProductRequestRegistrationCommandValidator ConstructSystemUnderTest()
        {
            return new CreateProductRequestRegistrationCommandValidator();
        }

        [Test]
        public void Validate_ShouldPass_WhenCommandValid()
        {
            var command = new CreateProductRequestRegistrationCommand
            {
                Registration = new ProductRequestRegistration
                {
                    ExternalId = Guid.NewGuid(),
                    Description = "Desc",
                    ProductRequestTypeId = Guid.NewGuid(),
                    Tasks = new[]
                    {
                        new ProductRequestRegistrationTask { ProductRequestTaskId = Guid.NewGuid()}
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenDescriptionEmpty()
        {
            var command = new CreateProductRequestRegistrationCommand
            {
                Registration = new ProductRequestRegistration
                {
                    ExternalId = Guid.NewGuid(),
                    Description = string.Empty,
                    ProductRequestTypeId = Guid.NewGuid(),
                    Tasks = new[]
                    {
                        new ProductRequestRegistrationTask{ProductRequestTaskId = Guid.NewGuid()}
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(2, result.Errors.Count);
            Assert.AreEqual("Registration.Description", result.Errors.ElementAt(0).PropertyName);
            Assert.AreEqual("Registration.Description", result.Errors.ElementAt(1).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenDescriptionNull()
        {
            var command = new CreateProductRequestRegistrationCommand
            {
                Registration = new ProductRequestRegistration
                {
                    ExternalId = Guid.NewGuid(),
                    Description = null,
                    ProductRequestTypeId = Guid.NewGuid(),
                    Tasks = new[]
                    {
                        new ProductRequestRegistrationTask{ProductRequestTaskId = Guid.NewGuid()}
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Registration.Description", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenDescriptionMongerThen1024()
        {
            var command = new CreateProductRequestRegistrationCommand
            {
                Registration = new ProductRequestRegistration
                {
                    ExternalId = Guid.NewGuid(),
                    Description = new String('0', 1025),
                    ProductRequestTypeId = Guid.NewGuid(),
                    Tasks = new[]
                    {
                        new ProductRequestRegistrationTask{ProductRequestTaskId = Guid.NewGuid()}
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Registration.Description", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenExternalIdEmpty()
        {
            var command = new CreateProductRequestRegistrationCommand
            {
                Registration = new ProductRequestRegistration
                {
                    ExternalId = Guid.Empty,
                    Description = "Desc",
                    ProductRequestTypeId = Guid.NewGuid(),
                    Tasks = new[]
                    {
                        new ProductRequestRegistrationTask{ProductRequestTaskId = Guid.NewGuid()}
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Registration.ExternalId", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenProductRequestTypeIdEmpty()
        {
            var command = new CreateProductRequestRegistrationCommand
            {
                Registration = new ProductRequestRegistration
                {
                    ExternalId = Guid.NewGuid(),
                    Description = "Desc",
                    ProductRequestTypeId = Guid.Empty,
                    Tasks = new[]
                    {
                        new ProductRequestRegistrationTask{ProductRequestTaskId = Guid.NewGuid()}
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Registration.ProductRequestTypeId", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenAssigneesHasEmptyProductRequestTaskId()
        {
            var command = new CreateProductRequestRegistrationCommand
            {
                Registration = new ProductRequestRegistration
                {
                    ExternalId = Guid.NewGuid(),
                    Description = "Desc",
                    ProductRequestTypeId = Guid.NewGuid(),
                    Tasks = new[]
                    {
                        new ProductRequestRegistrationTask{ProductRequestTaskId = Guid.Empty}
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Registration.Tasks[0].ProductRequestTaskId", result.Errors.ElementAt(0).PropertyName);
        }
    }
}
