describe("Release Calendar Controller", function () {
    var sut, mocks, deferred;

    beforeEach(function () {
        module("app");
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope) {
        mocks = {
            $q: $q,
            $scope: $rootScope.$new(),
            $rootScope: $rootScope,
            common: {
                logger: jasmine.createSpyObj("logger", ["getLogger"]),
                activateController: jasmine.createSpy("activateController").and.returnValue({ then: jasmine.createSpy("then") }),
                sendEvent: window.jasmine.createSpy("sendEvent"),
                handleEvent: window.jasmine.createSpy("handleEvent"),
                $q: $q
            },
            config: {
                events: {
                    businessUnitsLoaded: "businessUnitsLoaded",
                }
            },
            remiapi: jasmine.createSpyObj("remiapi", ["executeCommand", "releaseEnums", "get"]),
            authService: {
                identity: {
                    businessUnits: [{ Name: "business unit" }],
                },
                isLoggedIn: true
            },
            localData: window.jasmine.createSpyObj("localData", ["getEnum"])
        };
        deferred = $q.defer();
        mocks.remiapi.releaseEnums.and.returnValue(deferred.promise);

        var getEnumsDefer = $q.defer();
        mocks.localData.getEnum.and.returnValue(getEnumsDefer.promise);

        mocks.remiapi.get = window.jasmine.createSpyObj("get", ["products"]);
        var d = $q.defer();
        mocks.remiapi.get.products.and.returnValue(d.promise);

        spyOn(mocks.$scope, "$on").and.callThrough();
    }));

    function prepareSystemUnderTest() {
        inject(function ($controller) {
            sut = $controller("releaseCalendar", mocks);
        });
    }

    it("should call initialization methods when activated", function () {
        prepareSystemUnderTest();

        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("releaseCalendar");
        expect(mocks.$scope.$on).toHaveBeenCalledWith("$destroy", jasmine.any(Function));
        expect(mocks.common.activateController).toHaveBeenCalledWith(jasmine.any(Array), "releaseCalendar");
        expect(mocks.localData.getEnum).toHaveBeenCalledWith("ReleaseType");
        expect(mocks.common.handleEvent).toHaveBeenCalledWith("businessUnitsLoaded", jasmine.any(Function), mocks.$scope);
    });
});

