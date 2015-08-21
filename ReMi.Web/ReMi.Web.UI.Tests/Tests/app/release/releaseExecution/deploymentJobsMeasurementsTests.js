describe("DeploymentJobsMeasurements Controller", function () {
    var sut, mocks, logger;
    var $q, $rootScope;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });

        mocks = {
            common: {
                logger: window.jasmine.createSpyObj("logger", ["getLogger"]),
                activateController: window.jasmine.createSpy("activateController").and.returnValue({ then: window.jasmine.createSpy("then") }),
                handleEvent: window.jasmine.createSpy("handleEvent"),
                getParentScope: window.jasmine.createSpy("getParentScope").and.returnValue({ vm: {} })
            },
            config: { events: { notificationReceived: "notifications.received", loggedIn: "auth.loggedIn" } },
            remiapi: {
                get: window.jasmine.createSpyObj("get", ["getDeploymentJobsMeasurements"]),
                post: window.jasmine.createSpyObj("post", ["rePopulateMeasurements"])
            },
            authService: window.jasmine.createSpyObj("authService", ["identity", "isLoggedIn"]),
            notifications: window.jasmine.createSpyObj("notifications", ["subscribe", "unsubscribe"]),
            localData: window.jasmine.createSpyObj("localData", ["getEnum"])
        };

        logger = window.jasmine.createSpyObj("logger", ["console", "error", "info", "warn"]);
        mocks.common.logger.getLogger.and.returnValue(logger);

        inject(function ($controller, _$q_, _$rootScope_) {
            $q = _$q_;
            $rootScope = _$rootScope_;
            var getEnumsDefer = $q.defer();
            mocks.localData.getEnum.and.returnValue(getEnumsDefer.promise);

            mocks.$scope = $rootScope.$new();

            mocks.remiapi.get.getDeploymentJobsMeasurements.and.returnValue($q.when());

            sut = $controller("deploymentJobsMeasurements", mocks);
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("deploymentJobsMeasurements");
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), "deploymentJobsMeasurements", mocks.$scope);

        expect(mocks.common.handleEvent).toHaveBeenCalledWith("release.ReleaseWindowLoadedEvent", window.jasmine.any(Function), mocks.$scope);
    });

    it("should initialize controller when gets release window id", function () {
        spyOn(sut, "getDeploymentJobsMeasurements");
        var releaseWindow = { ExternalId: "external id", SignedOff: "date" };

        sut.releaseWindowLoadedEventHandler(releaseWindow);

        expect(sut.releaseWindowId).toEqual("external id");
        expect(sut.state.bindedToReleaseWindow).toEqual(true);
        expect(sut.isVisible).toEqual(true);
        expect(sut.getDeploymentJobsMeasurements).toHaveBeenCalled();
    });

    it("should initialize controller but lieave invisible when gets release window that not signed off", function () {
        spyOn(sut, "getDeploymentJobsMeasurements");
        var releaseWindow = { ExternalId: "external id", SignedOff: null };

        sut.releaseWindowLoadedEventHandler(releaseWindow);

        expect(sut.state.bindedToReleaseWindow).toEqual(true);
        expect(sut.isVisible).toEqual(false);
        expect(sut.getDeploymentJobsMeasurements).not.toHaveBeenCalled();
    });

    it("should switch controller to unbind state when gets empty release window", function () {
        sut.releaseWindowLoadedEventHandler();

        expect(sut.state.bindedToReleaseWindow).toEqual(false);
    });

    it("should get measurements", function () {
        var deferred = $q.defer();
        mocks.remiapi.get.getDeploymentJobsMeasurements.and.returnValue(deferred.promise);
        var data = { Measurements: ["a", "b"], GoServerUrl: "url" };

        sut.releaseWindowId = "1";

        sut.getDeploymentJobsMeasurements();
        deferred.resolve(data);
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(mocks.remiapi.get.getDeploymentJobsMeasurements).toHaveBeenCalledWith("1");
        expect(sut.measurements).toEqual(["a", "b"]);
        expect(sut.goServerUrl).toEqual("url");
    });

    describe("rePopulateMeasurements", function () {
        it("should post command and refresh changes, when reload measurements invoked", function () {
            sut.releaseWindowId = "release id";

            spyOn(sut, "refreshMeasurements");
            var deferred = $q.defer();
            mocks.remiapi.post.rePopulateMeasurements.and.returnValue(deferred.promise);

            sut.rePopulateMeasurements();
            deferred.resolve();
            mocks.$scope.$digest();

            expect(sut.state.isBusy).toEqual(true);
            expect(mocks.remiapi.post.rePopulateMeasurements).toHaveBeenCalledWith({
                "ReleaseWindowId": "release id"
            });
            expect(sut.refreshMeasurements).toHaveBeenCalled();
        });

        it("should not refresh changes list, when command was rejected", function () {
            sut.releaseWindowId = "release id";

            spyOn(sut, "refreshMeasurements");
            var deferred = $q.defer();
            mocks.remiapi.post.rePopulateMeasurements.and.returnValue(deferred.promise);

            sut.rePopulateMeasurements();
            deferred.reject();
            mocks.$scope.$digest();

            expect(sut.state.isBusy).toEqual(false);
            expect(sut.refreshMeasurements).not.toHaveBeenCalled();
        });
    });
});

