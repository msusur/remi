(function () {
    "use strict";

    var controllerId = "deploymentJobsMeasurements";
    angular.module("app").controller(controllerId, deploymentJobsMeasurements);

    function deploymentJobsMeasurements(remiapi, common, $scope, localData, $filter, notifications, config) {
        var logger = common.logger.getLogger(controllerId);
        var deploymentMeasurementsPopulatedEvent = "DeploymentMeasurementsPopulatedEvent";

        var vm = this;
        vm.state = {
            isBusy: false,
            isModalBusy: false,
            bindedToReleaseWindow: false
        };
        vm.parent = common.getParentScope($scope, function (sc) {
            return sc && sc.vm && sc.vm.controllerId === "release";
        }).vm;

        vm.isVisible = false;
        vm.releaseWindowId = null;
        vm.releaseWindowType = "";
        vm.goServerUrl = "";
        vm.measurements = [];
        vm.measurementColumns = [
            { field: "StepName", style: { width: "50%" }, formatCallback: stepCallback },
            { field: "StartTime", caption: "Start time", formatCallback: dateTimeFormatCallback, style: { width: "10%", 'text-align': "right" } },
            { field: "FinishTime", caption: "Finish time", formatCallback: timeFormatCallback, style: { width: "10%", 'text-align': "right" } },
            { field: "DurationLabel", caption: "Duration", style: { width: "10%", 'text-align': "right" } },
            {
                field: "JobStage",
                caption: "Completed",
                style: { width: "10%" },
                filter: $filter("formatenum"),
                filterDataGetter: function () { return vm.jobStage; }
            },
            { field: "BuildNumber", style: { width: "5%", 'text-align': "right" }, caption: "Build" },
            { field: "NumberOfTries", style: { width: "5%", 'text-align': "right" }, caption: "Tries" }
        ];

        vm.hideModal = hideModal;

        vm.refreshMeasurements = refreshMeasurements;
        vm.getDeploymentJobsMeasurements = getDeploymentJobsMeasurements;
        vm.rePopulateMeasurements = rePopulateMeasurements;

        vm.releaseWindowLoadedEventHandler = releaseWindowLoadedEventHandler;
        common.handleEvent("release.ReleaseWindowLoadedEvent", vm.releaseWindowLoadedEventHandler, $scope);
        common.handleEvent(config.events.notificationReceived, serverNotificationHandler, $scope);
        $scope.$on("$destroy", scopeDestroyHandler);

        activate();

        function activate() {
            common.activateController([init()], controllerId, $scope)
                .then(function () { logger.console("Activated Deploy Jobs Measurements View"); });
        }

        function scopeDestroyHandler() {
            notifications.unsubscribe(deploymentMeasurementsPopulatedEvent);
        }

        function init() {
            return localData.getEnum("JobStage")
                .then(function (jobStage) { vm.jobStage = jobStage; })
                .then(function () { releaseWindowLoadedEventHandler(vm.parent.currentReleaseWindow); });
        }

        function timeFormatCallback(td) {
            var m = moment(td.text());
            if (m.isValid()) {
                td.text(m.format("H:mm:ss"));
            }
        }

        function dateTimeFormatCallback(td) {
            var m = moment(td.text());
            if (m.isValid()) {
                td.text(m.format("DD MMM  H:mm:ss"));
            }
        }

        function stepCallback(td, value, rowData) {
            var aDetails = $("<a href=\"\" >" + td.text() + "</a>");
            aDetails.css("display", "inline-block");

            aDetails.on("click", function (e) {
                e.preventDefault();

                vm.currentStep = angular.copy(rowData);
                var mStart = moment(vm.currentStep.StartTime);
                vm.currentStep.StartTime = mStart.format("DD MMM H:mm:ss");
                var mFinish = moment(vm.currentStep.FinishTime);
                vm.currentStep.FinishTime = mFinish.format("DD MMM H:mm:ss");

                $scope.$apply(function () {
                    $("#stepDetailsModal").modal("show");
                });
            });

            var span = $("<span></span>");
            span.append(aDetails);
            td.empty().append(span);

            if (rowData.Locator) {
                var aGo = $("<a href=\"" + rowData.Locator + "\" target=\"_blank\"><i class=\"fa fa-external-link\" style=\"padding-left: 5px\"></i></a>");
                td.append(aGo);
            }
        }

        function hideModal() {
            $("#stepDetailsModal").modal("hide");
        }

        function refreshMeasurements() {
            return getDeploymentJobsMeasurements();
        }

        function getDeploymentJobsMeasurements() {
            if (!vm.releaseWindowId)
                return null;

            vm.state.isBusy = true;
            return remiapi.get.getDeploymentJobsMeasurements(vm.releaseWindowId)
                .then(function (data) {
                    vm.measurements = data.Measurements;
                    vm.goServerUrl = data.GoServerUrl;
                }, function (error) {
                    logger.error("Can't get deploy jobs measurements");
                    logger.error(error);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function releaseWindowLoadedEventHandler(releaseWindow) {
            vm.deploymentJobsMeasurements = [];

            if (releaseWindow) {
                if (!!vm.releaseWindow) {
                    notifications.unsubscribe(deploymentMeasurementsPopulatedEvent);
                }
                vm.releaseWindowId = releaseWindow.ExternalId;
                vm.releaseWindowType = releaseWindow.ReleaseType;

                vm.state.bindedToReleaseWindow = true;
                vm.isVisible = !!releaseWindow.SignedOff;

                if (vm.isVisible)
                    vm.getDeploymentJobsMeasurements();

                logger.console("Binded to release window " + releaseWindow.ExternalId);
                notifications.subscribe(deploymentMeasurementsPopulatedEvent, { 'ReleaseWindowId': releaseWindow.ExternalId });
            } else {
                vm.releaseWindowId = "";
                vm.releaseWindowType = "";

                vm.state.bindedToReleaseWindow = false;
                vm.isVisible = false;

                notifications.unsubscribe(deploymentMeasurementsPopulatedEvent);
                logger.console("Unbind release window ");
            }
        }

        function rePopulateMeasurements() {
            if (!vm.releaseWindowId)
                return null;

            vm.state.isBusy = true;
            return remiapi.post.rePopulateMeasurements({
                ReleaseWindowId: vm.releaseWindowId
            })
                .then(function () {
                    vm.refreshMeasurements();
                }, function (error) {
                    logger.error("Can't get deploy jobs measurements");
                    logger.error(error);
                    vm.state.isBusy = false;
                });
        }

        function serverNotificationHandler(notification) {
            if (notification.name === deploymentMeasurementsPopulatedEvent) {
                vm.isVisible = true;
                vm.getDeploymentJobsMeasurements();
            }
        }
    }
})()
