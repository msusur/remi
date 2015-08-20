using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.Configuration;

namespace ReMi.QueryValidators.Configuration
{
    public class GetCommandsByNamesValidator : RequestValidatorBase<GetCommandsByNamesRequest>
    {
        public GetCommandsByNamesValidator()
        {
            RuleFor(x => x.Names).NotEmpty();
        }
    }
}
