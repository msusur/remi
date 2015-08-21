describe("ReleaseQaStatus Controller", function() {
        var sut, mocks, logger;

        beforeEach(function() {
            module("app", function ($provide) { $provide.value("authService", {}) });
        });

        beforeEach(angular.mock.inject(function($q, $rootScope) {
            
            mocks = {
                $scope: $rootScope.$new(),
                $rootScope: $rootScope,
                common: {
                    logger: window.jasmine.createSpyObj('logger', ['getLogger']),
                    activateController: window.jasmine.createSpy('activateController'),
                    handleEvent: window.jasmine.createSpy('handleEvent'),
                    getParentScope: window.jasmine.createSpy("getParentScope").and.returnValue({ vm: {} })
                },
                remiapi:
                {
                    get: window.jasmine.createSpyObj('get', ['qaStatus'])
                },
                $q: $q
            };
            logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info', 'warn']);
            mocks.common.logger.getLogger.and.returnValue(logger);
    }));

    function prepareSut() {
        inject(function ($controller) {
            sut = $controller('releaseQaStatus', mocks);
        });
    }


    it("should call initialization methods when activated", function () {
        prepareSut();
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('releaseQaStatus');
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), 'releaseQaStatus', mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('release.ReleaseWindowLoadedEvent', jasmine.any(Function), mocks.$scope);
    });

    it("should get qa status qaChecks when checkStatus success", function () {
        var deferred = mocks.$q.defer();
        mocks.remiapi.get.qaStatus.and.returnValue(deferred.promise);
        prepareSut();
        sut.state = { isBusy: true };
        sut.releaseWindow = { ExternalId: '1', ReleaseType: 'Scheduled', Products: ['product'] };

        sut.checkStatus();
        deferred.resolve({ Content: 'qaChecks' });
        mocks.$scope.$digest();

        expect(mocks.remiapi.get.qaStatus).toHaveBeenCalledWith(sut.releaseWindow.Products[0]);
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.qaChecks).toEqual('qaChecks');
    });

    it("should log an error when checkStatus failed", function () {
        var deferred = mocks.$q.defer();
        mocks.remiapi.get.qaStatus.and.returnValue(deferred.promise);
        prepareSut();
        sut.state = { isBusy: true };
        sut.releaseWindow = { ExternalId: '1', ReleaseType: 'Scheduled', Products: ['product'] };

        spyOn(console, 'log');

        sut.checkStatus();
        deferred.reject("error message");
        mocks.$scope.$digest();

        expect(logger.console).toHaveBeenCalledWith('error message');
        expect(logger.error).toHaveBeenCalledWith('Cannot retrieve QA Status');
        expect(sut.state.isBusy).toEqual(false);
    });

    it("should switch controller to bound and visible state, when gets a release window", function () {
        var deferred = mocks.$q.defer();
        mocks.remiapi.get.qaStatus.and.returnValue(deferred.promise);
        var eventHandler;
        mocks.common.handleEvent.and.callFake(function(eventName, eh) {
            eventHandler = eh;
        });
        prepareSut();
        var releaseWindow = { ExternalId: '1', ReleaseType: 'Scheduled', Products: ['product'] };
        eventHandler(releaseWindow);

        expect(sut.state.bindedToReleaseWindow).toEqual(true);
        expect(sut.state.visible).toEqual(true);
    });

    it("should switch controller to unbound and invisible state, when gets an empty release window", function () {
        var deferred = mocks.$q.defer();
        mocks.remiapi.get.qaStatus.and.returnValue(deferred.promise);
        var eventHandler;
        mocks.common.handleEvent.and.callFake(function (eventName, eh) {
            eventHandler = eh;
        });
        prepareSut();
        var releaseWindow = undefined;
        eventHandler(releaseWindow);

        expect(sut.state.bindedToReleaseWindow).toEqual(false);
        expect(sut.state.visible).toEqual(false);
    });
});
