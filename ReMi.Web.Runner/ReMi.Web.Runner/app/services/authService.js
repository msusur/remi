(function () {
    "use strict";

    var serviceId = "authService";

    angular.module("app").factory(serviceId, ["$rootScope", "$http", "$timeout", "$location", "common", "config", "remiapi", "$window", authService]);

    function authService($rootScope, $http, $timeout, $location, common, config, remiapi, $window) {
        var $q = common.$q;
        var logger = common.logger.getLogger(serviceId);
        var events = config.events;
        var identityDeferred = $q.defer();

        var service = {
            state: {
                isBusy: false
            },
            isLoggedIn: false,
            token: "",
            identity: {
                name: "",
                fullname: "",
                email: "",
                role: "",
                externalId: "",
                products: [],
                roleId: ""
            },
            startSession: startSession,
            login: login,
            logout: logout,
            authorize: authorize,
            changeState: changeState,
            identityPromise: identityDeferred.promise
        };

        service.selectProduct = function (newProduct) {
            var oldProduct = service.identity.product || { Name: "<empty>" };

            service.identity.product = newProduct;

            if (newProduct) {
                common.$broadcast(events.productContextChanged, newProduct, oldProduct);

                logger.console("Changed active product from " + oldProduct.Name + " to " + newProduct.Name);

            } else {
                common.$broadcast(events.productContextChanged);

                logger.console("Removed active product ");
            }
        };


        restoreSecurityContext();

        return service;

        function startSession(data) {
            service.changeState(true);
            var sessionId = newGuid();

            return remiapi.post
                .startSession({ Login: data.login, Password: data.password, SessionId: sessionId })
                .then(function () { return remiapi.get.session(sessionId); })
                .then(login)
                .then(loadPermissions)
                .finally(function () {
                    service.changeState(false);
                });
        }

        function logout() {
            var deferred = $q.when();

            return deferred
                .then(function () {
                    var old = {};

                    if (service.identity)
                        old = { name: service.identity.name, fullname: service.identity.fullname };

                    setServiceData(old);
                    $window.sessionStorage.setItem("remiCommands", JSON.stringify([]));
                    $window.sessionStorage.setItem("remiQueries", JSON.stringify([]));

                    return old;
                });
        }

        function login(response) {
            var deferred = $q.when();

            return deferred
                .then(function () {
                    setServiceData({
                        name: response.Account.Name,
                        fullname: response.Account.FullName,
                        email: response.Account.Email,
                        role: response.Account.Role.Description,
                        externalId: response.Account.ExternalId,
                        products: response.Account.Products,
                        roleId: response.Account.Role.ExternalId
                    },
                        {
                            token: response.Token,
                            isLoggedIn: true
                        });

                    return service.identity;
                });
        }

        function loadPermissions() {
            return remiapi.get.permissions(service.identity.roleId)
                .then(function (response) {
                    try {
                        $window.sessionStorage.setItem("remiCommands", JSON.stringify(response.Commands));
                        $window.sessionStorage.setItem("remiQueries", JSON.stringify(response.Queries));

                        $rootScope.$broadcast("pemissions.loaded", {});
                    } catch (e) {
                        logger.console("Cannot set api methods");
                    }
                }, function (error) {
                    $window.sessionStorage.setItem("remiCommands", JSON.stringify([]));
                    $window.sessionStorage.setItem("remiQueries", JSON.stringify([]));
                    logger.error("Cannot load api methods");
                    console.log(error);
                });
        }

        // perform security check for current user according to his role
        function authorize(accessLevel, userRole) {
            var role = userRole;
            if (userRole === undefined || !userRole)
                if (service.identity && service.identity.role)
                    role = service.identity.role;
                else
                    return false;

            if (accessLevel instanceof Array) {
                for (var i = 0; i < accessLevel.length; i++) {
                    if (role.toLowerCase() === accessLevel[i].toLowerCase())
                        return true;
                }
            }
            else {
                if (role.toLowerCase() === accessLevel.toLowerCase())
                    return true;
            }

            return false;
        }

        // load logged in account from storage
        function restoreSecurityContext() {
            if (!localStorage["securityContext"] || localStorage["securityContext"].length === 0) {
                setServiceData();
                return null;
            }
            common.$broadcast(config.events.spinnerToggle, { show: true, message: "Authenticating user ..." });

            service.changeState(true);

            var context = JSON.parse(localStorage["securityContext"]);

            setAuthHeader(context.token);

            return remiapi.checkSession()
                .then(login)
                .then(loadPermissions)
                .catch(function (statucCode) {
                    if (statucCode === 401) {
                        logger.info("Your current session has expired. Please log in again");

                        service.logout().then(function () {
                            if ($location.path() !== "/login") {
                                $location.path("/login");
                            }
                        });
                    }
                })
                .finally(function () {
                    service.changeState(false);
                    common.$broadcast(config.events.spinnerToggle, { show: false });
                });

        }

        function getDefaultProduct(products) {
            if (!products) return null;

            var result = products[0];
            products.forEach(function (product) {
                if (product.IsDefault) result = product;
            });

            return result;
        }

        function notifyAllSecurityContextChanged(identity, token) {
            if (identity) {
                common.$broadcast(events.loggedIn, {
                    name: identity.name,
                    fullname: identity.fullname,
                    token: token
                });
                common.$broadcast(events.authContextChanged, {});

                logger.console("Logged in!");

            } else {
                common.$broadcast(events.loggedOut, {});
                common.$broadcast(events.authContextChanged, {});

                logger.console("Logged out!");
            }
        }

        function setServiceData(identity, session) {

            if (identity && session) {

                service.identity = angular.copy(identity);

                service.token = session.token;
                service.isLoggedIn = session.isLoggedIn;

                localStorage["securityContext"] = JSON.stringify({ identity: service.identity, token: service.token });

                setAuthHeader(service.token);

                notifyAllSecurityContextChanged(service.identity, service.token);

                service.selectProduct(getDefaultProduct(identity.products));

            } else {

                service.identity = {
                    name: "",
                    fullname: "",
                    email: "",
                    role: "",
                    externalId: "",
                    products: [],
                    roleId: ""
                };

                service.isLoggedIn = false;
                service.token = "";

                localStorage["securityContext"] = "";

                setAuthHeader();

                if (identity) {
                    notifyAllSecurityContextChanged();

                    service.selectProduct();
                }
            }
            identityDeferred.resolve(service.identity);
        }

        function setAuthHeader(token) {
            if (token)
                $http.defaults.headers.common.Authorization = "token " + token;
            else
                $http.defaults.headers.common.Authorization = null;
        }

        function changeState(isBusy) {
            service.state.isBusy = isBusy;

            common.$broadcast(events.authStateChanged, { isBusy: isBusy });
        }
    };
})();
