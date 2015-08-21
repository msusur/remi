describe("Charts Directive ", function () {
    var scope, container, element, targetScope;
    var html, compiled;
    var $httpBackend, $compile, $timeout, $q, $filter;
    var chartHtml;

    beforeEach(function () {
        html = '<div data-remi-chart="" data-charts="vm.charts.scheduledRelease" data-measurement="vm.scheduledReleaseMeasurements" data-chart-title="Scheduled Releases Metrics"></div>';
        chartHtml = '<div class="widget wlightblue"></div>';

        module("app", function ($provide) { $provide.value("authService", {}) });

        inject(function ($rootScope, _$compile_, $templateCache, $injector, _$q_, _$httpBackend_, _$timeout_, _$filter_) {
            $timeout = _$timeout_;
            $compile = _$compile_;
            $q = _$q_;
            $filter = _$filter_;

            scope = $rootScope.$new();
            scope.charts = ['test'];
            scope.measurement = 'msmnt';
            scope.name = 'somename';
            
            $httpBackend = _$httpBackend_;
            $httpBackend.when('GET', 'app/common/directives/tmpls/remiChart.html').respond(chartHtml);
            $httpBackend.when('GET', 'app/common/directives/tmpls/remiChartContextMenu.html').respond(chartHtml);
            $httpBackend.when('GET', 'app/releaseCalendar/releaseCalendar.html').respond('<div></div>');

            prepareDirective(scope);
        });
    });

    function prepareDirective(s) {
        container = angular.element(html);
        compiled = $compile(container);
        element = compiled(s);
        s.$digest();          

        $timeout.flush();             

        $httpBackend.flush();

        targetScope = container.isolateScope();
    }

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    it('should have correct options for chart', function () {
        var coordinats = [287, 3489];
        
        var y = targetScope.options.chart.y(coordinats);

        expect(targetScope.options.chart.type).toEqual('lineChart');
        expect(targetScope.options.chart.forceY).toEqual([0]);
        expect(targetScope.options.chart.useInteractiveGuideline).toEqual(true);
        expect(y).toEqual(3489);
    });

    it('should evaluate pages number when page size is greater than list length', function () {
        targetScope.pageSize = 10;
        targetScope.measurements = [{}];

        var pagesNumber = targetScope.pagesNumber();

        expect(pagesNumber).toEqual(1);
    });

    it('should evaluate pages number when all pages are full', function () {
        targetScope.pageSize = 3;
        targetScope.measurements = [{}, {}, {}, {}, {}, {}];

        var pagesNumber = targetScope.pagesNumber();

        expect(pagesNumber).toEqual(2);
    });

    it('should evaluate pages number when not all pages are full', function () {
        targetScope.pageSize = 3;
        targetScope.measurements = [{}, {}, {}, {}, {}, {}, {}];

        var pagesNumber = targetScope.pagesNumber();

        expect(pagesNumber).toEqual(3);
    });

    it('should evaluate pages number when there is no input data', function () {
        targetScope.pageSize = 3;

        var pagesNumber = targetScope.pagesNumber();

        expect(pagesNumber).toEqual(0);
    });

    it('should change page if it has acceptable number', function () {
        targetScope.pages = 3;
        spyOn(targetScope, 'fillmeasurementData');

        targetScope.changePage(2);

        expect(targetScope.page).toEqual(2);
        expect(targetScope.fillmeasurementData).toHaveBeenCalled();
    });

    it('should not change page if it has number less than acceptable', function () {
        targetScope.pages = 3;
        targetScope.page = 1;

        targetScope.changePage(0);

        expect(targetScope.page).toEqual(1);
    });

    it('should not change page if it has number greater than acceptable', function () {
        targetScope.pages = 3;
        targetScope.page = 1;

        targetScope.changePage(4);

        expect(targetScope.page).toEqual(1);
    });

    it('should change page size if it has acceptable number', function () {
        targetScope.measurements = [{}, {}, {}];
        spyOn(targetScope, 'fillmeasurementData');

        targetScope.changePageSize(2);

        expect(targetScope.page).toEqual(1);
        expect(targetScope.pageSize).toEqual(2);
        expect(targetScope.fillmeasurementData).toHaveBeenCalled();
    });

    it('should not change page size if it has number less than acceptable', function () {
        targetScope.measurements = [{}, {}, {}];
        targetScope.pageSize = 3;

        targetScope.changePageSize(0);

        expect(targetScope.pageSize).toEqual(3);
    });

    it('should fill measurement data', function () {
        targetScope.measurements = [
            { ReleaseWindow: { StartTime: '2014-06-12 16:03:11.743' }, Metrics: [{ Name: 'test', Value: 21}] },
            { ReleaseWindow: { StartTime: '2014-07-12 16:03:11.743' }, Metrics: [{ Name: 'test', Value: 23 }] },
            { ReleaseWindow: { StartTime: '2014-08-12 16:03:11.743' }, Metrics: [{ Name: 'test', Value: 17 }] },
            { ReleaseWindow: { StartTime: '2014-09-12 16:03:11.743' }, Metrics: [{ Name: 'test', Value: 83 }] },
            { ReleaseWindow: { StartTime: '2014-10-12 16:03:11.743' }, Metrics: [{ Name: 'test', Value: 12 }] },
            { ReleaseWindow: { StartTime: '2014-11-12 16:03:11.743' }, Metrics: [{ Name: 'test', Value: 45 }] },
            { ReleaseWindow: { StartTime: '2014-12-12 16:03:11.743' }, Metrics: [{ Name: 'test', Value: 23 }] }
        ];
        targetScope.pageSize = 3;
        targetScope.page = 2;
        targetScope.pagesNumber = function () { return 3; };
        targetScope.charts = ['test'];

        targetScope.fillmeasurementData();

        expect(targetScope.allowManage.pageNumber.up).toEqual(true);
        expect(targetScope.allowManage.pageNumber.down).toEqual(true);
        expect(targetScope.allowManage.pageSize.down).toEqual(true);
        expect(targetScope.measurementData.length).toEqual(1);
        expect(targetScope.measurementData[0].values.length).toEqual(3);
        expect(targetScope.measurementData[0].values[0][1]).toEqual(23);
        expect(targetScope.measurementData[0].values[1][1]).toEqual(17);
        expect(targetScope.measurementData[0].values[2][1]).toEqual(83);
    });

    it('should show sample', function () {
        targetScope.table = 'Show';

        targetScope.toggleSample();

        expect(targetScope.table).toEqual('Hide');
    });

    it('should hide sample', function () {
        targetScope.table = 'Hide';

        targetScope.toggleSample();

        expect(targetScope.table).toEqual('Show');
    });
});

