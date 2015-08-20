(function () {
    'use strict';

    var controllerId = 'accounts';

    angular.module('app').controller(controllerId,
        ['$scope', '$rootScope', '$timeout', 'common', 'remiapi', 'config', 'localData', accounts]);

    function accounts($scope, $rootScope, $timeout, common, remiapi, config, localData) {
        var $q = common.$q;
        var logger = common.logger.getLogger(controllerId);

        var vm = this;

        vm.state = {
            isBusy: false,
            isAccountModalBusy: false
        };
        vm.accountModalMode = '';

        vm.accounts = [];
        vm.currentAccount = {};

        vm.showAccountAdd = showAccountAdd;
        vm.showAccountUpdate = showAccountUpdate;
        vm.showAccountModalMode = showAccountModalMode;
        vm.hideCurrentAccountModal = hideCurrentAccountModal;

        vm.refreshAccounts = refreshAccounts;
        vm.saveAccount = saveAccount;


        vm.roles = [];

        vm.businessUnits = getAllBusinessUnits();

        vm.roleChanged = roleChanged;

        common.handleEvent(config.events.businessUnitsLoaded, businesUnitsLoadedHandler, $scope);

        vm.accountSorter = function (account) {
            return account.Email;
        };

        activate();

        return vm;

        function activate() {
            common.activateController([getAccountRoles(), refreshAccounts()], controllerId)
                .then(function () { logger.console('Activated Accounts View'); });
        }

        function hideCurrentAccountModal() {
            $('#accountModal').modal('hide');
        }

        function validate() {
            var deferred = $q.defer();

            if (!vm.currentAccount.Products
                || vm.currentAccount.Products.length == 0
                || Enumerable.From(vm.currentAccount.Products).All(function(p) { return !p.IsDefault; })) {
                common.showInfoMessage('Invalid data', 'Please choose at least one package and select default.');
                deferred.reject('Request is invalid');
            } else {
                deferred.resolve(vm.currentAccount);
            }

            return deferred.promise;
        }

        function getAccountRoles() {
            return remiapi.getRoles().then(function (response) {
                vm.roles = response.Roles.filter(function (item) {
                    return item.Name != 'NotAuthenticated';
                });
            });
        }

        function updateAccounts(account) {
            var found = findInArray(vm.accounts, function (item) {
                return item.ExternalId == account.ExternalId;
            });

            if (found) {
                vm.accounts.splice(vm.accounts.indexOf(found), 1);
            }

            vm.accounts.push(account);
            return true;
        }

        function refreshAccounts() {
            vm.state.isBusy = true;
            vm.accounts = [];

            return remiapi
                .getAccounts()
                .then(function (data) {
                    vm.accounts = data.Accounts;
                }, function (status) {
                    if (status != 401) {
                        common.showErrorMessage('<p>Request failed!</p>');
                    }
                }).finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function showAccountAdd() {
            vm.currentAccount = {
                Role: vm.roles[0],
                RoleId: vm.roles[0].ExternalId,
                ExternalId: newGuid()
            };

            selectAndCheckBusinessUnits([]);

            vm.showAccountModalMode('add');
        }

        function showAccountUpdate(account) {
            vm.currentAccount = angular.copy(account);
            selectAndCheckBusinessUnits(vm.currentAccount.Products);
            vm.currentAccount.RoleId = vm.currentAccount.Role.ExternalId;

            vm.showAccountModalMode('update');
        }

        function showAccountModalMode(mode) {
            vm.accountModalMode = mode;

            $('#accountModal').modal({ backdrop: 'static', keyboard: true });
        }

        function saveAccount(operationType) {
            vm.state.isAccountModalBusy = true;

            var deferred = $q.when();

            return deferred
                .then(fillAccountProducts)
                .then(validate)
                .then(function () {
                    vm.roles.forEach(function (role) {
                        if (role.Name == vm.currentAccount.Role.Name) {
                            vm.currentAccount.Role = role;
                        }
                    });

                    if (operationType == 'add') {
                        return remiapi.createAccount({ Account: vm.currentAccount });
                    }

                    return remiapi.updateAccount({ Account: vm.currentAccount });
                })
                .then(function () {
                    var account = angular.copy(vm.currentAccount);

                    updateAccounts(account);

                    $('#accountModal').modal('hide');
                })
                .catch(function (fault) {
                    logger.error('Can\'t save account');
                    console.log(fault);
                })
                .finally(function () { vm.state.isAccountModalBusy = false; });
        }

        function selectAndCheckBusinessUnits(accountPackages) {
            var enumerableAccountPackages = Enumerable.From(accountPackages);
            var businessUnits = Enumerable.From(vm.businessUnits);
            businessUnits.ForEach(function (bu) {
                if (bu.Packages && bu.Packages.length > 0) {
                    Enumerable.From(bu.Packages).ForEach(function ($package) {
                        $package.Checked = false;
                        $package.IsDefault = false;
                        var found = enumerableAccountPackages.Where(function (p) {
                            return $package.ExternalId.toLowerCase() == p.ExternalId.toLowerCase();
                        }).FirstOrDefault();
                        if (found) {
                            $package.Checked = true;
                            $package.IsDefault = found.IsDefault;
                        }
                    });
                }
            });
            vm.businessUnits = businessUnits.ToArray();
        }

        function selectAllBusinessUnits() {
            var hasDefault = false;
            var businessUnits = Enumerable.From(vm.businessUnits);
            businessUnits.ForEach(function (bu) {
                if (bu.Packages && bu.Packages.length > 0) {
                    Enumerable.From(bu.Packages).ForEach(function ($package) {
                        if ($package.IsDefault) hasDefault = true;
                        else $package.IsDefault = false;
                        $package.Checked = true;
                    });
                }
            });
            vm.businessUnits = businessUnits.ToArray();
            if (!hasDefault) {
                vm.businessUnits[0].Packages[0].IsDefault = true;
            }
        }

        function fillAccountProducts() {
            vm.currentAccount.Products = [];
            vm.businessUnits.forEach(function (bu) {
                bu.Packages.forEach(function (p) {
                    if (p.Checked) {
                        var selectedProduct = angular.copy(p);
                        vm.currentAccount.Products.push(selectedProduct);
                    }
                });
            });
        }

        function roleChanged() {
            vm.currentAccount.Role = Enumerable.From(vm.roles)
                .Where(function (x) {
                    return x.ExternalId.toLowerCase() == vm.currentAccount.RoleId.toLowerCase();
                }).FirstOrDefault();
            if (vm.currentAccount.Role.Name == 'Admin') {
                selectAllBusinessUnits();
            }
            for (var counter = 0; counter < vm.roles.length; counter++) {
                if (vm.roles[counter].Value == vm.currentAccount.Role.Name) {
                    vm.currentAccount.Role.Description = vm.roles[counter].Text;
                    break;
                }
            }
        }

        function getAllBusinessUnits() {
            return angular.copy(localData.businessUnits);
        }

        function businesUnitsLoadedHandler(data) {
            vm.businessUnits = angular.copy(data);
        }
    }
})();

