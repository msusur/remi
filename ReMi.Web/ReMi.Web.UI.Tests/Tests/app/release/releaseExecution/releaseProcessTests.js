describe("Release Process Controller", function () {
    var sut, mocks, logger;
    var $q, $rootScope;

    beforeEach(function () {
        module("app");

        mocks = {
            common: {
                logger: jasmine.createSpyObj('logger', ['getLogger']),
                activateController: jasmine.createSpy('activateController').and.returnValue({ then: jasmine.createSpy('then') }),
                handleEvent: window.jasmine.createSpy('handleEvent')
            },
            config: { events: { notificationReceived: 'notifications.received', loggedIn: 'auth.loggedIn' } },
            remiapi: jasmine.createSpyObj('remiapi', [
                'getMetrics', 'updateMetrics', 'saveReleaseIssues']),
            authService: jasmine.createSpyObj('authService', ['identity', 'isLoggedIn']),
            notifications: jasmine.createSpyObj('notifications', ['subscribe', 'unsubscribe'])
        };

        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);

        inject(function ($controller, _$q_, _$rootScope_, _$filter_) {
            $q = _$q_;
            $rootScope = _$rootScope_;
            mocks.$filter = _$filter_;
            mocks.$scope = $rootScope.$new();
            mocks.$scope.registerWidget = function (name) { return true; };
            spyOn(mocks.$scope, 'registerWidget');

            sut = $controller('releaseProcess', mocks);
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('releaseProcess');
        expect(mocks.common.activateController).toHaveBeenCalledWith(jasmine.any(Array), 'releaseProcess', mocks.$scope);

        expect(mocks.common.handleEvent).toHaveBeenCalledWith('releaseProcess.isParticipantEvent', sut.releaseProcessParticipationHandler, mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('release.ReleaseWindowLoadedEvent', jasmine.any(Function), mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('notifications.received', jasmine.any(Function), mocks.$scope);
    });

    it("should allow manage release passing for Admin", function () {
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity.role = 'Admin';

        var result = sut.allowManaging();

        expect(result).toBe(true);
    });

    it("should allow manage release passing for ReleaseEngineer", function () {
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity.role = 'Release engineer';

        var result = sut.allowManaging();

        expect(result).toBe(true);
    });

    it("should allow manage release passing for ExecutiveManager", function () {
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity.role = 'Executive manager';

        var result = sut.allowManaging();

        expect(result).toBe(true);
    });

    it("should allow manage release passing for PO", function () {
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity.role = 'Product owner';

        var result = sut.allowManaging();

        expect(result).toBe(true);
    });


    it("should initialize controller, when gets release window id", function () {
        spyOn(sut, 'getMetrics');
        var releaseWindow = { ExternalId: 'external id' };

        sut.releaseWindowLoadedEventHandler(releaseWindow);

        expect(sut.isReleaseClosed).toEqual(false);
        expect(sut.state.bindedToReleaseWindow).toEqual(true);
        expect(sut.getMetrics).toHaveBeenCalledWith(releaseWindow.ExternalId);
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('MetricsUpdatedEvent', { 'ReleaseWindowId': 'external id' });
    });

    it("should initialize controller, when gets release window id and window was already loaded", function () {
        spyOn(sut, 'getMetrics');
        var releaseWindow = { ExternalId: 'external id' };
        sut.releaseWindow = { ExternalId: '1' };

        sut.releaseWindowLoadedEventHandler(releaseWindow);

        expect(sut.isReleaseClosed).toEqual(false);
        expect(sut.state.bindedToReleaseWindow).toEqual(true);
        expect(sut.getMetrics).toHaveBeenCalledWith(releaseWindow.ExternalId);
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('MetricsUpdatedEvent');
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('MetricsUpdatedEvent', { 'ReleaseWindowId': 'external id' });
    });

    it("should switch controller to unbind state, when gets empty release window", function () {
        sut.releaseWindowLoadedEventHandler();

        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('MetricsUpdatedEvent');
        expect(sut.state.bindedToReleaseWindow).toEqual(false);
    });

    it("should get metrics", function () {
        var deferred = $q.defer();
        mocks.remiapi.getMetrics.and.returnValue(deferred.promise);
        var data = { Metrics: ['a', 'b'] };
        spyOn(sut, 'evaluateMetrics');
        mocks.authService.isLoggedIn = true;
        sut.releaseWindow = { ExternalId: '1' };

        sut.getMetrics();
        deferred.resolve(data);
        mocks.$scope.$digest();

        expect(sut.evaluateMetrics).toHaveBeenCalled();
        expect(sut.state.display).toEqual(true);
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.metrics).toEqual(['a', 'b']);
    });

    it("should reject getting metrics", function () {
        var deferred = $q.defer();
        mocks.remiapi.getMetrics.and.returnValue(deferred.promise);
        mocks.authService.isLoggedIn = true;
        sut.releaseWindow = { ExternalId: '1' };

        sut.getMetrics();
        deferred.reject('error');
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(logger.error).toHaveBeenCalledWith('Cannot get metrics');
        expect(logger.console).toHaveBeenCalledWith('error');
    });

    it("should hide issues", function () {
        sut.issuesBackUp = 'backup';
        sut.releaseWindow = { Issues: '' };

        sut.hideIssuesModal();

        expect(sut.releaseWindow.Issues).toEqual(sut.issuesBackUp);
    });

    it("should show issues", function () {
        sut.issuesBackUp = '';
        sut.releaseWindow = { StartTime: '2014-06-12 16:03:11.743' , Issues: 'issue'};

        sut.showIssues();

        expect(sut.issuesBackUp).toEqual('issue');
    });

    it("should handle release process participation event", function () {
        spyOn(sut, 'evaluateMetrics');

        sut.releaseProcessParticipationHandler();

        expect(sut.evaluateMetrics).toHaveBeenCalled();
    });

    
    it("should update metrics", function () {
        var deferred = $q.defer();
        mocks.remiapi.updateMetrics.and.returnValue(deferred.promise);
        sut.releaseWindow = { ExternalId: '1' };
        sut.metrics = [{ Order: 2 }, { Order: 3 }];

        sut.updateMetrics({Order: 2});
        deferred.resolve();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
    });

    it("should reject updating metrics", function () {
        var deferred = $q.defer();
        mocks.remiapi.updateMetrics.and.returnValue(deferred.promise);
        sut.releaseWindow = { ExternalId: '1' };
        sut.metrics = [{ Order: 2 }, { Order: 3 }];

        sut.updateMetrics({ Order: 2 });
        deferred.reject('error');
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(logger.error).toHaveBeenCalledWith('Cannot perform action');
        expect(logger.console).toHaveBeenCalledWith('error');
    });

    it("should update release state", function () {
        var notification = {
            name: 'MetricsUpdatedEvent',
            data: { Metric: { MetricType: 'a', ExecutedOn: '2014-06-12 16:03:11.743', ExternalId: '11' } },
        };
        spyOn(sut, 'evaluateMetrics');
        sut.metrics = [{ExternalId: '11'}];

        sut.serverNotificationHandler(notification);

        expect(sut.metrics[0].ExecutedOn).toEqual(notification.data.Metric.ExecutedOn);
        expect(sut.evaluateMetrics).toHaveBeenCalled();
    });

    it("should evaluate metrics", function () {
        sut.metrics = [
            { ExecutedOn: 'aaa', active: true },
            { ExecutedOn: null, active: false },
            { ExecutedOn: null, active: false }
        ];
        spyOn(sut, 'allowManaging');
        sut.allowManaging.and.returnValue(true);

        sut.evaluateMetrics();

        expect(sut.metrics[0].active).toEqual(false);
        expect(sut.metrics[1].active).toEqual(true);
        expect(sut.metrics[2].active).toEqual(false);
        expect(sut.allowManaging).toHaveBeenCalled();
    });

    it("should update release issues", function () {
        var notification = {
            name: 'ReleaseIssuesUpdatedEvent',
            data: { Issues: 'test' }
        };
        sut.releaseWindow = { Issues: '', StartTime: '2014-06-12 16:03:11.743' };

        sut.serverNotificationHandler(notification);

        expect(sut.releaseWindow.Issues).toEqual('test');
        expect(sut.issuesBackUp).toEqual('test');
        expect(logger.info).toHaveBeenCalledWith('Release issues updated');
    });

    it("should save issues", function () {
        var deferred = $q.defer();
        mocks.remiapi.saveReleaseIssues.and.returnValue(deferred.promise);
        sut.releaseWindow = { ExternalId: '1', Issues: 'smth', StartTime: '2014-06-12 16:03:11.743' };
        spyOn(sut, 'allowManaging');
        sut.allowManaging.and.returnValue(true);

        sut.saveIssues();
        deferred.resolve();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(sut.issuesBackUp).toEqual('smth');
        expect(sut.allowManaging).toHaveBeenCalled();
    });

    it("should reject saving issues", function () {
        var deferred = $q.defer();
        mocks.remiapi.saveReleaseIssues.and.returnValue(deferred.promise);
        sut.releaseWindow = { ExternalId: '1', StartTime: '2014-06-12 16:03:11.743', Issues: 'smth' };
        spyOn(sut, 'hideIssuesModal');
        spyOn(sut, 'allowManaging');
        sut.allowManaging.and.returnValue(true);

        sut.saveIssues();
        deferred.reject('error');
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(logger.error).toHaveBeenCalledWith('Cannot save issues');
        expect(logger.console).toHaveBeenCalledWith('error');
        expect(sut.hideIssuesModal).toHaveBeenCalled();
        expect(sut.allowManaging).toHaveBeenCalled();
    });
});

