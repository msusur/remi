(function () {
    "use strict";
    // changin date from UTC to Local and displaying in wanted format
    angular.module("common").filter("localdate", function () {
        return function (value, format) {
            format = format ? format : "L HH:mm";
            if (value && Object.prototype.toString.call(value) === "[object Date]") {
                return moment(value).local().format(format);

            } else if (typeof value === "string") {
                var parsed = moment(value);
                if (parsed.isValid())
                    return parsed.local().format(format);
                else {
                    console.log("localdate filter failed for " + value);
                }
            }

            return value;
        };
    });
})();
