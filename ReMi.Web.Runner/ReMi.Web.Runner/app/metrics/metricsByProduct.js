(function () {
    'use strict';

    var controllerId = 'metricsByProduct';

    angular.module('app').controller(controllerId, ['$scope', '$location', 'common', 'config', 'authService', 'notifications', 'remiapi', metricsByProduct]);

    function metricsByProduct($scope, $location, common, config, authService, notifications, remiapi) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;
        vm.state = {
            isBusy: false
        };

        
        vm.product = authService.identity.product;

        // data for charts
        vm.measurements = undefined;
        vm.scheduledReleaseMeasurements = undefined;

        vm.getMeasurements = getMeasurements;
        vm.defaultMeasurements = defaultMeasurements;
        vm.productContextChanged = productContextChanged;

        common.handleEvent(config.events.productContextChanged, vm.productContextChanged, $scope);

        vm.charts = {
            scheduledRelease: ['Down time', 'Deploy time'],
            automatedReleases: ['Deploy time'],
            overallTimes: ['Overall time'],
            deployJobTiming: []
        };
        vm.overallTimeMeasurements = null;
        vm.scheduledReleaseMeasurements = null;
        vm.automatedReleaseMeasurements = null;
        vm.deployJobTimingMeasurements = null;

        activate();

        function activate() {
            common.activateController([vm.defaultMeasurements()], controllerId)
                .then(function () { logger.console('Activated Product Metrics View'); });
        }

        function getMeasurements(product) {
            vm.scheduledReleaseMeasurements = undefined;
            vm.deployJobTimingMeasurements = undefined;

            vm.state.isBusy = true;

            return remiapi.get
                .getMeasurements(product)
                .then(function (data) {
                    vm.overallTimeMeasurements = data.Measurements;
                    
                    if (!vm.product || vm.product.ReleaseTrack != 'Automated') {
                        for (var c = 0; c < data.Measurements.length; c++) {
                            for (var i = 0; i < 3; i++) {
                                if (data.Measurements[c].Metrics[i] == null) {
                                    data.Measurements[c].Metrics[i] = {
                                        Type: 'DownTime',
                                        Value: 0,
                                        Name: 'Down time'
                                    };
                                    break;
                                }
                            }
                        }

                        vm.scheduledReleaseMeasurements = data.Measurements.
                            filter(function (x) { return x.ReleaseWindow.ReleaseType == 'Scheduled'; });
                        
                        vm.automatedReleaseMeasurements = [];
                    } else {
                        vm.automatedReleaseMeasurements = data.Measurements.
                            filter(function (x) { return x.ReleaseWindow.ReleaseType == 'Automated'; });

                        vm.scheduledReleaseMeasurements = [];
                    }
                }, function (error) {
                    logger.error('Cannot get measurements');
                    console.log(error);
                })
                .then(function () { return remiapi.get.getDeploymentJobsMeasurementsByProduct(product); })
                .then(function (data) {
                    if (data && data.Measurements)
                        vm.deployJobTimingMeasurements = data.Measurements;
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function defaultMeasurements() {
            if (authService.isLoggedIn) {
                if (!vm.scheduledReleaseMeasurements) {
                    vm.getMeasurements(authService.identity.product.Name);
                }
            }
        }

        function productContextChanged(product) {
            vm.charts.deployJobTiming = [];
            if (authService.isLoggedIn) {
                vm.product = product;
                vm.getMeasurements(product.Name);
            }
        }
    }
})()
