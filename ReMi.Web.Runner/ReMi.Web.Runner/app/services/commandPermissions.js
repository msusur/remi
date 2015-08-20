(function () {
    'use strict';

    var serviceId = 'commandPermissions';

    angular.module('app').factory(serviceId, ['$timeout', 'common', 'authService', 'remiapi', commandPermissions]);

    function commandPermissions($timeout, common, authService, remiapi) {
        var $q = common.$q;
        var logger = common.logger.getLogger(serviceId);

        var service = {
            state: {
                isBusy: false
            },
            readPermissions: readPermissions,
            permissions: {},
            checkCommand: checkCommand
        };

        return service;

        function readPermissions(commands) {
            var deferred = $q.defer();

            if (!commands || commands.length == 0) {
                logger.console('List of commands should be specified!');
                deferred.reject();
            }
            else {
                service.state.isBusy = true;

                $timeout(function () {
                    var names = commands.join();
                    remiapi
                        .get.commandsNyNames(names)
                        .then(function (data) {
                            service.permissions = {};
                            for (var i = 0; i < data.Commands.length; i++) {
                                var command = data.Commands[i];
                                service.permissions[command.Name] = command.Roles;
                            }
                            service.state.isBusy = false; //need to have this for handlers which checks permissions just after promise resolving

                            deferred.resolve(service.permissions);
                        })
                        .finally(function () {
                            service.state.isBusy = false;
                        });
                }, 10);
            }

            return deferred.promise;
        }

        function checkCommand(name) {
            if (!name)
                throw ('Command name not specified');

            if (authService.state.isBusy) return { result: false, pending: true, comment: 'Getting account information from server', command: name };
            if (!authService.isLoggedIn) return { result: false, pending: false, comment: 'No logged in account', command: name };
            if (service.state.isBusy) return { result: false, pending: true, comment: 'Getting permissions from server', command: name };

            var roles = service.permissions[name];
            if (!roles)
                return { result: false, pending: false, comment: 'No permissions for specified command', command: name };

            var role = Enumerable.From(roles)
                .FirstOrDefault(null, function (x) { return x.Description == authService.identity.role; });

            if (!role)
                return { result: false, pending: false, comment: 'Command not allowed', command: name };

            return { result: true, pending: false, comment: 'Command allowed', command: name };
        }
    };
})();
