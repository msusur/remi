(function () {
    'use strict';

    angular.module('app').directive('riskPriorityIcon', [function riskPriorityIcon() {
        return {
            restrict: 'A',
            replace: false,
            scope: {
                priority: '='
            },
            link: function (scope, tElement, tAttrs) {

                var getIcon = function (priority) {
                    var p = (priority || '').toLowerCase();

                    var icon, color;
                    switch (p) {
                        case 'high':
                            icon = 'fa-arrow-up ';
                            color = 'red';
                            break;

                        case 'low':
                            icon = 'fa-long-arrow-down';
                            color = 'green';
                            break;

                        default:
                            icon = 'fa-long-arrow-up';
                            color = 'blue';
                    }
                    return { 'icon': icon, 'color': color };
                };

                scope.$watch('priority', function () {
                    var ico = getIcon(scope.priority);

                    $(tElement).removeAttr('class');

                    $(tElement)
                        .addClass('fa')
                        .addClass(ico.icon)
                        .css('color', ico.color)
                        .attr('title', scope.priority);
                });
            }
        };
    }
    ]);

})();
