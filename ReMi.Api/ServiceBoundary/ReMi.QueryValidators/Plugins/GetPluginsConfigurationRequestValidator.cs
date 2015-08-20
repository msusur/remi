using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.Plugins;

namespace ReMi.QueryValidators.Plugins
{
    public class GetPluginsConfigurationRequestValidator : RequestValidatorBase<GetPluginsConfigurationRequest>
    {
        public GetPluginsConfigurationRequestValidator()
        {
            RuleFor(x => x.PluginId).NotEmpty();
        }
    }
}
