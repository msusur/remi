(function () {
    'use strict';
    var controllerId = 'releaseParticipant';
    angular.module('app').controller(controllerId, ['$scope', '$rootScope', 'common', 'config', 'remiapi', 'authService', 'notifications', releaseParticipant]);

    function releaseParticipant($scope, $rootScope, common, config, remiapi, authService, notifications) {
        var $q = common.$q;

        var vm = this;
        var logger = common.logger.getLogger(controllerId);

        vm.controllerId = controllerId;
        vm.$scope = $scope;
        vm.state = {
            isBusy: false,
            bindedToReleaseWindow: false
        };

        vm.addReleaseParticipants = addReleaseParticipants;
        vm.removeReleaseParticipant = removeReleaseParticipant;
        vm.releaseWindowLoadedEventHandler = releaseWindowLoadedEventHandler;
        vm.confirmParticipation = confirmParticipation;
        vm.serverNotificationHandler = serverNotificationHandler;
        vm.getReleaseParticipants = getReleaseParticipants;
        vm.getSelectedAccounts = getSelectedAccounts;
        vm.isAllowManualConfirm = isAllowManualConfirm;
        vm.releaseParticipantAddedEventHandler = releaseParticipantAddedEventHandler;
        vm.releaseParticipantRemovedEventHandler = releaseParticipantRemovedEventHandler;
        vm.updateParticipant = updateParticipant;

        vm.refreshList = refreshList;

        common.handleEvent('release.ReleaseWindowLoadedEvent', vm.releaseWindowLoadedEventHandler, $scope);
        common.handleEvent(config.events.notificationReceived, vm.serverNotificationHandler, $scope);

        $scope.$on('$destroy', scopeDestroyHandler);

        activate();

        function activate() {
            common.activateController([], controllerId, $scope)
                .then(function () { logger.console('Activated Release Participant Widget'); });
        }

        function addReleaseParticipants(accounts) {
            if (!accounts || accounts.length == 0) {
                logger.console('No participants were selected.');
                return $q.when();
            }

            vm.state.isBusy = true;

            var participantsToAdd = [];
            for (var i = 0; i < accounts.length; i++) {
                participantsToAdd.push({
                    'Account': accounts[i],
                    'ReleaseWindowId': vm.releaseWindowId,
                    'ReleaseParticipantId': newGuid()
                });
            }

            var commandId = newGuid();
            return remiapi
                .executeCommand('AddReleaseParticipantCommand', commandId, { Participants: participantsToAdd })
                .then(function () {
                    Enumerable.From(participantsToAdd)
                        .ForEach(function (item) {
                            var participant = {
                                'Account': angular.copy(item.Account),
                                'ReleaseParticipantId': item.ReleaseParticipantId,
                                'ReleaseWindowId': item.releaseWindowId,
                                'IsParticipationConfirmed': item.IsParticipationConfirmed
                            };

                            vm.updateParticipant(participant);
                        });
                })
                .catch(function (fault) {

                    console.log('error', fault);
                    logger.error('Can\'t add release participant');
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function getSelectedAccounts() {
            var result = [];

            vm.releaseParticipants.forEach(function (item) {
                result.push(item.Account);
            });

            return result;
        }

        function removeReleaseParticipant(participant) {
            vm.state.isBusy = true;

            var commandId = newGuid();

            return remiapi
                .executeCommand(
                    'RemoveReleaseParticipantCommand',
                    commandId,
                    {
                        Participant: {
                            ReleaseWindowId: vm.releaseWindowId,
                            Account: {
                                Role: participant.Account.Role,
                                FullName: participant.Account.FullName,
                                Email: participant.Account.Email
                            },
                            ReleaseParticipantId: participant.ReleaseParticipantId
                        }
                    })
                .then(function () {
                    releaseParticipantRemovedEventHandler({ Participant: participant });
                }, function (error) {
                    logger.error('Cannot remove release participant');
                    logger.console(error);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        };

        function refreshList() {
            if (!vm.state.bindedToReleaseWindow)
                return $q.when();

            return vm.getReleaseParticipants(vm.releaseWindowId);
        }

        function isAllowManualConfirm(participant) {
            return participant.Account.Email == authService.identity.email
                && !participant.IsParticipationConfirmed;
        }

        function getReleaseParticipants(releaseWindowId) {
            vm.state.isBusy = true;

            vm.releaseParticipants = [];

            return remiapi
                .releaseParticipants(releaseWindowId)
                .then(function (response) {
                    vm.releaseParticipants = response.Participants;

                    for (var counter = 0; counter < vm.releaseParticipants.length; counter++) {
                        vm.releaseParticipants[counter].manualConfirmationAllowed = vm.isAllowManualConfirm(vm.releaseParticipants[counter]);
                    }
                }, function (response) {
                    logger.error('Cannot get release participants');
                    logger.console(response);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function releaseWindowLoadedEventHandler(releaseWindow) {
            if (releaseWindow) {
                vm.releaseWindowId = releaseWindow.ExternalId;
                vm.product = releaseWindow.Product;
                vm.state.bindedToReleaseWindow = true;

                vm.getReleaseParticipants(vm.releaseWindowId);

                logger.console('Binded to release window ' + releaseWindow.ExternalId);
                notifications.subscribe('ReleaseParticipationConfirmedEvent', { 'ReleaseWindowId': vm.releaseWindowId });
                notifications.subscribe('ReleaseParticipantsAddedEvent', { 'ReleaseWindowId': vm.releaseWindowId });
                notifications.subscribe('ReleaseParticipantRemovedEvent', { 'ReleaseWindowId': vm.releaseWindowId });
            } else {
                vm.releaseWindowId = '';
                vm.state.bindedToReleaseWindow = false;

                vm.releaseParticipants = [];

                logger.console('Unbind release window');
                notifications.unsubscribe('ReleaseParticipationConfirmedEvent');
                notifications.unsubscribe('ReleaseParticipantsAddedEvent');
                notifications.unsubscribe('ReleaseParticipantRemovedEvent');
            }
        }

        function confirmParticipation(participant) {
            vm.state.isBusy = true;

            var commandId = newGuid();
            return remiapi.executeCommand('ApproveReleaseParticipationCommand',
                    commandId, { "ReleaseParticipantId": participant.ReleaseParticipantId })
                .then(function () {
                    logger.info('Your participation was successfully confirmed');
                }, function (error) {
                    logger.error('Your participation was not confirmed');
                    logger.console(error);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function serverNotificationHandler(notification) {
            if (notification.name == 'ReleaseParticipationConfirmedEvent') {
                releaseParticipationConfirmedHandler(notification.data);
            }

            if (notification.name == 'ReleaseParticipantsAddedEvent') {
                vm.releaseParticipantAddedEventHandler(notification.data);
            }

            if (notification.name == 'ReleaseParticipantRemovedEvent') {
                vm.releaseParticipantRemovedEventHandler(notification.data);
            }
        }

        function releaseParticipationConfirmedHandler(data) {
            for (var i = 0; i < vm.releaseParticipants.length; i++) {
                if (vm.releaseParticipants[i].ReleaseParticipantId == data.ReleaseParticipantId) {
                    vm.releaseParticipants[i].IsParticipationConfirmed = true;
                    vm.releaseParticipants[i].manualConfirmationAllowed = vm.isAllowManualConfirm(vm.releaseParticipants[i]);
                    break;
                }
            }
        }

        function releaseParticipantAddedEventHandler(data) {
            data.Participants.forEach(function (item) {
                var participant = {
                    'Account': angular.copy(item.Account),
                    'ReleaseParticipantId': item.ReleaseParticipantId,
                    'ReleaseWindowId': item.releaseWindowId,
                    'IsParticipationConfirmed': item.IsParticipationConfirmed
                };

                participant.manualConfirmationAllowed = vm.isAllowManualConfirm(participant);

                if (participant.Account.Role.Name == 'BasicUser') {
                    participant.Account.Role.Name = 'TeamMember';
                    participant.Account.Role.Description = 'Team member';
                }

                vm.updateParticipant(participant);
            });
        }

        function releaseParticipantRemovedEventHandler(data) {
            var found = Enumerable.From(vm.releaseParticipants)
                .FirstOrDefault(null, function (x) { return x.ReleaseParticipantId == data.Participant.ReleaseParticipantId; });
            if (found)
                vm.updateParticipant(found, true);
        }

        function scopeDestroyHandler() {
            notifications.unsubscribe('ReleaseParticipationConfirmedEvent');
            notifications.unsubscribe('ReleaseParticipantsAddedEvent');
            notifications.unsubscribe('ReleaseParticipantRemovedEvent');
        }

        function updateParticipant(participant, isRemove) {
            if (!participant || !participant.Account) return;

            var found = Enumerable.From(vm.releaseParticipants)
                .FirstOrDefault(null, function (x) { return x.Account.ExternalId == participant.Account.ExternalId; });

            if (found) {
                var idx = vm.releaseParticipants.indexOf(found);
                if (isRemove) {
                    vm.releaseParticipants.splice(idx, 1);
                } else {
                    vm.releaseParticipants[idx] = participant;
                }
            } else if (!isRemove) {
                vm.releaseParticipants.push(participant);
            }
        }
    }
})();
