using System;
using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.Metrics;

namespace ReMi.QueryValidators.Metrics
{
    public class GetMeasurementsRequestValidator : RequestValidatorBase<GetMeasurementsRequest>
    {
        public GetMeasurementsRequestValidator()
        {
            RuleFor(x => x.Product).NotNull().NotEmpty();
        }
    }
}
