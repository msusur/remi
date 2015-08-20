(function () {
    'use strict';

    /// External control used for tree grid control: http://maxazan.github.io/jquery-treegrid/

    var directiveId = 'treeGrid';

    angular.module('app').directive(directiveId, [function treeGrid() {
        return {
            restrict: 'A',
            scope: {
                rows: '@',
                columns: '=',
                childItemsProperty: '@'
            },
            link: function (scope, tElement) {
                scope.elementRef = tElement;
            },
            controller: function ($scope) {
                $scope.state = {
                    isBusy: false
                };

                $scope.tableData = [];
                $scope.refreshContent = refreshContent;
                $scope.fillRows = fillRows;
                $scope.fillRowCells = fillRowCells;
                $scope.fillHeaderRowTemplate = fillHeaderRowTemplate;
                $scope.fillRowChildContent = fillRowChildContent;

                $scope.$parent.$watch($scope.rows, function (val) {
                    $scope.tableData = val;
                    $scope.refreshContent();
                }, true);

                var indexer = 0;
                $scope.columns = getColumns(0);

                function refreshContent() {
                    var parentEl = $($scope.elementRef);

                    $scope.state.isBusy = true;

                    if ($scope.tableData) {
                        var tableEl = $('<table class="tree table table-striped"></table>');

                        indexer = 0;

                        $scope.fillHeaderRowTemplate(tableEl);
                        $scope.fillRows($scope.tableData, indexer, tableEl);
                        var responseiveWrapperEl = $('<div class="table-responsive"></div>');
                        responseiveWrapperEl.append(tableEl);

                        responseiveWrapperEl.find('table.tree').treegrid({ initialState: 'collapsed' });
                        parentEl.empty().append(responseiveWrapperEl);

                        $scope.state.isBusy = false;
                    }
                }

                function fillRows(data, parentIndex, parentEl) {
                    if (!parentEl || !data) return;

                    for (var i = 0; i < data.length; i++) {
                        var item = data[i];
                        var rowIndexer = $scope.fillRowCells(item, parentIndex, parentEl);

                        $scope.fillRowChildContent(item, rowIndexer, parentEl);
                    }
                }

                function fillRowCells(rowData, parentIndex, parentEl) {
                    var rowIndexer = ++indexer;

                    var rowEl = $('<tr class="treegrid-' + rowIndexer + '"></tr>');
                    if (parentIndex)
                        rowEl.addClass('treegrid-parent-' + parentIndex);

                    if ($scope.columns) {
                        for (var j = 0; j < $scope.columns.length; j++) {
                            var column = $scope.columns[j];
                            var value = rowData[column.field] || '';
                            if (column.filter && column.filterDataGetter) {
                                var filterData = column.filterDataGetter();
                                value = column.filter(value, filterData);
                            }
                            var cellEl = $('<td>' + value + '</td>');
                            if (column.formatCallback) {
                                column.formatCallback(cellEl, value, rowData);
                            }


                            if (column.style) {
                                var keys = getKeys(column.style);
                                for (var idx in keys) {
                                    cellEl.css(keys[idx], column.style[keys[idx]]);
                                }

                            } else
                                cellEl.css('width', 'auto');

                            rowEl.append(cellEl);
                        }
                    } else
                        rowEl.append('<td></div>');

                    parentEl.append(rowEl);

                    return rowIndexer;
                }

                function fillRowChildContent(item, parentIndex, parentEl) {
                    var dataRows = item[$scope.childItemsProperty];

                    if (dataRows && dataRows.length > 0) {
                        $scope.fillRows(dataRows, parentIndex, parentEl);
                    }
                }

                function getColumns() {
                    if ($scope.columns)
                        return $scope.columns;

                    return [];
                }

                function fillHeaderRowTemplate(parentEl) {
                    if ($scope.columns) {
                        var rowEl = $('<tr></tr>');
                        for (var i = 0; i < $scope.columns.length; i++) {
                            var column = $scope.columns[i];
                            var cellEl = $('<th>' + (column.caption || column.field) + '</th>');

                            if (column.style) {
                                var keys = getKeys(column.style);
                                for (var idx in keys) {
                                    cellEl.css(keys[idx], column.style[keys[idx]]);
                                }
                            } else
                                cellEl.css('width', 'auto');
                            rowEl.append(cellEl);
                        }
                        parentEl.append(rowEl);
                    }
                }

                function getKeys(obj) {
                    if (!obj) return [];

                    var keys = [];
                    for (var key in obj) {
                        keys.push(key);
                    }
                    return keys;
                }
            }
        };
    }
    ]);
})();
