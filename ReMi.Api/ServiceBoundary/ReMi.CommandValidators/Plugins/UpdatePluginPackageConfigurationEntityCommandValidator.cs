using FluentValidation;
using ReMi.Commands.Plugins;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Plugins
{
    public class UpdatePluginPackageConfigurationEntityCommandValidator : RequestValidatorBase<UpdatePluginPackageConfigurationEntityCommand>
    {
        public UpdatePluginPackageConfigurationEntityCommandValidator()
        {
            RuleFor(x => x.PluginKey).NotEmpty();
            RuleFor(x => x.PackageName).NotEmpty();
            RuleFor(x => x.PropertyName).NotEmpty();
        }
    }
}
