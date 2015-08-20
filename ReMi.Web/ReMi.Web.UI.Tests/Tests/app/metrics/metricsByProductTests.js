describe("Metrics By Product Controller", function () {
    var sut, mocks, logger;
    var $q, $rootScope;

    beforeEach(function () {
        module("app");

        mocks = {
            common: {
                logger: window.jasmine.createSpyObj("logger", ["getLogger"]),
                activateController: window.jasmine.createSpy("activateController").and.returnValue({ then: window.jasmine.createSpy("then") }),
                handleEvent: window.jasmine.createSpy("handleEvent"),
                $timeout: window.jasmine.createSpy("$timeout")
            },
            config: { events: { notificationReceived: "notifications.received", loggedIn: "auth.loggedIn", productContextChanged: "productContextChanged" } },
            remiapi: window.jasmine.createSpyObj("remiapi", ["get"]),
            authService: { identity: {product: "a"}, isloggedIn: true },
            notifications: window.jasmine.createSpyObj("notifications", ["subscribe", "unsubscribe"])
        };

        logger = window.jasmine.createSpyObj("logger", ["console", "error", "info", "warn"]);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.remiapi.get = window.jasmine.createSpyObj("get", ["getDeploymentJobsMeasurementsByProduct", "getMeasurements"]);
        
        inject(function ($controller, _$q_, _$rootScope_, _$filter_) {
            $q = _$q_;
            $rootScope = _$rootScope_;
            mocks.$filter = _$filter_;
            mocks.$scope = $rootScope.$new();
            var deferred = $q.defer();

            mocks.remiapi.get.getMeasurements.and.returnValue(deferred.promise);
            mocks.remiapi.get.getDeploymentJobsMeasurementsByProduct.and.returnValue($q.when());

            sut = $controller("metricsByProduct", mocks);
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("metricsByProduct");
        expect(mocks.common.handleEvent).toHaveBeenCalledWith("productContextChanged", sut.productContextChanged, mocks.$scope);
    });

    it("should have correct charts", function () {     
        expect(sut.charts.scheduledRelease).toEqual(["Down time", "Deploy time"]);
        expect(sut.charts.overallTimes).toEqual(["Overall time"]);
    });

    it("should get measurements", function () {
        var deferred = $q.defer();
        mocks.remiapi.get.getMeasurements.and.returnValue(deferred.promise);
        mocks.remiapi.get.getDeploymentJobsMeasurementsByProduct.and.returnValue(deferred.promise);
        var data = {
            Measurements: [
                { ReleaseWindow: { ReleaseType: "Scheduled" }, Metrics: "wonderful" },
                { ReleaseWindow: { ReleaseType: "Hotfix" }, Metrics: "beautiful" },
                { ReleaseWindow: { ReleaseType: "Scheduled" }, Metrics: "nice" }
            ]
        };

        sut.getMeasurements("test");
        deferred.resolve(data);
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(sut.scheduledReleaseMeasurements.length).toEqual(2);
        expect(sut.scheduledReleaseMeasurements[0].Metrics).toEqual("wonderful");
        expect(sut.scheduledReleaseMeasurements[1].Metrics).toEqual("nice");
    });

    it("should reject getting measurements", function () {
        var deferred = $q.defer();
        mocks.remiapi.get.getMeasurements.and.returnValue(deferred.promise);
        mocks.remiapi.get.getDeploymentJobsMeasurementsByProduct.and.returnValue(deferred.promise);

        sut.getMeasurements("test");
        deferred.reject("error");
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(logger.error).toHaveBeenCalledWith("Cannot get measurements");
    });

    it("should get measurement when product context changed", function () {
        spyOn(sut, "getMeasurements");
        mocks.authService.isLoggedIn = true;

        sut.productContextChanged({Name: "product"});

        expect(sut.getMeasurements).toHaveBeenCalledWith("product");
    });

    it("should clear deploy jobs timing colums when product context changed", function () {
        mocks.authService.isLoggedIn = true;

        sut.charts.deployJobTiming = ["A", "B"];

        sut.productContextChanged({ Name: "product" });

        expect(sut.charts.deployJobTiming.length).toBe(0);
    });
});

