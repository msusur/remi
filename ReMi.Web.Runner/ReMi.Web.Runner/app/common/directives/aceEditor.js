(function () {
    "use strict";

    var directiveId = "aceEditor";

    angular.module("app").directive(directiveId, [aceEditor]);

    function aceEditor() {
        return {
            restrict: "A",
            scope: {
                ngModel: "=",
                ngChange: "&",
                mode: "@",
                ngDisabled: "="
            },
            transclude: true,
            controller: function ($scope, $element, $timeout) {
                var vm = $scope;

                vm.init = init;
                vm.destroy = destroy;

                init(vm);

                vm.$on("$destroy", destroy);

                vm.$watch("ngModel", updateModel, true);
                vm.$watch("ngDisabled", setReadOnly, false);

                function updateModel() {
                    if (!vm.stopEdit) {
                        vm.stopEdit = true; //sync with changes from other way

                        if (vm.editor)
                            vm.destroy();
                        vm.init(vm);

                        vm.stopEdit = false;
                    }
                }

                function destroy() {
                    if (vm.editor) {
                        vm.editor.destroy();
                        delete vm.editor;
                    }
                }

                function init(scope) {
                    if (!scope.ngModel && (!scope.mode || scope.mode === "json"))
                        scope.ngModel = {};
                    $timeout(function () {
                        scope.editor = initEditor(scope, $element[0], scope.ngModel, scope.mode,
                            function(editor) {
                                var val = editor.getValue();
                                scope.ngModel = tryParse(val, scope.ngModel, scope.mode);
                            });
                    });
                }

                function initEditor(scope, elem, data, mode, updateData) {
                    var editor = ace.edit(elem);
                    mode = mode ? mode : "json";
                    if (mode === "json")
                        data = formatJson(data);
                    editor.setTheme("ace/theme/github");
                    editor.getSession().setMode("ace/mode/" + mode);
                    editor.setValue(data);
                    editor.clearSelection();
                    editor.on("change", function () {
                        if (!scope.stopEdit) {
                            scope.stopEdit = true; //sync with changes from other way
                            scope.$apply(function () {
                                updateData(editor);
                                if (scope.ngChange)
                                    scope.ngChange();
                            });
                            scope.stopEdit = false;
                        }
                    });
                    editor.setReadOnly(!!vm.ngDisabled);

                    return editor;
                }

                function tryParse(value, originalValue, mode) {
                    try {
                        if (mode && mode !== "json")
                            return value;
                        if (typeof originalValue === "object")
                            return JSON.parse(value);
                        return value;
                    }
                    catch (err) {
                        return originalValue;
                    }
                }

                function formatJson(val) {
                    try {
                        if (typeof val === "object")
                            return JSON.stringify(val, null, 4);
                        var o = JSON.parse(val);
                        return JSON.stringify(o, null, 4);
                    }
                    catch (err) {
                        return JSON.stringify({}, null, 4);
                    }
                }

                function setReadOnly() {
                    if (vm.editor) {
                        vm.editor.setReadOnly(!!vm.ngDisabled);
                    }
                }
            }
        };
    }
})()
