(function () {
    'use strict';

    var controllerId = 'roles';

    angular.module('app').controller(controllerId,
        ['$scope', 'common', 'remiapi', 'config', roles]);

    function roles($scope, common, remiapi, config) {
        var $q = common.$q;
        var logger = common.logger.getLogger(controllerId);

        var vm = this;

        vm.state = {
            isBusy: false
        };
        vm.roleModalMode = '';

        vm.validator = undefined;

        vm.roles = [];
        vm.currentRole = {};

        vm.updateRole = updateRole;
        vm.removeRole = removeRole;
        vm.refreshRoles = refreshRoles;
        vm.showRoleAdd = showRoleAdd;
        vm.showRoleUpdate = showRoleUpdate;
        vm.saveRole = saveRole;
        vm.deleteRole = deleteRole;

        vm.showRoleUpdate = showRoleUpdate;
        vm.showRoleModalMode = showRoleModalMode;
        vm.hideCurrentRoleModal = hideCurrentRoleModal;
        vm.validate = validate;

        vm.getRoles = getRoles;

        $scope.$on('$destroy', scopeDestroyHandler);

        activate();

        return vm;

        function activate() {
            common.activateController([vm.getRoles(), initUi()], controllerId)
                .then(function () { logger.console('Activated Roles View'); });
        }

        function scopeDestroyHandler() {
        }

        function initUi() {
            vm.validator = $('#roleModalForm').validate({
                errorClass: 'invalid-data',
                errorPlacement: function () { },
            });
        }

        function hideCurrentRoleModal() {
            $('#roleModal').modal('hide');

            resetValidaton();
        }

        function validate() {
            if (!$('#roleModalForm').valid()) {
                throw ("Invalid data!");
            }

            return vm.currentRole;
        }

        function resetValidaton() {
            if (vm.validator)
                vm.validator.resetForm();

            $('#role-name').removeClass('invalid-data');
        }

        function getRoles() {
            vm.state.isBusy = true;
            vm.roles = [];

            return remiapi
                .getRoles()
                .then(function (response) {
                    vm.roles = response.Roles;
                }).finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function updateRole(role) {
            removeRole(role);

            vm.roles.push(role);
            return true;
        }

        function removeRole(role) {
            var found = Enumerable.From(vm.roles)
                .FirstOrDefault(null, function (item) { return item.ExternalId == role.ExternalId; });

            if (found) {
                vm.roles.splice(vm.roles.indexOf(found), 1);
            }
            return true;
        }

        function refreshRoles() {
            return vm.getRoles();
        }

        function showRoleAdd() {
            vm.currentRole = {
                Name: '',
                Description: '',
                ExternalId: newGuid()
            };

            vm.showRoleModalMode('add');
        }

        function showRoleUpdate(role) {
            vm.currentRole = angular.copy(role);

            vm.showRoleModalMode('update');
        }

        function showRoleModalMode(mode) {
            vm.roleModalMode = mode;

            $('#roleModal').modal({ backdrop: 'static', keyboard: true });
        }

        function saveRole(operationType) {
            vm.state.isBusy = true;

            var deferred = $q.when();

            return deferred
                .then(vm.validate)
                .then(function () {
                    if (operationType == 'add') {
                        return remiapi.createRole({ Role: vm.currentRole });
                    }

                    return remiapi.updateRole({ Role: vm.currentRole });
                })
                .then(function () {
                    vm.updateRole(vm.currentRole);

                    $('#roleModal').modal('hide');
                })
                .catch(function (ex) {
                    logger.error('Can\'t save role!');
                    console.log('error', ex);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function deleteRole(role) {
            vm.state.isBusy = true;

            return remiapi
                .deleteRole({ Role: role })
                .then(function () {
                    vm.removeRole(role);
                })
                .catch(function (ex) {
                    logger.error('Can\'t delete role!');
                    console.log('error', ex);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }
    }
})();
