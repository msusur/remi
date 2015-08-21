(function () {
    "use strict";

    var app = angular.module("app");

    var controllerId = "shell";

    app.controller(controllerId, shell);

    function shell($scope, $rootScope, $location, $route, $window, routes, common, config, authService, notifications, $timeout) {
        var vm = this;
        var logger = common.logger.getLogger(controllerId);
        var $q = common.$q;

        vm.busyMessage = "Please wait ...";
        vm.isBusy = true;
        vm.spinnerOptions = {
            radius: 40,
            lines: 7,
            length: 0,
            width: 30,
            speed: 1.7,
            corners: 1.0,
            trail: 100,
            color: "#F58A00"
        };

        vm.findRouteByUrl = findRouteByUrl;
        vm.findRouteByTemplateUrl = findRouteByTemplateUrl;
        vm.restoreRequestedPage = restoreRequestedPage;
        vm.getAccess = getAccess;
        vm.serverNotificationHandler = serverNotificationHandler;
        vm.handleNotificationOccuredForUserEvent = handleNotificationOccuredForUserEvent;

        $rootScope.$on("$locationChangeStart", locationChangeStartHandler);
        $rootScope.$on("$routeUpdate", routeUpdateHandler);

        var notificationOccuredForUserEventName = "NotificationOccuredForUserEvent";

        common.handleEvent(config.events.controllerActivateSuccess, controllerActivateSuccessHandler, $scope);
        common.handleEvent(config.events.controllerActivateError, controllerActivateErrorHandler, $scope);
        common.handleEvent(config.events.spinnerToggle, spinnerToggleHandler, $scope);
        common.handleEvent(config.events.loggedIn, loggedInHandler, $scope);
        common.handleEvent(config.events.loggedOut, loggedOutHandler, $scope);
        common.handleEvent(config.events.notificationReceived, vm.serverNotificationHandler, $scope);

        //check access on app load when client requests non-root path
        checkAccess(vm.findRouteByTemplateUrl($route.current.loadedTemplateUrl))
            .then(function () {
                activate();
            }, function (rediredtTo) {
                if ($location.path() !== rediredtTo)
                    $location.path(rediredtTo);
            });

        function activate() {
            logger.console("ReMi loaded!", null, true);
            common.activateController([], controllerId);
        }

        function getAccess(access) {
            var accessLevel = true;
            if (access.commands && access.commands instanceof Array) {
                accessLevel = access.commands.length === 0
                    ? true : getAccessLevel(access.commands, true);
            }

            if (access.queries && access.queries instanceof Array) {
                accessLevel = access.queries.length === 0
                    ? accessLevel : accessLevel && getAccessLevel(access.queries);
            }

            return accessLevel;
        }

        function getAccessLevel(apiNames, isCommand) {
            var api;
            try {
                var apiType = isCommand ? "remiCommands" : "remiQueries";
                api = JSON.parse($window.sessionStorage.getItem(apiType));
            } catch (e) {
                logger.console("Cannot get api methods");
                return false;
            }

            return apiNames.length === apiNames.filter(function (s) {
                return api.indexOf(s) >= 0;
            }).length;
        }

        function checkAccess(next) {
            var deffered = $q.defer();

            if (!next || !next.config) {
                logger.console("Route is not valid!");
                deffered.reject("/");
            } else
                try {
                    var routeConfig = next.config;
                    if (routeConfig.access && !routeConfig.access.allowAnonymous) {
                        var search;
                        if (!authService.isLoggedIn && authService.state.isBusy) {
                            search = $location.search();
                            search.returnPath = encodeURI($location.path());
                            $location.search(search);

                            deffered.reject("/");

                        } else if ((routeConfig.access.commands
                            || routeConfig.access.queries) && !vm.getAccess(routeConfig.access)) {
                            logger.warn("The page is not accessible!");
                            deffered.reject("/");
                        } else if ((routeConfig.access.commands
                            || routeConfig.access.queries) && vm.getAccess(routeConfig.access)) {
                            deffered.resolve();
                        } else if (!authService.isLoggedIn) {
                            logger.warn("You need to log in!");
                            search = $location.search();
                            search.returnPath = encodeURI($location.path());
                            $location.search(search);

                            deffered.reject("/login");

                        } else {
                            deffered.resolve();
                        }
                    } else {
                        deffered.resolve();
                    }
                } catch (ex) {
                    console.log("error", ex);
                    deffered.reject("/");
                }

            return deffered.promise;
        }

        function toggleSpinner(on, message) {
            $timeout(function () {
                vm.isBusy = on;
                vm.busyMessage = message || "Please wait ...";
            });
        }

        function findRouteByUrl(url) {
            return Enumerable
                   .From(routes)
                   .Where(function (x) { return x.url === url; })
                   .FirstOrDefault();
        }

        function findRouteByTemplateUrl(templateUrl) {
            return Enumerable
                   .From(routes)
                   .Where(function (x) { return x.config.templateUrl === templateUrl; })
                   .FirstOrDefault();
        }

        function locationChangeStartHandler(event, next, previous) {
            if (checkPathsAreSame(next, previous))
                return;

            var foundRoute = findRouteByUrl($location.path());

            if (foundRoute) { //perform only for existing routes
                if (foundRoute.redirectUrl) {
                    foundRoute = findRouteByUrl(foundRoute.redirectUrl);
                }
                toggleSpinner(true);
                checkAccess(foundRoute)
                    .then(function () {
                        common.sendEvent(config.events.navLocationChange, { next: next });
                    }, function (rediredtTo) {
                        event.preventDefault();
                        if (rediredtTo)
                            $location.path(rediredtTo);
                    });
            }
        }

        function checkPathsAreSame(next, previous) {
            if (!next || !previous)
                return false;
            var index = next.indexOf("?");
            if (index === -1)
                index = previous.indexOf("?");
            if (index === -1)
                return next === previous;
            else
                return next.substr(0, index) === previous.substr(0, index);
        }

        function routeUpdateHandler(event, route) {
            common.sendEvent(config.events.navRouteUpdate, { route: route });
        }

        function loggedInHandler() {
            vm.restoreRequestedPage();

            notifications.subscribe(notificationOccuredForUserEventName, {});
        }

        function loggedOutHandler() {
            notifications.unsubscribe(notificationOccuredForUserEventName);
        }

        function restoreRequestedPage() {
            var search = $location.search();

            var returnPath = search.returnPath;
            if (!!returnPath && $location.path() !== "/login") {
                $location.path(returnPath).search("returnPath", null);
            }
        }

        function controllerActivateSuccessHandler(data) {
            if (data.controllerId !== "topnav") { toggleSpinner(false); }
        }

        function controllerActivateErrorHandler() { toggleSpinner(false); }

        function spinnerToggleHandler(data) {
            toggleSpinner(data.show, data.message);
        }

        function serverNotificationHandler(notification) {
            if (notification.name === notificationOccuredForUserEventName) {
                vm.handleNotificationOccuredForUserEvent(notification.data);
            }

            return $q.when();
        };

        function handleNotificationOccuredForUserEvent(data) {
            if (data && data.Message) {
                if (data.Type === "Error") {
                    common.showErrorMessage(data.Message);
                } else if (data.Type === "Warning") {
                    common.showWarnMessage(data.Message);
                } else {
                    common.showInfoMessage("Server event", data.Message);
                }
            }
        }
    };
})();
