describe("Auth Service", function () {
    var sut, mocks;
    var deferred;

    beforeEach(function () {
        mocks = {
            common: {
                logger: jasmine.createSpyObj("logger", ["getLogger"]),
                $q: jasmine.createSpyObj("common.$q", ["defer"])
            },
            config: jasmine.createSpyObj("config", ["events"]),
            remiapi: jasmine.createSpyObj("remiapi", ["executeCommand", "getAccountRoles"]),
            notifications: jasmine.createSpyObj("notifications", ["subscribe", "unsubscribe"])
        };
        mocks.common.$q.defer.and.returnValue({
            promise: { then: jasmine.createSpy("then") },
            resolve: jasmine.createSpy("resolve")
        });
        mocks.remiapi.get = jasmine.createSpyObj("remiapi.get", ["permissions"]);
        mocks.remiapi.getAccountRoles.and.callFake(function () { return mocks.common.$q.defer().promise; });
        mocks.remiapi.get.permissions.and.callFake(function () { return mocks.common.$q.defer().promise; });

        module("app", function ($provide) {
            $provide.value("common", mocks.common);
            $provide.value("config", mocks.config);
            $provide.value("remiapi", mocks.remiapi);
            $provide.value("notifications", mocks.notifications);
        });

        inject(function (_authService_, _$q_) {
            mocks.common.$q = _$q_;
            deferred = mocks.common.$q.defer();

            sut = _authService_;
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("authService");
    });
});

