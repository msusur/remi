using System;
using NUnit.Framework;
using ReMi.TestUtils.UnitTests;
using ReMi.Queries.Subscriptions;
using ReMi.QueryValidators.Subscriptions;

namespace ReMi.QueryValidators.Tests.Subscriptions
{
    public class GetAccountNotificationSubscriptionsRequestValidatorTests 
        : TestClassFor<GetAccountNotificationSubscriptionsRequestValidator>
    {
        protected override GetAccountNotificationSubscriptionsRequestValidator ConstructSystemUnderTest()
        {
            return new GetAccountNotificationSubscriptionsRequestValidator();
        }

        [Test]
        public void Validate_ShouldReturnValidStatus_ForCorrectAccountGuid()
        {
            var request = new GetAccountNotificationSubscriptionsRequest
            {
               AccountId = Guid.NewGuid()
            };

            var result = Sut.Validate(request);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldNotReturnValidStatus_ForEmptyAccountGuid()
        {
            var request = new GetAccountNotificationSubscriptionsRequest
            {
                AccountId = Guid.Empty
            };

            var result = Sut.Validate(request);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.True(result.Errors[0].ErrorMessage.Contains("Account Id")
                        && result.Errors[0].ErrorMessage.Contains(Guid.Empty.ToString()));
        }
    }
}
