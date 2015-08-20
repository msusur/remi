(function () {
    'use strict';

    var directiveId = 'ruleEditor';

    angular.module('app').directive(directiveId, [ruleEditor]);

    function ruleEditor() {
        return {
            restrict: 'A',
            templateUrl: 'app/common/directives/tmpls/ruleEditor.html',
            scope: {
                rule: '&',
                isBusy: '='
            },
            transclude: true,
            controller: function ($scope, $element, $timeout) {
                var vm = $scope;

                vm.isBusy = true;
                vm.ruleDesc = getRule(vm.rule);
                vm.viewMode = 'code';

                vm.init = init;
                vm.destroy = destroy;
                vm.getRules = getRule;
                vm.switchViewMode = switchViewMode;

                if (vm.ruleDesc) {
                    init(vm, vm.ruleDesc);
                }

                vm.$on('$destroy', destroy);

                vm.$parent.$watch(vm.rule, updateRuleDesc, true);

                function updateRuleDesc(val) {
                    if (!vm.stopEdit) {
                        vm.isBusy = true;
                        vm.stopEdit = true; //sync with changes from other way

                        if (vm.editor)
                            vm.destroy();
                        vm.ruleDesc = vm.getRules(val);
                        vm.init(vm);

                        vm.stopEdit = false;
                    }
                }

                function switchViewMode() {
                    vm.viewMode = vm.viewMode == 'code' ? 'testData' : 'code';
                }

                function destroy() {
                    if (vm.editor) {
                        vm.editor.destroy();
                        vm.editor = undefined;
                    }
                    if (vm.accountTestDataEditor) {
                        vm.accountTestDataEditor.destroy();
                        vm.accountTestDataEditor = undefined;
                    }
                    if (vm.editors) {
                        for (var i in vm.editors) {
                            var e = vm.editors[i];
                            e.destroy();
                        }
                        vm.editors = undefined;
                    }
                }

                function init(scope) {
                    if (!scope.ruleDesc) {
                        vm.isBusy = false;
                        return;
                    }
                    $timeout(function () {
                        if (!scope.ruleDesc.AccountTestData)
                            scope.ruleDesc.AccountTestData = { JsonData: "{}" };
                        scope.editor = initEditor(scope, $element.find('.rule-editor').get(0), scope.ruleDesc.Script, 'csharp',
                            function (editor) { scope.ruleDesc.Script = editor.getValue(); });
                        scope.accountTestDataEditor = initEditor(scope, $element.find('.account-test-data').get(0),
                            formatJson(scope.ruleDesc.AccountTestData.JsonData), 'json',
                            function (editor) {
                                scope.ruleDesc.AccountTestData.JsonData = editor.getValue();
                            });
                        scope.editors = {};
                        for (var i in scope.ruleDesc.Parameters) {
                            var parameter = scope.ruleDesc.Parameters[i];
                            var editor = initEditor(scope, $element.find('.' + parameter.ExternalId).get(0),
                                formatJson(parameter.TestData.JsonData), 'json',
                                function (e) {
                                    e.parameter.TestData.JsonData = e.getValue();
                                });
                            editor.parameter = parameter;
                            scope.editors[parameter.Name] = editor;
                        }
                        vm.isBusy = false;
                    });
                }

                function initEditor(scope, elem, data, mode, updateData) {
                    var editor = ace.edit(elem);
                    editor.setTheme("ace/theme/github");
                    editor.getSession().setMode("ace/mode/" + mode);
                    editor.setValue(data);
                    editor.clearSelection();
                    editor.on('change', function () {
                        if (!scope.stopEdit) {
                            scope.stopEdit = true; //sync with changes from other way
                            scope.$apply(function () {
                                updateData(editor);
                            });
                            scope.stopEdit = false;
                        }
                    });

                    return editor;
                }

                function getRule(val) {
                    var temp;
                    if (typeof (val) == "function") {
                        temp = val();
                    } else
                        temp = val;

                    if (temp && temp.ExternalId)
                        return temp;
                    else if (temp && temp.Rule)
                        return temp.Rule;

                    return temp;
                }

                function formatJson(val) {
                    try {
                        var o = JSON.parse(val);
                        return JSON.stringify(o, null, 4);
                    }
                    catch (err) {
                        return JSON.stringify({}, null, 4);
                    }
                }
            }
        };
    }
})()
