using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.Plugins;

namespace ReMi.QueryValidators.Plugins
{
    public class GetGlobalPluginConfigurationEntityRequestValidator : RequestValidatorBase<GetGlobalPluginConfigurationEntityRequest>
    {
        public GetGlobalPluginConfigurationEntityRequestValidator()
        {
            RuleFor(x => x.PluginId).NotEmpty();
        }
    }
}
