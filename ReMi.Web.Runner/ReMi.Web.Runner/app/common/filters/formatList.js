(function () {
    'use strict';

    angular.module('common').filter('formatList', function () {
        return function (array, getProperty, separator) {
            if (!array || !getProperty) return '';

            if (!separator)
                separator = ', ';

            if (getProperty.indexOf('.') >= 0)
                throw 'Nested property getter not allowed!';

            var result = '';
            for (var i = 0; i < array.length; i++) {
                if (array[i] && array[i][getProperty]) {
                    if (result.length != 0)
                        result += separator;

                    result += array[i][getProperty];
                }
            }

            return result;
        };
    });

})();
