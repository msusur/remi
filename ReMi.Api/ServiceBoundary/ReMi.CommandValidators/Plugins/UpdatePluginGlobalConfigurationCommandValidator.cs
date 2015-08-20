using FluentValidation;
using ReMi.Commands.Plugins;
using ReMi.Common.Cqrs.FluentValidation;

namespace ReMi.CommandValidators.Plugins
{
    public class UpdatePluginGlobalConfigurationCommandValidator : RequestValidatorBase<UpdatePluginGlobalConfigurationCommand>
    {
        public UpdatePluginGlobalConfigurationCommandValidator()
        {
            RuleFor(x => x.PluginId).NotEmpty();
            RuleFor(x => x.JsonValues).NotEmpty();
        }
    }
}
