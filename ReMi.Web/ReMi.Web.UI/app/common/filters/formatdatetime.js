(function () {
    'use strict';

    angular.module('common').filter('formatdatetime', function () {
        return function (value) {
            if (value && Object.prototype.toString.call(value) === '[object Date]') {
                return moment(value).format('L HH:mm');

            } else if (typeof value === 'string') {
                try {
                    var parsed = Date.parse(value);
                    if (parsed)
                        return moment(parsed).format('L HH:mm');

                } catch (err) {
                    console.log('formatdatetime filter failed for ', value, err);
                }
            }

            return value;
        };
    });

})();
