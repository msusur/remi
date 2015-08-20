using FluentValidation;
using ReMi.Commands;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Common.Utils;

namespace ReMi.CommandValidators.Api
{
    public class UpdateApiDescriptionCommandValidator : RequestValidatorBase<UpdateApiDescriptionCommand>
    {
        public UpdateApiDescriptionCommandValidator()
        {
            RuleFor(x => x.ApiDescription.Method)
                .Must(x => x.ToLower() == "get" || x.ToLower() == "post")
                .WithMessage("Incorrect HTTP method in API description");

            RuleFor(x => x.ApiDescription.Url)
                .Must(x => !x.IsNullOrEmpty())
                .WithMessage("API description URL should not be empty");
        }
    }
}
