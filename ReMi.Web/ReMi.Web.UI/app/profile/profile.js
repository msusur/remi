(function () {
    'use strict';

    var controllerId = 'profile';

    angular.module('app')
        .controller(controllerId, [
            '$scope', 'common', 'config', 'authService', 'localData', 'remiapi', profile]);

    function profile($scope, common, config, authService, localData, remiapi) {
        var log = common.logger.getLogger(controllerId);

        var vm = this;
        vm.account = {};
        vm.businessUnits = [];
        vm.state = {
            isBusy: false
        };

        vm.readAccountInfo = readAccountInfo;
        vm.updateAccountPackages = updateAccountPackages;

        common.handleEvent(config.events.businessUnitsLoaded, businesUnitsLoadedHandler, $scope);

        activate();

        function activate() {
            var promises = [
                readAccountInfo()
            ];

            common.activateController(promises, controllerId)
                .then(function () { log.console('Activated Profile View'); });
        }

        function readAccountInfo() {
            if (!authService.isLoggedIn) throw "No active logged in account";

            vm.state.isBusy = true;

            vm.account = angular.copy(authService.identity);

            return remiapi.get.businessUnits(true)
                .then(function (response) {
                    initBusinessUnits(localData.businessUnits, response.BusinessUnits);
                }, function () {
                    log.error('Cannot load business units');
                })
            .finally(function () {
                vm.state.isBusy = false;
            });
        }

        function businesUnitsLoadedHandler(data) {
            initBusinessUnits(data, vm.businessUnits);
        }

        function initBusinessUnits(userBusinessUnits, businessUnits) {
            vm.userBusinessUnits = userBusinessUnits;
            vm.businessUnits = businessUnits;

            if (!userBusinessUnits || !businessUnits
                || userBusinessUnits.length === 0
                || businessUnits.length === 0) return;

            var userPackagesEnum = Enumerable.From(userBusinessUnits).SelectMany(function (bu) { return bu.Packages; });
            var packagesEnum = Enumerable.From(businessUnits).SelectMany(function (bu) { return bu.Packages; });

            packagesEnum.ForEach(function ($package) {
                var found = userPackagesEnum.Where(function (p) { return p.ExternalId === $package.ExternalId; })
                    .FirstOrDefault();
                $package.Checked = !!found;
                $package.IsDefault = found && found.IsDefault;
            });
        }

        function updateAccountPackages() {
            var accountId = vm.account.externalId;
            if (!accountId) {
                common.showWarnMessage("AccountId is empty");
                return null;
            }
            var packagesEnumerable = Enumerable.From(vm.businessUnits)
                .SelectMany(function (x) { return x.Packages; })
                .Where(function (x) { return x.Checked; });
            var packageIds = packagesEnumerable.Select(function (x) { return x.ExternalId; }).ToArray();
            if (!packageIds || packageIds.length === 0) {
                common.showWarnMessage("Please check at least one packages");
                return null;
            }
            var defaultPackage = packagesEnumerable.Where(function (x) { return x.IsDefault; }).FirstOrDefault();
            if (!defaultPackage || !defaultPackage.ExternalId) {
                common.showWarnMessage("Please select default package");
                return null;
            }
            vm.state.isBusy = true;
            return remiapi.post.updateAccountPackages({
                AccountId: accountId,
                PackageIds: packageIds,
                DefaultPackageId: defaultPackage.ExternalId
            })
                .then(function () {
                    common.$broadcast(config.events.productsAddedForUser, {});
                }, function (error) {
                    common.showErrorMessage('Could not update user packages');
                    log.error(error);
                    vm.readAccountInfo();
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }
    }
})();
