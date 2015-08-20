(function () {
    'use strict';

    // Define the common module 
    // Contains services:
    //  - common
    //  - logger
    //  - spinner
    var commonModule = angular.module('common', []);

    // Must configure the common service and set its 
    // events via the commonConfigProvider
    commonModule.provider('commonConfig', function () {
        this.config = {
            // These are the properties we need to set
            //controllerActivateSuccessEvent: '',
            //spinnerToggleEvent: ''
        };

        this.$get = function () {
            return {
                config: this.config
            };
        };
    });

    if (!String.prototype.format) {
        String.prototype.format = function () {
            var args = arguments;
            return this.replace(/{(\d+)}/g, function (match, number) {
                return typeof args[number] != 'undefined' ? args[number] : match;
            });
        };
    }
    //if (!Object.prototype.renameProperty) {
    //    Object.prototype.renameProperty = function (oldName, newName) {
    //        // Check for the old property name to avoid a ReferenceError in strict mode.
    //        if (this.hasOwnProperty(oldName)) {
    //            this[newName] = this[oldName];
    //            delete this[oldName];
    //        }
    //        return this;
    //    };
    //}

    commonModule.factory('common',
        ['$q', '$rootScope', '$timeout', '$location', 'commonConfig', 'logger', common]);

    function common($q, $rootScope, $timeout, $location, commonConfig, logger) {
        var throttles = {};

        var service = {
            // common angular dependencies
            $broadcast: $broadcast,
            $q: $q,
            $timeout: $timeout,

            // generic
            activateController: activateController,
            deactivateController: deactivateController,
            createSearchThrottle: createSearchThrottle,
            debouncedThrottle: debouncedThrottle,
            isNumber: isNumber,
            logger: logger, // for accessibility
            textContains: textContains,
            showErrorMessage: showErrorMessage,
            showInfoMessage: showInfoMessage,
            showWarnMessage: showWarnMessage,
            showValidationError: showValidationError,
            showAccessDeniedMessage: showAccessDeniedMessage,
            sendEvent: sendEvent,
            handleEvent: handleEvent,
            getParentScope: getParentScope
        };

        return service;

        function showErrorMessage(message) {
            $q.all([
                $('#errorMessage .modal-body .message').html(message),
                $('#errorMessage .modal-title').html('Error occurred!')
            ]).then(function () {
                $('#errorMessage').modal('show');
            });
        }
        function showWarnMessage(message) {
            $q.all([
                $('#warningMessage .modal-body .message').html(message),
                $('#warningMessage .modal-title').html('Warning!')
            ]).then(function () {
                $('#warningMessage').modal('show');
            });
        }
        function showInfoMessage(title, message) {
            $q.all([
                $('#infoMessage .modal-body .message').html(message),
                $('#infoMessage .modal-title').html(title)
            ]).then(function () {
                $('#infoMessage').modal('show');
            });
        }

        function showAccessDeniedMessage(message) {
            $q.all([
                $('#warningMessage .modal-body .message').html(message),
                $('#warningMessage .modal-title').html('Access Denied!')
            ]).then(function () {
                $('#warningMessage').modal('show');
            });

        }

        function showValidationError(validationErrors) {
            var message = '<ul>';
            if (validationErrors && validationErrors.length > 0) {
                if (typeof validationErrors == "string") {
                    message = validationErrors;
                } else {
                    for (var i in validationErrors) {
                        message += '<li>' + validationErrors[i].ErrorMessage + '</li>';
                    }
                    message += '</ul>';
                }
            } else {
                message = "Request is invalid";
            }
            $q.all([
                $('#warningMessage .modal-body .message').html(message),
                $('#warningMessage .modal-title').html('Operation Invalid')
            ]).then(function () {
                $('#warningMessage').modal('show');
            });

        }

        function activateController(promises, controllerId, scope) {
            if (scope) {
                promises.splice(0, 0, registerMe(scope, controllerId));
            }

            return $q.all(promises)
                .then(function (eventArgs) {
                    var data = { controllerId: controllerId };

                    sendEvent(commonConfig.config.controllerActivateSuccessEvent, data);

                }, function (status) {
                    sendEvent(commonConfig.config.controllerActivateErrorEvent);
                });
        }

        function deactivateController(promises, controllerId, scope) {
            if (scope) {
                promises.splice(0, 0, unregisterMe(scope, controllerId));
            }

            return $q.all(promises)
                .then(function (eventArgs) {
                    var data = { controllerId: controllerId };

                    sendEvent(commonConfig.config.controllerDeactivateEvent, data);

                }, function (status) {
                    sendEvent(commonConfig.config.controllerDeactivateErrorEvent);
                });
        }

        function registerMe(scope, controllerId) {
            if (scope.registerWidget) {
                scope.registerWidget(controllerId);
            }
        }

        function unregisterMe(scope, controllerId) {
            if (scope.unregisterWidget) {
                scope.unregisterWidget(controllerId);
            }
        }

        function $broadcast() {
            return $rootScope.$broadcast.apply($rootScope, arguments);
        }

        // Could call with the following paremeter sets:
        // 1) [string] eventName, [object] data, [bool] isBroadcast
        // 2) [string] eventName, [bool] isBroadcast
        // 3) [string] eventName
        function sendEvent(arg1, arg2, arg3) {
            var data, isBroadcast;

            var eventName = arg1;
            if (!eventName || (typeof eventName != 'string')) { throw ("Invalid arguments for sendEvent [eventName]"); }

            var sendFunc = function (b, e, d) {
                if (b)
                    return $rootScope.$broadcast(e, d);

                return $rootScope.$emit(e, d);
            };

            if (arguments.length == 1) {
                return sendFunc(false, eventName);

            } else if (arguments.length == 2) {

                if (typeof arg2 == 'boolean') {
                    isBroadcast = arg2;
                    return sendFunc(isBroadcast, eventName);
                } else {
                    data = arg2;
                    return sendFunc(false, eventName, data);
                }

            } else
                if (arguments.length == 3) {
                    data = arg2;

                    if (typeof arg3 == 'boolean') {
                        isBroadcast = arg3;
                        return sendFunc(isBroadcast, eventName, data);
                    } else {
                        throw ("Invalid arguments for sendEvent [isBroadcast]");
                    }
                }

            throw ("Invalid arguments for sendEvent");
        }

        // Could call with two paremeter sets:
        // 1) [string] eventName, [func] handler, [$scope] local scope
        // 2) [string] eventName, [func] handler
        function handleEvent(arg1, arg2, arg3) {
            if (arguments.length >= 2) {
                var scope;

                var eventName = arg1;
                if (typeof eventName != 'string') { throw ("Invalid arguments for handleEvent [eventName]"); }

                var handler = arg2;
                if (typeof handler != 'function') { throw ("Invalid arguments for handleEvent [handler]"); }

                if (arguments.length == 3) {
                    scope = arg3;

                    var destroyHandler = $rootScope.$on(eventName, function (e, data) { handler(data); });
                    scope.$on('$destroy', function () { destroyHandler(); });

                    return true;

                } else if (arguments.length == 2) {
                    return $rootScope.$on(eventName, function (event, data) {
                        handler(data);
                    });
                }
            }

            throw ("Invalid arguments for handleEvent");
        }


        function createSearchThrottle(viewmodel, list, filteredList, filter, delay) {
            // After a delay, search a viewmodel's list using 
            // a filter function, and return a filteredList.

            // custom delay or use default
            delay = +delay || 300;
            // if only vm and list parameters were passed, set others by naming convention 
            if (!filteredList) {
                // assuming list is named sessions, filteredList is filteredSessions
                filteredList = 'filtered' + list[0].toUpperCase() + list.substr(1).toLowerCase(); // string
                // filter function is named sessionFilter
                filter = list + 'Filter'; // function in string form
            }

            // create the filtering function we will call from here
            var filterFn = function () {
                // translates to ...
                // vm.filteredSessions 
                //      = vm.sessions.filter(function(item( { returns vm.sessionFilter (item) } );
                viewmodel[filteredList] = viewmodel[list].filter(function (item) {
                    return viewmodel[filter](item);
                });
            };

            return (function () {
                // Wrapped in outer IFFE so we can use closure 
                // over filterInputTimeout which references the timeout
                var filterInputTimeout;

                // return what becomes the 'applyFilter' function in the controller
                return function (searchNow) {
                    if (filterInputTimeout) {
                        $timeout.cancel(filterInputTimeout);
                        filterInputTimeout = null;
                    }
                    if (searchNow || !delay) {
                        filterFn();
                    } else {
                        filterInputTimeout = $timeout(filterFn, delay);
                    }
                };
            })();
        }

        function debouncedThrottle(key, callback, delay, immediate) {
            // Perform some action (callback) after a delay. 
            // Track the callback by key, so if the same callback 
            // is issued again, restart the delay.

            var defaultDelay = 1000;
            delay = delay || defaultDelay;
            if (throttles[key]) {
                $timeout.cancel(throttles[key]);
                throttles[key] = undefined;
            }
            if (immediate) {
                callback();
            } else {
                throttles[key] = $timeout(callback, delay);
            }
        }

        function isNumber(val) {
            // negative or positive
            return /^[-]?\d+$/.test(val);
        }

        function textContains(text, searchText) {
            return text && -1 !== text.toLowerCase().indexOf(searchText.toLowerCase());
        }

        function getParentScope(scope, condition) {
            if (!scope || !condition || typeof condition !== "function")
                return undefined;
            if (condition(scope))
                return scope;
            return getParentScope(scope.$parent, condition);
        }
    }
})();
