(function () {
    "use strict";

    var controllerId = "releaseJobs";
    angular.module("app").controller(controllerId, ["remiapi", "common", "$scope", releaseJobs]);

    function releaseJobs(remiapi, common, $scope) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;
        vm.state = {
            isBusy: false,
            bindedToReleaseWindow: false
        };

        vm.parent = common.getParentScope($scope, function (sc) {
            return sc && sc.vm && sc.vm.controllerId === "release";
        }).vm;

        vm.isAllowManage = false;
        vm.releaseWindowId = null;
        vm.releaseJobs = [];

        vm.refreshJobs = refreshJobs;
        vm.getReleaseJobs = getReleaseJobs;
        vm.updateJob = updateJob;

        vm.releaseWindowLoadedEventHandler = releaseWindowLoadedEventHandler;
        common.handleEvent("release.ReleaseWindowLoadedEvent", vm.releaseWindowLoadedEventHandler, $scope);

        activate();

        function activate() {
            common.activateController([releaseWindowLoadedEventHandler(vm.parent.currentReleaseWindow)], controllerId, $scope)
                .then(function () { logger.console("Activated Release Jobs view"); });
        }

        function updateJob(job) {
            if (!job) {
                logger.warn("Job not selected");
                return null;
            }

            vm.state.isBusy = true;

            return remiapi.post.updateReleaseJob({ ReleaseJob: job, ReleaseWindowId: vm.releaseWindowId })
                .then(function () {

                }, function () {
                    job.IsIncluded = !job.IsIncluded;
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function refreshJobs() {
            return getReleaseJobs();
        }

        function getReleaseJobs() {
            if (!vm.releaseWindowId)
                return null;

            vm.state.isBusy = true;

            return remiapi.get.getReleaseJobs(vm.releaseWindowId)
                .then(function (data) {
                    vm.releaseJobs = data.ReleaseJobs;
                }, function (error) {
                    logger.error("Cannot get release jobs");
                    logger.error(error);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function releaseWindowLoadedEventHandler(releaseWindow) {
            if (releaseWindow) {
                vm.releaseWindowId = releaseWindow.ExternalId;
                vm.state.bindedToReleaseWindow = true;
                vm.isAllowManage = !releaseWindow.ClosedOn && !releaseWindow.SignedOff;

                vm.getReleaseJobs();

                logger.console("Binded to release window " + releaseWindow.ExternalId);
            } else {
                vm.releaseWindowId = "";
                vm.state.bindedToReleaseWindow = false;
                vm.releaseJobs = [];
                vm.isAllowManage = false;

                logger.console("Unbind release window ");
            }
        }
    }
})()
