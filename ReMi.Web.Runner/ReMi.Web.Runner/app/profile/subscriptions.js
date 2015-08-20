(function () {
    'use strict';

    var controllerId = 'subscriptions';

    angular.module('app').controller(controllerId, ['$scope', 'authService', 'remiapi', 'common', subscriptions]);

    function subscriptions($scope, authService, remiapi, common) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;

        vm.save = save;
        vm.getSubscriptions = getSubscriptions;

        vm.subscriptionsList = [];
        vm.state = {
            isBusy: false
        };
        
        vm.getSubscriptions();

        function getSubscriptions() {
            remiapi.get.userNotificationSubscriptions(authService.identity.externalId)
                .then(function (data) {
                    vm.subscriptionsList = data.NotificationSubscriptions;
                }, function (error) {
                    logger.error('Cannot get your notification subscriptions');
                    logger.console(error);
                });
        }


        function save(item) {
            vm.state.isBusy = true;
            remiapi.post.updateUserNotificationSubscriptions({ NotificationSubscriptions: [item] })
                .then(function() {
                    },
                    function(error) {
                        item.Subscribed = !item.Subscribed;
                        logger.error('Cannot update your notification subscriptions');
                        logger.console(error);
                    })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }
    }
})()
