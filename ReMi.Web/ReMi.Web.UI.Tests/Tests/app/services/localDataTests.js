describe("Local Data", function () {
    var sut, mocks, scope;
    var $q;

    beforeEach(function () {
        mocks = {
            common: {
                logger: jasmine.createSpyObj("logger", ["getLogger"]),
                $q: jasmine.createSpyObj("$q", ["when", "defer"])
            },
            remiapi: { get: jasmine.createSpyObj("remiapi.get", ["enums"]) },
        };
        mocks.remiapi.get.enums.and.returnValue({ then: function () { } });

        module("app", function ($provide) {
            $provide.value("common", mocks.common);
            $provide.value("remiapi", mocks.remiapi);
        });
        mocks.common.$q.defer.and.returnValue({
            promise: true,
            resolve: jasmine.createSpy("resolve")
        });

        // ReSharper disable InconsistentNaming
        inject(function (_$q_, _localData_, _$rootScope_) {
            // ReSharper restore InconsistentNaming
            $q = _$q_;
            scope = _$rootScope_.$new();

            sut = _localData_;
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("localData");
        expect(mocks.remiapi.get.enums).toHaveBeenCalled();
    });
});

