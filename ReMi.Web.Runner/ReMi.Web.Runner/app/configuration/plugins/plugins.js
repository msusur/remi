(function () {
    "use strict";

    var controllerId = "plugins";

    angular.module("app").controller(controllerId, plugins);

    function plugins($scope, common, remiapi, config, localData, $routeParams, $route) {
        var logger = common.logger.getLogger(controllerId);
        var tabs = ["globalConfiguration", "packageConfiguration", "plugins"];

        var vm = this;

        vm.controllerId = controllerId;

        var lastRoute = $route.current;
        vm.tabName = getTabName($routeParams.tab);
        vm.activeTab = {};

        vm.state = {
            isBusy: false
        };

        vm.businessUnits = [];
        vm.pluginTypes = [];
        vm.packagesDeferred = common.$q.defer();
        vm.tabChange = tabChange;

        common.handleEvent(config.events.businessUnitsLoaded, businesUnitsLoadedHandler, $scope);
        common.handleEvent(config.events.locationChangeSuccess, locationChangedHandler, $scope);

        activate();

        return vm;

        function activate() {
            vm.initializationPromise = initialize();
            common.activateController([vm.initializationPromise, setActiveTab(vm.tabName)], controllerId)
                .then(function () { logger.console("Activated Plugins View"); });
        }

        function locationChangedHandler() {
            if ($route.current.$$route.title === lastRoute.$$route.title) {
                vm.tabName = getTabName($route.current.params.tab);
                setActiveTab(vm.tabName);
                $route.current = lastRoute;
            }
        }

        function setActiveTab(tabName) {
            angular.forEach(tabs, function (tab) {
                var isCurrent = tab === tabName;
                if (typeof vm.activeTab[tab] !== "undefined") {
                    vm.activeTab[tab].isCurrent = isCurrent;
                } else {
                    vm.activeTab[tab] = { isCurrent: isCurrent, loaded: false }
                }
                if (isCurrent) {
                    vm.activeTab[tab].loaded = true;
                }
            });
            $route.updateParams({ "tab": tabName });
        }

        function tabChange(tabName) {
            vm.tabName = getTabName(tabName);
            setActiveTab(vm.tabName);
        }

        function getTabName(tabName) {
            return tabName && tabs.indexOf(tabName) >= 0
                ? angular.copy(tabName)
                : tabs[0];
        }

        function initialize() {
            toggleSpinner(true);
            return getAllBusinessUnits()
                .then(function () {
                    return getReleaseTypes();
                }).finally(function () {
                    toggleSpinner(false);
                });
        }

        function getAllBusinessUnits() {
            return localData.businessUnitsPromise()
                .then(function (businessUnits) {
                    vm.businessUnits = angular.copy(businessUnits);
                });
        }

        function getReleaseTypes() {
            return localData.getEnum("PluginType")
                .then(function (pluginTypes) {
                    vm.pluginTypes = pluginTypes;
                });
        }

        function toggleSpinner(show) {
            vm.state.isBusy = show;
            common.$broadcast(config.events.spinnerToggle, { show: show, message: "Loading Plugins ..." });
        }

        function businesUnitsLoadedHandler(data) {
            vm.businessUnits = angular.copy(data);
        }
    }
})();

