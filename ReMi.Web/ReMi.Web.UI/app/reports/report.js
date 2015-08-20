(function () {
    'use strict';

    var controllerId = 'report';

    angular.module('app').controller(controllerId, ['$scope', 'remiapi', 'localData', 'common', 'authService', '$filter', 'DTOptionsBuilder', 'DTColumnDefBuilder', 'config', '$location', report]);

    function report($scope, remiapi, localData, common, authService, $filter, optionsBuilder, columnBuilder, config, $location) {
        var logger = common.logger.getLogger(controllerId);
        var vm = this;

        vm.loadReportTemplates = loadReportTemplates;
        vm.buildReport = buildReport;
        vm.downloadReportCsv = downloadReportCsv;
        vm.refreshReport = refreshReport;
        vm.checkArrayParameter = checkArrayParameter;
        vm.businessUnitsLoadedHandler = businessUnitsLoadedHandler;


        vm.currentReport = vm.tempReport = {};
        vm.state = { isBusy: true, locationInternalUpdate: false };
        vm.reports = [];
        vm.useUtcTime = false;
        vm.dateParametersPresent = false;
        vm.businessUnits = angular.copy(localData.businessUnits);

        common.handleEvent(config.events.businessUnitsLoaded, vm.businessUnitsLoadedHandler, $scope);
        common.handleEvent(config.events.navRouteUpdate, routeUpdateHandler, $scope);

        common.activateController([vm.loadReportTemplates()], controllerId, $scope)
            .then(function () {
                vm.state.isBusy = false;
                logger.console('Activated Close Release View');
                applyLocation();
            });

        function loadReportTemplates() {
            return remiapi.get.reportList().then(function (response) {
                vm.reports = response.ReportList.map(function (x) {
                    x.ReportParameters = x.ReportParameters.map(function (r) {
                        if (r.Type.toLowerCase() == 'datetime') {
                            r.Value = $filter('date')(new Date(), 'dd MMM yyyy HH:mm');
                        }
                        return r;
                    });
                    return x;
                });
                vm.currentReport = vm.tempReport = vm.reports[0];

                checkDateParametersPresent();
            }, function (error) {
                logger.error('Cannot get the list of report templates');
                logger.console(error);
            }
            );
        };

        function buildReport() {
            if (vm.currentReport.ReportParameters.filter(function (x) {
                return x.Type == 'Report.Packages';
            }).length > 0) {
                var businessUnits = getCheckedPackages();

                if (businessUnits.length == 0) {
                    logger.info('Please, select at least one package');
                    return null;
                }
            }

            for (var counter = 0; counter < vm.currentReport.ReportParameters; counter++) {
                if (!vm.currentReport.ReportParameters[counter].Value) {
                    logger.info(vm.currentReport.ReportParameters[counter].Description + ' parameter cannot be empty');
                    return null;
                }
            }
            var reportEntity = angular.copy(vm.currentReport);
            reportEntity.ReportParameters = reportEntity.ReportParameters.map(function (x) {
                if (x.Type.toLowerCase() == 'datetime') {
                    x.displayValue = $filter('date')(new Date(x.Value), 'dd MMM yyyy HH:mm:ss');
                    x.Value = encodeURIComponent(vm.useUtcTime ? moment.utc(x.Value).toISOString() : moment(x.Value).toISOString());
                }

                if (x.Type == 'Report.Packages') {
                    x.Value = businessUnits.map(function (y) {
                        return y.externalId;
                    }).toString();
                    x.displayValue = businessUnits.map(function (y) {
                        return y.name;
                    }).toString();
                }

                return x;
            });
            var url = '{0}?{1}'.format(reportEntity.ReportCreator,
                reportEntity.ReportParameters.map(function (x) {
                    return '{0}={1}'.format(x.Name, x.Value);
                }).join('&'));
            common.$broadcast(config.events.spinnerToggle, { show: true, message: 'Building ' + reportEntity.ReportName + ' Report ...' });
            vm.state.locationInternalUpdate = true;
            return remiapi.get.report(url).then(function (response) {
                setLocation(reportEntity.ReportCreator, reportEntity.ReportParameters);
                vm.report = {
                    parameters: reportEntity.ReportParameters.map(function (x) {
                        if (x.Type.toLowerCase() == 'datetime') {
                            x.Value = x.displayValue;
                        }
                        if (x.Type == 'Report.Packages') {
                            x.Value = x.displayValue.split(',');
                        }
                        return x;
                    }),
                    columnNames: response.Report.Headers,
                    content: response.Report.Data,
                    columns: response.Report.Headers.map(function (x) {
                        return columnBuilder.newColumnDef(response.Report.Headers.indexOf(x));
                    }),
                    options: optionsBuilder.newOptions(),
                    name: reportEntity.ReportName
                }
            }, function (error) {
                logger.error('Cannot get report ' + reportEntity.ReportName);
                logger.console(error);
            })
            .finally(function () {
                common.$broadcast(config.events.spinnerToggle, { show: false });
            });
        };

        function downloadReportCsv() {
            var data = vm.report.content;
            var csv = [];

            csv.push(vm.report.columnNames.join(','));

            for (var counter = 0; counter < data.length; counter++) {
                csv.push(data[counter].join(','));
            }
            var a = document.createElement('a');
            a.href = 'data:Application/octet-stream,' + encodeURIComponent(csv.join('\n'));
            a.target = '_blank';
            a.download = vm.currentReport.ReportCreator + '.csv';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
        }

        function refreshReport() {
            vm.report = null;
            copyParamtersValues(vm.currentReport.ReportParameters, vm.tempReport.ReportParameters);
            vm.tempReport = vm.currentReport;
            checkDateParametersPresent();
        }

        function getCheckedPackages() {
            return Enumerable.From(vm.businessUnits)
                .SelectMany(function (x) { return x.Packages; })
                .Where(function (x) { return x.Checked; })
                .Select(function (x) {
                    return {
                        name: x.Name,
                        externalId: x.ExternalId
                    };
                })
                .ToArray();
        }

        function businessUnitsLoadedHandler(data) {
            vm.businessUnits = angular.copy(data);
        }

        function checkArrayParameter(parameter) {
            return parameter instanceof Array;
        }

        function checkDateParametersPresent() {
            vm.dateParametersPresent = vm.currentReport.ReportParameters.filter(function (x) {
                return x.Type.toLowerCase() == 'datetime';
            }).length > 0;
        }

        function copyParamtersValues(newParams, oldParams) {
            var newEnum = Enumerable.From(newParams);
            var oldEnum = Enumerable.From(oldParams);

            newEnum.ForEach(function (x) {
                var found = oldEnum.Where(function (p) { return p.Name == x.Name; }).FirstOrDefault();
                if (found) {
                    x.Value = found.Value;
                }
            });
        }

        function setLocation(reportName, parameters) {
            $location.url($location.path());
            $location.search('report', reportName);
            if (parameters) {
                var paramsObject = {};
                parameters.forEach(function (x) {
                    paramsObject[x.Name] = x.Value;
                    return paramsObject;
                });
                $location.search('parameters', JSON.stringify(paramsObject));
            }
            $location.search('utc', (!!vm.useUtcTime).toString());
        }

        function applyLocation() {
            vm.report = null;
            vm.businessUnits = angular.copy(localData.businessUnits);
            Enumerable.From(vm.currentReport.ReportParameters).ForEach(function (x) {
                x.Value = x.Type === 'datetime' ? new Date() : null;
            });
            var queryString = $location.search();
            if (queryString && queryString.report) {
                var parameters = queryString.parameters ? JSON.parse(queryString.parameters) : {};
                var reportTemp = Enumerable.From(vm.reports)
                    .Where(function (x) { return x.ReportCreator == queryString.report; })
                    .FirstOrDefault();
                if (reportTemp && typeof (parameters) === "object"
                    && reportTemp.ReportParameters.length == Object.keys(parameters).length) {
                    vm.currentReport = vm.tempReport = reportTemp;
                    vm.useUtcTime = queryString.utc === 'true';
                    var reportParamsEnum = Enumerable.From(vm.currentReport.ReportParameters);
                    if (reportParamsEnum.All(function (x) { return !!parameters[x.Name]; })) {
                        reportParamsEnum.ForEach(function (x) {
                            switch (x.Type) {
                                case "datetime": x.Value = parseDate(parameters[x.Name]); break;
                                case "Report.Packages": selectPackages(parameters[x.Name].split(',')); break;
                                default: x.Value = parameters[x.Name]; break;
                            }
                        });
                        vm.buildReport();
                        vm.state.locationInternalUpdate = false;
                    }
                }
            }
        }

        function parseDate(value) {
            var decoded = decodeURIComponent(value);
            var date = vm.useUtcTime ? moment.utc(decoded) : moment(decoded);
            return date.isValid() ? date.toDate() : new Date();
        }

        function selectPackages(packageIds) {
            Enumerable.From(vm.businessUnits)
                .SelectMany(function (x) { return x.Packages; })
                .ForEach(function (x) {
                    if (packageIds.indexOf(x.ExternalId) >= 0) {
                        x.Checked = true;
                    }
                });
        }

        function routeUpdateHandler() {
            if (!vm.state.locationInternalUpdate)
                applyLocation();
            else
                vm.state.locationInternalUpdate = false;
        }

        return vm;
    }
})();
