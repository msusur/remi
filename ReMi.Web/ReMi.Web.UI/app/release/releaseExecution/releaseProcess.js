(function () {
    'use strict';

    var controllerId = 'releaseProcess';

    angular.module('app').controller(controllerId, ['$scope', 'common', 'config', 'authService', 'notifications', 'remiapi', '$filter', releaseProcess]);

    function releaseProcess($scope, common, config, authService, notifications, remiapi, $filter) {
        var logger = common.logger.getLogger(controllerId);
        var metricUpdatedEvent = 'MetricsUpdatedEvent';
        var releaseIssuesUpdatedEvent = 'ReleaseIssuesUpdatedEvent';

        var vm = this;
        vm.state = {
            isBusy: false,
            isLoaded: false,
            bindedToReleaseWindow: false,
            display: false
        };
        vm.metrics = undefined;
        vm.allowSiteDown = false;
        vm.allowSiteUp = false;
        vm.allowStartDeploy = false;
        vm.allowFinishDeploy = false;
        var allowManageAsProcessParticipant = false;

        vm.serverNotificationHandler = serverNotificationHandler;
        vm.releaseWindowLoadedEventHandler = releaseWindowLoadedEventHandler;
        vm.getMetrics = getMetrics;
        vm.evaluateMetrics = evaluateMetrics;
        vm.updateMetrics = updateMetrics;
        vm.hideIssuesModal = hideIssuesModal;
        vm.showIssues = showIssues;
        vm.saveIssues = saveIssues;
        vm.allowManaging = allowManaging;
        vm.releaseProcessParticipationHandler = releaseProcessParticipationHandler;


        common.handleEvent(config.events.notificationReceived, vm.serverNotificationHandler, $scope);
        common.handleEvent('release.ReleaseWindowLoadedEvent', vm.releaseWindowLoadedEventHandler, $scope);
        common.handleEvent('releaseProcess.isParticipantEvent', vm.releaseProcessParticipationHandler, $scope);
        $scope.$on('$destroy', scopeDestroyHandler);

        activate();

        function activate() {
            common.activateController([], controllerId, $scope)
                .then(function () { logger.console('Activated Release Passing View'); });
        }

        function releaseProcessParticipationHandler(event) {
            if (!allowManageAsProcessParticipant) {
                allowManageAsProcessParticipant = true;
                vm.evaluateMetrics();
            }
        }

        function releaseWindowLoadedEventHandler(releaseWindow) {
            if (releaseWindow) {
                if (!!vm.releaseWindow) {
                    notifications.unsubscribe(metricUpdatedEvent);
                    notifications.unsubscribe(releaseIssuesUpdatedEvent);
                }

                vm.releaseWindow = angular.copy(releaseWindow);
                vm.state.bindedToReleaseWindow = true;
                vm.isReleaseClosed = !!vm.releaseWindow.ClosedOn;

                vm.getMetrics(vm.releaseWindow.ExternalId);

                logger.console('Binded to release window ' + vm.releaseWindow.ExternalId);

                notifications.subscribe(metricUpdatedEvent, { 'ReleaseWindowId': vm.releaseWindow.ExternalId });
                notifications.subscribe(releaseIssuesUpdatedEvent, { 'ReleaseWindowId': vm.releaseWindow.ExternalId });
            } else {
                vm.releaseWindow = null;
                vm.state.bindedToReleaseWindow = false;

                logger.console('Unbind from release window ');

                notifications.unsubscribe(metricUpdatedEvent);
                notifications.unsubscribe(releaseIssuesUpdatedEvent);
            }
        }

        function getMetrics() {
            vm.state.isBusy = true;

            vm.metrics = undefined;

            return remiapi
                .getMetrics(vm.releaseWindow.ExternalId)
                .then(function (data) {
                    vm.metrics = $filter('orderBy')(data.Metrics, 'Order');
                    vm.state.display = (data.Metrics.length > 0) && authService.isLoggedIn;
                    vm.evaluateMetrics();
                }, function (error) {
                    logger.error('Cannot get metrics');
                    logger.console(error);
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function scopeDestroyHandler() {
            notifications.unsubscribe(metricUpdatedEvent);
            notifications.unsubscribe(releaseIssuesUpdatedEvent);
        }

        function serverNotificationHandler(notification) {
            if (notification.name == metricUpdatedEvent) {
                for (var counter = 0; counter < vm.metrics.length; counter++) {
                    if (vm.metrics[counter].ExternalId == notification.data.Metric.ExternalId) {
                        vm.metrics[counter] = notification.data.Metric;
                        logger.info('Release process: ' + notification.data.Metric.MetricType +
                            ' at ' + $filter('date')(notification.data.Metric.ExecutedOn, 'yyyy-MM-dd HH:mm'));
                        vm.evaluateMetrics();
                    }
                }
            }

            if (notification.name == releaseIssuesUpdatedEvent) {
                if (vm.releaseWindow) {
                    vm.releaseWindow.Issues = notification.data.Issues;
                    vm.issuesBackUp = vm.releaseWindow.Issues;
                    logger.info('Release issues updated');
                }
            }
        }

        function evaluateMetrics() {
            if (vm.metrics) {
                var active = true;
                for (var counter = 0; counter < vm.metrics.length; counter++) {
                    if (vm.metrics[counter].ExecutedOn == null) {
                        vm.metrics[counter].active = active && vm.allowManaging();
                        active = false;
                    } else {
                        vm.metrics[counter].active = false;
                    }
                }
            }
        }

        function updateMetrics(metric) {
            if (vm.metrics.filter(function (item) {
                return item.Order < metric.Order;
            }).length == 0) {
                if (actionInMoreThanNMinutes(vm.releaseWindow.StartTime, 15)) {
                    logger.warn('Release was not yet started');
                    return null;
                }
            }

            vm.state.isBusy = true;
            var commandData = {
                ReleaseWindowId: vm.releaseWindow.ExternalId,
                Metric: metric,
            };

            return remiapi.updateMetrics(commandData)
                .then(function () {
                    metric.ExecutedOn = moment().utc().format();
                    vm.evaluateMetrics();
                },
            function (error) {
                logger.error('Cannot perform action');
                logger.console(error);
            }).finally(function () {
                vm.state.isBusy = false;
            });
        }

        function hideIssuesModal() {
            vm.releaseWindow.Issues = vm.issuesBackUp;

            $('#releaseIssuesModal').modal('hide');
        }

        function showIssues() {
            if (actionInMoreThanNMinutes(vm.releaseWindow.StartTime, 15)) {
                logger.warn('Release was not yet started');
                return null;
            }
            vm.issuesBackUp = vm.releaseWindow.Issues;

            $('#releaseIssuesModal').modal('show');
            return null;
        }

        function saveIssues() {
            if (!vm.allowManaging()) {
                logger.warn('You have no permissions to perform this action');
                vm.hideIssuesModal();
                return null;
            }

            if (vm.releaseWindow.Issues == vm.issuesBackUp) {
                vm.hideIssuesModal();
                return null;
            }

            vm.state.isBusy = true;
            var commandData = {
                ReleaseWindow: vm.releaseWindow
            };

            remiapi.saveReleaseIssues(commandData).then(function () {
                vm.issuesBackUp = vm.releaseWindow.Issues;
            },
            function (error) {
                logger.error('Cannot save issues');
                logger.console(error);
            }).finally(function () {
                vm.state.isBusy = false;
                vm.hideIssuesModal();
            });
        }

        function allowManaging() {
            return allowManageAsProcessParticipant || authService.isLoggedIn;
        }
    }
})()
