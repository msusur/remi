(function () {
    'use strict';

    var controllerId = 'closeRelease';
    angular.module('app').controller(controllerId, ['remiapi', 'common', '$scope', closeRelease]);

    function closeRelease(remiapi, common, $scope) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;

        vm.hideCurrentCloseReleaseModal = hideCurrentCloseReleaseModal;
        vm.signOffRelease = signOffRelease;
        vm.addAddressee = addAddressee;
        vm.removeAddressee = removeAddressee;
        vm.getSelectedAccounts = getSelectedAccounts;

        vm.teamMembers = [];
        vm.addressees = [];
        vm.state = {};
        vm.state.isBusy = false;
        vm.addresseeSearch = '';
        vm.releaseNotes = '';

        var releaseClosedEvent = 'closeRelease.ReleaseClosedEvent';

        activate();

        function activate() {
            common.activateController([], controllerId, $scope)
                .then(function () { logger.console('Activated Close Release View'); });
        }

        function hideCurrentCloseReleaseModal() {
            $('#closeReleaseModal').modal('hide');
        }

        function signOffRelease(signature, releaseWindowId) {
            if (!signature || !signature.deferred || !signature.userName || !signature.password) {
                logger.warn('Please provide credentials');
                return null;
            }

            vm.state.isBusy = true;
            var notes = vm.releaseNotes;

            return remiapi
                .closeReleaseWindow({
                    Recipients: vm.addressees,
                    ReleaseWindowId: releaseWindowId,
                    UserName: signature.userName,
                    Password: signature.password,
                    ReleaseNotes: notes
                })
                .then(function () {
                    signature.deferred.resolve();
                    common.sendEvent(releaseClosedEvent);
                    $('#closeReleaseModal').modal('hide');
                }, function (fault) {
                    signature.deferred.reject();
                    console.log('error', fault);
                    logger.error('Cannot close the release');
                })
                .finally(function () { vm.state.isBusy = false; });
        }

        function addAddressee(accs) {
            for (var counter = 0; counter < accs.length; counter++) {
                vm.addressees.push(accs[counter]);
            }

            $('#addReleaseNotesAddresseeModal').modal('hide');
        }

        function getSelectedAccounts() {
            var result = vm.addressees;

            return result;
        }

        function removeAddressee(addressee) {
            for (var counter = 0; counter < vm.addressees.length; counter++) {
                if (vm.addressees[counter].ExternalId == addressee.ExternalId) {
                    vm.addressees.splice(counter, 1);
                    break;
                }
            }
        }
    }
})()
