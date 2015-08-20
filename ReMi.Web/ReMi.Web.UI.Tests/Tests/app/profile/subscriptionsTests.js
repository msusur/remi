describe("Subscriptions Controller", function () {
    var sut, mocks, logger;
    var $rootScope, $q, getDeferred, postDeferred;

    beforeEach(function () {
        module("app");

        mocks = {
            common: {
                logger: window.jasmine.createSpyObj('logger', ['getLogger']),
            },
            remiapi: window.jasmine.createSpyObj('remiapi', ['get', 'post']),
        };

        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);

        mocks.remiapi.get = window.jasmine.createSpyObj('get', ['userNotificationSubscriptions']);
        mocks.remiapi.post = window.jasmine.createSpyObj('post', ['updateUserNotificationSubscriptions']);

        inject(function ($controller, _$rootScope_, _$q_) {
            $q = _$q_;

            $rootScope = _$rootScope_;
            mocks.$scope = $rootScope.$new();

            getDeferred = $q.defer();
            postDeferred = $q.defer();
            mocks.remiapi.get.userNotificationSubscriptions.and.returnValue(getDeferred.promise);
            mocks.remiapi.post.updateUserNotificationSubscriptions.and.returnValue(postDeferred.promise);

            mocks.authService = { identity: { externalId: 'some id' } };

            sut = $controller('subscriptions', mocks);
        });
    });

    it("should get subscriptions", function () {
        sut.getSubscriptions();

        getDeferred.resolve({ NotificationSubscriptions: 'smth' });
        mocks.$scope.$digest();

        expect(sut.subscriptionsList).toEqual('smth');
    });

    it("should log error when getting subscriptions rejected", function () {
        sut.getSubscriptions();

        getDeferred.reject('error');
        mocks.$scope.$digest();

        expect(logger.error).toHaveBeenCalledWith('Cannot get your notification subscriptions');
        expect(logger.console).toHaveBeenCalledWith('error');
    });

    it("should save subscriptions", function () {
        sut.save('a');

        getDeferred.resolve({ NotificationSubscriptions: 'smth' });
        mocks.$scope.$digest();

        postDeferred.resolve();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toBe(false);
    });

    it("should log error when saving subscriptions rejected", function () {
        var item = { Subscribed: true };
        sut.save(item);

        getDeferred.resolve({ NotificationSubscriptions: 'smth' });
        mocks.$scope.$digest();

        postDeferred.reject('error');
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toBe(false);
        expect(item.Subscribed).toBe(false);
        expect(logger.error).toHaveBeenCalledWith('Cannot update your notification subscriptions');
        expect(logger.console).toHaveBeenCalledWith('error');
    });
});

