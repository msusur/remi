describe("Plugins Controller", function () {
    var sut, mocks, logger;
    var activateDeferred, businessUnitsDeferred, getEnumDeferred;

    beforeEach(function () {
        module("app");
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope, $routeParams, $route) {
        activateDeferred = $q.defer();
        businessUnitsDeferred = $q.defer();
        getEnumDeferred = $q.defer();
        mocks = {
            $scope: $rootScope.$new(),
            $rootScope: $rootScope,
            $routeParams: $routeParams,
            $route: $route,
            common: {
                logger: window.jasmine.createSpyObj("logger", ["getLogger"]),
                activateController: window.jasmine.createSpy("activateController"),
                handleEvent: window.jasmine.createSpy("handleEvent"),
                $q: $q,
                $broadcast: jasmine.createSpy("$broadcast")
            },
            config: {
                events: {
                    spinnerToggle: "spinnerToggle",
                    businessUnitsLoaded: "businessUnitsLoaded",
                    locationChangeSuccess: "locationChangeSuccess"
                }
            },
            localData: {
                businessUnitsPromise: function () { return businessUnitsDeferred.promise; },
                getEnum: jasmine.createSpy("getEnum")
            },
            remiapi: {
                get: jasmine.createSpyObj("remiapi.get", ["globalPlugins", "packagePlugins"]),
                post: jasmine.createSpyObj("remiapi.post", ["assignGlobalPlugin", "assignPackagePlugin"])
            },
            $q: $q
        };
        logger = window.jasmine.createSpyObj("logger", ["console", "error", "info", "warn"]);
        spyOn(mocks.$route, "updateParams");
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.common.activateController.and.returnValue(activateDeferred.promise);
        mocks.localData.getEnum.and.returnValue(getEnumDeferred.promise);
    }));

    function prepareSut() {
        inject(function ($controller) {
            sut = $controller("plugins", mocks);
        });
    }

    describe("Initialization", function () {
        it("should call activateController method, when created", function () {
            prepareSut();
            activateDeferred.resolve();
            mocks.$scope.$digest();

            expect(sut).toBeDefined();

            expect(mocks.common.handleEvent).toHaveBeenCalledWith(mocks.config.events.businessUnitsLoaded, jasmine.any(Function), mocks.$scope);
            expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("plugins");
            expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), "plugins");
            expect(logger.console).toHaveBeenCalledWith("Activated Plugins View");
            expect(mocks.common.$broadcast).toHaveBeenCalledWith(mocks.config.events.spinnerToggle, jasmine.any(Object));
            expect(mocks.common.$broadcast.calls.count()).toEqual(1);
            expect(sut.state.isBusy).toEqual(true);
            expect(sut.tabName).toEqual("globalConfiguration");
            expect(sut.activeTab).toEqual({
                "globalConfiguration": { isCurrent: true, loaded: true },
                "packageConfiguration": { isCurrent: false, loaded: false },
                "plugins": { isCurrent: false, loaded: false }
            });
            expect(mocks.common.handleEvent).toHaveBeenCalledWith(mocks.config.events.locationChangeSuccess, jasmine.any(Function), mocks.$scope);
            expect(mocks.$route.updateParams).toHaveBeenCalledWith({ "tab": "globalConfiguration" });
        });

        it("should call activate packageConfiguration tab, when 'packageConfiguration' parameter in path", function () {
            mocks.$routeParams.tab = "packageConfiguration";
            prepareSut();
            activateDeferred.resolve();
            mocks.$scope.$digest();

            expect(sut).toBeDefined();

            expect(sut.tabName).toEqual("packageConfiguration");
            expect(sut.activeTab).toEqual({
                "globalConfiguration": { isCurrent: false, loaded: false },
                "packageConfiguration": { isCurrent: true, loaded: true },
                "plugins": { isCurrent: false, loaded: false }
            });
            expect(mocks.$route.updateParams).toHaveBeenCalledWith({ "tab": "packageConfiguration" });
        });

        it("should assign business units, when promise resolved", function () {
            prepareSut();

            var businessUnits = { Test: "test data" };
            businessUnitsDeferred.resolve(businessUnits);
            mocks.$scope.$digest();

            expect(sut.businessUnits).toEqual(businessUnits);
        });

        it("should assign resolved enum PluginType, when businessUnits resolved", function () {
            prepareSut();

            var pluginTypes = { Test: "Some test data" };
            businessUnitsDeferred.resolve({});
            getEnumDeferred.resolve(pluginTypes);
            mocks.$scope.$digest();

            expect(sut.pluginTypes).toEqual(pluginTypes);
        });

        it("should resolve initialization promise, show and hide spinner, when initialization finished", function () {
            prepareSut();

            var expectedInitialization = false;
            sut.initializationPromise.then(function () { expectedInitialization = true; });

            businessUnitsDeferred.resolve({});
            getEnumDeferred.resolve({});
            mocks.$scope.$digest();


            expect(mocks.common.$broadcast.calls.count()).toEqual(2);
            expect(expectedInitialization).toEqual(true);
            expect(sut.state.isBusy).toEqual(false);
        });
    });

    describe("tabChange", function () {
        it("should set active tab, when called", function () {
            prepareSut();

            expect(sut.tabName).toEqual("globalConfiguration");
            expect(mocks.$route.updateParams.calls.count()).toEqual(1);
            expect(mocks.$route.updateParams).toHaveBeenCalledWith({ "tab": "globalConfiguration" });

            sut.tabChange("plugins");

            expect(sut.tabName).toEqual("plugins");
            expect(sut.activeTab).toEqual({
                "globalConfiguration": { isCurrent: false, loaded: true },
                "packageConfiguration": { isCurrent: false, loaded: false },
                "plugins": { isCurrent: true, loaded: true }
            });
            expect(mocks.$route.updateParams.calls.count()).toEqual(2);
            expect(mocks.$route.updateParams).toHaveBeenCalledWith({ "tab": "plugins" });
        });
    });

    describe("BusinessUnitsLoaded", function () {
        it("should repopulate business units, when business units loaded", function () {
            var eventHandler = {};
            mocks.common.handleEvent.and.callFake(function (evt, handler) {
                eventHandler[evt] = handler;
            });
            prepareSut();

            expect(sut.businessUnits).toEqual([]);
            var businessUnits = { Test: "Test data" };
            eventHandler[mocks.config.events.businessUnitsLoaded](businessUnits);

            expect(sut.businessUnits).toEqual(businessUnits);

            //check if it was copied
            businessUnits.Test = "different data";
            expect(sut.businessUnits).not.toEqual(businessUnits);
        });
    });

    describe("LocationChangeSuccess", function () {
        it("should switch active tab, when triggered", function () {
            var eventHandler = {};
            mocks.common.handleEvent.and.callFake(function (evt, handler) {
                eventHandler[evt] = handler;
            });
            mocks.$route.current = {
                $$route: { title: "plugin" },
                params: { tab: "packageConfiguration" }
            };
            prepareSut();

            expect(sut.tabName).toEqual("globalConfiguration");

            eventHandler[mocks.config.events.locationChangeSuccess]();

            expect(sut.tabName).toEqual("packageConfiguration");
            expect(sut.activeTab).toEqual({
                "globalConfiguration": { isCurrent: false, loaded: true },
                "packageConfiguration": { isCurrent: true, loaded: true },
                "plugins": { isCurrent: false, loaded: false }
            });
        });
    });
});
