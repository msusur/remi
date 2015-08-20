using System;
using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.Metrics;

namespace ReMi.QueryValidators.Metrics
{
    public class GetMetricsRequestValidator : RequestValidatorBase<GetMetricsRequest>
    {
        public GetMetricsRequestValidator()
        {
            RuleFor(r => r.ReleaseWindowId).Must(x => x != Guid.Empty);
        }
    }
}
