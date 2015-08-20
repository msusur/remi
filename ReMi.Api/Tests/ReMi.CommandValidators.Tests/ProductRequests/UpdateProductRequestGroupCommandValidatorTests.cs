using System;
using System.Linq;
using NUnit.Framework;
using ReMi.BusinessEntities.Auth;
using ReMi.BusinessEntities.ProductRequests;
using ReMi.Commands.ProductRequests;
using ReMi.CommandValidators.ProductRequests;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.ProductRequests
{
    [TestFixture]
    public class UpdateProductRequestGroupCommandValidatorTests : TestClassFor<UpdateProductRequestGroupCommandValidator>
    {
        protected override UpdateProductRequestGroupCommandValidator ConstructSystemUnderTest()
        {
            return new UpdateProductRequestGroupCommandValidator();
        }

        [Test]
        public void Validate_ShouldPass_WhenCommandValid()
        {
            var command = new UpdateProductRequestGroupCommand
            {
                RequestGroup = new ProductRequestGroup
                {
                    ExternalId = Guid.NewGuid(),
                    Name = "Name",
                    ProductRequestTypeId = Guid.Empty,
                    Assignees = new[]
                    {
                        new Account{ExternalId = Guid.NewGuid()}
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenNameEmpty()
        {
            var command = new UpdateProductRequestGroupCommand
            {
                RequestGroup = new ProductRequestGroup
                {
                    ExternalId = Guid.NewGuid(),
                    Name = string.Empty,
                    ProductRequestTypeId = Guid.NewGuid(),
                    Assignees = new[]
                    {
                        new Account{ExternalId = Guid.NewGuid()}
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(2, result.Errors.Count);
            Assert.AreEqual("RequestGroup.Name", result.Errors.ElementAt(0).PropertyName);
            Assert.AreEqual("RequestGroup.Name", result.Errors.ElementAt(1).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenNameNull()
        {
            var command = new UpdateProductRequestGroupCommand
            {
                RequestGroup = new ProductRequestGroup
                {
                    ExternalId = Guid.NewGuid(),
                    Name = null,
                    ProductRequestTypeId = Guid.NewGuid(),
                    Assignees = new[]
                    {
                        new Account{ExternalId = Guid.NewGuid()}
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("RequestGroup.Name", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenExternalIdEmpty()
        {
            var command = new UpdateProductRequestGroupCommand
            {
                RequestGroup = new ProductRequestGroup
                {
                    ExternalId = Guid.Empty,
                    Name = "name",
                    ProductRequestTypeId = Guid.NewGuid(),
                    Assignees = new[]
                    {
                        new Account{ExternalId = Guid.NewGuid()}
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("RequestGroup.ExternalId", result.Errors.ElementAt(0).PropertyName);
        }

        [Test]
        public void Validate_ShouldNotPass_WhenAssigneesHasEmptyExternalId()
        {
            var command = new UpdateProductRequestGroupCommand
            {
                RequestGroup = new ProductRequestGroup
                {
                    ExternalId = Guid.NewGuid(),
                    Name = "name",
                    ProductRequestTypeId = Guid.NewGuid(),
                    Assignees = new[]
                    {
                        new Account{ExternalId = Guid.Empty}
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("RequestGroup.Assignees[0].ExternalId", result.Errors.ElementAt(0).PropertyName);
        }
    }
}
