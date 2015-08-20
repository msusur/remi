using System;
using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.ReleaseExecution;

namespace ReMi.QueryValidators.ReleaseExecution
{
    public class GetSignOffsRequestValidator : RequestValidatorBase<GetSignOffsRequest>
    {
        public GetSignOffsRequestValidator()
        {
            RuleFor(x => x.ReleaseWindowId).Must(x => x != Guid.Empty);
        }
    }
}
