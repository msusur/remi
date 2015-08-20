(function () {
    'use strict';
    var controllerId = 'acknowledge';
    angular.module('app').controller(controllerId, ['remiapi', 'common', '$location', 'authService', acknowledge]);

    function acknowledge(remiapi, common, $location, authService) {
        var logger = {
            info: function (message) { common.logger.getLogFn(controllerId)(message); },
            warn: function (message) { common.logger.getLogFn(controllerId, 'warn')(message); },
            error: function (message) { common.logger.getLogFn(controllerId, 'error')(message); },
            console: function (message) { common.logger.getLogFn(controllerId, 'console')(message); },
            success: function (message) { common.logger.getLogFn(controllerId, 'success')(message); },
        };

        approve(logger, remiapi, $location, authService);
    }

    function approve(logger, remiapi, $location, authService) {
        var participantGuid = $location.search().approve;
        var guidRegex = /^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$/;
        if (!guidRegex.test(participantGuid)) {
            logger.error("Incorrect url");
            return;
        }
        var commandId = newGuid();
        remiapi.executeCommand('ApproveReleaseParticipationCommand', commandId, { "ReleaseParticipantId": participantGuid })
            .then(function () {
                redirect(authService, $location);
                logger.success('Your acknowledge was successfully handled');
            }, function (error) {
                redirect(authService, $location);
                logger.error('Your acknowledge was not handled. ' + error);
            });
    }

    function redirect(authService, $location) {
        var email = authService.identity.email;
        $location.search('approve', null);
        if (email == '') {
            $location.path('/login');
        } else {
            $location.path('/releasePlan');
        }
    }
})();
