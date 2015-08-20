using System.Collections.Generic;
using NUnit.Framework;
using ReMi.BusinessEntities.Subscriptions;
using ReMi.Commands.Subscriptions;
using ReMi.CommandValidators.Subscriptions;
using ReMi.TestUtils.UnitTests;

namespace ReMi.CommandValidators.Tests.Subscriptions
{
    public class UpdateNotificationSubscriptionsCommandValidatorTests
        : TestClassFor<UpdateNotificationSubscriptionsCommandValidator>
    {
        protected override UpdateNotificationSubscriptionsCommandValidator ConstructSystemUnderTest()
        {
            return new UpdateNotificationSubscriptionsCommandValidator();
        }

        [Test]
        public void Validate_ShouldReturnValidStatus_ForApiChangeAndReleaseWindowsSchedule()
        {
            var command = new UpdateNotificationSubscriptionsCommand
            {
                NotificationSubscriptions = new List<NotificationSubscription>
                {
                    new NotificationSubscription
                    {
                        NotificationName = "Api change",
                        Subscribed = true
                    },
                    new NotificationSubscription
                    {
                        NotificationName = "Release windows schedule"
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldReturnValidStatus_ForOnlyApiChanged()
        {
            var command = new UpdateNotificationSubscriptionsCommand
            {
                NotificationSubscriptions = new List<NotificationSubscription>
                {
                    new NotificationSubscription
                    {
                        NotificationName = "Api change",
                        Subscribed = true
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void Validate_ShouldReturnNotValidStatus_ForNotDefinedStep()
        {
            var command = new UpdateNotificationSubscriptionsCommand
            {
                NotificationSubscriptions = new List<NotificationSubscription>
                {
                    new NotificationSubscription
                    {
                        NotificationName = "Api change",
                        Subscribed = true
                    },
                    new NotificationSubscription
                    {
                        NotificationName = "",
                        Subscribed = true
                    }
                }
            };

            var result = Sut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Undefined notification name", result.Errors[0].ErrorMessage);
        }
    }
}
