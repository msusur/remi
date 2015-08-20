(function () {
    'use strict';

    var directiveId = 'remiChart';

    angular.module('app').directive(directiveId, [remiChart]);

    function remiChart() {
        return {
            restrict: 'A',
            templateUrl: 'app/common/directives/tmpls/remiChart.html',
            scope: {
                measurement: '@',
                charts: '=',
                chartTitle: '@',
                state: '@'
            },
            controller: function ($scope, $element, $http, $compile, $templateCache, $filter, $location) {
                var vm = $scope;

                vm.measurementData = undefined;
                vm.fillmeasurementData = fillmeasurementData;
                vm.changePage = changePage;
                vm.changePageSize = changePageSize;
                vm.index = index;
                vm.pagesNumber = pagesNumber;
                vm.toggleSample = toggleSample;
                vm.loadedContextMenuTemplate = loadedContextMenuTemplate;
                vm.turnOnPopupWindow = turnOnPopupWindow;
                vm.parseTemplate = parseTemplate;
                vm.loadedContextMenuTemplate = loadedContextMenuTemplate;

                vm.allowManage = {
                    pageNumber: { up: false, down: false },
                    pageSize: { down: false }
                };
                vm.tableData = undefined;
                vm.busy = false;

                vm.pages = 1;
                vm.page = 1;
                vm.pageSize = 20;
                vm.table = "Hide";

                vm.$parent.$watch(vm.measurement, function (val) {
                    vm.measurements = $filter('orderBy')(val, 'ReleaseWindow.StartTime');
                    vm.fillmeasurementData();
                }, true);

                vm.$parent.$watch(vm.state, function (val) {
                    vm.busy = val;
                }, true);

                vm.turnOnPopupWindow();

                $scope.$on('$destroy', function () {
                    $(document).unbind('click', vm.documentClickHandler);

                    if (vm.$chartContextMenu) {
                        vm.$chartContextMenu.hide();
                        vm.$chartContextMenu.remove();
                    }
                    vm.$chartContextMenu = null;
                });

                vm.options = {
                    chart: {
                        type: 'lineChart',
                        height: 350,
                        margin: {
                            top: 20,
                            right: 65,
                            bottom: 60,
                            left: 65
                        },
                        x: function (d) {
                            return d[0];
                        },
                        y: function (d) {
                            return d[1];
                        },

                        color: d3.scale.category10().range(),
                        useInteractiveGuideline: true,

                        xAxis: {
                            tickFormat: function (d) {
                                if (d == parseInt(d) && vm.measurements) {
                                    var comment;
                                    var m = vm.measurements[d];
                                    if (m) {
                                        comment = moment(new Date(m.ReleaseWindow.StartTime)).format('DD/MM/YY');

                                        return comment;
                                    }
                                }
                                return '';
                            },
                            showMaxMin: true
                        },

                        yAxis: {
                            axisLabel: 'Duration, minutes',
                            axisLabelDistance: 30
                        },
                        forceY: [0],
                        callback: function () {
                            d3.selectAll('.nvd3.nv-legend g').style('fill', "red");

                            d3.selectAll('.nv-group circle').style('cursor', 'pointer');

                            d3.selectAll('.nv-group circle').on('click', function () {
                                if (!vm.$chartContextMenu) return;

                                var circleData = d3.select(this).data();
                                if (circleData && circleData.length >= 0 && circleData[0].length >= 3) {
                                    var releaseWindowId = circleData[0][2];

                                    vm.clickOnReleaseWindowExternalId = releaseWindowId;

                                    var br = getOffsetRect(d3.select(this)[0][0]);

                                    $(document).unbind('click', vm.documentClickHandler);

                                    $(vm.$chartContextMenu)
                                        .data('ExternalId', releaseWindowId)
                                        .css({ left: br.left, top: br.top })
                                        .show();

                                    setTimeout(function () {
                                        $(document).bind('click', vm.documentClickHandler);
                                    }, 200);
                                }
                            });
                        }
                    }
                };

                function fillmeasurementData() {
                    if (vm.measurements) {
                        vm.pages = vm.pagesNumber();

                        vm.allowManage.pageNumber.up = vm.page < vm.pages;
                        vm.allowManage.pageNumber.down = vm.page > 1;
                        vm.allowManage.pageSize.down = vm.pageSize > 1;

                        if (vm.charts && vm.charts.length == 0 && vm.measurements.length > 0) {
                            //populate columns if they empty
                            var columns = [];
                            var metrics = vm.measurements[0].Metrics;
                            for (var i = 0; i < metrics.length; i++) {
                                columns.push(metrics[i].Name);
                            }
                            vm.charts = columns;
                        }

                        vm.measurementData = [];
                        for (var m = 0; m < vm.charts.length; m++) {
                            vm.measurementData.push({
                                key: vm.charts[m],
                                values: []
                            });
                        }
                        var finish = vm.measurements.length - (vm.page - 1) * vm.pageSize;
                        var start = finish <= vm.pageSize ? 0 : finish - vm.pageSize;

                        vm.tableData = [];

                        for (var counter = start; counter < finish; counter++) {
                            vm.tableData[counter - start] = [];

                            for (var c = 0; c < vm.charts.length; c++) {
                                vm.measurementData[c].values
                                    .push([
                                        counter,
                                        vm.measurements[counter]
                                            .Metrics
                                            .filter(function (x) {
                                                return x.Name == vm.charts[c];
                                            })[0].Value,
                                        vm.measurements[counter].ReleaseWindow.ExternalId
                                    ]);
                            }

                            vm.tableData[counter - start] = {
                                values: [],
                                window: vm.measurements[counter].ReleaseWindow
                            };

                            for (var v = 0; v < vm.charts.length; v++) {
                                vm.tableData[counter - start].values.push(
                                    vm.measurements[counter].Metrics.filter(function (item) {
                                        return item.Name == vm.charts[v];
                                    })[0].Value);
                            }
                        }
                    }
                }

                function getOffsetRect(elem) {
                    var box = elem.getBoundingClientRect();

                    var body = document.body;
                    var docElem = document.documentElement;

                    var scrollTop = window.pageYOffset || docElem.scrollTop || body.scrollTop;
                    var scrollLeft = window.pageXOffset || docElem.scrollLeft || body.scrollLeft;

                    var clientTop = docElem.clientTop || body.clientTop || 0;
                    var clientLeft = docElem.clientLeft || body.clientLeft || 0;

                    var top = box.top + scrollTop - clientTop;
                    var left = box.left + scrollLeft - clientLeft;

                    return { top: Math.round(top), left: Math.round(left) };
                }

                function turnOnPopupWindow() {
                    if (!vm.$chartContextMenu) {
                        vm.loadedContextMenuTemplate();
                    }

                    vm.documentClickHandler = function (e) {
                        $(vm.$chartContextMenu).hide();
                        $(document).unbind('click', vm.documentClickHandler);
                    };

                    vm.clickOnReleaseWindowExternalId = undefined;
                    vm.navigateToReleaseExecution = function () {
                        if (vm.clickOnReleaseWindowExternalId) {
                            $location.path('/releasePlan').search({ 'releaseWindowId': vm.clickOnReleaseWindowExternalId, 'tab': 'execution' });
                        }
                    };
                }

                function loadedContextMenuTemplate() {
                    var templ = $templateCache.get('remiChartContextMenu.html');

                    if (!templ) {
                        $http({ method: 'GET', url: 'app/common/directives/tmpls/remiChartContextMenu.html' })
                            .success(function (data) {
                                $templateCache.put('remiChartContextMenu.html', data);

                                parseTemplate(data);
                            });
                    } else {
                        parseTemplate(templ);
                    }
                };

                function parseTemplate(content) {
                    var el = $(content);
                    $('body').append(el);
                    el.find('a').bind('click', function (e) {
                        e.preventDefault();

                        $scope.$apply(function () {
                            vm.navigateToReleaseExecution();
                            vm.$chartContextMenu.hide();
                        });
                    });

                    $scope.$chartContextMenu = $compile(el)($scope);
                };

                function pagesNumber() {
                    if (vm.measurements) {
                        return (vm.measurements.length % vm.pageSize) == 0 ?
                        vm.measurements.length / vm.pageSize :
                        parseInt((vm.measurements.length / vm.pageSize) + 1);
                    }

                    return 0;
                }

                function changePage(par) {
                    if ((par >= 1) && (par <= vm.pages)) {
                        vm.page = par;
                        vm.fillmeasurementData();
                    }
                }

                function changePageSize(par) {
                    if ((par >= 1)) {
                        vm.pageSize = par;
                        vm.page = 1;
                        vm.fillmeasurementData();
                    }
                }

                function toggleSample() {
                    if (vm.table == 'Show') {
                        vm.table = 'Hide';
                    } else {
                        vm.table = 'Show';
                    }
                }

                function index($index) {
                    return $index + 1 + ((vm.page - 1) * vm.pageSize);
                }
            }
        };
    }
})()
