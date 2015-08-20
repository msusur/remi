(function () {
    'use strict';

    var controllerId = 'signOff';

    angular.module('app').controller(controllerId, ['$scope', 'common', 'config', 'authService', 'notifications', 'remiapi', signOff]);

    function signOff($scope, common, config, authService, notifications, remiapi) {
        var logger = common.logger.getLogger(controllerId);
        var releaseStatusChangedEventName = 'ReleaseStatusChangedEvent';
        var releaseSignersAddedEvent = 'ReleaseSignersAddedEvent';
        var releaseSignedOffEvent = 'ReleaseSignedOffBySignerEvent';
        var removeSignOffEvent = 'RemoveSignOffEvent';

        var vm = this;
        vm.state = {
            isBusy: false,
            isLoaded: false,
            bindedToReleaseWindow: false
        };
        vm.signers = [];
        var releaseProcessParticipantEvent = 'releaseProcess.isParticipantEvent';
        var isReleasePlanView = undefined;

        vm.serverNotificationHandler = serverNotificationHandler;
        vm.loggedInHandler = loggedInHandler;
        vm.sign = sign;
        vm.addSigners = addSigners;
        vm.removeSigner = removeSigner;
        vm.getSelectedAccounts = getSelectedAccounts;
        vm.getSigners = getSigners;
        vm.releaseWindowLoadedEventHandler = releaseWindowLoadedEventHandler;
        vm.signerAddedEventHandler = signerAddedEventHandler;
        vm.signerRemovedEventHandler = signerRemovedEventHandler;
        vm.releaseSignedEventHandler = releaseSignedEventHandler;
        vm.releaseSignedOffEventHandler = releaseSignedOffEventHandler;
        vm.init = init;
        vm.updateAllowManageSignOff = updateAllowManageSignOff;
        vm.allowSign = allowSign;
        vm.closeSigningDescriptionModal = closeSigningDescriptionModal;
        vm.showSigningDescriptionModal = showSigningDescriptionModal;
        vm.beforeShowSignatureForm = beforeShowSignatureForm;
        vm.updateSignoff = updateSignoff;
        vm.refreshList = refreshList;

        common.handleEvent(config.events.notificationReceived, vm.serverNotificationHandler, $scope);
        common.handleEvent(config.events.loggedIn, vm.loggedInHandler, $scope);
        common.handleEvent('release.ReleaseWindowLoadedEvent', vm.releaseWindowLoadedEventHandler, $scope);
        common.handleEvent('signOff.SignerAddedEvent', vm.signerAddedEventHandler, $scope);
        common.handleEvent('signOff.SignerRemovedEvent', vm.signerRemovedEventHandler, $scope);
        common.handleEvent('signOff.ReleaseSignedEvent', vm.releaseSignedEventHandler, $scope);

        vm.removeSignOffEventEventHandler = removeSignOffEventEventHandler;
        vm.releaseSignersAddedEventHandler = releaseSignersAddedEventHandler;

        $scope.$on('$destroy', scopeDestroyHandler);

        activate();

        function activate() {
            common.activateController([], controllerId, $scope)
                .then(function () { logger.console('Activated Release Sign Offs View'); });
        }

        function init(parameter) {
            isReleasePlanView = parameter;
        }

        function refreshList() {
            vm.getSigners(vm.releaseWindow.ExternalId);
        }

        function releaseWindowLoadedEventHandler(releaseWindow) {
            if (releaseWindow) {
                if (!!vm.releaseWindow) {
                    notifications.unsubscribe(releaseStatusChangedEventName);
                    notifications.unsubscribe(releaseSignersAddedEvent);
                    notifications.unsubscribe(releaseSignedOffEvent);
                    notifications.unsubscribe(removeSignOffEvent);
                }

                vm.releaseWindow = angular.copy(releaseWindow);
                vm.state.bindedToReleaseWindow = true;

                vm.isReleaseClosed = !!vm.releaseWindow.ClosedOn;
                vm.updateAllowManageSignOff();

                vm.getSigners(vm.releaseWindow.ExternalId);

                logger.console('Binded to release window ' + vm.releaseWindow.ExternalId);

                notifications.subscribe(releaseStatusChangedEventName, { 'ReleaseWindowId': vm.releaseWindow.ExternalId });
                notifications.subscribe(releaseSignersAddedEvent, { 'ReleaseWindowId': vm.releaseWindow.ExternalId });
                notifications.subscribe(releaseSignedOffEvent, { 'ReleaseWindowId': vm.releaseWindow.ExternalId });
                notifications.subscribe(removeSignOffEvent, { 'ReleaseWindowId': vm.releaseWindow.ExternalId });
            } else {
                vm.releaseWindow = null;
                vm.state.bindedToReleaseWindow = false;
                vm.signers = [];

                logger.console('Unbind from release window ');

                notifications.unsubscribe(releaseStatusChangedEventName);
                notifications.unsubscribe(releaseSignersAddedEvent);
                notifications.unsubscribe(releaseSignedOffEvent);
                notifications.unsubscribe(removeSignOffEvent);
            }
        }

        function beforeShowSignatureForm(defer) {
            if (defer == null) {
                return;
            }
            vm.state.isBusy = true;
            var parameters = {
                signOffs: JSON.stringify(vm.signers),
                releaseWindow: JSON.stringify(vm.releaseWindow)
            };
            remiapi.executeBusinessRule('Release', 'AllowCloseAfterSignOffRule', parameters)
                .then(function (result) {
                    if (result && result.Result == true) {
                        defer.reject(); // don't show form and close release
                        common.sendEvent(config.events.closeReleaseOnSignOffEvent);
                    }
                    else {
                        defer.resolve(); //means show form for regular sign off
                    }
                }, function () {
                    defer.resolve(); //means show form for regular sign off
                })
                .finally(function () { vm.state.isBusy = false; });
        }

        function sign(signature, p) {
            if (!signature || !signature.deferred || !signature.userName || !signature.password) {
                logger.warn('Please provide credentials');
                return null;
            }
            if (!vm.releaseWindow) {
                logger.warn('Release window not selected');
                signature.deferred.reject();
                return null;
            }
            if (!p || !p.Signer || !p.Signer.ExternalId) {
                signature.deferred.reject();
                return null;
            }

            vm.state.isBusy = true;

            return remiapi
                .signOffRelease({
                    ReleaseWindowId: vm.releaseWindow.ExternalId,
                    AccountId: p.Signer.ExternalId,
                    UserName: signature.userName,
                    Password: signature.password,
                    Comment: signature.Comment
                })
                .then(function () {
                    common.sendEvent('signOff.ReleaseSignedEvent', { AccountId: p.Signer.ExternalId });

                    signature.deferred.resolve();
                }, function (error) {
                    signature.deferred.reject();
                    logger.error('Cannot sign off the release');
                    logger.console(error);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function releaseSignedEventHandler(data) {
            vm.releaseSignedOffEventHandler(data);
        }

        function getSigners() {
            vm.state.isBusy = true;

            vm.signers = [];

            return remiapi
                .getSignOffs(vm.releaseWindow.ExternalId)
                .then(function (data) {
                    vm.signers = [];

                    data.SignOffs.forEach(function (s) {
                        var item = s;
                        item.AllowSign = vm.allowSign(item);

                        vm.signers.push(item);
                    });

                    if (isReleasePlanView) {
                        if (vm.signers.filter(function (x) {
                            return x.Signer.Email == authService.identity.email;
                        }).length > 0) {
                            common.sendEvent(releaseProcessParticipantEvent);
                        }
                    }
                }, function (error) {
                    logger.error('Cannot get sign offs');
                    logger.console(error);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function addSigners(accounts) {
            vm.state.isBusy = true;
            var signers = [];
            accounts.forEach(function (acc) { signers.push({ Signer: acc, ExternalId: newGuid() }); });

            return remiapi
                .addSignOffs({ 'SignOffs': signers, 'ReleaseWindowId': vm.releaseWindow.ExternalId })
                .then(function () {
                    Enumerable.From(signers)
                        .ForEach(function (item) {
                            common.sendEvent('signOff.SignerAddedEvent', { SignOff: item });
                        });
                }, function (error) {
                    logger.error('Cannot add release sign off');
                    logger.console(error);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function removeSigner(signer) {
            if (vm.signers.length == 1 && vm.signers[0].ExternalId == signer.ExternalId) {
                logger.warn('Release must have at least one user to sign off');
                return null;
            }
            vm.state.isBusy = true;

            return remiapi
                .removeSignOff({ 'AccountId': signer.Signer.ExternalId, 'SignOffId': signer.ExternalId, 'ReleaseWindowId': vm.releaseWindow.ExternalId })
                .then(function () {
                    common.sendEvent('signOff.SignerRemovedEvent', { SignOff: signer });

                }, function (error) {
                    logger.error('Cannot remove release sign off');
                    logger.console(error);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function getSelectedAccounts() {
            var result = [];

            vm.signers.forEach(function (item) {
                result.push(item.Signer);
            });

            return result;
        }

        function scopeDestroyHandler() {
            notifications.unsubscribe(releaseStatusChangedEventName);
            notifications.unsubscribe(releaseSignersAddedEvent);
            notifications.unsubscribe(releaseSignedOffEvent);
            notifications.unsubscribe(removeSignOffEvent);
        }

        function serverNotificationHandler(notification) {
            if (notification.name == releaseStatusChangedEventName) {
                vm.releaseWindow.Status = notification.data.ReleaseStatus;
                vm.updateAllowManageSignOff();
                vm.signers.forEach(function (s) {
                    s.AllowSign = vm.allowSign(s);
                });
                if (vm.releaseWindow.Status == "Closed") {
                    vm.getSigners(vm.releaseWindow.ExternalId);
                }
            }

            if (notification.name == releaseSignersAddedEvent) {
                vm.releaseSignersAddedEventHandler(notification.data);
            }

            if (notification.name == releaseSignedOffEvent) {
                vm.releaseSignedOffEventHandler(notification.data);
            }

            if (notification.name == removeSignOffEvent) {
                vm.removeSignOffEventEventHandler(notification.data);
            }
        }

        function releaseSignersAddedEventHandler(data) {
            data.SignOffs.forEach(function (item) {
                item.AllowSign = vm.allowSign(item);

                vm.updateSignoff(item);
            });
        }

        function removeSignOffEventEventHandler(data) {
            var found = Enumerable.From(vm.signers)
                .FirstOrDefault(null, function (x) { return x.Signer.ExternalId == data.AccountId; });
            if (found)
                vm.updateSignoff(found, true);
        }

        function signerAddedEventHandler(data) {
            vm.updateSignoff(data.SignOff);
        }

        function signerRemovedEventHandler(data) {
            vm.updateSignoff(data.SignOff, true);
        }

        function releaseSignedOffEventHandler(data) {
            for (var counter = 0; counter < vm.signers.length; counter++) {
                if (data.AccountId == vm.signers[counter].Signer.ExternalId) {
                    vm.signers[counter].SignedOff = true;
                    vm.signers[counter].Comment = data.Comment;
                    break;
                }
            }
        }

        function loggedInHandler() {
            vm.updateAllowManageSignOff();
        }

        function updateAllowManageSignOff() {
            vm.allowManageSignOff = authService.isLoggedIn && !(vm.releaseWindow && (vm.releaseWindow.Status == 'Signed off' || vm.releaseWindow.Status == 'Closed'));
        };

        function allowSign(signer) {
            if (!signer || !!signer.SignedOff)
                return false;

            return (vm.releaseWindow && vm.releaseWindow.Status == 'Approved');
        }

        function closeSigningDescriptionModal() {
            $('#signingDescriptionModal').modal('hide');
        }

        function showSigningDescriptionModal(signer) {
            vm.signingDescription = signer.Comment;
            $('#signingDescriptionModal').modal('show');
        }

        function updateSignoff(signer, isRemove) {
            if (!signer || !signer.Signer) return;

            var found = Enumerable.From(vm.signers)
                .FirstOrDefault(null, function (x) { return x.Signer.ExternalId == signer.Signer.ExternalId; });

            if (found) {
                var idx = vm.signers.indexOf(found);
                if (isRemove) {
                    vm.signers.splice(idx, 1);
                } else {
                    vm.signers[idx] = signer;
                }
            } else if (!isRemove) {
                vm.signers.push(signer);
            }
        }

    }
})()
