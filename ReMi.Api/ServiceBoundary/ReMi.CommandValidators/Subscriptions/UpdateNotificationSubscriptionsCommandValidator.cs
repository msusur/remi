using FluentValidation;
using ReMi.Commands.Subscriptions;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Common.Utils;

namespace ReMi.CommandValidators.Subscriptions
{
    public class UpdateNotificationSubscriptionsCommandValidator
        : RequestValidatorBase<UpdateNotificationSubscriptionsCommand>
    {
        public UpdateNotificationSubscriptionsCommandValidator()
        {
            RuleForEach(x => x.NotificationSubscriptions)
                .NotNull().Must(x => !x.NotificationName.IsNullOrEmpty())
                .WithMessage("Undefined notification name");
        }
    }
}
