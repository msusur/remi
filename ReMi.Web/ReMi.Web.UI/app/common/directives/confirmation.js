(function () {
    'use strict';

    angular.module('app').directive('confirmation', ['$parse',
        function confirmation($parse) {
            return {
                restrict: 'A',
                scope: {
                    ngClick: '&',
                    removingSubject: '@',
                    removingItem: '@',
                    isCritical: '@',
                    confirmType: '@',
                    targetItem: '@',
                    targetItemType: '@',
                    confirmation: '@'
                },
                link: function (scope, element, attrs) {
                    element
                        .off('click')
                        .bind('click', function (event) {
                            scope.$apply(function () {
                                scope.clickHandler();
                            });
                        });
                },
                controller: function ($scope, $element, $attrs, $transclude, $http, $compile, $templateCache) {
                    $scope.$dialog = null;

                    $scope.approve = function () {
                        $scope.ngClick($scope, $scope.data);
                        hideModal();
                    };

                    $scope.cancel = function () {
                        hideModal();
                    };
                    $scope.clickHandler = function () {
                        $scope.$dialog.modal('show');
                    };
                    $scope.$on('$destroy', function () {
                        if ($scope.$dialog)
                            $scope.$dialog.remove();
                    });
                    if ($scope.isCritical && $scope.isCritical.toLowerCase() === 'false')
                        $scope.actionRisk = 'Action';
                    else
                        $scope.actionRisk = 'Critical action';

                    if (!$scope.confirmation) {
                        switch ($scope.confirmType) {
                            case 'question':
                                $scope.confirmationText = $scope.confirmation;

                            case 'remove':
                            default:
                                var removeTemplate = 'Are you sure that you want to remove the {0} "{1}"?';

                                if ($scope.targetItem && $scope.targetItemType) {
                                    $scope.confirmationText = removeTemplate.format($scope.targetItemType, $scope.targetItem);
                                } else
                                    $scope.confirmationText = removeTemplate.format($scope.removingSubject, $scope.removingItem);
                        }
                    } else {
                        $scope.confirmationText = $scope.confirmation;
                    }


                    var loadedTemplateUrl = function () {
                        var templ = $templateCache.get('confirmation.html');
                        if (!templ) {
                            $http({ method: 'GET', url: 'app/common/directives/tmpls/confirmation.html' })
                                .success(function (data, status, headers, config) {
                                    $templateCache.put('confirmation.html', data);

                                    parseTemplate(data);
                                });
                        } else {
                            parseTemplate(templ);
                        }
                    };

                    var parseTemplate = function (data) {
                        var m = $(data);
                        $('body').after(m);

                        $scope.$dialog = $compile(m)($scope);

                        $scope.$dialog.on('hidden.bs.modal', function (e) {
                            $('.modal-backdrop').remove();
                        });

                        $scope.$dialog.on('shown.bs.modal', function (e) {
                            $scope.$dialog.find('button[name="cancelConfirmation"]').focus();
                        });
                    };

                    var hideModal = function () {
                        $scope.$dialog.modal('hide');
                        $('body').removeClass('modal-open');
                        $('.modal-backdrop').remove();
                    };

                    loadedTemplateUrl();
                }
            };
        }
    ]);
})();
