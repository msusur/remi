using System;
using ReMi.Common.WebApi;
using ReMi.Queries.Plugins;
using System.Web.Http;

namespace ReMi.Api.Controllers
{
    [RoutePrefix("plugins")]
    public class PluginsController : ApiController
    {
        public IImplementQueryAction<GetGlobalPluginConfigurationRequest, GetGlobalPluginConfigurationResponse> GetGlobalPluginConfigurationAction { get; set; }
        public IImplementQueryAction<GetPackagePluginConfigurationRequest, GetPackagePluginConfigurationResponse> GetPackagePluginConfigurationAction { get; set; }
        public IImplementQueryAction<GetPluginsConfigurationRequest, GetPluginsConfigurationResponse> GetPluginsConfigurationAction { get; set; }
        public IImplementQueryAction<GetPluginsRequest, GetPluginsResponse> GetPluginsAction { get; set; }
        public IImplementQueryAction<GetGlobalPluginConfigurationEntityRequest, GetGlobalPluginConfigurationEntityResponse> GetGlobalPluginConfigurationEntityAction { get; set; }
        public IImplementQueryAction<GetPackagePluginConfigurationEntityRequest, GetPackagePluginConfigurationEntityResponse> GetPackagePluginConfigurationEntityAction { get; set; }

        [HttpGet]
        [Route("global")]
        public GetGlobalPluginConfigurationResponse GetGlobalPlugins()
        {
            var request = new GetGlobalPluginConfigurationRequest();

            return GetGlobalPluginConfigurationAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("package")]
        public GetPackagePluginConfigurationResponse GetPackagePlugins()
        {
            var request = new GetPackagePluginConfigurationRequest();

            return GetPackagePluginConfigurationAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("")]
        public GetPluginsResponse GetPlugins()
        {
            var request = new GetPluginsRequest();

            return GetPluginsAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("{pluginId:Guid}")]
        public GetPluginsConfigurationResponse GetPlugins(Guid pluginId)
        {
            var request = new GetPluginsConfigurationRequest
            {
                PluginId = pluginId
            };

            return GetPluginsConfigurationAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("global/{pluginId}")]
        public GetGlobalPluginConfigurationEntityResponse GetGlobalPluginConfigurationEntity(Guid pluginId)
        {
            var request = new GetGlobalPluginConfigurationEntityRequest
            {
                PluginId = pluginId
            };

            return GetGlobalPluginConfigurationEntityAction.Handle(ActionContext, request);
        }

        [HttpGet]
        [Route("package/{pluginId}/{packageId}")]
        public GetPackagePluginConfigurationEntityResponse GetPackagePluginConfigurationEntity(Guid pluginId, Guid packageId)
        {
            var request = new GetPackagePluginConfigurationEntityRequest
            {
                PluginId = pluginId,
                PackageId = packageId
            };

            return GetPackagePluginConfigurationEntityAction.Handle(ActionContext, request);
        }
    }
}
