(function () {
    "use strict";

    var app = angular.module("app", [
        // Angular modules 
        "ngAnimate",        // animations
        "ngRoute",          // routing
        "ngSanitize",       // sanitizes html bindings (ex: sidebar.js)
        //'ngResource',       // making REST calls

        // Custom modules 
        "common",           // common functions, logger, spinner
        "common.bootstrap", // bootstrap dialog wrapper functions
        "fileUploaderModule",

        // 3rd Party Modules
        "ui.bootstrap",      // ui-bootstrap (ex: carousel, pagination, dialog)
        "textAngular",
        "nvd3",
        "datatables"
    ]);

    // Handle routing errors and success events
    app.run(["$route", "$location", "$rootScope", "authService", "config", "notifications", function ($route, $location, $rootScope, authService, config, notifications) {
        $rootScope.$on(config.events.sessionExpired,
            function () {
                authService.logout().then(function () {
                        notifications.unsubscribe("PermissionsUpdatedUiEvent");
                        $location.path("/login");
                    }
                );
            }
        );

        function refreshPermissions(items) {
            sessionStorage.setItem("remiCommands", JSON.stringify(items.Commands));
            sessionStorage.setItem("remiQueries", JSON.stringify(items.Queries));
        }

        $rootScope.$on(config.events.loggedIn, function() {
            notifications.subscribe("PermissionsUpdatedUiEvent", { RoleId: authService.identity.roleId });
        });

        $rootScope.$on(config.events.loggedOut, function () {
            notifications.unsubscribe("PermissionsUpdatedUiEvent");
        });

        $rootScope.$on(config.events.notificationReceived, function(event, notification) {
            if (notification.name === "PermissionsUpdatedUiEvent") {
                    if (!notification.data) {
                        return;
                    }
                    refreshPermissions(notification.data);
                }
            }
        );

        // Include $route to kick start the router.
    }]);
    //#region Custom Directives
    //#endregion
})();
