(function () {
    "use strict";

    var controllerId = "pluginsPackageConfiguration";

    angular.module("app").controller(controllerId,
        ["$scope", "common", "remiapi", pluginsPackageConfiguration]);

    function pluginsPackageConfiguration($scope, common, remiapi) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;

        vm.parent = common.getParentScope($scope, function (sc) {
            return sc && sc.vm && sc.vm.controllerId === "plugins";
        }).vm;

        vm.state = {
            isBusy: false
        };

        vm.parent.packagePlugins = [];

        vm.packagePluginChanged = packagePluginChanged;
        vm.refreshPackagePlugins = getPackagePlugins;

        activate();

        return vm;

        function activate() {
            common.activateController([initialize()], controllerId)
                .then(function () { logger.console("Activated PluginsPackageConfiguration Controller"); });
        }

        function initialize() {
            if (vm.parent && vm.parent.initializationPromise) {
                vm.parent.initializationPromise
                    .then(getPackagePlugins);
            }
        }


        function getPackagePlugins() {
            vm.state.isBusy = true;
            return remiapi.get.packagePlugins()
                .then(function (packagePlugins) {
                    vm.parent.packagePlugins = packagePlugins;
                    var plugins = Enumerable.From(vm.parent.packagePlugins.PackagePlugins);
                    var configuration = Enumerable.From(vm.parent.packagePlugins.PackagePluginConfiguration);
                    vm.parent.packagePlugins.Packages = configuration
                        .Select(function (x) {
                            return x.Value
                                ? {
                                    ExternalId: x.Value[Object.keys(x.Value)[0]].PackageId,
                                    Name: x.Value[Object.keys(x.Value)[0]].PackageName,
                                    BusinessUnit: x.Value[Object.keys(x.Value)[0]].BusinessUnit
                                } : undefined;
                        }).ToArray();
                    vm.parent.packagePlugins.PluginTypes = {};
                    if (vm.parent.packagePlugins.Packages.length > 0) {
                        Enumerable.From(configuration.First().Value)
                            .ForEach(function (x) {
                                vm.parent.packagePlugins.PluginTypes[x.Key] = plugins.Where(function (p) {
                                    return p.PluginTypes.indexOf(x.Key) >= 0;
                                }).ToArray();
                            });
                    }
                    vm.parent.packagesDeferred.resolve(vm.parent.packagePlugins.Packages);
                }, function (response, status) {
                    if (response !== 406 && status !== 406) {
                        logger.error("Unable to load package plugin configuration");
                        logger.error(JSON.stringify(response));
                    }
                }).finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function packagePluginChanged($package, pluginType) {
            if ($package && pluginType) {
                vm.state.isBusy = true;
                var pluginConfig = vm.parent.packagePlugins.PackagePluginConfiguration[$package.ExternalId][pluginType];
                var commandData = { PluginId: pluginConfig.PluginId, ConfigurationId: pluginConfig.ExternalId };
                remiapi.post.assignPackagePlugin(commandData)
                    .then(null, function (response, status) {
                        if (response !== 406 && status !== 406) {
                            logger.error("Unable to update configuration");
                            logger.error(JSON.stringify(response));
                        }
                        getPackagePlugins();
                    }).finally(function () {
                        vm.state.isBusy = false;
                    });
            }
        }
    }
})();

