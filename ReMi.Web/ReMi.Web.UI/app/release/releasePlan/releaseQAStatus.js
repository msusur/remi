(function () {
    'use strict';
    var controllerId = 'releaseQaStatus';
    angular.module('app').controller(controllerId, ['remiapi', 'common', '$rootScope', '$scope', releaseQaStatus]);

    function releaseQaStatus(remiapi, common, $rootScope, $scope) {
        
        var logger = common.logger.getLogger(controllerId);
        var self = this;

        self.state = {
            isBusy: false,
            bindedToReleaseWindow: false,
            visible: false
        };
        self.parent = common.getParentScope($scope, function (sc) {
            return sc && sc.vm && sc.vm.controllerId === "release";
        }).vm;

        self.qaChecks = undefined;
        self.checkStatus = checkStatus;
        common.handleEvent('release.ReleaseWindowLoadedEvent', releaseWindowLoadedEventHandler, $scope);

        activate();

        function activate() {
            common.activateController([releaseWindowLoadedEventHandler(self.parent.currentReleaseWindow)], controllerId, $scope);
        }

        

        function checkStatus() {
            if (!self.releaseWindow) return;

            self.state.isBusy = true;
            remiapi.get.qaStatus(self.releaseWindow.Products[0])
                .then(function (data) {
                    self.qaChecks = data.Content;
                },
                function(error) {
                    logger.error('Cannot retrieve QA Status');
                    logger.console(error);
                }
                )
            .finally(function () {
                self.state.isBusy = false;
            });
            
        }

        function releaseWindowLoadedEventHandler(releaseWindow) {
            self.releaseWindow = releaseWindow;
            if (releaseWindow) {
                self.state.bindedToReleaseWindow = true;
                self.checkStatus();
                self.state.visible = true;
                logger.console('Bound to release window ' + releaseWindow.ExternalId);

            } else {
                self.releaseWindowId = '';
                self.state.bindedToReleaseWindow = false;
                self.qaChecks = [];
                logger.console('Not bound to release window ');
            }
        }

        

    }
})()
