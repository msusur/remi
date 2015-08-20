using FluentValidation;
using ReMi.Commands.Plugins;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Plugins
{
    public class UpdatePluginGlobalConfigurationEntityCommandValidator : RequestValidatorBase<UpdatePluginGlobalConfigurationEntityCommand>
    {
        public UpdatePluginGlobalConfigurationEntityCommandValidator()
        {
            RuleFor(x => x.PluginKey).NotEmpty();
            RuleFor(x => x.PropertyName).NotEmpty();
        }
    }
}
