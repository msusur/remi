(function () {
    'use strict';

    var controllerId = 'productRegistration';

    angular.module('app').controller(controllerId,
        ['$scope', '$rootScope', 'common', 'remiapi', 'authService', 'localData', productRegistration]);

    function productRegistration($scope, $rootScope, common, remiapi, authService, localData) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;
        vm.state = {
            isBusy: false
        };
        vm.confirmation = {
            title: '',
            text: ''
        };
        vm.registrationStatuses = ['New', 'In progress', 'Completed'];
        vm.currentProductRegistration = {};
        vm.requestConfig = undefined;
        vm.registrations = [];
        vm.localData = localData;

        vm.activate = activate;
        vm.initUi = initUi;
        vm.getRequestConfig = getRequestConfig;
        vm.getProductRequestRegistrations = getProductRequestRegistrations;

        vm.prepareCurrentRecord = prepareCurrentRecord;
        vm.showProductRegistrationModal = showProductRegistrationModal;
        vm.hideProductRegistrationModal = hideProductRegistrationModal;
        vm.selectedRequestedTypeChanged = selectedRequestedTypeChanged;
        vm.saveProductRegistrationModal = saveProductRegistrationModal;
        vm.updateProductRegistration = updateProductRegistration;
        vm.deleteProductRegistration = deleteProductRegistration;

        vm.showCommentModal = showCommentModal;
        vm.saveCommentModal = saveCommentModal;
        vm.hideCommentModal = hideCommentModal;

        vm.currentTaskChanged = currentTaskChanged;

        vm.generateTaskIndex = generateTaskIndex;

        vm.showConfirmation = showConfirmation;
        vm.cancelConfirmation = cancelConfirmation;
        vm.approveConfirmation = approveConfirmation;

        vm.showRemovingReasonModal = showRemovingReasonModal;
        vm.hideRemovingReasonModal = hideRemovingReasonModal;

        vm.registrationsSorter = registrationsSorter;

        vm.activate();


        function activate() {
            common
                .activateController([
                    initUi(), getRequestConfig().then(vm.getProductRequestRegistrations)
                ], controllerId)
                .then(function () {
                    logger.console('Activated Product Registration View');
                });
        }

        function initUi() {
            $('#commentModal').on('shown.bs.modal', function () {
                $('#commentModal textarea.check-list-textarea').focus();
            });

            $('#productRegistrationModal').on('shown.bs.modal', function () {
                $('#productRegistrationModal #description').focus();
            });
        }

        function getRequestConfig() {
            vm.state.isBusy = true;

            return remiapi.get.productRegistrationsConfig()
                .then(function (data) {
                    vm.requestConfig = data.ProductRequestTypes;

                    vm.requestTypes = Enumerable.From(vm.requestConfig)
                        .Select(function (x) { return { Name: x.Name, ExternalId: x.ExternalId }; })
                        .ToArray();

                }, function (error) {
                    logger.error('Cannot get config for product registrations');
                    logger.console(error);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function getProductRequestRegistrations() {
            vm.state.isBusy = true;

            return remiapi.get.productRequestRegistrations()
                .then(function (data) {
                    vm.registrations = data.Registrations;

                }, function (error) {
                    logger.error('Cannot get product registrations');
                    logger.console(error);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function showProductRegistrationModal(registration) {
            if (!vm.requestConfig || vm.requestConfig.length == 0) {
                logger.warn('Templates for product request were not configured!');
                return;
            }

            vm.prepareCurrentRecord(registration);

            $scope.productRegistrationModalForm.$setPristine(); // need to remove red border when modal would be shown next time

            $('#productRegistrationModal').modal('show');
        }

        function hideProductRegistrationModal() {
            $('#productRegistrationModal').modal('hide');
            $('#productRegistrationModal').on('hidden.bs.modal', function () {
                $scope.$apply(function () {
                    vm.currentProductRegistration = {};
                    vm.taskIndex = {};
                });
                $('#productRegistrationModal').on('hidden.bs.modal', null);
            });
        }

        function saveProductRegistrationModal() {
            if ($scope.productRegistrationModalForm.$invalid) return null;

            var isNew = !vm.currentProductRegistration.ExternalId;
            var nonCompletedPresent = false;
            var completedPresent = false;

            var affectedTasks = [];
            proceedTaskHelper([vm.currentProductRegistration.RequestType], function (r, g, t) {
                affectedTasks.push({
                    ProductRequestTaskId: t.ExternalId,
                    IsCompleted: t.IsCompleted,
                    Comment: t.Comment,
                    LastChangedBy: t.LastChangedBy,
                    LastChangedOn: t.LastChangedOn
                });

                if (t.IsCompleted)
                    completedPresent = true;
                else
                    nonCompletedPresent = true;
            });

            var registration = {
                ExternalId: isNew ? newGuid() : vm.currentProductRegistration.ExternalId,
                Description: vm.currentProductRegistration.Description,
                ProductRequestTypeId: vm.currentProductRegistration.RequestType.ExternalId,
                ProductRequestType: vm.currentProductRegistration.RequestType.Name,
                Tasks: affectedTasks,
                CreatedOn: new Date(),
                CreatedBy: authService.identity.fullname
            };

            vm.state.isBusy = true;

            var saveFunc = isNew ? remiapi.post.createProductRequestRegistration : remiapi.post.updateProductRequestRegistration;

            return saveFunc({ Registration: registration })
                .then(function () {
                    if (!nonCompletedPresent && completedPresent) registration.Status = vm.registrationStatuses[2];
                    else if (nonCompletedPresent && completedPresent) registration.Status = vm.registrationStatuses[1];
                    else registration.Status = vm.registrationStatuses[0];

                    vm.updateProductRegistration(isNew ? 'add' : 'update', registration);
                    vm.hideProductRegistrationModal();
                })
                .catch(function (fault) {
                    console.log('error');
                    console.log(fault);
                    logger.error('Can\'t save registrations');
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function deleteProductRegistration(registration) {
            if (!registration || !registration.ExternalId) return null;

            vm.state.isBusy = true;

            return remiapi.post
                .deleteProductRequestRegistration({
                    RegistrationId: registration.ExternalId,
                    RemovingReason: vm.removingReason.removingReason,
                    Comment: vm.removingReason.comment
                })
                .then(function () {
                    vm.updateProductRegistration('delete', registration);
                    vm.hideRemovingReasonModal();
                })
                .catch(function (fault) {
                    console.log('error');
                    console.log(fault);
                    logger.error('Can\'t delete product request registration!');
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function updateProductRegistration(operation, registration) {
            if (operation == 'add') {
                vm.registrations.push(registration);
            }
            else {
                var found = Enumerable.From(vm.registrations)
                    .FirstOrDefault(null, function (x) { return x.ExternalId == registration.ExternalId; });

                if (found) {
                    var idx = vm.registrations.indexOf(found);
                    if (idx >= 0)
                        if (operation == 'update') {
                            vm.registrations[idx] = registration;
                        } else {
                            vm.registrations.splice(idx, 1);
                        }
                }
            }
        }

        function prepareCurrentRecord(registration) {
            if (!registration) {
                //new registration
                var defaultType = Enumerable.From(vm.requestConfig)
                    .FirstOrDefault(null, function (x) { return x.ExternalId == vm.requestTypes[0].ExternalId; });

                vm.currentProductRegistration = {
                    selectedRequestedType: vm.requestTypes[0],
                    ExternalId: '',
                    Description: '',
                    Status: '',
                    RequestType: angular.copy(defaultType)
                };

                vm.generateTaskIndex(defaultType);
            } else {
                //existing registration 
                var selectedType = Enumerable.From(vm.requestTypes)
                    .FirstOrDefault(null, function (x) { return x.ExternalId == registration.ProductRequestTypeId; });

                var selectedTypeConfig = Enumerable.From(vm.requestConfig)
                    .FirstOrDefault(null, function (x) { return x.ExternalId == registration.ProductRequestTypeId; });

                vm.currentProductRegistration = {
                    selectedRequestedType: selectedType,
                    ExternalId: registration.ExternalId,
                    Description: registration.Description,
                    Status: '',
                    RequestType: angular.copy(selectedTypeConfig)
                };

                vm.generateTaskIndex(vm.currentProductRegistration.RequestType);
                for (var i = 0; i < registration.Tasks.length; i++) {
                    var registrationTask = registration.Tasks[i];
                    var taskForEdit = vm.taskIndex[registrationTask.ProductRequestTaskId];
                    if (taskForEdit) {
                        taskForEdit.IsCompleted = registrationTask.IsCompleted;
                        taskForEdit.Comment = registrationTask.Comment;
                        taskForEdit.LastChangedBy = registrationTask.LastChangedBy;
                        taskForEdit.LastChangedOn = registrationTask.LastChangedOn;
                    }
                }
            }
        }

        function currentTaskChanged(task) {
            task.dirty = true;
            if (task) {
                if (!task.IsCompleted && !task.Comment && !vm.currentProductRegistration.ExternalId) {
                    task.LastChangedOn = null;
                    task.LastChangedBy = null;
                } else {
                    task.LastChangedOn = new Date();
                    task.LastChangedBy = authService.identity.fullname;
                }
            }
        }

        function selectedRequestedTypeChanged() {
            var foundCheckedCount = Enumerable.From(vm.currentProductRegistration.RequestType.RequestGroups)
                .Count(function (g) {
                    var foundCheckedTask = Enumerable.From(g.RequestTasks).FirstOrDefault(null, function (t) { return t.IsCompleted; });
                    return !!foundCheckedTask;
                });

            var updateTypeFunc = function () {
                var selectedType = Enumerable.From(vm.requestConfig)
                    .FirstOrDefault(null, function (x) { return x.ExternalId == vm.currentProductRegistration.selectedRequestedType.ExternalId; });

                vm.currentProductRegistration.RequestType = angular.copy(selectedType);
                vm.generateTaskIndex(vm.currentProductRegistration.RequestType);
            };

            if (foundCheckedCount > 0) {
                showConfirmation('Unselected type has checked items. Do you really want to change request type?', 'Change confirmation',
                    function () {
                        updateTypeFunc();
                    });
            } else {
                updateTypeFunc();
            }
        }

        function generateTaskIndex(type) {
            vm.taskIndex = {};
            proceedTaskHelper([type], function (r, g, t) {
                vm.taskIndex[t.ExternalId] = t;
            });
        }

        function showConfirmation(text, title, approveCallback) {
            vm.confirmation.title = title;
            vm.confirmation.text = text;
            vm.confirmation.approveCallback = approveCallback;

            $('#confirmDialog').modal('show');
        }
        function cancelConfirmation() {
            $('#confirmDialog').modal('hide');

            if (vm.currentProductRegistration.selectedRequestedType.ExternalId != vm.currentProductRegistration.RequestType.ExternalId) {
                var found = Enumerable.From(vm.requestTypes).FirstOrDefault(null, function (x) { return x.ExternalId == vm.currentProductRegistration.RequestType.ExternalId; });
                if (found)
                    vm.currentProductRegistration.selectedRequestedType = found;
            }
        }
        function approveConfirmation() {
            $('#confirmDialog').modal('hide');

            if (vm.confirmation.approveCallback)
                vm.confirmation.approveCallback();
        }

        function showCommentModal(task) {
            vm.currentTask = task;

            $('#commentModal').modal({ backdrop: 'static', keyboard: true });
        }
        function saveCommentModal() {
            hideCommentModal();
        }
        function hideCommentModal() {
            $('#commentModal').modal('hide');
            vm.currentTask = null;
        }

        function showRemovingReasonModal(p) {
            vm.removingReason = {
                removingReason: vm.localData.enums.RemovingReason[0].Name,
                comment: ''
            };
            vm.currentProductRegistration = p;
            $('#removingReasonModal').modal({ backdrop: 'static', keyboard: true });
        }
        function hideRemovingReasonModal() {
            $('#removingReasonModal').modal('hide');
            vm.removingReason = null;
            vm.currentProductRegistration = null;
        }

        function proceedTaskHelper(requestTypes, taskCallback, groupCallback, typeCallback) {
            Enumerable.From(requestTypes)
                .ForEach(function (r) {
                    if (!typeCallback || typeCallback(t))
                        Enumerable.From(r.RequestGroups)
                            .ForEach(function (g) {
                                if (!groupCallback || groupCallback(t, g))
                                    Enumerable.From(g.RequestTasks)
                                        .ForEach(function (t) {
                                            taskCallback(r, g, t);
                                        });
                            });
                });
        }

        function registrationsSorter(registration) {
            switch (registration.Status) {
                case vm.registrationStatuses[0]:
                case vm.registrationStatuses[1]:
                    return '0 ' + registration.Description;
                default:
                    return '1 ' + registration.Description;
            }
        }
    }
})();
