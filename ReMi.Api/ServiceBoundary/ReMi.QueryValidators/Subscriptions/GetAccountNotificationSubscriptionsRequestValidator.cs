using System;
using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.Subscriptions;

namespace ReMi.QueryValidators.Subscriptions
{
    public class GetAccountNotificationSubscriptionsRequestValidator 
        : RequestValidatorBase<GetAccountNotificationSubscriptionsRequest>
    {
        public GetAccountNotificationSubscriptionsRequestValidator()
        {
            RuleFor(x => x.AccountId).NotEqual(Guid.Empty);
        }
    }
}
