using System.Collections.Generic;
using FluentValidation;

namespace ReMi.CommandValidators.Common
{
    public class StringCollectionValidator : InlineValidator<string>
    {
        public StringCollectionValidator(string propertyName)
        {
            RuleFor(q => q).NotNull().NotEmpty().WithName(propertyName);
        }
    }
}
