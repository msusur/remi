(function () {
    'use strict';

    angular.module('app').directive('accountSignature', ['$parse', 'authService', 'common',
        function($parse, authService, common) {
            return {
                restrict: 'A',
                scope: {
                    ngClick: '&',
                    beforeShow: '&',
                    defaultUserName: '@',
                    description: '@'
                },
                link: function (scope, element) {
                    if (authService.isLoggedIn) {
                        element
                            .off('click')
                            .bind('click', function () {
                                scope.$apply(function () {
                                    scope.clickHandler();
                                });
                            });
                    } else {
                        element
                            .off('click')
                            //.addClass('disabled')
                            .remove();

                    }
                },
                controller: function ($scope, $element, $attrs, $transclude, $http, $compile, $templateCache) {

                    $scope.$dialog = null;
                    $scope.password = null;
                    $scope.state = {
                        isBusy: false
                    };

                    if (authService.isLoggedIn) {
                        $scope.userName = '';
                        $scope.password = '';
                        $scope.descriptionValue = null;

                        $scope.sign = function () {
                            if (!$scope.validate()) return;

                            $scope.state.isBusy = true;
                            var deferred = common.$q.defer();
                            $scope.data = {
                                deferred: deferred,
                                userName: $scope.userName,
                                password: $scope.password
                            };
                            if ($scope.description) {
                                $scope.data[$scope.description] = $scope.descriptionValue;
                            }

                            $scope.ngClick($scope, $scope.data);

                            deferred.promise
                                .then(function () {
                                    hideModal();
                                })
                                .finally(function () {
                                    $scope.password = null;
                                    $scope.state.isBusy = false;
                                });
                        };
                        $scope.validate = function () {
                            if ($scope.validator) {
                                return $scope.$dialog.find('form').valid();
                            }
                            return true;
                        };

                        $scope.cancel = function () {
                            hideModal();
                        };
                        $scope.clickHandler = function () {
                            var defer = common.$q.defer();

                            defer.promise.then(function () {
                                $scope.$dialog.modal('show');
                                if ($scope.description) {
                                    $scope.signatureContent = 'signature-content';
                                }
                            });
                            if (angular.isDefined($attrs.beforeShow)) {
                                $scope.beforeShow({ defer: defer });
                            } else {
                                defer.resolve();
                            }
                        };
                        $scope.$on('$destroy', function () {
                            if ($scope.$dialog)
                                $scope.$dialog.remove();
                        });

                        var loadedTemplateUrl = function () {
                            var templ = $templateCache.get('accountSignatureDialog.html');
                            if (!templ) {
                                $http({ method: 'GET', url: 'app/common/directives/tmpls/accountSignatureDialog.html' })
                                    .success(function (data) {
                                        $templateCache.put('accountSignatureDialog.html', data);

                                        parseTemplate(data);
                                    });
                            } else {
                                parseTemplate(templ);
                            }
                        };

                        var parseTemplate = function (data) {
                            var m = $(data);
                            $element.after(m);

                            $scope.$dialog = $compile(m)($scope);

                            $scope.$dialog.on('hidden.bs.modal', function () {
                                $('.modal-backdrop').remove();
                            });
                            $scope.$dialog.on('shown.bs.modal', function () {
                                $scope.$dialog.find('input[name="password"]').focus();
                            });

                            $scope.validator = $scope.$dialog.find('form').validate({
                                errorClass: 'invalid-data',
                                errorPlacement: function () { },
                            });
                        };

                        var hideModal = function () {
                            resetUserName();

                            $scope.$dialog.modal('hide');

                            if ($scope.validator) {
                                $scope.validator.resetForm();
                            }
                        };

                        var resetUserName = function () {
                            if (!!$scope.defaultUserName)
                                $scope.userName = $scope.defaultUserName;
                            else
                                $scope.userName = authService.identity.email;
                        };

                        resetUserName();

                        loadedTemplateUrl();
                    }
                }
            };
        }
    ]);

})();
