describe("Auth Service", function () {
    var sut, mocks;
    var $q;
    var deferred;

    beforeEach(function () {
        mocks = {
            common: {
                logger: jasmine.createSpyObj("logger", ["getLogger"]),
                $q: jasmine.createSpyObj("$q", ["when", "defer"])
            },
            config: jasmine.createSpyObj("config", ["events"]),
            remiapi: jasmine.createSpyObj("remiapi", ["executeCommand", "getAccountRoles"]),
            notifications: jasmine.createSpyObj("notifications", ["subscribe", "unsubscribe"])
        };

        module("app", function ($provide) {
            $provide.value("common", mocks.common);
            $provide.value("config", mocks.config);
            $provide.value("remiapi", mocks.remiapi);
            $provide.value("notifications", mocks.notifications);
        });
        mocks.common.$q.defer.and.returnValue({
            promise: true,
            resolve: jasmine.createSpy("resolve")
        });

        inject(function (_authService_, _$q_) {
            $q = _$q_;
            deferred = $q.defer();
            mocks.remiapi.getAccountRoles.and.returnValue(deferred.promise);

            sut = _authService_;
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("authService");
    });
});

