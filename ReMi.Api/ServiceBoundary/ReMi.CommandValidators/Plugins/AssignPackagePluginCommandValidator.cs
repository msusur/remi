using System;
using FluentValidation;
using ReMi.Commands.Plugins;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Plugins
{
    public class AssignPackagePluginCommandValidator : RequestValidatorBase<AssignPackagePluginCommand>
    {
        public AssignPackagePluginCommandValidator()
        {
            RuleFor(x => x.ConfigurationId).NotEmpty();
            When(x => x.PluginId.HasValue, () => RuleFor(x => x.PluginId)
                .Must(x => x != Guid.Empty)
                .WithMessage("PluginId cannot be empty GUID"));
        }
    }
}
