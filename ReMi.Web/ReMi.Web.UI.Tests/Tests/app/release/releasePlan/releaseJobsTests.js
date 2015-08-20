describe("ReleaseJobs Controller", function () {
    var sut, mocks, logger;
    var $q, $rootScope;

    beforeEach(function () {
        module("app");

        mocks = {
            common: {
                logger: window.jasmine.createSpyObj("logger", ["getLogger"]),
                activateController: window.jasmine.createSpy("activateController").and.returnValue({ then: window.jasmine.createSpy("then") }),
                handleEvent: window.jasmine.createSpy("handleEvent"),
                getParentScope: window.jasmine.createSpy("getParentScope").and.returnValue({ vm: {} })
            },
            config: { events: { notificationReceived: "notifications.received", loggedIn: "auth.loggedIn" } },
            remiapi: {
                get: window.jasmine.createSpyObj("get", ["getReleaseJobs"]),
                post: window.jasmine.createSpyObj("post", ["updateReleaseJob"])
            },
            authService: window.jasmine.createSpyObj("authService", ["identity", "isLoggedIn"]),
            notifications: window.jasmine.createSpyObj("notifications", ["subscribe", "unsubscribe"])
        };

        logger = window.jasmine.createSpyObj("logger", ["console", "error", "info", "warn"]);
        mocks.common.logger.getLogger.and.returnValue(logger);

        inject(function ($controller, _$q_, _$rootScope_) {
            $q = _$q_;
            $rootScope = _$rootScope_;

            mocks.$scope = $rootScope.$new();

            mocks.remiapi.get.getReleaseJobs.and.returnValue($q.when());

            sut = $controller("releaseJobs", mocks);
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("releaseJobs");
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), "releaseJobs", mocks.$scope);

        expect(mocks.common.handleEvent).toHaveBeenCalledWith("release.ReleaseWindowLoadedEvent", window.jasmine.any(Function), mocks.$scope);
    });

    it("should initialize controller when gets release window id", function () {
        spyOn(sut, "getReleaseJobs");
        var releaseWindow = { ExternalId: "external id" };

        sut.releaseWindowLoadedEventHandler(releaseWindow);

        expect(sut.state.bindedToReleaseWindow).toEqual(true);
        expect(sut.getReleaseJobs).toHaveBeenCalled();
        expect(sut.releaseWindowId).toEqual("external id");
    });

    it("should switch controller to unbind state when gets empty release window", function () {
        sut.releaseWindowLoadedEventHandler();

        expect(sut.state.bindedToReleaseWindow).toEqual(false);
    });

    it("should get jobs", function () {
        var deferred = $q.defer();
        mocks.remiapi.get.getReleaseJobs.and.returnValue(deferred.promise);
        var data = { ReleaseJobs: ["a", "b"] };

        sut.releaseWindowId = "1";

        sut.getReleaseJobs();
        deferred.resolve(data);
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(mocks.remiapi.get.getReleaseJobs).toHaveBeenCalledWith("1");
        expect(sut.releaseJobs).toEqual(["a", "b"]);
    });

    it("should update job", function () {
        var deferred = $q.defer();
        mocks.remiapi.post.updateReleaseJob.and.returnValue(deferred.promise);

        sut.releaseWindowId = "1";

        sut.updateJob({ ExternalId: "job id", IsIncluded: true });
        deferred.resolve();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(mocks.remiapi.post.updateReleaseJob).toHaveBeenCalledWith({ ReleaseJob: { ExternalId: "job id", IsIncluded: true }, ReleaseWindowId: "1" });
    });

    it("should revert isincluded when update job failed", function () {
        var deferred = $q.defer();
        mocks.remiapi.post.updateReleaseJob.and.returnValue(deferred.promise);

        sut.releaseWindowId = "1";
        var job = { ExternalId: "job id", IsIncluded: true };

        sut.updateJob(job);
        deferred.reject();
        mocks.$scope.$digest();

        expect(job.IsIncluded).toEqual(false);
    });
});

