(function () {
    'use strict';

    var directiveId = 'accountSearch';

    angular.module('app').directive(directiveId, ['$parse', 'remiapi', 'authService', 'common', 'config',
        function accountSearch($parse, remiapi, authService, common, config) {
            return {
                restrict: 'A',
                scope: {
                    onSubmit: '&',
                    getSelected: '&',
                    dialogTitle: '@',
                    multiSelect: '@',
                    ngDisabled: '=',
                    ngEnabled: '='
                },
                link: function (scope, element, attrs) {
                    element
                        .off('click')
                        .bind('click', function () {
                            scope.$apply(function () {
                                scope.clickHandler();
                            });
                        })
                        .addClass('pointer');
                },
                controller: function ($scope, $element, $attrs, $transclude, $http, $compile, $templateCache, $timeout) {
                    var logger = common.logger.getLogger(directiveId);

                    $scope.loggedInHandler = function () {
                        $scope.isAvailable = authService.isLoggedIn;
                    };
                    common.handleEvent(config.events.loggedIn, $scope.loggedInHandler, $scope);

                    $scope.$dialog = null;
                    $scope.state = {
                        isBusy: false,
                        busyCounter: 0
                    };
                    $scope.isEnabled = function () {
                        var isEnabled = true;

                        if (typeof $scope.ngDisabled == "boolean") {
                            isEnabled = !$scope.ngDisabled;
                        }
                        if (typeof $scope.ngEnabled == "boolean") {
                            isEnabled = $scope.ngEnabled && $scope.ngEnabled;
                        }

                        return isEnabled;
                    };

                    $scope.isAvailable = authService.isLoggedIn;

                    $scope.serviceCalled = false;
                    $scope.filterCriteria = '';
                    $scope.accounts = [];
                    $scope.showAccounts = [];

                    $scope.clickHandler = function () {
                        if ($scope.isAvailable)
                            showModal();
                    };

                    $scope.isMultiSelect = function () {
                        return !(($scope.multiSelect + '').toLowerCase() == 'false');
                    };

                    $scope.setSelected = function (item) {
                        item.selected = !item.selected;

                        if (item.selected && !$scope.isMultiSelect()) {
                            $scope.submit();
                        }
                    };

                    var showAccounts = function (criteria) {
                        for (var i = 0; i < $scope.accounts.length; i++) {
                            if (!$scope.accounts[i].used
                                && (!criteria
                                    || $scope.accounts[i].Account.FullName.toLowerCase().indexOf(criteria.toLowerCase()) > -1
                                    || $scope.accounts[i].Account.Email.toLowerCase().indexOf(criteria.toLowerCase()) > -1)
                                 && ($scope.showAccounts.filter(function(item) { return item.Account.Email.toLowerCase() == $scope.accounts[i].Account.Email.toLowerCase(); }).length == 0)) {
                                $scope.showAccounts.push($scope.accounts[i]);
                            }
                        }
                        return true;
                    };

                    $scope.searchByFilter = function () {
                        $scope.showAccounts = [];

                        var criteria = $scope.filterCriteria || '';
                        if (criteria.length < 4) {
                            $scope.serviceCalled = false;
                        }

                        if (!!criteria && !$scope.serviceCalled && criteria.length == 4) {
                            searchAccounts(criteria)
                                .then(function () { return showAccounts(criteria); });

                            $scope.serviceCalled = true;
                        } else {
                            showAccounts(criteria);
                        }
                    };

                    $scope.submit = function () {

                        var selectedItems = Enumerable.From($scope.accounts)
                            .Where("$.selected")
                            .Select("$.Account")
                            .ToArray();

                        $scope.onSubmit({ data: selectedItems });

                        hideModal();
                    };
                    $scope.cancel = function () {
                        hideModal();
                    };

                    $scope.$on('$destroy', function () {
                        if ($scope.$dialog) {
                            $scope.$dialog.remove();
                        }
                    });

                    var loadedTemplateUrl = function () {
                        var templ = $templateCache.get('accountSearchDialog.html');

                        if (!templ) {
                            $http({ method: 'GET', url: 'app/common/directives/tmpls/accountSearchDialog.html' })
                                .success(function (data, status, headers, config) {
                                    $templateCache.put('accountSearchDialog.html', data);

                                    parseTemplate(data);
                                });
                        } else {
                            parseTemplate(templ);
                        }
                    };

                    var parseTemplate = function (data) {
                        var m = $(data);
                        $('body').append(m);
                        //$element.after(m);

                        $scope.$dialog = $compile(m)($scope);

                        $scope.$dialog.on('hidden.bs.modal', function (e) {
                            //$('.modal-backdrop').remove();
                        });
                        $scope.$dialog.on('shown.bs.modal', function (e) {
                            $scope.$dialog.find('input[name="filterCriteria"]').focus();
                        });
                    };

                    var searchAccounts = function (criteria) {
                        $scope.state.isBusy = true;
                        $scope.state.busyCounter++;

                        return remiapi
                            .searchUsers(criteria)
                            .then(function (data) {
                                for (var i = 0; i < data.Accounts.length; i++) {

                                    var found = Enumerable
                                        .From($scope.accounts)
                                        .FirstOrDefault(null, function (item) {
                                            return item.Account.Email.toLowerCase() == data.Accounts[i].Email.toLowerCase();
                                        });

                                    if (!found) {
                                        $scope.accounts.push({ 'Account': data.Accounts[i], 'selected': false, 'used': false });
                                    }
                                }
                            }, function (error) {
                                console.log(error);
                                console.log('Cannot get users');
                            })
                            .finally(function () {
                                $scope.state.isBusy = false;
                                $scope.state.busyCounter--;
                            });
                    };

                    var getDbAccounts = function () {
                        $scope.accounts = [];

                        $scope.state.isBusy = true;
                        $scope.state.busyCounter++;

                        return remiapi
                            .getAccounts()
                            .then(function (data) {
                                data.Accounts.forEach(function (item) {
                                    $scope.accounts.push({ 'Account': item, 'selected': false, 'used': false });
                                });
                            })
                            .finally(function () {
                                $scope.state.isBusy = false;
                                $scope.state.busyCounter--;
                            });
                    };
                    
                    var showModal = function () {
                        if (!$scope.isEnabled()) {
                            logger.console('Service search is disabled');
                            logger.info('Not available');

                            return false;
                        }

                        var selectedAccounts = $scope.getSelected();

                        for (var i = $scope.accounts.length - 1; i >= 0; i--) {
                            var found = Enumerable
                                .From(selectedAccounts)
                                .FirstOrDefault(null, function (o) {
                                    return (o.Email && ($scope.accounts[i].Account.Email.toLowerCase() == o.Email.toLowerCase())) ||
                                            (o.ExternalId && ($scope.accounts[i].Account.ExternalId.toLowerCase() == o.ExternalId.toLowerCase()));
                                });

                            if (!!found)
                                $scope.accounts[i].used = true; //$scope.accounts.splice(i, 1);
                            else
                                $scope.accounts[i].used = false;
                        }

                        $scope.filterCriteria = '';
                        $scope.searchByFilter();

                        $scope.$dialog.modal('show');
                    };

                    var hideModal = function () {
                        $scope.$dialog.modal('hide');

                        if ($scope.validator) {
                            $scope.validator.resetForm();
                        }

                        for (var i = 0; i < $scope.accounts.length; i++) {
                            if ($scope.accounts[i].selected)
                                $scope.accounts[i].selected = false;
                        }

                        for (var i = 0; i < $scope.accounts.length; i++) {
                            if ($scope.accounts[i].selected)
                                $scope.accounts[i].selected = false;
                        }
                    };

                    $timeout(function () {
                        loadedTemplateUrl();

                        getDbAccounts()
                            .then($scope.searchByFilter);

                    }, 500);
                }

            };
        }
    ]);

})();
