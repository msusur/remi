using FluentValidation;
using ReMi.Common.Cqrs.FluentValidation;
using ReMi.Queries.Plugins;

namespace ReMi.QueryValidators.Plugins
{
    public class GetPackagePluginConfigurationEntityRequestValidator : RequestValidatorBase<GetPackagePluginConfigurationEntityRequest>
    {
        public GetPackagePluginConfigurationEntityRequestValidator()
        {
            RuleFor(x => x.PluginId).NotEmpty();
            RuleFor(x => x.PackageId).NotEmpty();
        }
    }
}
