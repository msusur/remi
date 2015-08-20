(function () {
    'use strict';

    var controllerId = 'releaseApprovers';

    angular.module('app')
        .controller(controllerId, ['$scope', '$rootScope', 'remiapi', 'common', 'config', 'authService', 'notifications', 'commandPermissions', releaseApprovers]);

    function releaseApprovers($scope, $rootScope, remiapi, common, config, authService, notifications, commandPermissions) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;
        vm.state = {
            isBusy: false,
            bindedToReleaseWindow: false
        };

        var releaseStatusChangedEventName = 'ReleaseStatusChangedEvent';
        var releaseApprovementEventName = 'ApprovementEvent';
        var approverRemovedFromReleaseEventName = 'ApproverRemovedFromReleaseEvent';
        var approverAddedToReleaseWindowEventName = 'ApproverAddedToReleaseWindowEvent';
        var releaseProcessParticipantEvent = 'releaseProcess.isParticipantEvent';

        vm.loggedInHandler = loggedInHandler;
        vm.releaseWindowLoadedEventHandler = releaseWindowLoadedEventHandler;
        vm.serverNotificationHandler = serverNotificationHandler;
        vm.releaseStatusChangedEventHandler = releaseStatusChangedEventHandler;
        vm.releaseApprovementEventHandler = releaseApprovementEventHandler;
        vm.approverRemovedFromReleaseEventHandler = approverRemovedFromReleaseEventHandler;
        vm.approverAddedToReleaseWindowEventHandler = approverAddedToReleaseWindowEventHandler;

        common.handleEvent('release.ReleaseWindowLoadedEvent', vm.releaseWindowLoadedEventHandler, $scope);
        common.handleEvent(config.events.notificationReceived, vm.serverNotificationHandler, $scope);
        common.handleEvent(config.events.loggedIn, vm.loggedInHandler, $scope);

        vm.isReleaseOpened = false;
        vm.releaseWindow = undefined;

        readPermissions();

        vm.releaseApprovers = [];
        vm.addApprovers = addApprovers;
        vm.removeApprover = removeApprover;
        vm.approve = approve;
        vm.refreshList = refreshList;
        vm.closeApprovementDescriptionModal = closeApprovementDescriptionModal;
        vm.showApprovementDescriptionModal = showApprovementDescriptionModal;
        vm.updateApprover = updateApprover;

        vm.manageOptions = {
            allowAdd: false,
            allowRemove: false,
            allowApprove: false
        };
        vm.adjustManageOptions = adjustManageOptions;

        vm.getSelectedAccounts = getSelectedAccounts;

        $scope.$on('$destroy', scopeDestroyHandler);

        activate();

        function activate() {
            common.activateController([], controllerId, $scope)
                .then(function () { logger.console('Activated Release Approvers View'); });
        }

        function scopeDestroyHandler() {
            notifications.unsubscribe(releaseApprovementEventName);
            notifications.unsubscribe(releaseStatusChangedEventName);
            notifications.unsubscribe(approverAddedToReleaseWindowEventName);
            notifications.unsubscribe(approverRemovedFromReleaseEventName);
        }

        function approve(signature, p) {
            if (!signature || !signature.deferred || !signature.userName || !signature.password) {
                logger.warn('Please provide credentials');
                return null;
            }
            if (!vm.releaseWindow) {
                signature.deferred.reject();
                logger.warn('Release window not selected');
                return null;
            }
            if (!p || !p.Account || !p.Account.ExternalId) {
                signature.deferred.reject();
                return null;
            }

            vm.state.isBusy = true;

            return remiapi
                .approveRelease({
                    ReleaseWindowId: vm.releaseWindow.ExternalId,
                    AccountId: p.Account.ExternalId,
                    UserName: signature.userName,
                    Password: signature.password,
                    Comment: signature.Comment
                })
                .then(function () {
                    var found = Enumerable.From(vm.releaseApprovers)
                        .Where(function (x) { return x.Account.ExternalId == p.Account.ExternalId; })
                        .FirstOrDefault();

                    if (found) {
                        found.IsApproved = true;
                        found.ApprovedOn = (new Date()).toString();
                    }

                    signature.deferred.resolve();
                }, function (fault) {
                    signature.deferred.reject();

                    console.log('error', fault);
                    if (fault && fault.Details)
                        logger.error('Cannot approve the release: <br />' + fault.Details);
                    else
                        logger.error('Cannot approve the release');
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function refreshList() {
            return getReleaseApprovers();
        }

        function getReleaseApprovers() {
            vm.state.isBusy = true;

            vm.releaseApprovers = [];

            return remiapi
                .getReleaseApprovers(vm.releaseWindow.ExternalId)
                .then(function (data) {
                    var items = [];
                    data.Approvers.forEach(function (approver) {
                        var item = approver;
                        item.IsApproved = !!item.ApprovedOn;

                        items.push(item);
                    });

                    vm.releaseApprovers = items;
                    if (vm.releaseApprovers.filter(function (x) {
                                return x.Account.Email == authService.identity.email;
                    }).length > 0) {
                        common.sendEvent(releaseProcessParticipantEvent);
                    }
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function releaseWindowLoadedEventHandler(releaseWindow) {
            if (!!vm.releaseWindow) {
                notifications.unsubscribe(releaseStatusChangedEventName);
                notifications.unsubscribe(releaseApprovementEventName);
                notifications.unsubscribe(approverAddedToReleaseWindowEventName);
                notifications.unsubscribe(approverRemovedFromReleaseEventName);
            }

            if (releaseWindow) {
                vm.releaseWindow = angular.copy(releaseWindow);
                vm.state.bindedToReleaseWindow = true;

                vm.isReleaseOpened = vm.releaseWindow.Status == 'Opened';
                vm.adjustManageOptions();

                getReleaseApprovers(vm.releaseWindow.ExternalId);

                logger.console('Binded to release window ' + vm.releaseWindow.ExternalId);

                notifications.subscribe(releaseStatusChangedEventName, { 'ReleaseWindowId': vm.releaseWindow.ExternalId });
                notifications.subscribe(releaseApprovementEventName, { 'ReleaseWindowId': vm.releaseWindow.ExternalId });
                notifications.subscribe(approverAddedToReleaseWindowEventName, { 'ReleaseWindowId': vm.releaseWindow.ExternalId });
                notifications.subscribe(approverRemovedFromReleaseEventName, { 'ReleaseWindowId': vm.releaseWindow.ExternalId });

            } else {
                vm.releaseWindow = null;
                vm.isReleaseOpened = false;

                vm.state.bindedToReleaseWindow = false;

                vm.releaseApprovers = [];
                vm.adjustManageOptions();

                logger.console('Unbind from release window ');

                notifications.unsubscribe(releaseStatusChangedEventName);
                notifications.unsubscribe(releaseApprovementEventName);
                notifications.unsubscribe(approverAddedToReleaseWindowEventName);
                notifications.unsubscribe(approverRemovedFromReleaseEventName);
            }
        }

        function addApprovers(accounts) {
            vm.state.isBusy = true;

            var approvers = [];
            for (var counter = 0; counter < accounts.length; counter++) {
                approvers.push({
                    Account: accounts[counter],
                    ReleaseWindowId: vm.releaseWindow.ExternalId,
                    ExternalId: newGuid()
                });
            }

            return remiapi
                .addReleaseApprovers({ 'Approvers': approvers })
                .then(function () {
                    Enumerable.From(approvers)
                        .ForEach(function (item) {
                            item.IsApproved = false;
                            item.ApprovedOn = '';

                            vm.updateApprover(item);
                        });

                }, function (fault) {
                    logger.error('Can\'t add release approver');
                    logger.console(fault);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function removeApprover(approver) {
            vm.state.isBusy = true;

            return remiapi
                .removeReleaseApprover({ 'ApproverId': approver.ExternalId, 'ReleaseWindowId': vm.releaseWindow.ExternalId })
                .then(function () {
                    vm.approverRemovedFromReleaseEventHandler({ 'ApproverId': approver.ExternalId });
                }, function (fault) {
                    logger.error('Can\'t remove release approver');
                    logger.console(fault);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function getSelectedAccounts() {
            var result = [];

            vm.releaseApprovers.forEach(function (item) {
                result.push(item.Account);
            });

            return result;
        }

        function serverNotificationHandler(notification) {
            if (!vm.releaseWindow) return false;

            if (notification.name == releaseStatusChangedEventName) {
                vm.releaseStatusChangedEventHandler(notification.data);
            }

            if (notification.name == releaseApprovementEventName) {
                vm.releaseApprovementEventHandler(notification.data);
            }

            if (notification.name == approverAddedToReleaseWindowEventName) {
                vm.approverAddedToReleaseWindowEventHandler(notification.data);
            }

            if (notification.name == approverRemovedFromReleaseEventName) {
                vm.approverRemovedFromReleaseEventHandler(notification.data);
            }

            return true;
        };

        function releaseApprovementEventHandler(data) {
            for (var counter = 0; counter < vm.releaseApprovers.length; counter++) {
                if (vm.releaseApprovers[counter].ExternalId == data.ApproverId) {
                    vm.releaseApprovers[counter].IsApproved = true;
                    vm.releaseApprovers[counter].ApprovedOn = (new Date()).toString();
                    vm.releaseApprovers[counter].Comment = data.Comment;
                    break;
                }
            }
        }

        function releaseStatusChangedEventHandler(data) {
            vm.releaseWindow.Status = data.ReleaseStatus;
            vm.isReleaseOpened = vm.releaseWindow.Status == 'Opened';
        }

        function approverRemovedFromReleaseEventHandler(data) {
            var found = Enumerable.From(vm.releaseApprovers)
                .FirstOrDefault(null, function (x) { return x.ExternalId == data.ApproverId; });
            if (found)
                vm.updateApprover(found, true);
        }

        function approverAddedToReleaseWindowEventHandler(data) {
            vm.updateApprover(data.Approver);
        }

        function loggedInHandler() {
            vm.adjustManageOptions();
        }

        function adjustManageOptions() {
            var checkAdd = commandPermissions.checkCommand('AddReleaseApproversCommand');
            if (checkAdd) {
                vm.manageOptions.allowAdd = checkAdd.result;
            }

            var checkRemove = commandPermissions.checkCommand('RemoveReleaseApproverCommand');
            if (checkRemove) {
                vm.manageOptions.allowRemove = checkRemove.result;
            }

            var checkApprove = commandPermissions.checkCommand('ApproveReleaseCommand');
            if (checkApprove) {
                vm.manageOptions.allowApprove = checkApprove.result;
            }
        }

        function readPermissions() {
            commandPermissions.readPermissions(['AddReleaseApproversCommand', 'RemoveReleaseApproverCommand', 'ApproveReleaseCommand'])
                        .then(function (permissions) {
                            if (!authService.state.isBusy)
                                vm.adjustManageOptions();
                        });
        }

        function closeApprovementDescriptionModal() {
            $('#releaseApproversDescriptionModal').modal('hide');
        }

        function showApprovementDescriptionModal(approver) {
            vm.approvementDescription = approver.Comment;
            $('#releaseApproversDescriptionModal').modal('show');
        }

        function updateApprover(approver, isRemove) {
            if (!approver || !approver.Account) return;

            var found = Enumerable.From(vm.releaseApprovers)
                .FirstOrDefault(null, function (x) { return x.Account.ExternalId == approver.Account.ExternalId; });

            if (found) {
                var idx = vm.releaseApprovers.indexOf(found);
                if (isRemove) {
                    vm.releaseApprovers.splice(idx, 1);
                } else {
                    vm.releaseApprovers[idx] = approver;
                }
            } else if (!isRemove) {
                vm.releaseApprovers.push(approver);
            }
        }
    }
})()
