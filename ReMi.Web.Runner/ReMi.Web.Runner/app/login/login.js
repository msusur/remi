(function () {
    'use strict';
    var controllerId = 'login';
    angular.module('app').controller(controllerId, ['$scope', '$rootScope', '$location', '$q', 'common', 'config', 'commonConfig', 'authService', 'remiapi', login]);

    function login($scope, $rootScope, $location, $q, common, config, commonConfig, authService, remiapi) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;
        vm.state = {
            isBusy: authService.state.isBusy,
            isManualLogin: false,
            isBusySettingDefaultProduct: false
        };

        common.handleEvent(config.events.authStateChanged, authStateChangedHandler, $scope);
        common.handleEvent(config.events.loggedIn, loggedInHandler, $scope);

        vm.isLoggedIn = authService.isLoggedIn;
        vm.username = authService.isLoggedIn ? authService.identity.name : '';
        vm.password = '';
        vm.products = undefined;
        vm.defaultProduct = undefined;

        vm.leavePage = leavePage;

        vm.logout = logoutAction;

        vm.login = loginAction;
        vm.getProducts = getProducts;
        vm.selectDefaultProduct = selectDefaultProduct;

        $scope.$on('$destroy', scopeDestroyHandler);

        activate();

        function activate() {
            common.activateController([
                leavepageIfLoggedin()
            ], controllerId)
                .then(function () { logger.console('Activated Login View'); });
        }

        function leavepageIfLoggedin() {
            if (vm.isLoggedIn) {
                common.$timeout(function () {
                    vm.leavePage();
                }, 500);
            }
        }

        function scopeDestroyHandler() {

        }

        function getProducts() {
            return remiapi.getProducts().then(function (data) {
                vm.products = data.Products;
                vm.defaultProduct = vm.products[0];
                $('#selectDefaultProductModal').modal('show');
            }, function (error) {
                logger.error('Cannot get products');
                logger.console(error);
            }
            );
        }

        function authStateChangedHandler(data) {
            if (data.isBusy)
                vm.state.isBusy = data.isBusy;
        }

        function loggedInHandler(data) {
            vm.isLoggedIn = true;
            vm.username = data.fullname;

            if (!vm.state.isManualLogin)
                vm.leavePage();
        }

        function logoutAction() {
            common.$broadcast(config.events.spinnerToggle, { show: true, message: 'Processing log out ...' });

            authService.logout().then(function (data) {
                vm.isLoggedIn = false;
                vm.username = '';
                vm.password = '';

                $location.path('/').search({});

                logger.success('Bye, ' + data.fullname + '!');
            })
            .finally(function () { common.$broadcast(config.events.spinnerToggle, { show: false }); });
        };

        function loginAction() {
            if ('' === vm.username || '' === vm.password) {
                logger.warn('User name or password is empty');
                return;
            }

            vm.state.isBusy = true;
            vm.state.isManualLogin = true;

            common.$broadcast(config.events.spinnerToggle, { show: true, message: 'Processing log in ...' });

            authService
                .startSession({ login: vm.username, password: vm.password })
                .then(function (response) {
                    vm.username = authService.identity.name;
                    vm.isLoggedIn = authService.isLoggedIn;

                    vm.leavePage();
                    logger.success('Welcome, ' + authService.identity.fullname + '!');
                }, function (status) {
                    if (status === 404) {
                        logger.warn('Invalid user/password');
                    } else {
                        logger.error('Operation failed!');
                    }
                    vm.password = '';
                })
                .finally(function () {
                    common.$broadcast(config.events.spinnerToggle, { show: false });

                    vm.state.isBusy = false;
                });
        };

        function leavePage() {
            var search = $location.search();

            var returnPath = search.returnPath;
            if (!!returnPath && returnPath !== '/login') {
                $location.path(returnPath).search('returnPath', null);
            }
            else
                $location.path('/').search({});
        }

        function selectDefaultProduct() {
            vm.state.isBusySettingDefaultProduct = true;

            var data = {
                Account: {
                    ExternalId: authService.identity.externalId,
                    Products: [{ ExternalId: vm.defaultProduct.ExternalId, IsDefault: true }]
                }
            }

            return remiapi.defaultProductForNewUser(data).then(function () {
                vm.defaultProduct.IsDefault = true;
                authService.identity.products = [vm.defaultProduct];
                authService.identity.product = vm.defaultProduct;
                common.sendEvent(config.events.productsAddedForUser);

                logger.success('Welcome, ' + authService.identity.fullname + '!');
            }, function (error) {
                logger.error('Cannot set default product');
                logger.console(error);
            }).finally(function () {
                vm.state.isBusySettingDefaultProduct = false;
                $('#selectDefaultProductModal').modal('hide');
                vm.leavePage();
            });

        }
    }
})();
