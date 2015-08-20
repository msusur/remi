using System;
using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.ReleasePlan;

namespace ReMi.QueryValidators.ReleasePlan
{
    public class GetReleaseContentInformationRequestValidator :
        RequestValidatorBase<GetReleaseContentInformationRequest>
    {
        public GetReleaseContentInformationRequestValidator()
        {
            RuleFor(x => x.ReleaseWindowId)
                .NotEmpty()
                .Must(x => x != Guid.Empty);
        }
    }
}
