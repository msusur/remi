(function () {
    'use strict';

    var controllerId = 'commands';

    angular.module('app').controller(controllerId,
        ['$scope', '$rootScope', 'notifications', 'common', 'remiapi', 'authService', 'config', 'rulesService', commands]);

    function commands($scope, $rootScope, notifications, common, remiapi, authService, config, rulesService) {
        var logger = common.logger.getLogger(controllerId);
        var $q = common.$q;
        
        var vm = this;

        vm.state = {
            isBusy: false,
            ruleState: rulesService.state
        };
        
        vm.commands = [];
        vm.roles = [];

        vm.roleChecked = roleChecked;
        vm.changeCommandRole = changeCommandRole;
        vm.serverNotificationHandler = serverNotificationHandler;

        vm.editRule = editRule;
        vm.generateNewRule = generateNewRule;
        vm.testBusinessRule = testBusinessRule;
        vm.savePermissionRule = saveRule;
        vm.deleteRule = deleteRule;

        common.handleEvent(config.events.notificationReceived, vm.serverNotificationHandler, $scope);

        $scope.$on('$destroy', scopeDestroyHandler);

        activate();

        return vm;

        function scopeDestroyHandler() {
            notifications.unsubscribe('RoleCreatedEvent');
            notifications.unsubscribe('RoleUpdatedEvent');
            notifications.unsubscribe('RoleDeletedEvent');
        }

        function activate() {
            common.activateController([
                getInitialData()],
                controllerId)
                .then(function () {
                    notifications.subscribe('RoleCreatedEvent', {});
                    notifications.subscribe('RoleUpdatedEvent', {});
                    notifications.subscribe('RoleDeletedEvent', {});
                    logger.console('Activated Command Permissions View');
                });
        }

        function getInitialData() {
            vm.state.isBusy = true;
            return $q.all([remiapi.getRoles(), remiapi.getCommands()])
                .then(function (results) {
                    vm.roles = results[0].Roles;
                    initCommands(results[1].Commands, vm.roles);
            }).finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function initCommands(commands, roles) {
            var enumerable = Enumerable.From(commands);
            var groups = enumerable
                .GroupBy(function (x) { return x.Group; })
                .Select(function (x) { return { Name: x.First().Group, Count: x.Count() }; });
            var done = {};
            enumerable.ForEach(function (x) {
                if (!done[x.Group]) {
                    var group = groups.First(function (y) { return y.Name == x.Group; });
                    x.Count = group.Count;
                    done[x.Group] = true;
                }
            });
            enumerable.ForEach(function (command) {
                Enumerable.From(roles).ForEach(function (role) {
                    command[role.ExternalId] = roleChecked(command, role);
                });
            });
            vm.commands = enumerable.ToArray();
        }

        function roleChecked(command, role) {
            return Enumerable.From(command.Roles)
                .Any(function (x) { return x.ExternalId == role.ExternalId; });
        }

        function changeCommandRole(command, role) {
            vm.state.isBusy = true;
            var commandId = newGuid();
            var data = {
                CommandId: command.CommandId,
                RoleExternalId: role.ExternalId
            };
            var value = command[role.ExternalId] = !command[role.ExternalId];
            var promise = value ? addCommandToRole(commandId, data) : removeCommandFromRole(commandId, data);
            promise.then(null, function () {
                command[role.ExternalId] = !value;
            }).finally(function () {
                vm.state.isBusy = false;
            });
        }

        function addCommandToRole(commandId, data) {
            return remiapi.executeCommand('AddCommandToRoleCommand', commandId, data);
        }

        function removeCommandFromRole(commandId, data) {
            return remiapi.executeCommand('RemoveCommandFromRoleCommand', commandId, data);
        }

        function serverNotificationHandler(notification) {
            if (['RoleCreatedEvent', 'RoleUpdatedEvent', 'RoleDeletedEvent'].indexOf(notification.name) >= 0) {
                getInitialData();
            }
        }

        function editRule(command) {
            vm.currentCommand = command;
            $('#commandRuleEditor').modal({ backdrop: 'static', keyboard: true });

            rulesService.getPermissionRule(command.Name)
                .then(function (rule) {
                    vm.currentRule = rule;
                }, function () {
                    $('#commandRuleEditor').modal('hide');
                });
        };

        function generateNewRule() {
            var command = vm.currentCommand;

            rulesService.generateNewRule(command.Name, command.Namespace)
                .then(function (rule) {
                    vm.currentRule = rule;
                });
        };

        function testBusinessRule() {
            if (vm.currentRule) {
                rulesService.testBusinessRule(vm.currentRule);
            }
        };

        function saveRule() {
            if (!vm.currentRule || !vm.currentCommand) {
                return;
            }

            rulesService.savePermissionRule(vm.currentRule, 'command', vm.currentCommand.CommandId)
                .then(function () {
                    $('#commandRuleEditor').modal('hide');
                    vm.currentCommand.HasRuleApplied = true;
                });
        };

        function deleteRule() {
            if (!vm.currentRule || !vm.currentCommand) {
                return;
            }

            rulesService.deleteRule(vm.currentRule.ExternalId)
                .then(function () {
                    vm.currentCommand.HasRuleApplied = false;
                    vm.currentRule = null;
                    $('#commandRuleEditor').modal('hide');
                });
        };
    }
})();
