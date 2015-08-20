(function () {
    "use strict";

    var controllerId = "pluginsConfiguration";

    angular.module("app").controller(controllerId, pluginsConfiguration);

    function pluginsConfiguration($scope, common, config, remiapi, $location, localData) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;

        vm.parent = common.getParentScope($scope, function (sc) {
            return sc && sc.vm && sc.vm.controllerId === "plugins";
        }).vm;

        vm.state = {
            isBusy: false
        };
        vm.activeTab = {};

        vm.parent.plugins = [];

        vm.packages = [];
        vm.refreshPlugins = getPlugins;
        vm.getGlobalPluginConfiguration = getGlobalPluginConfiguration;
        vm.getPackagePluginConfiguration = getPackagePluginConfiguration;
        vm.valueHasChanged = valueHasChanged;
        vm.addNameValue = addNameValue;
        vm.removeNameValue = removeNameValue;
        vm.saveGlobalConfiguration = saveGlobalConfiguration;
        vm.savePackageConfiguration = savePackageConfiguration;
        vm.setActiveTab = setActiveTab;
        vm.loadPluginConfiguration = loadPluginConfiguration;

        common.handleEvent(config.events.navRouteUpdate, routeUpdateHandler, $scope);

        activate();

        return vm;

        function activate() {
            common.activateController([initialize()], controllerId)
                .then(function () { logger.console("Activated PluginsConfiguration Controller"); });
        }

        function initialize() {
            if (vm.parent && vm.parent.initializationPromise) {
                vm.parent.initializationPromise
                    .then(getPlugins);
            }
        }

        function routeUpdateHandler() {
            var plugin = getSearchedPlugin(vm.parent.plugins);
            setActiveTab(vm.parent.plugins, plugin);
        }

        function getSearchedPlugin(plugins) {
            var params = $location.search();
            if (params && params.plugin) {
                for (var i in plugins) {
                    if (plugins.hasOwnProperty(i)) {
                        var plugin = plugins[i];
                        if (plugin.PluginKey === params.plugin) return plugin;
                    }
                }
            }
            return plugins[0];
        }

        function setActiveTab(plugins, plugin) {
            if (!plugin || !plugins || plugins.length <= 0) return;

            angular.forEach(plugins, function (p) {
                var isCurrent = p.PluginKey === plugin.PluginKey;
                if (typeof vm.activeTab[p.PluginKey] !== "undefined") {
                    vm.activeTab[p.PluginKey].isCurrent = isCurrent;
                } else {
                    vm.activeTab[p.PluginKey] = { isCurrent: isCurrent, loaded: false }
                }
                if (isCurrent && !vm.activeTab[p.PluginKey].loaded) {
                    vm.activeTab[p.PluginKey].loaded = true;
                    loadPluginConfiguration(plugin);
                }
            });
            $location.search("plugin", plugin.PluginKey);
        }

        function getPlugins() {
            vm.state.isBusy = true;
            return remiapi.get.plugins()
                .then(function (plugins) {
                    vm.parent.plugins = plugins && plugins.Plugins ? plugins.Plugins : [];
                    if (vm.parent.plugins) {
                        setActiveTab(vm.parent.plugins, getSearchedPlugin(vm.parent.plugins));
                    }
                }, function (response, status) {
                    if (response !== 406 && status !== 406) {
                        logger.error("Unable to load plugins");
                        logger.error(response && response.Details ? response.Details : JSON.stringify(response));
                    }
                }).finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function loadPluginConfiguration(plugin) {
            vm.state.isBusy = true;
            return remiapi.get.pluginConfituration(plugin.PluginId)
                .then(function (config) {
                    if (config && config.Plugin) {
                        angular.copy(config.Plugin, plugin);
                        if (!plugin.PackageConfiguration) return;
                        localData.businessUnitsPromise().then(function (businessUnits) {
                            if (!vm.packages || vm.packages.length === 0) {
                                vm.packages = Enumerable.From(businessUnits).SelectMany(function (x) { return x.Packages; }).ToArray();
                            }
                            angular.forEach(vm.packages, function ($package) {
                                if (!plugin.PackageConfiguration[$package.ExternalId]) {
                                    plugin.PackageConfiguration[$package.ExternalId] = {};
                                }
                            });
                        });
                    }
                }, function (response, status) {
                    if (response !== 406 && status !== 406) {
                        logger.error("Unable to load plugins");
                        logger.error(response && response.Details ? response.Details : JSON.stringify(response));
                    }
                }).finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function getGlobalPluginConfiguration(plugin) {
            if (!plugin || !plugin.PluginId)
                return common.$q.when();
            vm.state.isBusy = true;
            return remiapi.get.globalPluginConfiguration(plugin.PluginId)
                .then(function (globalPluginConfiguration) {
                    if (globalPluginConfiguration) {
                        angular.forEach(globalPluginConfiguration.GlobalConfiguration, function (value, key) {
                            plugin.GlobalConfiguration[key] = value;
                        });
                        plugin.haveChanges = false;
                    }
                }, function (response, status) {
                    if (response !== 406 && status !== 406) {
                        logger.error("Unable to load global configuration");
                        logger.error(response && response.Details ? response.Details : JSON.stringify(response));
                    }
                }).finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function getPackagePluginConfiguration(plugin, $package) {
            if (!plugin || !plugin.PluginId || !$package || !$package.ExternalId)
                return common.$q.when();
            vm.state.isBusy = true;
            return remiapi.get.packagePluginConfiguration(plugin.PluginId, $package.ExternalId)
                .then(function (plupackagePluginConfigurationgins) {
                    if (plupackagePluginConfigurationgins) {
                        var config = plugin.PackageConfiguration[$package.ExternalId];
                        angular.forEach(plupackagePluginConfigurationgins.PackageConfiguration, function (value, key) {
                            config[key] = value;
                        });
                        config.haveChanges = false;
                    }
                }, function (response, status) {
                    if (response !== 406 && status !== 406) {
                        logger.error("Unable to load package configuration");
                        logger.error(response && response.Details ? response.Details : JSON.stringify(response));
                    }
                }).finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function valueHasChanged(plugin, $package) {
            if (!plugin)
                return;
            if (!$package) {
                plugin.haveChanges = true;
            } else {
                plugin.PackageConfiguration[$package.ExternalId].haveChanges = true;
            }
        }

        function addNameValue(list, plugin, $package, propertyName) {
            if (!list) {
                list = $package
                    ? plugin.PackageConfiguration[$package.ExternalId][propertyName] = []
                    : plugin.GlobalConfiguration[propertyName] = [];
            }
            list.push({ Name: "", Value: "" });
            valueHasChanged(plugin, $package);
        }

        function removeNameValue(list, item, plugin, $package) {
            var index = list.indexOf(item);
            if (index >= 0) {
                list.splice(index, 1);
            }
            valueHasChanged(plugin, $package);
        }

        function saveGlobalConfiguration(plugin) {
            if (!plugin || !plugin.PluginId)
                return common.$q.when();
            var command = { PluginId: plugin.PluginId, JsonValues: JSON.stringify(plugin.GlobalConfiguration) };
            vm.state.isBusy = true;
            return remiapi.post.updatePluginGlobalConfiguration(command)
                .then(function () {
                    plugin.haveChanges = false;
                }, function (response, status) {
                    if (response !== 406 && status !== 406) {
                        logger.error("Unable to save plugin global configuration");
                        logger.error(response && response.Details ? response.Details : JSON.stringify(response));
                    }
                }).finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function savePackageConfiguration(plugin, $package) {
            if (!plugin || !plugin.PluginId || !$package || !$package.ExternalId)
                return common.$q.when();
            vm.state.isBusy = true;
            delete plugin.PackageConfiguration[$package.ExternalId].haveChanges;
            plugin.PackageConfiguration[$package.ExternalId].PackageId = $package.ExternalId;
            var command = {
                PluginId: plugin.PluginId,
                PackageId: $package.ExternalId,
                JsonValues: JSON.stringify(plugin.PackageConfiguration[$package.ExternalId])
            };
            return remiapi.post.updatePluginPackageConfiguration(command)
                .then(null, function (response, status) {
                    if (response !== 406 && status !== 406) {
                        logger.error("Unable to save plugin package configuration");
                        logger.error(response && response.Details ? response.Details : JSON.stringify(response));
                    }
                    plugin.PackageConfiguration[$package.ExternalId].haveChanges = true;
                }).finally(function () {
                    vm.state.isBusy = false;
                });
        }
    }
})();

