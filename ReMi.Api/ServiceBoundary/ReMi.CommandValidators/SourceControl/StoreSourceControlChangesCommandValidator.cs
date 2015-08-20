using System;
using FluentValidation;
using ReMi.Commands.SourceControl;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.SourceControl
{
    public class StoreSourceControlChangesCommandValidator : RequestValidatorBase<StoreSourceControlChangesCommand>
    {
        public StoreSourceControlChangesCommandValidator()
        {
            RuleFor(request => request.ReleaseWindowId).Must(r => r != Guid.Empty);
        }
    }
}
