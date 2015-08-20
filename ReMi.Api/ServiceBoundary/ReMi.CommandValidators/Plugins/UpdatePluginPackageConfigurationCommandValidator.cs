using FluentValidation;
using ReMi.Commands.Plugins;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Plugins
{
    public class UpdatePluginPackageConfigurationCommandValidator : RequestValidatorBase<UpdatePluginPackageConfigurationCommand>
    {
        public UpdatePluginPackageConfigurationCommandValidator()
        {
            RuleFor(x => x.PluginId).NotEmpty();
            RuleFor(x => x.PackageId).NotEmpty();
            RuleFor(x => x.JsonValues).NotEmpty();
        }
    }
}
