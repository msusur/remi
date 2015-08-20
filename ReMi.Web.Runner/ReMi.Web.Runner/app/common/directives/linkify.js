(function () {
    'use strict';

    var directiveId = 'linkify';

    angular.module('app').directive(directiveId, ['$timeout', linkify]);

    function linkify($timeout) {
        return {
            restrict: 'A',
            templateUrl: 'app/common/directives/tmpls/linkify.html',
            scope: {
                data: '=',
                editor: '=',
                editdisabled: '='
            },
            link: function ($scope, elem) {
                $scope.toggleEditPlaceHolder = function () {
                    $scope.editor = !$scope.editor;

                    if (elem && elem[0] && $scope.editor) {
                        $timeout(function() {
                            $(elem[0]).find('textarea').focus();
                        });
                    }
                };
            }
        };
    }
})()
