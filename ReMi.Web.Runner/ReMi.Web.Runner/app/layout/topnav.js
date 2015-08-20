(function () {
    'use strict';

    var controllerId = 'topnav';
    angular.module('app')
        .controller(controllerId, ['$rootScope', '$scope', '$location', 'common', 'config', 'authService', 'remiapi', 'localData', 'notifications', topnav]);

    function topnav($rootScope, $scope, $location, common, config, authService, remiapi, localData, notifications) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;

        vm.title = 'R.e.M.i.';

        vm.isLoggedIn = authService.isLoggedIn;
        vm.userDisplayName = authService.identity.fullname;
        vm.logOut = function() {
            authService.logout().then(function() { $location.path('/').search({}); });
        };
        vm.productChanged = function(bu, $package) {
            vm.package = $package;
            vm.businessUnit = bu;
            authService.selectProduct($package);
        };

        $rootScope.$on(config.events.loggedIn, loggedInHandler);

        $rootScope.$on(config.events.productsAddedForUser, productAddedForUserhandler);

        $rootScope.$on(config.events.loggedOut, loggedOutHandler);

        $rootScope.$on(config.events.productContextChanged, productContextChanged);

        notifications.subscribe('BusinessUnitsChangedEvent', {});
        common.handleEvent(config.events.notificationReceived, serverNotificationHandler, $scope);

        $scope.$on('$destroy', scopeDestroyHandler);
        
        activate();

        function activate() {
            common.activateController([
                populateBusinessUnits()
            ], controllerId);
        }

        function scopeDestroyHandler() {
            notifications.unsubscribe('BusinessUnitsChangedEvent');
        }

        function loggedInHandler(event, data) {
            vm.isLoggedIn = true;
            vm.userDisplayName = data.fullname;

            populateBusinessUnits();
        }

        function productAddedForUserhandler() {
            populateBusinessUnits();
        }

        function loggedOutHandler() {
            vm.isLoggedIn = false;
            vm.userDisplayName = '';

            vm.businessUnits = [];
            vm.package = null;
        }

        function productContextChanged(event, $package) {
            if (!vm.businessUnits) return;
            vm.package = $package;
            if ($package && vm.businessUnits && vm.businessUnits.length > 0) {
                vm.businessUnit = Enumerable.From(vm.businessUnits)
                    .First(function(x) {
                        return Enumerable.From(x.Packages).Any(function(p) {
                            return p.ExternalId == $package.ExternalId;
                        });
                    });
            }
        }

        function businessUnitsChangeHandler(businessUnits) {
            vm.businessUnits = businessUnits;
            localData.businessUnitsResolve(vm.businessUnits);
            vm.businessUnit = Enumerable.From(vm.businessUnits)
                .Where(function (x) { return Enumerable.From(x.Packages).Any(function (p) { return p.IsDefault; }); })
                .FirstOrDefault();
            vm.package = vm.businessUnit && vm.businessUnit.Packages
                ? Enumerable.From(vm.businessUnit.Packages).Where(function(p) { return p.IsDefault; }).FirstOrDefault()
                : undefined;
            common.$broadcast(config.events.businessUnitsLoaded, businessUnits);
        }

        function populateBusinessUnits() {
            return remiapi.get.businessUnits()
                .then(function(response) {
                    businessUnitsChangeHandler(response.BusinessUnits);
                }, function() {
                    logger.error('Cannot load business units');
                });
        }

        function serverNotificationHandler(notification) {
            if (notification.name == 'BusinessUnitsChangedEvent') {
                populateBusinessUnits();
            }
        }
    }
})();
