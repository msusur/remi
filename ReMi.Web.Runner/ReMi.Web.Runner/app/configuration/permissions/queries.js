(function () {
    'use strict';

    var controllerId = 'queries';

    angular.module('app').controller(controllerId,
        ['$scope', '$rootScope', 'notifications', 'common', 'remiapi', 'authService', 'config', 'rulesService', queries]);

    function queries($scope, $rootScope, notifications, common, remiapi, authService, config, rulesService) {
        var logger = common.logger.getLogger(controllerId);
        var $q = common.$q;

        var vm = this;

        vm.state = {
            isBusy: false,
            ruleState: rulesService.state
        };

        vm.queries = [];
        vm.roles = [];

        vm.roleChecked = roleChecked;
        vm.changeQueryRole = changeQueryRole;
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
                    logger.console('Activated Query Permissions View');
                });
        }

        function getInitialData() {
            vm.state.isBusy = true;
            return $q.all([remiapi.getRoles(), remiapi.getQueries()])
                .then(function (results) {
                    vm.roles = results[0].Roles;
                    initQueries(results[1].Queries, vm.roles);
                }).finally(function () {
                    vm.state.isBusy = false;
                });
        }
        
        function initQueries(queryItems, roles) {
            var enumerable = Enumerable.From(queryItems);
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
            enumerable.ForEach(function (query) {
                Enumerable.From(roles).ForEach(function (role) {
                    query[role.ExternalId] = roleChecked(query, role);
                });
            });

            vm.queries = enumerable.ToArray();
        }

        function roleChecked(query, role) {
            return Enumerable.From(query.Roles)
                .Any(function (x) { return x.ExternalId == role.ExternalId; });
        }

        function changeQueryRole(query, role) {
            vm.state.isBusy = true;

            var data = {
                QueryId: query.QueryId,
                RoleExternalId: role.ExternalId
            };

            var value = query[role.ExternalId] = !query[role.ExternalId];

            var promise = value ? remiapi.addQueryToRole(data) : remiapi.removeQueryFromRole(data);
            promise.then(null, function () {
                query[role.ExternalId] = !value;
            }).finally(function () {
                vm.state.isBusy = false;
            });
        }

        function serverNotificationHandler(notification) {
            if (['RoleCreatedEvent', 'RoleUpdatedEvent', 'RoleDeletedEvent'].indexOf(notification.name) >= 0) {
                getInitialData();
            }
        }

        function editRule(query) {
            vm.currentQuery = query;
            $('#queryRuleEditor').modal({ backdrop: 'static', keyboard: true });

            rulesService.getPermissionRule(query.Name)
                .then(function (rule) {
                    vm.currentRule = rule;
                }, function () {
                    $('#queryRuleEditor').modal('hide');
                });
        };

        function generateNewRule() {
            var query = vm.currentQuery;

            rulesService.generateNewRule(query.Name, query.Namespace)
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
            if (!vm.currentRule || !vm.currentQuery) {
                return;
            }

            rulesService.savePermissionRule(vm.currentRule, 'query', vm.currentQuery.QueryId)
                .then(function () {
                    $('#queryRuleEditor').modal('hide');
                    vm.currentQuery.HasRuleApplied = true;
                });
        };

        function deleteRule() {
            if (!vm.currentRule || !vm.currentQuery) {
                return;
            }

            rulesService.deleteRule(vm.currentRule.ExternalId)
                .then(function () {
                    vm.currentQuery.HasRuleApplied = false;
                    vm.currentRule = null;
                    $('#queryRuleEditor').modal('hide');
                });
        };
    }
})();
