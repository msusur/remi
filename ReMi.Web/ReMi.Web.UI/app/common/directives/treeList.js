(function () {
    'use strict';

    var directiveId = 'treeList';

    angular.module('app').directive(directiveId, ['common', 'config', treeList]);

    function treeList() {
        return {
            restrict: 'A',
            templateUrl: 'app/common/directives/tmpls/treeList.html',
            transclude: true,
            scope: {
                options: '=',
                treeList: '='
            },
            controller: function ($scope) {
                var vm = $scope;
                var options = {};
                
                vm.toggle = toggle;
                vm.checkParent = checkParent;
                vm.parentCheckStateClass = parentCheckStateClass;
                vm.checkChild = checkChild;
                vm.hasChildren = hasChildren;
                vm.hasAllChecked = hasAllChecked;
                vm.checkAllText = checkAllText;
                vm.checkAll = checkAll;
                vm.hasAllExpanded = hasAllExpanded;
                vm.toggleAllText = toggleAllText;
                vm.toggleAll = toggleAll;
                vm.selectChild = selectChild;
                vm.parentSelected = parentSelected;

                vm.data = buildData(vm.treeList);

                vm.$watch('treeList', treeListDataChanged, false);
                vm.$watch('options', optionsChanged, true);

                initializeOptions();

                function initializeOptions() {
                    options.childrenArrayProperty = vm.options.childrenArrayProperty ? vm.options.childrenArrayProperty : 'Children';
                    options.childCheckProperty = vm.options.childCheckProperty ? vm.options.childCheckProperty : 'Checked';
                    options.childSelectProperty = vm.options.childSelectProperty ? vm.options.childSelectProperty : 'Selected';
                    options.parentNameProperty = vm.options.parentNameProperty ? vm.options.parentNameProperty : 'Name';
                    options.childNameProperty = vm.options.childNameProperty ? vm.options.childNameProperty : 'Name';

                    vm.hideCheckBoxes = !vm.options.allowCheck || vm.options.allowCheck == 'false';
                    vm.hideRadioButtons = !vm.options.allowSelect || vm.options.allowSelect == 'false';
                    vm.selectTooltipText = vm.options.selectTooltip ? vm.options.selectTooltip : '';
                    vm.checkTooltipText = vm.options.checkTooltip ? vm.options.checkTooltip : '';
                    vm.disableSelectIfNotChecked = !!vm.options.disableSelectIfNotChecked;
                }

                function toggle(item) {
                    item.Expanded = !item.Expanded;
                }
                function checkParent(item) {
                    item.Checked = !item.Checked;
                    if (vm.hasChildren(item)) {
                        item.Children.forEach(function (c) {
                            checkChild(c, item.Checked);
                        });
                    }
                }
                function parentCheckStateClass(item) {
                    var allChecked = true;
                    var atLeastOneChecked = false;
                    if (vm.hasChildren(item)) {
                        item.Children.forEach(function (c) {
                            if (!c.Checked) allChecked = false;
                            if (c.Checked) atLeastOneChecked = true;
                        });
                        item.Checked = allChecked;
                    } else {
                        item.Checked = !!item.Checked;
                        allChecked = item.Checked;
                    }
                    return allChecked ? 'fa-check' : atLeastOneChecked ? 'fa-square' : '';
                }
                function hasChildren(item) {
                    return item.Children && item.Children.length > 0;
                }
                function hasAllChecked() {
                    return Enumerable.From(vm.data)
                        .All(function (x) {
                            return Enumerable.From(x.Children)
                                .All(function (c) {
                                    return c.Checked;
                                });
                        });
                }
                function checkAllText() {
                    return vm.hasAllChecked() ? 'uncheck all' : 'check all';
                }
                function checkAll() {
                    var checked = !vm.hasAllChecked();
                    vm.data.forEach(function (x) {
                        x.Checked = checked;
                        x.Children.forEach(function (c) {
                            checkChild(c, checked);
                        });
                    });
                }
                function hasAllExpanded() {
                    return Enumerable.From(vm.data)
                        .All(function (x) {
                            return x.Expanded;
                        });
                }
                function toggleAllText() {
                    return vm.hasAllExpanded() ? 'collapse all' : 'expand all';
                }
                function toggleAll() {
                    var expanded = !vm.hasAllExpanded();
                    vm.data.forEach(function (x) {
                        x.Expanded = expanded;
                    });
                }
                function checkChild(item, value) {
                    item.Checked = !!value;
                    item.Element[options.childCheckProperty] = !!value;
                    if (vm.disableSelectIfNotChecked && !value) {
                        item.Selected = false;
                        item.Element[options.childSelectProperty] = false;
                    }
                }
                function selectChild(child) {
                    if (vm.hideRadioButtons || (vm.disableSelectIfNotChecked && !child.Checked))
                        return;

                    vm.data.forEach(function (parent) {
                        parent.Children.forEach(function (c) {
                            c.Selected = false;
                            c.Element[options.childSelectProperty] = false;
                        });
                    });
                    child.Selected = true;
                    child.Element[options.childSelectProperty] = true;
                }
                function parentSelected(parent) {
                    return Enumerable.From(parent.Children).Any(function (c) {
                        return c.Selected;
                    });
                }
                function treeListDataChanged() {
                    vm.data = buildData(vm.treeList);
                }
                function optionsChanged() {
                    initializeOptions();
                    vm.data = buildData(vm.treeList);
                }

                function buildData(dataArray) {
                    var data = [];
                    if (dataArray && dataArray.length) {
                        dataArray.forEach(function (parent) {
                            var parentElement = {
                                Expanded: false,
                                Name: parent[options.parentNameProperty],
                                Element: parent,
                                Children: []
                            };
                            if (parent[options.childrenArrayProperty] && parent[options.childrenArrayProperty].length > 0) {
                                parent[options.childrenArrayProperty].forEach(function (child) {
                                    parentElement.Children.push({
                                        Name: child[options.childNameProperty],
                                        Checked: !!child[options.childCheckProperty],
                                        Selected: !!child[options.childSelectProperty],
                                        Element: child
                                    });
                                    if (child[options.childSelectProperty]) parent.Selected = true;
                                });
                            }
                            data.push(parentElement);
                        });
                    }
                    return data;
                }
            }
        };
    }
})()
