(function () {
    "use strict";

    var directiveId = "ngContextmenu";

    angular.module("app").directive(directiveId, ["$parse", ngContextmenu]);

    function ngContextmenu($parse) {
        return {
            restrict: "A",
            link: function (scope, elem, attr) {
                var fn = $parse(attr[directiveId]);
                var contextmenuHandler = function (event) {
                    var callback = function () {
                        fn(scope, { $event: event });
                    };
                    scope.$apply(callback);
                };
                elem.bind("contextmenu", contextmenuHandler);
                scope.$on("$destroy", function () {
                    elem.unbind("contextmenu", contextmenuHandler);
                });
            }
        }


    }
})()
