(function () {
    'use strict';

    angular.module('common').filter('formatArray', function () {
        return function (array, separator) {
            if (!array) return '';

            if (!separator)
                separator = ', ';

            var result = '';
            for (var i = 0; i < array.length; i++) {
                if (array[i] && array[i]) {
                    if (result.length != 0)
                        result += separator;

                    result += array[i];
                }
            }

            return result;
        };
    });

})();
