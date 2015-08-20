(function () {
    'use strict';

    var controllerId = 'productRegistrationConfig';

    angular.module('app').controller(controllerId,
        ['$scope', '$rootScope', 'common', 'remiapi', productRegistrationConfig]);

    function productRegistrationConfig($scope, $rootScope, common, remiapi) {
        var logger = common.logger.getLogger(controllerId);
        var $q = common.$q;

        var vm = this;
        vm.state = {
            isBusy: false
        };
        vm.activate = activate;
        vm.requestConfig = [];

        vm.currentType = {
            Name: '',
            ExternalId: ''
        };
        vm.currentTask = {
            Name: '',
            ExternalId: ''
        };
        vm.currentGroup = {
            Name: '',
            ExternalId: ''
        };

        vm.initUi = initUi;
        vm.getRequestData = getRequestData;

        vm.saveTypeModal = saveTypeModal;
        vm.showTypeModal = showTypeModal;
        vm.hideTypeModal = hideTypeModal;
        vm.updateTypeList = updateTypeList;
        vm.deleteType = deleteType;

        vm.showGroupModal = showGroupModal;
        vm.hideGroupModal = hideGroupModal;
        vm.saveGroupModal = saveGroupModal;
        vm.updateGroupList = updateGroupList;
        vm.deleteGroup = deleteGroup;
        vm.getGroupAssignees = getGroupAssignees;
        vm.addGroupAssignees = addGroupAssignees;
        vm.removeGroupAssignee = removeGroupAssignee;

        vm.showTaskModal = showTaskModal;
        vm.hideTaskModal = hideTaskModal;
        vm.saveTaskModal = saveTaskModal;
        vm.updateTaskList = updateTaskList;
        vm.deleteTask = deleteTask;

        vm.typeModalFormValidator = undefined;
        vm.taskModalFormValidator = undefined;
        vm.groupModalFormValidator = undefined;

        vm.validateTypeModalForm = validateTypeModalForm;
        vm.validateTaskModalForm = validateTaskModalForm;
        vm.validateGroupModalForm = validateGroupModalForm;


        vm.resetValidaton = resetValidaton;

        vm.activate();

        function activate() {
            common.activateController([vm.initUi(), vm.getRequestData()], controllerId)
                .then(function () {
                    logger.console('Activated Product Registration Config View');
                });
        }

        function getRequestData() {
            vm.state.isBusy = true;

            return remiapi.get.productRegistrationsConfig()
                .then(function (data) {
                    //console.log(data);
                    vm.requestConfig = data.ProductRequestTypes;
                }, function (error) {
                    logger.error('Cannot get product registration config');
                    logger.console(error);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function initUi() {
            vm.typeModalFormValidator = $('#typeModalForm').validate({
                errorClass: 'invalid-data',
                errorPlacement: function () { },
            });
            vm.taskModalFormValidator = $('#taskModalForm').validate({
                errorClass: 'invalid-data',
                errorPlacement: function () { },
            });
            vm.groupModalFormValidator = $('#groupModalForm').validate({
                errorClass: 'invalid-data',
                errorPlacement: function () { },
            });

            $('#typeModal').on('shown.bs.modal', function (e) {
                $('#typeModal #type-name').focus();
            });
            $('#taskModal').on('shown.bs.modal', function (e) {
                $('#taskModal #task-name').focus();
            });
            $('#groupModal').on('shown.bs.modal', function (e) {
                $('#groupModal #group-name').focus();
            });
        }

        function resetValidaton() {
            if (vm.typeModalFormValidator)
                vm.typeModalFormValidator.resetForm();
            if (vm.taskModalFormValidator)
                vm.taskModalFormValidator.resetForm();
            if (vm.groupModalFormValidator)
                vm.groupModalFormValidator.resetForm();
        }


        function showTypeModal(type) {
            if (type) {
                vm.currentType.Name = type.Name;
                vm.currentType.ExternalId = type.ExternalId;
            } else {
                vm.currentType.Name = '';
                vm.currentType.ExternalId = '';
            }

            $('#typeModal').modal('show');
        }
        function hideTypeModal() {
            $('#typeModal').modal('hide');

            vm.resetValidaton();
        }
        function saveTypeModal() {
            if (!vm.validateTypeModalForm()) return null;

            var isNew = !vm.currentType.ExternalId;

            vm.state.isBusy = true;

            var type = {
                Name: vm.currentType.Name,
                ExternalId: isNew ? newGuid() : vm.currentType.ExternalId,
                RequestGroups: []
            };

            return $q.when()
                .then(function () {
                    if (isNew)
                        return remiapi.post.createProductRequestType({ RequestType: type });

                    return remiapi.post.updateProductRequestType({ RequestType: type });
                })
            .then(function () {
                vm.updateTypeList(isNew ? 'add' : 'update', type);
                vm.hideTypeModal();
            })
            .catch(function (fault) {
                console.log('error');
                console.log(fault);
                logger.error('Can\'t save request type');
            })
            .finally(function () {
                vm.state.isBusy = false;
            });
        }
        function updateTypeList(operation, type) {
            if (operation == 'update') {
                var found = Enumerable.From(vm.requestConfig)
                    .FirstOrDefault(null, function (x) { return x.ExternalId == type.ExternalId; });

                if (found) {
                    found.Name = type.Name;
                    return;
                }
            } else if (operation == 'add') {
                vm.requestConfig.push(type);

            } else {
                var idx = vm.requestConfig.indexOf(type);
                if (idx >= 0)
                    vm.requestConfig.splice(idx, 1);
            }
        }
        function deleteType(type) {
            if (!type.ExternalId) return null;

            vm.state.isBusy = true;

            return $q.when()
                .then(function () {
                    return remiapi.post.deleteProductRequestType({ RequestTypeId: type.ExternalId });
                })
            .then(function () {
                vm.updateTypeList('delete', type);
            })
            .catch(function (fault) {
                console.log('error');
                console.log(fault);
                logger.error('Can\'t delete request type!');
            })
            .finally(function () {
                vm.state.isBusy = false;
            });
        }

        function showGroupModal(type, group) {
            if (group) {
                vm.currentGroup.Name = group.Name;
                vm.currentGroup.ExternalId = group.ExternalId;
                vm.currentGroup.Assignees = angular.copy(group.Assignees);
            } else {
                vm.currentGroup.Name = '';
                vm.currentGroup.ExternalId = '';
                vm.currentGroup.Assignees = [];
            }
            vm.currentGroup.Type = type;

            $('#groupModal').modal('show');
        }
        function hideGroupModal() {
            $('#groupModal').modal('hide');

            vm.resetValidaton();

            vm.currentGroup.Type = null;
            vm.currentGroup.Assignees = [];
        }
        function saveGroupModal() {
            if (!vm.validateGroupModalForm()) return null;

            var isNew = !vm.currentGroup.ExternalId;

            vm.state.isBusy = true;

            var type = vm.currentGroup.Type;

            var group = {
                Name: vm.currentGroup.Name,
                ExternalId: isNew ? newGuid() : vm.currentGroup.ExternalId,
                RequestTasks: [],
                Assignees: vm.currentGroup.Assignees,
                ProductRequestTypeId: type.ExternalId
            };

            return $q.when()
                .then(function () {
                    if (isNew)
                        return remiapi.post.createProductRequestGroup({ RequestGroup: group });

                    return remiapi.post.updateProductRequestGroup({ RequestGroup: group });
                })
            .then(function () {
                vm.updateGroupList(isNew ? 'add' : 'update', type, group);
                vm.hideGroupModal();
            })
            .catch(function (fault) {
                console.log('error');
                console.log(fault);
                logger.error('Can\'t save request group');
            })
            .finally(function () {
                vm.state.isBusy = false;
            });
        }
        function deleteGroup(type, group) {
            if (!group.ExternalId) return null;

            vm.state.isBusy = true;

            return $q.when()
                .then(function () {
                    return remiapi.post.deleteProductRequestGroup({ RequestGroupId: group.ExternalId });
                })
            .then(function () {
                vm.updateGroupList('delete', type, group);
            })
            .catch(function (fault) {
                console.log('error');
                console.log(fault);
                logger.error('Can\'t delete request group!');
            })
            .finally(function () {
                vm.state.isBusy = false;
            });
        }
        function updateGroupList(operation, type, group) {
            if (!type) return;

            var foundType = Enumerable.From(vm.requestConfig)
                .FirstOrDefault(null, function (x) { return x.ExternalId == type.ExternalId; });

            if (!foundType) {
                logger.console('Parent Request Type not found!');
                return;
            }

            if (operation == 'update') {
                var foundGroup = Enumerable.From(foundType.RequestGroups)
                    .FirstOrDefault(null, function (x) { return x.ExternalId == group.ExternalId; });

                if (foundGroup) {
                    foundGroup.Name = group.Name;
                    foundGroup.Assignees = group.Assignees;

                    return;
                }
            } else if (operation == 'add') {

                if (typeof (foundType.RequestGroups) == "undefined" || foundType.RequestGroups == null)
                    foundType.RequestGroups = [];

                foundType.RequestGroups.push(group);

            } else {
                var idx = type.RequestGroups.indexOf(group);
                if (idx >= 0)
                    type.RequestGroups.splice(idx, 1);
            }
        }
        function getGroupAssignees() {
            if (!vm.currentGroup.Assignees)
                return [];

            var assignees = Enumerable.From(vm.currentGroup.Assignees)
                .Select(function (x) { return { ExternalId: x.ExternalId, FullName: x.FullName }; })
                .ToArray();

            return assignees;
        }
        function addGroupAssignees(data) {
            if (!data) return;

            if (typeof vm.currentGroup.Assignees == "undefined" || vm.currentGroup.Assignees == null)
                vm.currentGroup.Assignees = [];

            for (var i = 0; i < data.length; i++) {
                vm.currentGroup.Assignees.push(
                    angular.copy(data[i]));
            }
        }
        function removeGroupAssignee(group, assignee) {
            if (!group || !assignee) return;

            var idx = group.Assignees.indexOf(assignee);
            if (idx >= 0)
                group.Assignees.splice(idx, 1);
        }

        function showTaskModal(type, group, task) {
            if (task) {
                vm.currentTask.Question = task.Question;
                vm.currentTask.ExternalId = task.ExternalId;
            } else {
                vm.currentTask.Question = '';
                vm.currentTask.ExternalId = '';
            }
            vm.currentTask.Type = type;
            vm.currentTask.Group = group;

            $('#taskModal').modal('show');
        }
        function hideTaskModal() {
            $('#taskModal').modal('hide');

            vm.resetValidaton();

            vm.currentTask.Group = null;
            vm.currentTask.Type = null;
        }
        function saveTaskModal() {
            if (!vm.validateTaskModalForm()) return null;

            var isNew = !vm.currentTask.ExternalId;

            vm.state.isBusy = true;

            var type = vm.currentTask.Type;
            var group = vm.currentTask.Group;

            var task = {
                ExternalId: isNew ? newGuid() : vm.currentTask.ExternalId,
                Question: vm.currentTask.Question,
                ProductRequestGroupId: group.ExternalId
            };

            return $q.when()
                .then(function () {
                    if (isNew)
                        return remiapi.post.createProductRequestTask({ RequestTask: task });

                    return remiapi.post.updateProductRequestTask({ RequestTask: task });
                })
            .then(function () {
                vm.updateTaskList(isNew ? 'add' : 'update', type, group, task);
                vm.hideTaskModal();
            })
            .catch(function (fault) {
                console.log('error');
                console.log(fault);
                logger.error('Can\'t save request task');
            })
            .finally(function () {
                vm.state.isBusy = false;
            });
        }
        function updateTaskList(operation, type, group, task) {
            if (!type) return;
            var foundType = Enumerable.From(vm.requestConfig)
                .FirstOrDefault(null, function (x) { return x.ExternalId == type.ExternalId; });
            if (!foundType) {
                logger.console('Parent Request Type not found!');
                return;
            }

            if (!group) return;
            var foundGroup = Enumerable.From(foundType.RequestGroups)
                .FirstOrDefault(null, function (x) { return x.ExternalId == group.ExternalId; });
            if (!foundGroup) {
                logger.console('Parent Request Group not found!');
                return;
            }

            if (operation == 'update') {
                var foundTask = Enumerable.From(foundGroup.RequestTasks)
                    .FirstOrDefault(null, function (x) { return x.ExternalId == task.ExternalId; });

                if (foundTask) {
                    foundTask.Question = task.Question;
                    return;
                }
            } else if (operation == 'add') {
                if (typeof (foundGroup.RequestTasks) == "undefined" || foundGroup.RequestTasks == null)
                    foundGroup.RequestTasks = [];

                foundGroup.RequestTasks.push(task);
            } else {
                var idx = group.RequestTasks.indexOf(task);
                if (idx >= 0)
                    group.RequestTasks.splice(idx, 1);
            }
        }
        function deleteTask(type, group, task) {
            if (!task.ExternalId) return null;

            vm.state.isBusy = true;

            return $q.when()
                .then(function () {
                    return remiapi.post.deleteProductRequestTask({ RequestTaskId: task.ExternalId });
                })
            .then(function () {
                vm.updateTaskList('delete', type, group, task);
            })
            .catch(function (fault) {
                console.log('error');
                console.log(fault);
                logger.error('Can\'t delete request task!');
            })
            .finally(function () {
                vm.state.isBusy = false;
            });
        }

        function validateTypeModalForm() {
            var manualValidation = true;

            if (!$('#typeModalForm').valid() || !manualValidation) {
                throw "Invalid data!";
            }

            return true;
        }
        function validateTaskModalForm() {
            var manualValidation = true;

            if (!$('#taskModalForm').valid() || !manualValidation) {
                throw "Invalid data!";
            }

            return true;
        }
        function validateGroupModalForm() {
            var manualValidation = true;

            if (!$('#groupModalForm').valid() || !manualValidation) {
                throw "Invalid data!";
            }

            return true;
        }
    }
})();
