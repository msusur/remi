(function () {
    "use strict";

    var controllerId = "pluginsGlobalConfiguration";

    angular.module("app").controller(controllerId,
        ["$scope", "common", "remiapi", pluginsGlobalConfiguration]);

    function pluginsGlobalConfiguration($scope, common, remiapi) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;

        vm.parent = common.getParentScope($scope, function (sc) {
            return sc && sc.vm && sc.vm.controllerId === "plugins";
        }).vm;

        vm.state = {
            isBusy: false
        };

        vm.parent.globalPlugins = [];

        vm.globalPluginChanged = globalPluginChanged;
        vm.refreshGlobalPlugins = getGlobalPlugins;

        activate();

        return vm;

        function activate() {
            common.activateController([initialize()], controllerId)
                .then(function () { logger.console("Activated PluginsGlobalConfiguration Controller"); });
        }

        function initialize() {
            if (vm.parent && vm.parent.initializationPromise) {
                vm.parent.initializationPromise
                    .then(getGlobalPlugins);
            }
        }

        function getGlobalPlugins() {
            vm.state.isBusy = true;
            return remiapi.get.globalPlugins()
                .then(function (globalPlugins) {
                    vm.parent.globalPlugins = globalPlugins;
                    var plugins = Enumerable.From(vm.parent.globalPlugins.GlobalPlugins);
                    Enumerable.From(vm.parent.globalPlugins.GlobalPluginConfiguration)
                        .ForEach(function (x) {
                            x.Plugins = plugins.Where(function (p) {
                                return p.PluginTypes.indexOf(x.PluginType) >= 0;
                            }).ToArray();
                        });
                }, function (response, status) {
                    if (response !== 406 && status !== 406) {
                        logger.error("Unable to load global plugin configuration");
                        logger.error(JSON.stringify(response));
                    }
                }).finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function globalPluginChanged(pluginConfig) {
            if (pluginConfig) {
                vm.state.isBusy = true;
                var commandData = { PluginId: pluginConfig.PluginId, ConfigurationId: pluginConfig.ExternalId };
                remiapi.post.assignGlobalPlugin(commandData)
                    .then(null, function (response, status) {
                        if (response !== 406 && status !== 406) {
                            logger.error("Unable to update configuration");
                            logger.error(JSON.stringify(response));
                        }
                        getGlobalPlugins();
                    }).finally(function () {
                        vm.state.isBusy = false;
                    });
            }
        }
    }
})();

