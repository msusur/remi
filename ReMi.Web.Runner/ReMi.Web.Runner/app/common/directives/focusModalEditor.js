(function () {
    'use strict';

    var directiveId = 'focusModalEditor';

    angular.module('app').directive(directiveId, [focusModalEditor]);

    function focusModalEditor() {
        return {
            restrict: 'A',
            scope: {
                focusModalEditor: '@'
            },
            link: function ($scope, elem) {
                if (elem && elem[0] && $scope.focusModalEditor) {
                    var modal = $(elem[0]);
                    modal.on('shown.bs.modal', function (evt) {
                        $(evt.currentTarget).find($scope.focusModalEditor).focus();
                    });
                    $scope.$on('$destroy', function () {
                        modal.off('shown.bs.modal');
                    });
                }
            }
        };
    }
})()
