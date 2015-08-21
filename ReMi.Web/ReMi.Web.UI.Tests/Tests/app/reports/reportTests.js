describe("Report Controller", function () {
    var sut, mocks, logger, reportDeferred, reportListDeferred;
    var $q, $rootScope, activateCallback, handlers = {};

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });

        mocks = {
            common: {
                logger: window.jasmine.createSpyObj('logger', ['getLogger']),
                activateController: window.jasmine.createSpy('activateController').and.returnValue(
                    {
                        then: window.jasmine.createSpy('then').and.callFake(function (callback) {
                            activateCallback = callback;
                        })
                    }),
                handleEvent: window.jasmine.createSpy('handleEvent'),
                $broadcast: jasmine.createSpy('$broadcast')
            },
            config: {
                events: {
                    businessUnitsLoaded: 'auth.businessUnitsLoaded',
                    spinnerToggle: 'spinnerToggle',
                    navRouteUpdate: 'navRouteUpdate'
                }
            },
            remiapi: window.jasmine.createSpyObj('remiapi', ['get']),
            authService: {
                identity: {
                    products: [{ Name: 'a' }]
                }, isloggedIn: true
            },
            DTOptionsBuilder: window.jasmine.createSpyObj('DTOptionsBuilder', ['newOptions']),
            DTColumnDefBuilder: window.jasmine.createSpyObj('DTColumnDefBuilder', ['newColumnDef']),
            localData: { businessUnits: ['a', 'b'] },
            $location: jasmine.createSpyObj("$location", ["search", "path", "url"])
        };

        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.common.handleEvent.and.callFake(function(event, handler) { handlers[event] = handler; });
        mocks.remiapi.get = window.jasmine.createSpyObj('get', ['reportList', 'report', 'enums']);

        inject(function ($controller, _$q_, _$rootScope_, _$filter_) {
            $q = _$q_;
            $rootScope = _$rootScope_;
            mocks.$filter = _$filter_;
            mocks.$scope = $rootScope.$new();
            reportDeferred = $q.defer(), reportListDeferred = $q.defer();

            mocks.remiapi.get.report.and.returnValue(reportDeferred.promise);
            mocks.remiapi.get.reportList.and.returnValue(reportListDeferred.promise);

            sut = $controller('report', mocks);
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('report');
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('auth.businessUnitsLoaded', sut.businessUnitsLoadedHandler, mocks.$scope);
    });

    it("should apply query string and build report when activated and query string exists", function () {
        sut.businessUnits = mocks.localData.businessUnits = [{
            Packages: [{ Checked: false, ExternalId: 'guidA', Name: 'a' }]
        }, {
            Packages: [
               { Checked: false, ExternalId: 'guidB', Name: 'b' },
               { Checked: false, ExternalId: 'guidC', Name: 'c' }
            ]
        }];
        sut.reports = [{
            ReportParameters: [
                { Type: 'datetime', Name: 'par1', Value: '2014-10-10T00:00:00' },
                { Type: 'Report.Packages', Name: 'par2' }
            ],
            ReportName: 'Report name',
            ReportCreator: 'ReportName'
        }];
        mocks.$location.search.and.returnValue({
            report: 'ReportName',
            parameters: '{"par1":"2015-10-10T00%3A00%3A00","par2":"guidA,guidB"}'
        });
        spyOn(sut, 'buildReport');
        spyOn(moment.fn, 'toDate').and.callFake(function (date) {
            return '2015-10-10T00:00:00.000Z';
        });

        activateCallback();


        expect(mocks.$location.search).toHaveBeenCalled();
        expect(sut.buildReport).toHaveBeenCalled();
        expect(sut.businessUnits[0].Packages[0].Checked).toBeTruthy();
        expect(sut.businessUnits[1].Packages[0].Checked).toBeTruthy();
        expect(sut.businessUnits[1].Packages[1].Checked).toBeFalsy();
        expect(sut.currentReport).toEqual(sut.reports[0]);
        expect(sut.tempReport).toEqual(sut.reports[0]);
        expect(sut.currentReport.ReportParameters[0].Value).toEqual('2015-10-10T00:00:00.000Z');
    });

    it("should clean parameters on activation, when no query string", function () {
        sut.businessUnits = [{
            Packages: [{ Checked: false, ExternalId: 'guidA', Name: 'a' }]
        }, {
            Packages: [
               { Checked: false, ExternalId: 'guidB', Name: 'b' },
               { Checked: false, ExternalId: 'guidC', Name: 'c' }
            ]
        }];
        sut.reports = [{
            ReportParameters: [
                { Type: 'datetime', Name: 'par1', Value: '2014-10-10T00:00:00' },
                { Type: 'Report.Packages', Name: 'par2' }
            ],
            ReportName: 'Report name',
            ReportCreator: 'ReportName'
        }];
        spyOn(sut, 'buildReport');
        sut.currentReport = sut.reports[0];

        activateCallback();


        expect(sut.buildReport).not.toHaveBeenCalled();
        expect(sut.businessUnits).toEqual(['a', 'b']);
        expect(sut.currentReport.ReportParameters[0].Value).not.toEqual(new Date('2014-10-10T00:00:00'));
    });

    it("should resolve getting report list when promise succeed", function () {
        sut.loadReportTemplates();
        reportListDeferred.resolve({
            ReportList: [
                { ReportParameters: [{ Type: 'datetime' }] },
                { ReportParameters: [{ Type: 'string' }] }
            ]
        });
        mocks.$scope.$digest();

        expect(sut.reports.length).toEqual(2);
        expect(sut.currentReport.ReportParameters[0].Type).toEqual('datetime');
    });

    it("should reject getting report list when promise failed", function () {
        sut.loadReportTemplates();
        reportListDeferred.reject('error');
        mocks.$scope.$digest();

        expect(logger.error).toHaveBeenCalledWith('Cannot get the list of report templates');
        expect(logger.console).toHaveBeenCalledWith('error');
    });

    it("should resolve getting report and set query string when promise succeed", function () {
        sut.businessUnits = [{
            Packages: [
                {
                    Checked: true,
                    ExternalId: 'guidA',
                    Name: 'a'
                },
                {
                    Checked: true,
                    ExternalId: 'guidB',
                    Name: 'b'
                }
            ]
        }];
        sut.currentReport = {
            ReportParameters: [
                {
                    Type: 'datetime',
                    Name: 'par1',
                    Value: '2014-10-10T00:00:00'
                },
                {
                    Type: 'Report.Packages',
                    Name: 'par2'
                }
            ],
            ReportName: 'Report name',
            ReportCreator: 'ReportName'
        };
        mocks.$location.path.and.returnValue('report url path');
        spyOn(moment.fn, 'toISOString').and.callFake(function(date) {
            return this._i + '.000Z';
        });

        sut.buildReport();
        reportDeferred.resolve({
            Report: {
                Headers: ['f', 's'],
                Data: [
                    [1, 2]
                ]
            }
        });
        mocks.$scope.$digest();


        expect(sut.report.columnNames).toEqual(['f', 's']);
        expect(sut.report.content[0]).toEqual([1, 2]);
        expect(mocks.remiapi.get.report).toHaveBeenCalledWith(
            "ReportName?par1=2014-10-10T00%3A00%3A00.000Z&par2=guidA,guidB");
        expect(mocks.DTOptionsBuilder.newOptions).toHaveBeenCalled();
        expect(mocks.DTColumnDefBuilder.newColumnDef).toHaveBeenCalledWith(0);
        expect(mocks.DTColumnDefBuilder.newColumnDef).toHaveBeenCalledWith(1);
        expect(mocks.common.$broadcast).toHaveBeenCalledWith('spinnerToggle', {
            show: true,
            message: 'Building Report name Report ...'
        });
        expect(mocks.common.$broadcast).toHaveBeenCalledWith('spinnerToggle', { show: false });
        expect(mocks.$location.path).toHaveBeenCalled();
        expect(mocks.$location.url).toHaveBeenCalledWith('report url path');
        expect(mocks.$location.search).toHaveBeenCalledWith('report', 'ReportName');
        expect(mocks.$location.search).toHaveBeenCalledWith('parameters', '{"par1":"2014-10-10T00%3A00%3A00.000Z","par2":"guidA,guidB"}');
    });

    it("should abandon building report when promise failed", function () {
        sut.businessUnits = [{
            Packages: [
                {
                    Checked: true,
                    Name: 'a'
                },
                {
                    Checked: true,
                    Name: 'b'
                }
            ]
        }];
        sut.currentReport = {
            ReportName: 'bad',
            ReportParameters: [
                {
                    Type: 'datetime',
                    Name: 'par1',
                    Value: '2014-10-10T00:00:00'
                },
                {
                    Type: 'Product',
                    Name: 'par2'
                }
            ]
        };
        sut.buildReport();
        reportDeferred.reject('error');
        mocks.$scope.$digest();

        expect(logger.error).toHaveBeenCalledWith('Cannot get report bad');
        expect(logger.console).toHaveBeenCalledWith('error');
    });

    it("should check array parameter when called", function () {
        var res = sut.checkArrayParameter([1, 2]);
        expect(res).toBeTruthy();
        res = sut.checkArrayParameter(1);
        expect(res).toBeFalsy();
    });

    it("should handle business units loading event when called", function () {
        sut.businessUnitsLoadedHandler('ac');

        expect(sut.businessUnits).toEqual('ac');
    });

    it("should copy values of same parameters and set currentReport, when report has changed", function () {
        sut.reports = 'some value';
        sut.currentReport = {
            ReportParameters: [{ Type: 'datetime', Name: 'par1', Value: '2014-10-10T00:00:00' }],
            ReportCreator: 'Report1'
        };
        sut.tempReport = {
            ReportParameters: [{ Type: 'datetime', Name: 'par1', Value: '2015-10-10T00:00:00' }],
            ReportCreator: 'Report2'
        };

        sut.refreshReport();

        expect(sut.report).toBeNull();
        expect(sut.currentReport).toEqual(sut.tempReport);
        expect(sut.currentReport.ReportParameters[0].Value).toEqual('2015-10-10T00:00:00');
        expect(sut.dateParametersPresent).toBeTruthy();
    });

    it("should apply parameters, when route has changed", function () {
        expect(mocks.$location.search.calls.any()).toBeFalsy();

        handlers.navRouteUpdate();

        expect(mocks.$location.search).toHaveBeenCalled();
        expect(mocks.$location.search.calls.count()).toEqual(1);
    });
});

