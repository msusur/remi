using FluentValidation;
using ReMi.CommandValidators.Common;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.ContinuousDelivery;

namespace ReMi.QueryValidators.ContinuousDelivery
{
    public class GetContinuousDeliveryStatusValidator : RequestValidatorBase<GetContinuousDeliveryStatusRequest>
    {
        public GetContinuousDeliveryStatusValidator()
        {
            RuleFor(x => x.Products).NotEmpty();

            RuleFor(x => x.Products)
                .SetCollectionValidator(new StringCollectionValidator("Products"))
                .WithMessage("At least one product should be specified");
        }
    }
}
