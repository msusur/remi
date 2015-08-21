describe("Release Controller", function () {
    var sut, mocks, logger, commandDeffered, getNearReleasesDefer, releaseEnumsDefer, getEnumsDefer, getBusinessUnitsDefer;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });
    });

    beforeEach(inject(function ($q, $rootScope, $timeout, $httpBackend) {
        mocks = {
            $rootScope: $rootScope,
            $q: $q,
            $scope: $rootScope.$new(),
            $location: window.jasmine.createSpyObj('$location', ['path', 'search']),
            common: {
                logger: window.jasmine.createSpyObj('logger', ['getLogger']),
                $q: $q,
                activateController: window.jasmine.createSpy('activateController').and.returnValue({ then: window.jasmine.createSpy('then') }),
                sendEvent: window.jasmine.createSpy('sendEvent'),
                handleEvent: window.jasmine.createSpy('handleEvent'),
                $broadcast: window.jasmine.createSpy('$broadcast')
            },
            config: {
                events: {
                    productContextChanged: "productContextChanged",
                    notificationReceived: "notificationReceived",
                    navRouteUpdate: "navRouteUpdate",
                    closeReleaseOnSignOffEvent: "closeReleaseOnSignOffEvent",
                    businessUnitsLoaded: "businessUnitsLoaded"
                }
            },
            remiapi: window.jasmine.createSpyObj('remiapi', ['executeCommand', 'getRelease', 'getNearReleases', 'getAccounts', 'releaseEnums']),
            authService:
            {
                selectProduct: jasmine.createSpy('selectProduct'),
                identity: { product: { Name: 'test product name' }, products: [{ Name: 'test product name' }] }
            },
            notifications: window.jasmine.createSpyObj('notifications', ['subscribe', 'unsubscribe']),
            timeout: $timeout,
            localData: window.jasmine.createSpyObj('localData', ['getEnum', 'businessUnits']),
        };
        $httpBackend.when('GET', 'app/releaseCalendar/releaseCalendar.html').respond('<div></div>');

        mocks.remiapi.post = window.jasmine.createSpyObj('post', ['checkQaStatus']);
        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);

        mocks.$location.search.and.returnValue({});
        mocks.$location.path.and.returnValue(mocks.$location);
        commandDeffered = $q.defer();
        getNearReleasesDefer = $q.defer();
        getEnumsDefer = $q.defer();
        getBusinessUnitsDefer = $q.defer();
        var p1 = $q.defer();
        mocks.remiapi.getAccounts.and.returnValue(p1.promise);
        mocks.remiapi.executeCommand.and.returnValue(commandDeffered.promise);
        mocks.remiapi.post.checkQaStatus.and.returnValue(commandDeffered.promise);
        mocks.remiapi.getNearReleases.and.returnValue(getNearReleasesDefer.promise);
        mocks.localData.getEnum.and.returnValue(getEnumsDefer.promise);
        mocks.localData.businessUnitsPromise = function () { return getBusinessUnitsDefer.promise; }
        releaseEnumsDefer = $q.defer();
        mocks.remiapi.releaseEnums.and.returnValue(releaseEnumsDefer.promise);
        //mocks.remiapi.releaseEnums.and.returnValue({ then: window.jasmine.createSpy('then') });

        p1.resolve([]);
    }));

    it("should call initialization methods when activated", function () {
        spyOn(mocks.$scope, '$on');

        inject(function ($controller) {
            sut = $controller('release', mocks);

        });

        expect(sut).toBeDefined();

        expect(mocks.localData.getEnum).toHaveBeenCalledWith('ReleaseType');

        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('release');
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), 'release');

        expect(mocks.common.handleEvent).toHaveBeenCalledWith(mocks.config.events.notificationReceived, window.jasmine.any(Function), mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith(mocks.config.events.navRouteUpdate, window.jasmine.any(Function), mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith(mocks.config.events.productContextChanged, window.jasmine.any(Function), mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith(mocks.config.events.closeReleaseOnSignOffEvent, window.jasmine.any(Function), mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('releaseContent.ticketsLoaded', window.jasmine.any(Function), mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('releaseChanges.releaseChangesLoaded', window.jasmine.any(Function), mocks.$scope);

        expect(mocks.$scope.$on).toHaveBeenCalledWith('$destroy', window.jasmine.any(Function));
    });

    it("should not load release window when query does not has releaseWindowId parameter", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });

        mocks.authService.identity.product = 'ABC';

        sut.queryParameters.releaseWindowId = undefined;

        spyOn(sut, 'setProduct');
        spyOn(sut, 'setReleaseWindowId');

        sut.queryParameters = {};
        sut.loadReleaseWindow();

        expect(sut.setProduct).toHaveBeenCalledWith('ABC');
        expect(sut.setReleaseWindowId).not.toHaveBeenCalled();
    });

    it("should load release window when query has releaseWindowId parameter", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });

        spyOn(sut, 'setProduct');
        spyOn(sut, 'setReleaseWindowId');
        sut.setReleaseWindowId.and.returnValue({ then: window.jasmine.createSpy('then') }),

        sut.queryParameters = { releaseWindowId: 'abc' };
        sut.loadReleaseWindow();

        expect(sut.setProduct).not.toHaveBeenCalled();
        expect(sut.setReleaseWindowId).toHaveBeenCalledWith('abc');
    });

    it("should not get next upcoming release when not all widgets are registered", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });

        mocks.authService.identity.product = 'ABC';

        spyOn(sut, 'setProduct');
        spyOn(sut, 'setReleaseWindowId');

        sut.queryParameters = {};
        sut.registerWidget('xyz');

        expect(sut.setProduct).not.toHaveBeenCalled();
        expect(sut.setReleaseWindowId).not.toHaveBeenCalled();
    });

    it("should ignore registered widget if it already has been loaded", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        mocks.authService.identity.product = 'ABC';

        sut.queryParameters = {};

        spyOn(sut, 'setReleaseWindowId');
        spyOn(sut, 'setProduct');

        registerAllWidgetsHelper(sut);

        var result = sut.registerWidget('releaseTask');

        expect(result).toBeNull();
    });

    it("should get null from register widgest when all widgets were registered", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        mocks.authService.identity.product = 'ABC';

        sut.isAllWidgetsLoaded = true;

        sut.queryParameters = { releaseWindowId: '0x001' };

        var result = sut.registerWidget('abc');

        expect(result).toBeNull();
    });

    it("should broadcast events, when all widgets are registered", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        var defer = mocks.$q.defer();
        sut.queryParameters = {};
        sut.currentReleaseWindow = { ReleaseType: 'Scheduled' };
        spyOn(sut, 'broadcastEvents').and.returnValue(defer.promise);

        var lastResult = registerAllWidgetsHelper(sut);
        defer.resolve();
        mocks.$scope.$digest();

        expect(lastResult).not.toBeNull();
        expect(sut.broadcastEvents).toHaveBeenCalled();
    });

    it("should load release by Id and load related information when setReleaseWindowId invoked", function () {

        mocks.authService.isLoggedIn = true;

        spyOn(sut, 'unbindRelease');
        spyOn(sut, 'loadReleaseWindowById');
        spyOn(sut, 'getNearReleases');

        var deferred = mocks.$q.defer();
        sut.unbindRelease.and.returnValue(deferred.promise);
        sut.loadReleaseWindowById.and.returnValue(deferred.promise);
        sut.getNearReleases.and.returnValue(deferred.promise);

        sut.setReleaseWindowId('123');

        deferred.resolve();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toBe(false);

        expect(sut.unbindRelease).toHaveBeenCalled();
        expect(sut.loadReleaseWindowById).toHaveBeenCalledWith('123');
        expect(sut.getNearReleases).toHaveBeenCalled();
    });

    it("should change product context when loaded release by Id with not same product as currently selected and it not found in account's available products", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity = {
            products: [
                { Name: 'product 123', ExternalId: '123' }
            ]
        };

        spyOn(sut, 'bindRelease');

        var deferred = mocks.$q.defer();
        mocks.remiapi.getRelease.and.returnValue(deferred.promise);

        sut.product = null;
        sut.loadReleaseWindowById('123');

        deferred.resolve({ ReleaseWindow: { ExternalId: '123', Products: ['test product'] } });
        mocks.$scope.$digest();

        expect(sut.product).not.toBeNull();
        expect(sut.product.Name).toBe('test product');
        expect(sut.product.ExternalId).not.toBeDefined();
        expect(mocks.common.$broadcast).not.toHaveBeenCalled();
    });

    it("should change product context when loaded release by Id with not same product as currently selected and it is found in account's available products", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity = {
            products: [
                { Name: 'product 123', ExternalId: '123' },
                { Name: 'product abc', ExternalId: 'abc' }
            ]
        };

        spyOn(sut, 'bindRelease');

        var deferred = mocks.$q.defer();
        mocks.remiapi.getRelease.and.returnValue(deferred.promise);

        sut.product = null;
        sut.loadReleaseWindowById('123');

        deferred.resolve({ ReleaseWindow: { ExternalId: '123', Products: ['product abc'] } });
        mocks.$scope.$digest();

        expect(sut.product).not.toBeNull();
        expect(sut.product.Name).toBe('product abc');
        expect(sut.product.ExternalId).toBe('abc');
        expect(mocks.authService.selectProduct).toHaveBeenCalledWith({ Name: 'product abc', ExternalId: 'abc' });
    });

    it("should not change product context when loaded release by Id with the same product as currently selected ", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity = {
            products: [
                { Name: 'product 123', ExternalId: '123' },
                { Name: 'product abc', ExternalId: 'abc' }
            ]
        };
        sut.product = { Name: 'product 123', ExternalId: '000' };

        spyOn(sut, 'bindRelease');

        var deferred = mocks.$q.defer();
        mocks.remiapi.getRelease.and.returnValue(deferred.promise);

        sut.loadReleaseWindowById('0x123');

        deferred.resolve({ ReleaseWindow: { ExternalId: '0x321', Products: ['product 123'] } });
        mocks.$scope.$digest();

        expect(sut.product).not.toBeNull();
        expect(sut.product.Name).toBe('product 123');
        expect(sut.product.ExternalId).toBe('000');
    });

    it("should redirect to default path when loadReleaseWindowById failed on widget register", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        var defer = mocks.$q.defer();
        sut.currentReleaseWindow = { ReleaseType: 'Scheduled' };
        spyOn(sut, 'broadcastEvents').and.returnValue(defer.promise);
        sut.queryParameters = { releaseWindowId: '0x001' };

        registerAllWidgetsHelper(sut);
        defer.reject();
        mocks.$scope.$digest();

        expect(mocks.$location.path).toHaveBeenCalledWith('/');
    });

    it("should not read release data when loadReleaseWindowById invoked by not authenticated user", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        mocks.authService.isLoggedIn = false;

        var deferred = mocks.$q.defer();
        mocks.remiapi.getRelease.and.returnValue(deferred.promise);

        sut.loadReleaseWindowById('abc');

        deferred.resolve();
        mocks.$scope.$digest();

        expect(mocks.remiapi.getRelease).not.toHaveBeenCalled();
    });

    it("should not read release data when loadReleaseWindowById invoked with empty id", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        mocks.authService.isLoggedIn = true;

        var deferred = mocks.$q.defer();
        mocks.remiapi.getRelease.and.returnValue(deferred.promise);

        sut.loadReleaseWindowById('');

        deferred.resolve();
        mocks.$scope.$digest();

        expect(mocks.remiapi.getRelease).not.toHaveBeenCalled();
    });

    it("should get release when loadReleaseWindowById called", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        mocks.authService.isLoggedIn = true;

        spyOn(sut, 'bindRelease');

        var deferred = mocks.$q.defer();
        mocks.remiapi.getRelease.and.returnValue(deferred.promise);

        sut.loadReleaseWindowById('0x123');

        deferred.resolve({ ReleaseWindow: { ExternalId: '0x321', Products: ['test product'] } });
        mocks.$scope.$digest();

        expect(mocks.remiapi.getRelease).toHaveBeenCalledWith('0x123');
        expect(sut.bindRelease).toHaveBeenCalledWith({ ExternalId: '0x321', Products: ['test product'] });
        expect(sut.product).not.toBeNull();
        expect(sut.product.Name).toBe('test product');
    });

    it("should clear state when unbindRelease called", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.people = ['people'];
        sut.state.isBindedToRelease = true;
        sut.currentReleaseWindow = { ExternalId: '0x123' };

        sut.unbindRelease();

        mocks.$scope.$digest();

        expect(sut.state.isBindedToRelease).toBe(false);
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseStatusChangedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseDecisionChangedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseWindowClosedEvent');
        expect(mocks.common.sendEvent).toHaveBeenCalledWith('release.ReleaseWindowLoadedEvent');
        expect(sut.people.length).toBe(0);
        expect(sut.currentReleaseWindow).toBeNull();
        expect(sut.releases.length).toBe(0);
    });

    it("should not do anything when unbindRelease called with empty current release", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.people = ['people'];
        sut.state.isBindedToRelease = true;
        sut.currentReleaseWindow = null;

        sut.unbindRelease();

        mocks.$scope.$digest();

        expect(sut.state.isBindedToRelease).toBe(true);
        expect(mocks.notifications.unsubscribe).not.toHaveBeenCalledWith('ReleaseStatusChangedEvent');
        expect(mocks.notifications.unsubscribe).not.toHaveBeenCalledWith('ReleaseDecisionChangedEvent');
        expect(mocks.notifications.unsubscribe).not.toHaveBeenCalledWith('ReleaseWindowClosedEvent');
        expect(mocks.common.sendEvent).not.toHaveBeenCalledWith('release.ReleaseWindowLoadedEvent');
        expect(sut.people.length).toBe(1);
        expect(sut.currentReleaseWindow).toBeNull();
        expect(sut.releases.length).toBe(0);
    });

    it("should not broadcast events when not all widgets are loaded", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        mocks.authService.identity.product = 'ABC';

        spyOn(sut, 'broadcastEvents');

        sut.productContextChanged('test product');

        expect(sut.broadcastEvents).not.toHaveBeenCalled();
    });

    it("should not reload release plan when current release is empty after route update", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        spyOn(sut, 'loadReleaseWindowById');

        sut.routeUpdateHandler();

        mocks.$scope.$digest();

        expect(sut.loadReleaseWindowById).not.toHaveBeenCalled();
    });

    it("should reload release plan when current release filled and differ from requested after route update", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.currentReleaseWindow = { ExternalId: '0x123' };

        spyOn(sut, 'setReleaseWindowId');
        var deferred = mocks.$q.defer();
        sut.setReleaseWindowId.and.returnValue(deferred.promise);

        mocks.$location.search.and.returnValue({ releaseWindowId: 'abc' });

        sut.routeUpdateHandler();

        deferred.resolve();

        mocks.$scope.$digest();

        expect(sut.setReleaseWindowId).toHaveBeenCalledWith('abc');
    });

    it("should redirect to default path when loading of requested release failed after route update", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.currentReleaseWindow = { ExternalId: '0x123' };

        spyOn(sut, 'setReleaseWindowId');
        var deferred = mocks.$q.defer();
        sut.setReleaseWindowId.and.returnValue(deferred.promise);

        mocks.$location.search.and.returnValue({ releaseWindowId: 'abc' });

        sut.routeUpdateHandler();

        deferred.reject();

        mocks.$scope.$digest();

        expect(sut.setReleaseWindowId).toHaveBeenCalledWith('abc');
        expect(mocks.$location.path).toHaveBeenCalledWith('/');
    });

    it("should not reload release plan when requested the same release as current after route update", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.currentReleaseWindow = { ExternalId: '0x123' };
        spyOn(sut, 'setReleaseWindowId');
        mocks.$location.search.and.returnValue({ releaseWindowId: '0x123' });

        sut.routeUpdateHandler();

        mocks.$scope.$digest();

        expect(sut.setReleaseWindowId).not.toHaveBeenCalled();
    });

    it("should not choose the release to bind when invoked with empty id", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        spyOn(sut, 'setReleaseWindowId');

        sut.chooseToBind('');

        mocks.$scope.$digest();

        expect(sut.setReleaseWindowId).not.toHaveBeenCalled();
    });

    it("should not choose the release to bind when invoked with release without ExternalId field", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        spyOn(sut, 'setReleaseWindowId');

        sut.chooseToBind({ ExternalX: 'abc' });

        mocks.$scope.$digest();

        expect(sut.setReleaseWindowId).not.toHaveBeenCalled();
    });

    it("should load release by Id when correct release choosed for binding", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        spyOn(sut, 'setReleaseWindowId');

        sut.chooseToBind({ ExternalId: 'abc' });

        mocks.$scope.$digest();

        expect(sut.setReleaseWindowId).toHaveBeenCalledWith('abc');
    });

    it("should not request for near releases when product is empty", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.product = null;

        sut.getNearReleases();

        mocks.$scope.$digest();

        expect(mocks.remiapi.getNearReleases).not.toHaveBeenCalled();
    });

    it("should request for near releases when product is valid", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.product = { Name: 'prod' };

        var deferred = mocks.$q.defer();
        mocks.remiapi.getNearReleases.and.returnValue(deferred.promise);

        sut.getNearReleases();

        deferred.resolve({
            ReleaseWindows: [
                { ExternalId: 'abc', ClosedOn: '1970' },
                { ExternalId: '123', ClosedOn: null }
            ]
        });

        mocks.$scope.$digest();

        expect(mocks.remiapi.getNearReleases).toHaveBeenCalledWith('prod');
        expect(sut.releases).not.toBeNull();
        expect(sut.releases.length).toBe(2);
        expect(sut.releases[0].ExternalId).toBe('abc');
        expect(sut.releases[1].ExternalId).toBe('123');
    });

    it("should return first non closed release when requested for near release", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.product = { Name: 'prod' };

        var deferred = mocks.$q.defer();
        mocks.remiapi.getNearReleases.and.returnValue(deferred.promise);

        var nearRelease;
        sut.getNearReleases().then(function (result) { nearRelease = result; });

        deferred.resolve({
            ReleaseWindows: [
                { ExternalId: 'abc', ClosedOn: '1970' },
                { ExternalId: '123', ClosedOn: null }
            ]
        });

        mocks.$scope.$digest();

        expect(nearRelease).not.toBeNull();
        expect(nearRelease.ExternalId).toBe('123');
    });

    it("should return first release when requested for near release and all releases are closed", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.product = { Name: 'prod' };

        var deferred = mocks.$q.defer();
        mocks.remiapi.getNearReleases.and.returnValue(deferred.promise);

        var nearRelease;
        sut.getNearReleases().then(function (result) { nearRelease = result; });

        deferred.resolve({
            ReleaseWindows: [
                { ExternalId: 'abc', ClosedOn: '1970' },
                { ExternalId: '123', ClosedOn: '1971' }
            ]
        });

        mocks.$scope.$digest();

        expect(nearRelease).not.toBeNull();
        expect(nearRelease.ExternalId).toBe('abc');
    });

    it("should save description", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.currentReleaseWindow = { Description: 'desc' };
        sut.descriptionBackUp = 'backup';
        sut.state = {
            isBusy: false
        };

        sut.saveDescription();
        commandDeffered.resolve();
        mocks.$scope.$digest();

        expect(sut.descriptionBackUp).toEqual('desc');
        expect(sut.currentReleaseWindow.Description).toEqual('desc');
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.showDescriptionEditor).toEqual(false);
    });

    it("should reject saving description", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.currentReleaseWindow = { Description: 'desc' };
        sut.descriptionBackUp = 'backup';
        sut.state = {
            isBusy: false
        };

        spyOn(console, 'log');

        sut.saveDescription();
        commandDeffered.reject('error');
        mocks.$scope.$digest();

        expect(sut.descriptionBackUp).toEqual('backup');
        expect(sut.currentReleaseWindow.Description).toEqual('backup');
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.showDescriptionEditor).toEqual(false);
        expect(logger.error).toHaveBeenCalledWith('Cannot update release description');
        expect(console.log).toHaveBeenCalledWith('error');
    });

    it("should don't change description when its model was not changed", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.currentReleaseWindow = { Description: 'desc' };
        sut.descriptionBackUp = 'desc';
        sut.state = {
            isBusy: false
        };

        sut.saveDescription();

        expect(logger.console).toHaveBeenCalledWith('Release description was not changed');
    });

    it("should unsubscribe events, when destroyed", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });

        mocks.$rootScope.$broadcast('$destroy', {});

        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseStatusChangedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseDecisionChangedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseWindowClosedEvent');
    });

    it("should bind release window, when bindRelease method invoked", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });

        var releaseWindow = {
            ReleaseType: 'random type',
            ReleaseDecision: 'random decision',
            ExternalId: 'random guid',
            Status: 'random status'
        };

        spyOn(sut, 'processReleaseStatus');
        spyOn(sut, 'initReleaseTypeClass');
        spyOn(sut, 'initReleaseDecisionClass');

        sut.bindRelease(releaseWindow);

        expect(sut.releaseSelector.isOpen).toBe(false);
        expect(sut.state.isBindedToRelease).toBe(true);
        expect(sut.isClosed).toBe(false);
        expect(sut.enableCloseRelease).toBe(false);
        expect(sut.currentReleaseWindow).toBe(releaseWindow);
        expect(sut.processReleaseStatus).toHaveBeenCalled();
        expect(sut.initReleaseTypeClass).toHaveBeenCalledWith(releaseWindow.ReleaseType);
        expect(sut.initReleaseDecisionClass).toHaveBeenCalledWith(releaseWindow.ReleaseDecision);
    });

    it("should broadcast all events after timeout, when broadcastEvents called", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });

        var releaseWindow = {
            ReleaseType: 'random type',
            ReleaseDecision: 'random decision',
            ExternalId: 'random guid',
            Status: 'random status'
        };
        sut.currentReleaseWindow = releaseWindow;

        sut.broadcastEvents();

        expect(mocks.common.sendEvent).not.toHaveBeenCalledWith();

        mocks.timeout.flush();

        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ReleaseStatusChangedEvent',
            window.jasmine.objectContaining({ ReleaseWindowId: releaseWindow.ExternalId }));
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ReleaseDecisionChangedEvent',
            window.jasmine.objectContaining({ ReleaseWindowId: releaseWindow.ExternalId }));
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ReleaseWindowClosedEvent',
            window.jasmine.objectContaining({ ReleaseWindowId: releaseWindow.ExternalId }));

        expect(mocks.$location.search).toHaveBeenCalledWith('releaseWindowId', 'random guid');
    });

    it("should initReleaseTypeClass initialise class info according to release type, when invoked", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });

        sut.initReleaseTypeClass('random type');

        expect(sut.releaseTypeClass).toEqual('random type-type');
    });

    it("should initReleaseDecisionClass initialise class info according to release decision, when invoked", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });

        sut.initReleaseDecisionClass('random decision');

        expect(sut.releaseDecisionClass).toEqual(window.jasmine.objectContaining({
            css: 'random-decision-decision-class',
            icon: 'fa-question'
        }));
    });

    it("should handle ReleaseStatusChangedEvent, when event occurs", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });

        spyOn(sut, 'handleReleaseStatusChange');

        var notification = { name: 'ReleaseStatusChangedEvent', data: 'random data' };
        sut.serverNotificationHandler(notification);

        expect(sut.handleReleaseStatusChange).toHaveBeenCalledWith(notification.data);
    });

    it("should handle ReleaseDecisionChangedEvent, when event occurs", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });

        spyOn(sut, 'handleReleaseDecisionChange');

        var notification = { name: 'ReleaseDecisionChangedEvent', data: 'random data' };
        sut.serverNotificationHandler(notification);

        expect(sut.handleReleaseDecisionChange).toHaveBeenCalledWith(notification.data);
    });

    it("should refresh view, when ReleaseStatusChangedEvent is handled", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.currentReleaseWindow = {};

        var data = { ReleaseStatus: 'random status' };
        sut.handleReleaseStatusChange(data);

        expect(sut.currentReleaseWindow.Status).toEqual(data.ReleaseStatus);
    });

    it("should refresh view, when handleReleaseDecisionChange is handled", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });
        sut.currentReleaseWindow = {};
        spyOn(sut, 'initReleaseDecisionClass');

        var data = { ReleaseDecision: 'random decision' };
        sut.handleReleaseDecisionChange(data);

        expect(sut.currentReleaseWindow.ReleaseDecision).toEqual(data.ReleaseDecision);
        expect(sut.initReleaseDecisionClass).toHaveBeenCalledWith(data.ReleaseDecision);
    });

    it("should intersect release changes and tickets when related data loaded", function () {
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });

        var releaseChanges = [{ Description: 'blabla' }, { Description: 'TCKT-1 blabla', Identifier: 'sh1' }, { Description: 'TCKT-1 blabla', Identifier: 'sh2' }, { Description: 'TCKT-2, TCKT-3 | TCKT-4 blabla', Identifier: 'sh3' }, { Description: 'blabla' }];
        var tickets = [{ TicketName: 'AAA-1' }, { TicketName: 'TCKT-1', TicketUrl: 'url1' }, { TicketName: 'TCKT-3', TicketUrl: 'url3' }, { TicketName: 'TCKT-4', TicketUrl: 'url4' }, { TicketName: 'AAA-2' }];

        sut.releaseChangesLoadedHandler(releaseChanges);
        sut.ticketsLoadedHandler(tickets);

        expect(releaseChanges[0].tickets).toBeUndefined();
        expect(releaseChanges[1].tickets[0].name).toBe('TCKT-1');
        expect(releaseChanges[1].tickets[0].url).toBe('url1');
        expect(releaseChanges[2].tickets[0].name).toBe('TCKT-1');
        expect(releaseChanges[2].tickets[0].url).toBe('url1');
        expect(releaseChanges[3].tickets.length).toBe(2);
        expect(releaseChanges[3].tickets[0].name).toBe('TCKT-3');
        expect(releaseChanges[3].tickets[0].url).toBe('url3');
        expect(releaseChanges[3].tickets[1].name).toBe('TCKT-4');
        expect(releaseChanges[3].tickets[1].url).toBe('url4');
        expect(releaseChanges[4].tickets).toBeUndefined();

        expect(tickets[0].releaseChanges).toBeUndefined();
        expect(tickets[1].releaseChanges.length).toBe(2);
        expect(tickets[1].releaseChanges[0].identifier).toBe('sh1');
        expect(tickets[1].releaseChanges[1].identifier).toBe('sh2');
        expect(tickets[2].releaseChanges[0].identifier).toBe('sh3');
        expect(tickets[3].releaseChanges[0].identifier).toBe('sh3');
        expect(tickets[4].releaseChanges).toBeUndefined();
    });

    it("should have business units when related data loaded", function () {

        inject(function ($controller) {
            sut = $controller('release', mocks);

        });
        getEnumsDefer.resolve([{ Name: "TestName" }, { Name: "Automated" }]);
        getBusinessUnitsDefer.resolve("Test BusinessUnits");
        mocks.$scope.$digest();

        expect(sut.releaseTypes).toEqual([{ Name: "TestName" }]);
        expect(sut.businessUnits).toEqual("Test BusinessUnits");
    });

    it("should update business unites, when businessUnitsUpdated event is fired", function () {
        var callback;
        mocks.common.handleEvent.and.callFake(function (eventName, eventHandler) {
            if (eventName === mocks.config.events.businessUnitsLoaded) {
                callback = eventHandler;
            }
        });
        mocks.localData.businessUnits = "Test Business Units";
        inject(function ($controller) {
            sut = $controller('release', mocks);
        });

        callback();

        expect(sut.businessUnits).toEqual("Test Business Units");
    });
    function registerAllWidgetsHelper(controller) {
        controller.registerWidget('releaseTask');
        controller.registerWidget('releaseApprovers');
        controller.registerWidget('closeRelease');
        controller.registerWidget('releaseContent');
        controller.registerWidget('releaseParticipant');
        controller.registerWidget('signOff');
        controller.registerWidget('releaseProcess');
        return controller.registerWidget('checkList');
    }

    describe("hasPlugin", function() {
        it("should return proper value, when plugin assign or not to release window", function() {
            inject(function ($controller) {
                sut = $controller('release', mocks);
            });
            sut.currentReleaseWindow = {
                Plugins: [
                    { PluginType: "ReleaseContent" }
                ]
            };

            expect(sut.hasPlugin("ReleaseContent")).toEqual(true);
            expect(sut.hasPlugin("SourceControl")).toEqual(false);
        });
    });
});

