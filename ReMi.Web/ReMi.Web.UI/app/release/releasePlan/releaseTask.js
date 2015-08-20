(function () {
    "use strict";

    var controllerId = "releaseTask";
    angular.module("app").controller(controllerId, ["$rootScope", "$scope", "common", "config", "remiapi", "authService", "notifications", releaseTask]);

    function releaseTask($rootScope, $scope, common, config, remiapi, authService, notifications) {
        var $q = common.$q;
        var logger = common.logger.getLogger(controllerId);

        var vm = this;
        vm.$scope = $scope;

        vm.parent = common.getParentScope($scope, function (sc) {
            return sc && sc.vm && sc.vm.controllerId === "release";
        }).vm;

        vm.state = {
            isBusy: false,
            bindedToReleaseWindow: false
        };

        vm.dragging = initDragging();

        vm.validator = undefined;

        vm.init = init;
        vm.initDragging = initDragging;

        vm.uploaderUrl = ""; //remiapi.getAbsolutePath('/upload');

        vm.releaseTaskTypes = [];
        vm.releaseTaskRisks = [];
        vm.releaseTaskEnvironments = [];

        vm.currentOperationType = "";

        vm.releaseWindowId = undefined;
        vm.isClosed = false;

        vm.releaseTasks = [];

        vm.currentTask = {};

        var releaseTaskUpdatedEvent = "releaseTask.ReleaseTaskUpdatedEvent";
        var releaseTaskDeletedEvent = "releaseTask.ReleaseTaskDeletedEvent";
        var releaseTasksOrderUpdatedEvent = "releaseTask.ReleaseTasksOrderUpdatedEvent";

        vm.createTask = function () {

            var currentTask = {
                ExternalId: newGuid(),
                ReleaseWindowId: vm.releaseWindowId,
                Type: vm.releaseTaskTypes[0].Value,
                Risk: vm.releaseTaskRisks[0].Value,
                Description: "",
                CreatedBy: authService.identity.name,
                RequireSiteDown: false,
                Assignee: authService.identity.fullname,
                AssigneeExternalId: authService.identity.externalId,
                CompletedBy: "",
                FilesForUpload: [],
                Attachments: [],
                CreateHelpDeskTicket: false,
                HelpDeskReference: undefined,
                HelpDeskUrl: undefined,
                WhereTested: vm.releaseTaskEnvironments[0].Value
            };

            return currentTask;
        };

        var assignee = null;

        vm.releaseWindowLoadedEventHandler = releaseWindowLoadedEventHandler;
        common.handleEvent("release.ReleaseWindowLoadedEvent", vm.releaseWindowLoadedEventHandler, $scope);
        common.handleEvent(releaseTasksOrderUpdatedEvent, handleInternalReleaseTasksOrderUpdated, $scope);
        common.handleEvent(releaseTaskDeletedEvent, handleInternalReleaseTasksDeletedEvent, $scope);
        common.handleEvent(config.events.notificationReceived, serverNotificationHandler, $scope);

        $scope.$on("$destroy", scopeDestroyHandler);

        vm.getReleaseTasks = getReleaseTasks;
        vm.refreshReleaseTasks = refreshReleaseTasks;
        vm.addOrReplaceReleaseTask = addOrReplaceReleaseTask;
        vm.completeReleaseTask = completeReleaseTask;

        vm.createReleaseTask = createReleaseTask;
        vm.editReleaseTask = editReleaseTask;
        vm.removeReleaseTask = removeReleaseTask;
        vm.saveReleaseTask = saveReleaseTask;

        vm.hideCurrentReleaseTaskModal = hideCurrentReleaseTaskModal;

        vm.selectAssignee = selectAssignee;
        vm.getSelectedAssignees = getSelectedAssignees;

        vm.applyReleaseTasksOrder = applyReleaseTasksOrder;

        vm.sendAttchment = sendAttchment;
        vm.isManualConfirmationAllowed = isManualConfirmationAllowed;
        vm.confirmTask = confirmTask;

        vm.currentOperationTypeActionTitle = function () {
            if (vm.currentOperationType === "create")
                return "Add";

            return "Update";
        };

        activate();

        function activate() {
            common
                .activateController([
                    initUi(),
                    getReleaseTaskTypes(),
                    getReleaseTaskRisks(),
                    getReleaseTaskEnvironments(),
                    releaseWindowLoadedEventHandler(vm.parent.currentReleaseWindow)
                ], controllerId, $scope)
                .then(function () { logger.console("Activated Release Tasks View"); });
        }

        function scopeDestroyHandler() {
            notifications.unsubscribe("HelpDeskTaskCreatedEvent");
            notifications.unsubscribe("TaskCompletedEvent");
            notifications.unsubscribe("ReleaseTaskUpdatedEvent");
            notifications.unsubscribe("ReleaseTaskCreatedEvent");
            notifications.unsubscribe("ReleaseTaskDeletedEvent");
            notifications.unsubscribe("ReleaseTasksOrderUpdatedEvent");
        }

        function getReleaseTaskTypes() {
            return remiapi.getReleaseTaskTypes().then(function (data) {
                if (data.ReleaseTaskTypes) {
                    vm.releaseTaskTypes = data.ReleaseTaskTypes;
                }
            });
        }

        function getReleaseTaskRisks() {
            return remiapi.getReleaseTaskRisks().then(function (data) {
                if (data.Risks) {
                    vm.releaseTaskRisks = data.Risks;
                }
            });
        }

        function getReleaseTaskEnvironments() {
            return remiapi.getReleaseTaskEnvironments().then(function (data) {
                if (data.Environments) {
                    vm.releaseTaskEnvironments = data.Environments;
                }
            });
        }

        function initUi() {
            vm.validator = $("#releaseTaskModalForm").validate({
                errorClass: "invalid-data",
                errorPlacement: function () { }
            });

            $(document).ready(function () {
                $("#releasetask-length-of-run").numeric();
            });
        }

        function createReleaseTask() {
            if (!vm.state.bindedToReleaseWindow) {
                logger.warn("Release window not assigned to tasks");
                return;
            }

            vm.currentOperationType = "create";

            vm.currentTask = vm.createTask();

            $("#releaseTaskModal").modal("show");
        }

        function editReleaseTask(task) {
            vm.currentOperationType = "edit";

            vm.currentTask = angular.copy(task);
            if (vm.currentTask.CompletedOn === false) {
                vm.currentTask.CompletedOn = undefined;
            }

            $("#releaseTaskModal").modal("show");
        };

        function removeReleaseTask(task) {
            vm.state.isBusy = true;
            return remiapi.deleteReleaseTask({ ReleaseTaskId: task.ExternalId })
                .then(function () {
                    var index = vm.releaseTasks.indexOf(task);
                    vm.releaseTasks.splice(index, 1);
                    common.sendEvent(releaseTaskDeletedEvent, task);
                }).finally(function () {
                    vm.state.isBusy = false;
                });
        };

        function hideCurrentReleaseTaskModal() {
            $("#releaseTaskModal").modal("hide");

            resetValidaton();
        }

        function validate() {
            var manualValidation = true;

            if (!$("#releasetask-assignee").text()) {
                $("#releasetask-assignee").addClass("invalid-data");
                manualValidation = false;
            }

            if (!$("#releaseTaskModalForm").valid() || !manualValidation) {
                throw "Invalid data!";
            }

            return true;
        }

        function resetValidaton() {
            if (vm.validator)
                vm.validator.resetForm();

            $("#releasetask-assignee").removeClass("invalid-data");
        }

        function saveReleaseTask() {
            vm.state.isBusy = true;

            return $q.when()
                .then(validate)
                .then(checkAccounts)
                .then(sendAttchment)
                .then(convertUploadFilesToAttachment)
                .then(function () {
                    if (vm.currentOperationType === "create")
                        return remiapi.createReleaseTask({ ReleaseTask: vm.currentTask });

                    return remiapi.updateReleaseTask({ ReleaseTask: vm.currentTask });
                })
                .then(function () {
                    delete vm.currentTask.FilesForUpload;

                    var taskClone = clone(vm.currentTask);

                    return taskClone;
                })
                .then(function (task) {

                    return $q.all([
                        vm.addOrReplaceReleaseTask(task),
                        common.sendEvent(releaseTaskUpdatedEvent, vm.currentTask)
                    ]).then(function () {
                        vm.hideCurrentReleaseTaskModal();
                    });

                })
                .catch(function (fault) {

                    console.log("error", fault);
                    logger.error("Can't save release task");
                })
                .finally(function () { vm.state.isBusy = false; });
        }

        function applyReleaseTasksOrder(releaseTasks) {
            vm.state.isBusy = true;

            var data = {};
            for (var i in releaseTasks) {
                if (releaseTasks.hasOwnProperty(i)) {
                    var task = releaseTasks[i];
                    data[task.ExternalId] = +i + 1;
                }
            }
            return remiapi.updateReleaseTasksOrder({ ReleaseTasksOrder: data })
                .then(function () {
                    common.sendEvent(releaseTasksOrderUpdatedEvent);
                    vm.state.isBusy = false;
                }, function (error) {
                    logger.error("Cannot update release task order");
                    logger.console(error);
                    vm.getReleaseTasks(vm.releaseWindowId);
                });
        }

        // check if accounts used in task already exists on server. if no then they will be created
        function checkAccounts() {
            var accounts = [];

            if (!!assignee)
                accounts.push(assignee);

            if (accounts.length > 0)
                return remiapi.checkAccounts({ ReleaseWindowId: vm.releaseWindowId, Accounts: accounts });

            return $q.when();
        }

        function sendAttchment() {
            var deferred = $q.defer();

            if (vm.currentTask.FilesForUpload && vm.currentTask.FilesForUpload.length > 0) {
                $("#attachmentUpload").fileupload("send", { files: vm.currentTask.FilesForUpload })
                    .success(function (result) { deferred.resolve(result); })
                    .error(function () { deferred.reject(); });
            } else {
                deferred.resolve();
            }

            return deferred.promise;
        }

        function convertUploadFilesToAttachment() {
            if (!vm.currentTask.Attachments)
                vm.currentTask.Attachments = [];

            if (vm.currentTask.FilesForUpload)
                vm.currentTask.FilesForUpload.forEach(function (file) {
                    vm.currentTask.Attachments.push({
                        Name: file.name,
                        Size: file.size,
                        Type: file.type,
                        ServerName: file.serverName || ""
                    });
                });
        }

        function selectAssignee(accounts) {
            if (!accounts || accounts.length === 0) {
                logger.warn("Failed to update assignee!");
                return;
            };

            assignee = accounts[0];

            vm.currentTask.Assignee = assignee.FullName;
            vm.currentTask.AssigneeExternalId = assignee.ExternalId;
        }

        function getReleaseTasks(releaseWindowId) {
            vm.state.isBusy = true;

            return remiapi.getReleaseTasks(releaseWindowId)
                .then(function (data) {
                    for (var counter = 0; counter < data.ReleaseTasks.length; counter++) {
                        data.ReleaseTasks[counter].IsConfirmed = data.ReleaseTasks[counter].ReceiptConfirmedOn == null ? false : true;
                        data.ReleaseTasks[counter].IsCompleted = data.ReleaseTasks[counter].CompletedOn == null ? false : true;
                    }
                    return vm.releaseTasks = data.ReleaseTasks;
                }, function () {
                    vm.releaseTasks = [];
                }).finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function releaseWindowLoadedEventHandler(releaseWindow) {
            if (releaseWindow) {
                vm.releaseWindowId = releaseWindow.ExternalId;
                vm.releaseType = releaseWindow.ReleaseType;
                vm.state.bindedToReleaseWindow = true;
                vm.isClosed = !!releaseWindow.ClosedOn;

                vm.getReleaseTasks(vm.releaseWindowId);

                logger.console("Binded to release window " + releaseWindow.ExternalId);

                notifications.subscribe("HelpDeskTaskCreatedEvent", { 'ReleaseWindowId': vm.releaseWindowId });
                notifications.subscribe("TaskCompletedEvent", { 'ReleaseWindowId': vm.releaseWindowId });
                notifications.subscribe("ReleaseTaskUpdatedEvent", { 'ReleaseWindowId': vm.releaseWindowId });
                notifications.subscribe("ReleaseTaskCreatedEvent", { 'ReleaseWindowId': vm.releaseWindowId });
                notifications.subscribe("ReleaseTaskDeletedEvent", { 'ReleaseWindowId': vm.releaseWindowId });
                notifications.subscribe("ReleaseTasksOrderUpdatedEvent", { 'ReleaseWindowId': vm.releaseWindowId });
            } else {
                vm.releaseWindowId = "";
                vm.state.bindedToReleaseWindow = false;
                vm.releaseTasks = [];
                vm.isClosed = false;

                logger.console("Unbind release window");

                notifications.unsubscribe("HelpDeskTaskCreatedEvent");
                notifications.unsubscribe("TaskCompletedEvent");
                notifications.unsubscribe("ReleaseTaskUpdatedEvent");
                notifications.unsubscribe("ReleaseTaskCreatedEvent");
                notifications.unsubscribe("ReleaseTaskDeletedEvent");
                notifications.unsubscribe("ReleaseTasksOrderUpdatedEvent");
            }
        }

        function refreshReleaseTasks() {
            vm.getReleaseTasks(vm.releaseWindowId);
        }

        function addOrReplaceReleaseTask(task) {
            if (!task)
                return;

            var found = false;

            for (var i = 0; i < vm.releaseTasks.length; i++) {
                if (vm.releaseTasks[i].ExternalId === task.ExternalId) {
                    vm.releaseTasks[i] = task;
                    found = true;
                    break;
                }
            }

            if (!found)
                vm.releaseTasks.push(task);
        }

        function helpDeskTaskCreatedEventhandler(event) {
            if (!event || !event.HelpDeskTask) return;

            var taskId = event.HelpDeskTask.ReleaseTaskId;

            Enumerable.From(vm.releaseTasks).ForEach(function (task) {
                if (task.ExternalId === taskId) {
                    $scope.$apply(function () {
                        task.HelpDeskReference = event.HelpDeskTask.Number;
                        task.HelpDeskUrl = event.HelpDeskTask.Url;
                    });
                }
            });
        }

        function serverNotificationHandler(notification) {
            if (notification.name === "HelpDeskTaskCreatedEvent") {
                helpDeskTaskCreatedEventhandler(notification.data);
            }
            else if (notification.name === "TaskCompletedEvent") {
                taskCompletedEventHandler(notification.data);
            }
            else if (notification.name === "ReleaseTaskUpdatedEvent") {
                releaseTaskUpdatedEventHandler(notification.data);
            }
            else if (notification.name === "ReleaseTaskCreatedEvent") {
                releaseTaskCreatedEventHandler(notification.data);
            }
            else if (notification.name === "ReleaseTaskDeletedEvent") {
                releaseTaskDeletedEventHandler(notification.data);
            }
            else if (notification.name === "ReleaseTasksOrderUpdatedEvent") {
                handleReleaseTasksOrderUpdated(notification.data);
            }
        }

        function handleReleaseTasksOrderUpdated(event) {
            getReleaseTasks(event.ReleaseWindowId);
            if (vm.showToast)
                logger.info("Release Tasks order has changed");
        }

        function handleInternalReleaseTasksOrderUpdated() {
            if (!vm.allowDragAndDrop)
                getReleaseTasks(vm.releaseWindowId);
        }

        function handleInternalReleaseTasksDeletedEvent(task) {
            if (task) {
                var found = Enumerable.From(vm.releaseTasks)
                    .Where(function (x) { return x.ExternalId === task.ExternalId; })
                    .FirstOrDefault();
                if (found && task !== found) {
                    var index = vm.releaseTasks.indexOf(found);
                    vm.releaseTasks.splice(index, 1);
                }
            }
        }

        function releaseTaskUpdatedEventHandler(data) {
            $scope.$apply(function () {
                for (var counter = 0; counter < vm.releaseTasks.length; counter++) {
                    if (vm.releaseTasks[counter].ExternalId === data.ReleaseTask.ExternalId) {
                        var task = vm.releaseTasks[counter];

                        task.IsCompleted = !!data.ReleaseTask.CompletedOn;
                        task.Assignee = data.ReleaseTask.Assignee;
                        task.AssigneeExternalId = data.ReleaseTask.AssigneeExternalId;

                        if (!task.HelpDeskReference && data.ReleaseTask.HelpDeskReference) {
                            task.HelpDeskReference = data.ReleaseTask.HelpDeskReference;
                            task.HelpDeskUrl = data.ReleaseTask.HelpDeskUrl;
                        }

                        task.Description = data.ReleaseTask.Description;
                        task.Type = data.ReleaseTask.Type;
                        task.Risk = data.ReleaseTask.Risk;
                        task.LengthOfRun = data.ReleaseTask.LengthOfRun;

                        if (vm.showToast) {
                            logger.info("Release Task " + task.Description + " was updated");
                        }
                        break;
                    }
                }
            });
        }

        function releaseTaskCreatedEventHandler(data) {
            $scope.$apply(function () {
                var found = Enumerable.From(vm.releaseTasks)
                        .Where(function (x) { return x.ExternalId === data.ReleaseTask.ExternalId; })
                        .FirstOrDefault();

                if (!found) {
                    var task = data.ReleaseTask;
                    vm.releaseTasks.push(task);
                    if (vm.showToast) {
                        logger.info("Release Task " + task.Description + " was created");
                    }
                }
            });
        }

        function releaseTaskDeletedEventHandler(data) {
            $scope.$apply(function () {
                var task = Enumerable.From(vm.releaseTasks)
                        .Where(function (x) { return x.ExternalId === data.ReleaseTask.ExternalId; })
                        .FirstOrDefault();

                if (task) {
                    var index = vm.releaseTasks.indexOf(task);
                    vm.releaseTasks.splice(index, 1);
                    if (vm.showToast) {
                        logger.info("Release Task " + task.Description + " was deleted");
                    }
                }
            });
        }

        function taskCompletedEventHandler(data) {
            $scope.$apply(function () {
                for (var counter = 0; counter < vm.releaseTasks.length; counter++) {
                    if (vm.releaseTasks[counter].ExternalId === data.ReleaseTaskExternalId) {
                        vm.releaseTasks[counter].IsCompleted = true;
                        vm.releaseTasks[counter].Assignee = data.AssigneeName;

                        if (vm.showToast) {
                            logger.info("Release Task " + vm.releaseTasks[counter].Description + " was completed");
                        }
                        break;
                    }
                }
            });
        }

        function completeReleaseTask(task) {
            var currentUser = authService.isLoggedIn ? authService.identity : null;

            if (!currentUser) {
                return $q.when();
            }

            vm.state.isBusy = true;

            return remiapi
                .completeReleaseTask({ ReleaseTaskExtetnalId: task.ExternalId })
                .then(function () { }, function (error) {
                    logger.error("Cannot complete release task");
                    logger.console("Cannot complete release task");
                    logger.console(error);
                })
                .finally(function () { vm.state.isBusy = false; });
        }

        function getSelectedAssignees() {
            var result = [];
            if (vm.currentTask.Assignee && vm.currentTask.AssigneeExternalId) {
                result.push({ FullName: vm.currentTask.Assignee, ExternalId: vm.currentTask.AssigneeExternalId });
            }

            return result;
        }

        function init(flag) {
            vm.showToast = (flag === "true");
            vm.allowDragAndDrop = !vm.showToast;
        }

        function initDragging() {
            return {
                currentDragged: null,
                drag: function (task) {
                    if (task) {
                        vm.dragging.currentDragged = task;
                    }
                },
                drop: function (task) {
                    if (task && vm.dragging.currentDragged) {
                        var originalIndex = vm.releaseTasks.indexOf(vm.dragging.currentDragged);
                        if (originalIndex >= 0) {
                            vm.releaseTasks.splice(originalIndex, 1);
                            var destinationIndex = vm.releaseTasks.indexOf(task);
                            if (destinationIndex >= 0 && destinationIndex !== originalIndex) {
                                $scope.$apply(function () {
                                    vm.releaseTasks.splice(destinationIndex, 0, vm.dragging.currentDragged);
                                });
                                vm.applyReleaseTasksOrder(vm.releaseTasks);
                            } else {
                                vm.releaseTasks.splice(originalIndex, 0, vm.dragging.currentDragged);
                            }
                        }
                    }
                },
                dragEnd: function () {
                    vm.dragging.currentDragged = null;
                }
            };
        }

        function isManualConfirmationAllowed(task) {
            return task.AssigneeExternalId === authService.identity.externalId
                && !task.IsConfirmed;
        }

        function confirmTask(task) {
            if (!vm.isManualConfirmationAllowed(task)) return;

            vm.state.isBusy = true;
            remiapi.post.confirmReleaseTask({ ReleaseTaskId: task.ExternalId })
                .then(function() {
                    task.IsConfirmed = true;
                }, function (error) {
                    logger.error("Cannot confirm release task");
                    logger.console("Cannot confirm release task");
                    logger.console(error);
                })
                .finally(function () { vm.state.isBusy = false; });
        }
    }

})();
