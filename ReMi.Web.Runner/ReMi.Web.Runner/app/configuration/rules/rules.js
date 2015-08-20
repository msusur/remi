(function () {
    'use strict';

    var controllerId = 'rules';

    angular.module('app').controller(controllerId,
        ['common', 'rulesService', rules]);

    function rules(common, rulesService) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;

        vm.state = {
            isBusy: false,
            ruleState: rulesService.state
        };
        vm.ruleGroups = {};

        vm.editRule = editRule;
        vm.testBusinessRule = testBusinessRule;
        vm.saveRule = saveRule;

        activate();

        function activate() {
            common.activateController([getRules()], controllerId)
                .then(function () {
                    logger.console('Activated Rules View');
                });
        }

        function getRules() {
            return rulesService.getRules()
                 .then(function (ruleGroups) {
                     vm.ruleGroups = ruleGroups;
                 });
        }

        function editRule(rule) {
            vm.currentRuleView = rule;
            vm.currentRule = undefined;
            $('#ruleEditor').modal({ backdrop: 'static', keyboard: true });

            rulesService.getRule(rule.ExternalId)
                .then(function (ruleDesc) {
                    if (ruleDesc)
                        vm.currentRule = ruleDesc;
                }, function () {
                    $('#ruleEditor').modal('hide');
                });
        };

        function testBusinessRule() {
            if (vm.currentRule) {
                rulesService.testBusinessRule(vm.currentRule);
            }
        };

        function saveRule() {
            if (!vm.currentRule) {
                return;
            }

            rulesService.saveRule(vm.currentRule)
                .then(function () {
                    $('#ruleEditor').modal('hide');
                    vm.currentRuleView.CodeBeggining = vm.currentRule.Script.length <= 30 ? vm.currentRule.Script : vm.currentRule.Script.substr(0, 30) + ' ...';
                    vm.currentRuleView = undefined;
                    vm.currentRule = undefined;
                });
        };
    }
})();
