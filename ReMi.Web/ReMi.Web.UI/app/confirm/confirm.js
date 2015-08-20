(function () {
    'use strict';

    var controllerId = 'confirm';

    angular.module('app')
        .controller(controllerId, ['$location', '$route', '$q', 'remiapi', 'common', 'authService', confirm]);

    function confirm($location, $route, $q, remiapi, common, authService) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;
        vm.params = $route.current.params;

        approve();

        function approve() {
            var params = getConfirmParameters();

            if (!params) {
                leavePage();
                return null;
            }

            var commandId = newGuid();
            return remiapi
                .executeCommand(params.CommandName, commandId, params.Data)
                .then(function () {
                    logger.success('The acknowledgment has been successfully handled');
                }, function () {
                    logger.error('The acknowledgment has been failed.');
                }).finally(function () {
                    leavePage();
                });
        }

        // Method implement confirmation logic according to input parameters. 
        // It maps input parameters to name of api command that would be executed withing confirmation.
        function getConfirmParameters() {
            var params = vm.params;

            if (params.releaseTaskReviewerUpdate) {
                return buildResponse('ConfirmReleaseTaskReviewCommand', 'ReleaseTaskId', params.releaseTaskReviewerUpdate);
            } else if (params.releaseTaskImplementorUpdate) {
                return buildResponse('ConfirmReleaseTaskImplementationCommand', 'ReleaseTaskId', params.releaseTaskImplementorUpdate);
            } else if (params.releaseTaskUpdateAcknowledge) {
                return buildResponse('ConfirmReleaseTaskReceiptCommand', 'ReleaseTaskId', params.releaseTaskUpdateAcknowledge);
            }

            logger.error('The acknowledgment is not configured');

            return null;
        }

        function buildResponse(apiCommand, apiParameterName, parameterValue) {
            var getResponse = function (resultValue) {
                var result = { CommandName: apiCommand, Data: {} };
                result.Data[apiParameterName] = resultValue;
                return result;
            };

            if (!!parameterValue && validateGuid(parameterValue)) {
                return getResponse(parameterValue);
            }

            logger.error("Incorrect url format");

            return null;
        }

        function validateGuid(code) {
            return /^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$/
                .test(code);
        }

        function leavePage() {
            var releaseWindowId = vm.params.releaseWindowId;

            if (authService.isLoggedIn) {
                if (releaseWindowId) {
                    $location.path('/releasePlan').search({ 'releaseWindowId': releaseWindowId });
                } else {
                    $location.path('/').search({});
                }
            } else {
                $location.path('/login').search({});
            }
        }
    }
})();
