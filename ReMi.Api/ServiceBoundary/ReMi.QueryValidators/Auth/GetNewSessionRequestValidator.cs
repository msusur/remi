using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.Auth;

namespace ReMi.QueryValidators.Auth
{
    public class GetNewSessionRequestValidator : RequestValidatorBase<GetNewSessionRequest>
    {
        public GetNewSessionRequestValidator()
        {
            RuleFor(x => x.SessionId).NotEmpty();
        }
    }
}
