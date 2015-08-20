(function () {
    'use strict';

    function subscription(service, name, filter) {
        this.id = newGuid();
        this.name = name;
        this.filter = filter;
        this.state = subscriptionState.disconnected; // disconnected, connecting, connected

        this.service = service;
        this.switchOn = function () {
            var s = this;
            if (s.state == subscriptionState.connected) { return true; }
            if (s.state == subscriptionState.connecting) { return false; }

            s.state = subscriptionState.connecting;
            s.service
                .hubProxy
                .invoke('subscribe', s.name, s.filter)
                .done(function () {
                    s.state = subscriptionState.connected;
                    console.log('[' + serviceId + ']  Subscription on event ' + s.name + ' approved');
                })
                .fail(function (fault) {
                    s.state = subscriptionState.disconnected;
                    console.log('[' + serviceId + ']  Failed to subscribe on event ' + s.name + '. Error: ' + fault);
                });

            return true;
        };
    }

    var subscriptionState = {
        disconnected: 'disconnected',
        connecting: 'connecting',
        connected: 'connected'
    };

    var serviceId = 'notifications';
    angular.module('app').factory(serviceId, ['$rootScope', 'common', 'config', 'remiapi', 'authService', notifications]);

    function notifications($rootScope, common, config, remiapi, authService) {
        var $timeout = common.$timeout;
        var logger = common.logger.getLogger(serviceId);
        var events = config.events;

        var service = {
            clientId: '',
            hubProxy: null,
            hubConnection: null,
            subscriptions: [],
            state: {
                isAlive: false,
                isTurningOn: function () {
                    var found = Enumerable
                                .From(service.subscriptions)
                                .Where(function (x) { return x.state == subscriptionState.connecting; })
                                .ToArray();

                    return found && found.length > 0;
                }
            },
            timeoutPromise: null,
            timeoutPromiseCalled: 0,
            subscribe: subscribe,
            unsubscribe: unsubscribe,
            unsubscribeAll: unsubscribeAll,
            notify: notify,
            connectHub: connectHub,
            disconnectHub: disconnectHub,
            turnOnSubscriptions: turnOnSubscriptions,
            setSubscriptionsDisconnected: setSubscriptionsDisconnected,
            monitorSubscriptions: monitorSubscriptions,
            registerClientConnection: registerClientConnection,
        };

        $rootScope.$on(events.loggedIn, function (event, data) {
            connectHub();
        });

        $rootScope.$on(events.loggedOut,
            function (event) {
                disconnectHub();
            });

        tryRestoreHub();

        return service;

        //register callback for a specific notification name
        function subscribe(name, filter) {
            var sFilter = filter;
            if (typeof filter !== 'string')
                sFilter = JSON.stringify(filter);

            var found = Enumerable
                .From(service.subscriptions)
                .Where(function (x) { return x.name == name && x.filter == sFilter; })
                .ToArray();

            if (!found || found.length == 0) {
                var newSubscription = new subscription(service, name, sFilter);
                service.subscriptions.push(newSubscription);
            }

            monitorSubscriptions();
        }

        function unsubscribeAll() {
            for (var i = service.subscriptions.length - 1; i >= 0; i--) {
                var s = service.subscriptions[i];
                service.unsubscribe(s.name);
            }
        }

        function unsubscribe(name, filter) {
            var sFilter = filter || '';
            if (filter && (typeof (filter) !== 'string'))
                sFilter = JSON.stringify(filter);

            var connectionRemoved = false;
            for (var i = service.subscriptions.length - 1; i >= 0; i--) {
                var s = service.subscriptions[i];
                if (s.name == name && (!filter || !s.filter || s.filter == sFilter)) {
                    delete service.subscriptions[i].service;
                    service.subscriptions.splice(i, 1);
                    connectionRemoved = true;
                }
            }

            if (connectionRemoved)
                logger.console('Subscription on event ' + name + ' was cancelled');

            if (service.state.isAlive && !!service.hubProxy) {
                if (filter)
                    service.hubProxy.invoke('unsubscribe', name, sFilter);
                else
                    service.hubProxy.invoke('unsubscribeall', name);
            } else
                return false;

            return true;
        }

        function turnOnSubscriptions() {
            if (service.state.isTurningOn()) {
                logger.console('Previous attempt to turn on subscriptions not finished.');
                return false;
            }

            if (!authService.isLoggedIn || !service.state.isAlive || !service.hubProxy) {
                logger.console('Could\'t turn on notifications. Connection is not started.');
                return false;
            }

            var result = true;
            for (var i = 0; i < service.subscriptions.length; i++) {
                if (!result & service.subscriptions[i].switchOn())
                    result = false;
            }

            return result;
        }

        function setSubscriptionsDisconnected() {
            for (var i = 0; i < service.subscriptions.length; i++) {
                service.subscriptions[i].state = subscriptionState.disconnected;
            }
        }

        function monitorSubscriptions() {
            if (service.timeoutPromise != null) {
                //logger.console('Previous timeout not completed. service.state.isTurningOn=' + service.state.isTurningOn());
                return false;
            }

            var performTimeoutFunc = function () {
                if (service.timeoutPromiseCalled > 30) {
                    service.timeoutPromiseCalled = 0;
                    service.timeoutPromise = null;
                    logger.console('Turn off subscriptions because connection is not established.');

                    return false;
                }
                service.timeoutPromise = $timeout(function () {
                    logger.console('Timeout turns on subscriptions.');
                    service.timeoutPromiseCalled++;

                    service.timeoutPromise = null;
                    if (!turnOnSubscriptions())
                        performTimeoutFunc();
                }, 2000);

                return true;
            };

            if (!service.state.isAlive) {
                logger.console('Connection not opened. Delay subscription');
                performTimeoutFunc();
                return false;
            }

            var disconnected = Enumerable
                    .From(service.subscriptions)
                    .Where(function (x) { return x.state == subscriptionState.disconnected; })
                    .ToArray();

            if (disconnected && disconnected.length != 0) {
                logger.console('Monitoring subscriptions state. Found disconnected items=' + disconnected.length);

                turnOnSubscriptions();
                performTimeoutFunc();

            } else {
                var connecting = Enumerable
                        .From(service.subscriptions)
                        .Where(function (x) { return x.state == subscriptionState.connecting; })
                        .ToArray();

                if (connecting && connecting.length != 0) {
                    logger.console('Monitoring subscriptions state. Found connecting items=' + connecting.length);

                    performTimeoutFunc();
                }
            }
            return true;
        }

        //process notification from server
        function notify(name, data) {

            var parsedData;
            try {
                parsedData = JSON.parse(data);

                common.sendEvent(config.events.notificationReceived, { 'name': name, 'data': parsedData });

            } catch (ex) {
                console.log(ex);

                logger.console('Can\'t get notification\'s data: ' + data);
            }
        }

        function tryRestoreHub() {
            if (authService.isLoggedIn)
                return connectHub();

            return false;
        }

        function connectHub() {
            try {
                service.hubConnection = $.hubConnection(remiapi.apiPath + '/signalr', { useDefaultPath: false });
                //hubConnection.logging = true;

                service.hubProxy = service.hubConnection.createHubProxy('notificationsHub');
                service.hubProxy.state.token = authService.token;
                service.hubProxy.state.email = authService.identity.email;
                service.hubProxy.state.name = authService.identity.name;

                service.hubProxy.on('notify', notify);
                logger.console('Hub proxy class generated');

                service.hubConnection.connectionSlow(function () {
                    logger.console('We are currently experiencing difficulties with the connection.');
                });

                service.hubConnection.reconnected(onHubReconnected);
                service.hubConnection.disconnected(onHunDisconnected);

                service.hubConnection.error(function (error) {
                    logger.console('SignalR error: ' + error);
                });

                service.hubConnection.start()
                    .done(function () {
                        service.clientId = service.hubConnection.id;
                        service.state.isAlive = true;

                        common.$broadcast(events.notificationConnected, 'connect');

                        logger.console('SignalR connection esteblished. ConnectionId=' + service.hubConnection.id + ', Transport=' + service.hubConnection.transport.name);

                        registerClientConnection();
                    })
                    .fail(function (error) {
                        service.clientId = '';
                        service.state.isAlive = false;

                        logger.console('SignalR can\'t connect. Error: ' + error);
                    });

                return true;
            } catch (z) {
                logger.console('Error!');
                logger.console(z);
            }

            return false;
        }

        function registerClientConnection() {
            return service.hubProxy
                .invoke('register', authService.token).done(function () {
                    logger.console('Registration on server succeeded');

                    common.$broadcast(events.notificationRegistered);

                    monitorSubscriptions();

                }).fail(function (error) {
                    logger.console('Registration on server failed. Error: ' + error);
                });
        }

        function onHubReconnected() {
            logger.console('Reconnected. ConnectionId=' + service.hubConnection.id);

            service.clientId = service.hubConnection.id;
            service.state.isAlive = true;

            common.$broadcast(events.notificationConnected, 'reconnect');

            setSubscriptionsDisconnected();

            return registerClientConnection(service.hubProxy);
        }

        function onHunDisconnected() {
            logger.console('Disconnected. ConnectionId=' + service.clientId);

            service.clientId = '';
            service.state.isAlive = false;

            common.$broadcast(events.notificationDisconnected);

            setSubscriptionsDisconnected();
        }

        function disconnectHub() {
            logger.console('Disconnect from notification hub');

            service.unsubscribeAll();

            if (service.hubConnection) {
                service.hubConnection.stop();
                delete service.hubConnection;
            }

            if (service.hubProxy) {
                delete service.hubProxy;
            }
        }
    };

})();
