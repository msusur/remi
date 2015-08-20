(function () {
    'use strict';

    var serviceId = 'rulesService';

    angular.module('app').factory(serviceId, ['common', 'remiapi', rulesService]);

    function rulesService(common, remiapi) {
        var $q = common.$q;
        var logger = common.logger.getLogger(serviceId);

        var service = {
            state: {
                isBusy: false
            },
            getRules: getRules,
            getRule: getRule,
            getPermissionRule: getPermissionRule,
            generateNewRule: generateNewRule,
            testBusinessRule: testBusinessRule,
            saveRule: saveRule,
            savePermissionRule: savePermissionRule,
            deleteRule: deleteRule
        };

        return service;

        function getRules() {
            var deferred = $q.defer();

            service.state.isBusy = true;

            remiapi.get.businessRules()
                .then(function (data) {
                    deferred.resolve(data.Rules);
                }, function (response, status) {
                    if (response != 406 && status != 406) {
                        common.showErrorMessage('Sorry there was problem with getting rule details. Check log file for details.');
                        logger.error(JSON.stringify(response));
                    }
                    deferred.reject(response, status);
                }).finally(function () {
                    service.state.isBusy = false;
                });

            return deferred.promise;
        }

        function getRule(ruleId) {
            var deferred = $q.defer();
            if (!ruleId) {
                logger.console('ruleId cannot be null');
                deferred.reject();
            } else {
                service.state.isBusy = true;

                remiapi.get.businessRuleById(ruleId)
                    .then(function (data) {
                        deferred.resolve(data.Rule);
                    }, function (response, status) {
                        if (response != 406 && status != 406) {
                            common.showErrorMessage('Sorry there was problem with getting rule details. Check log file for details.');
                            logger.error(JSON.stringify(response));
                        }
                        deferred.reject(response, status);
                    }).finally(function () {
                        service.state.isBusy = false;
                    });
            }

            return deferred.promise;
        }

        function getPermissionRule(name) {
            var deferred = $q.defer();
            if (!name || name.length == 0) {
                logger.console('Name cannot be null');
                deferred.reject();
            } else {
                service.state.isBusy = true;

                remiapi.get.businessRuleByName("Permissions", name + "Rule")
                    .then(function (data) {
                        deferred.resolve(data.Rule);
                    }, function (response, status) {
                        if (response != 406 && status != 406) {
                            common.showErrorMessage('Sorry there was problem with getting rule details. Check log file for details.');
                            logger.error(JSON.stringify(response));
                        }
                        deferred.reject(response, status);
                    }).finally(function () {
                        service.state.isBusy = false;
                    });
            }

            return deferred.promise;
        }

        function generateNewRule(name, namespace) {
            var deferred = $q.defer();
            if (!name || name.length == 0) {
                logger.console('Name cannot be null');
                deferred.reject();
            }
            else if (!namespace || namespace.length == 0) {
                logger.console('Namespace cannot be null');
                deferred.reject();
            } else {
                service.state.isBusy = true;

                remiapi.get.generatedBusinessRule(namespace, name)
                    .then(function (data) {
                        deferred.resolve(data.Rule);
                    }, function (response, status) {
                        if (response != 406 && status != 406) {
                            common.showErrorMessage('Sorry there was problem with generating rule. Check log file for details.');
                            logger.error(JSON.stringify(response));
                        }
                        deferred.reject(response, status);
                    }).finally(function () {
                        service.state.isBusy = false;
                    });
            }

            return deferred.promise;
        }

        function testBusinessRule(rule) {
            var deferred = $q.defer();
            if (!rule) {
                logger.console('Rule cannot be null');
                deferred.reject();
            } else {
                service.state.isBusy = true;
                remiapi.testBusinessRule(rule)
                    .then(function (data) {
                        common.showInfoMessage('Rule result', JSON.stringify(data.Result, null, 4));
                        deferred.resolve(data.Result);
                    }, function (response, status) {
                        if (response != 406 && status != 406) {
                            common.showErrorMessage('Sorry there was problem with testing rule. Check log file for details.');
                            logger.error(JSON.stringify(response));
                        }
                        deferred.reject(response, status);
                    }).finally(function () {
                        service.state.isBusy = false;
                    });
            }
            return deferred.promise;
        }

        function saveRule(rule) {
            var deferred = $q.defer();
            if (!rule) {
                logger.console('Rule cannot be null');
                deferred.reject();
            } else {
                service.state.isBusy = true;

                remiapi.post.saveRule({ Rule: rule })
                    .then(function () {
                        deferred.resolve();
                    }, function (response, status) {
                        if (response != 406 && status != 406) {
                            common.showErrorMessage('Sorry there was problem with saving rule. Check log file for details.');
                            if (response) logger.error(JSON.stringify(response));
                        }
                        deferred.reject(response, status);
                    }).finally(function () {
                        service.state.isBusy = false;
                    });
            }
            return deferred.promise;
        }

        function savePermissionRule(rule, type, messageId) {
            var deferred = $q.defer();
            if (!rule || !messageId || !type) {
                logger.console('Rule and messageId cannot be null');
                deferred.reject();
            } else {
                service.state.isBusy = true;
                var data = { Rule: rule };
                data[type == 'command' ? 'CommandId' : 'QueryId'] = messageId;

                var p = type == 'command' ? remiapi.post.saveCommandPermissionRule(data) : remiapi.post.saveQueryPermissionRule(data);
                p.then(function () {
                    deferred.resolve();
                }, function (response, status) {
                    if (response != 406 && status != 406) {
                        common.showErrorMessage('Sorry there was problem with saving rule. Check log file for details.');
                        if (response) logger.error(JSON.stringify(response));
                    }
                    deferred.reject(response, status);
                }).finally(function () {
                    service.state.isBusy = false;
                });
            }
            return deferred.promise;
        }

        function deleteRule(ruleId) {
            var deferred = $q.defer();
            if (!ruleId) {
                logger.console('RuleId cannot be null');
                deferred.reject();
            } else {
                service.state.isBusy = true;

                remiapi.post.deletePermissionRule({
                    RuleId: ruleId
                }).then(function () {
                    deferred.resolve();
                }, function (response, status) {
                    if (response != 406 && status != 406) {
                        common.showErrorMessage('Sorry there was problem with deleting rule. Check log file for details.');
                        if (response) logger.error(JSON.stringify(response));
                    }
                    deferred.reject(response, status);
                }).finally(function () {
                    service.state.isBusy = false;
                });
            }
            return deferred.promise;
        }
    };
})();
