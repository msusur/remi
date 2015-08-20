describe("Topnav Controller", function () {
    var sut, mocks, logger, getBusinessUnitsDeferred;
    var businessUnitChangedEventHandler;

    beforeEach(function () {
        module("app");

        mocks = {
            common: {
                logger: jasmine.createSpyObj("logger", ["getLogger"]),
                activateController: jasmine.createSpy("activateController"),
                $broadcast: jasmine.createSpy("$broadcast"),
                handleEvent: jasmine.createSpy("handleEvent")
            },
            config: {
                events: {
                    loggedOut: "auth.loggedOut",
                    loggedIn: "auth.loggedIn",
                    productsAddedForUser: "productsAddedForUser",
                    productContextChanged: "productContextChanged",
                    businessUnitsLoaded: "businessUnitsLoaded",
                    notificationReceived: "notificationReceived"
                }
            },
            authService: { identity: { fullname: "full name" }, isLoggedIn: true },
            localData: {
                businessUnitsResolve: jasmine.createSpy("businessUnitsResolve")
            },
            remiapi: { get: jasmine.createSpyObj("get", ["businessUnits"]) },
            $location: jasmine.createSpyObj("$location", ["path"]),
            notifications: jasmine.createSpyObj("notifications", ["subscribe", "unsubscribe"])
        };
        logger = window.jasmine.createSpyObj("logger", ["console", "error", "info", "warn"]);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.common.handleEvent.and.callFake(function(event, callback) {
            businessUnitChangedEventHandler = callback;
        });

        inject(function ($controller, _$rootScope_, _$q_) {
            mocks.$rootScope = _$rootScope_;
            mocks.$q = _$q_;
            mocks.$scope = _$rootScope_.$new();
            getBusinessUnitsDeferred = _$q_.defer();
            spyOn(mocks.$rootScope, "$on").and.callThrough();
            mocks.remiapi.get.businessUnits.and.returnValue(getBusinessUnitsDeferred.promise);
            sut = $controller("topnav", mocks);
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("topnav");
        expect(sut.title).toEqual("R.e.M.i.");
        expect(sut.isLoggedIn).toEqual(true);
        expect(sut.userDisplayName).toEqual("full name");
        expect(mocks.$rootScope.$on).toHaveBeenCalledWith("auth.loggedOut", jasmine.any(Function));
        expect(mocks.$rootScope.$on).toHaveBeenCalledWith("auth.loggedIn", jasmine.any(Function));
        expect(mocks.$rootScope.$on).toHaveBeenCalledWith("productsAddedForUser", jasmine.any(Function));
        expect(mocks.$rootScope.$on).toHaveBeenCalledWith("productContextChanged", jasmine.any(Function));
        expect(mocks.common.activateController).toHaveBeenCalledWith([jasmine.any(Object)], "topnav");
        expect(mocks.remiapi.get.businessUnits).toHaveBeenCalled();
        expect(mocks.common.handleEvent).toHaveBeenCalledWith(mocks.config.events.notificationReceived, jasmine.any(Function), mocks.$scope);
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith("BusinessUnitsChangedEvent", {});
    });

    it("should populate business units and packages, when loaded from server", function () {
        var response = {
            BusinessUnits: [
                { Packages: [{ IsDefault: false, Name: "1.1" }], Name: "1" },
                { Packages: [{ IsDefault: false, Name: "2.1" }, { IsDefault: true, Name: "2.2" }], Name: "2" },
                { Packages: [{ IsDefault: false, Name: "3.1" }], Name: "3" }
            ]
        }

        getBusinessUnitsDeferred.resolve(response);
        mocks.$scope.$digest();

        expect(sut.businessUnit.Name).toEqual("2");
        expect(sut.package.Name).toEqual("2.2");
        expect(sut.businessUnits).toEqual(response.BusinessUnits);
        expect(mocks.localData.businessUnitsResolve).toHaveBeenCalledWith(response.BusinessUnits);
        expect(mocks.common.$broadcast).toHaveBeenCalledWith("businessUnitsLoaded", response.BusinessUnits);
    });

    it("should log error, when business unites failed to load", function () {

        getBusinessUnitsDeferred.reject();
        mocks.$scope.$digest();

        expect(sut.businessUnit).toBeUndefined();
        expect(sut.package).toBeUndefined();
        expect(sut.businessUnits).toBeUndefined();
        expect(mocks.localData.businessUnits).toBeUndefined();
        expect(logger.error).toHaveBeenCalledWith("Cannot load business units");
    });

    it("should logout and relocate to login page, when logout invoked", function () {
        var deferred = mocks.$q.defer();
        var search = jasmine.createSpy("search");
        mocks.authService.logout = jasmine.createSpy("logout");
        mocks.authService.logout.and.returnValue(deferred.promise);

        mocks.$location.path.and.returnValue({ search: search });

        sut.logOut();
        deferred.resolve();
        mocks.$scope.$digest();

        expect(mocks.authService.logout).toHaveBeenCalled();
        expect(mocks.$location.path).toHaveBeenCalledWith("/");
        expect(search).toHaveBeenCalledWith({});
    });

    it("should select new package, when business unit and packages were choosen", function () {
        var bu = { Name: "business unit" }, $package = { Name: "package " };
        mocks.authService.selectProduct = jasmine.createSpy("selectProduct");

        sut.productChanged(bu, $package);

        expect(sut.businessUnit).toEqual(bu);
        expect(sut.package).toEqual($package);
        expect(mocks.authService.selectProduct).toHaveBeenCalledWith($package);
    });

    it("should filled out user data and pupulate business units, when logged in", function () {
        var data = { fullname: "some name" };

        mocks.$rootScope.$broadcast("auth.loggedIn", data);

        expect(sut.isLoggedIn).toEqual(true);
        expect(sut.userDisplayName).toEqual("some name");
        expect(mocks.remiapi.get.businessUnits).toHaveBeenCalled();
        expect(mocks.remiapi.get.businessUnits.calls.count()).toEqual(2);
    });

    it("should pupulate user business units, when product assign to user", function () {
        mocks.$rootScope.$broadcast("productsAddedForUser", {});

        expect(mocks.remiapi.get.businessUnits.calls.count()).toEqual(2);
    });

    it("should clear user data, when logged out", function () {
        mocks.$rootScope.$broadcast("auth.loggedOut", {});

        expect(sut.isLoggedIn).toEqual(false);
        expect(sut.userDisplayName).toEqual("");
        expect(sut.businessUnits).toEqual([]);
        expect(sut.package).toBeNull();
        expect(mocks.remiapi.get.businessUnits.calls.count()).toEqual(1);
    });

    it("should set package and business unit, when product context has changed", function () {
        sut.businessUnits = [
            { Packages: [{ IsDefault: false, ExternalId: "1.1" }], ExternalId: "1" },
            { Packages: [{ IsDefault: false, ExternalId: "2.1" }, { IsDefault: true, ExternalId: "2.2" }], ExternalId: "2" },
            { Packages: [{ IsDefault: false, ExternalId: "3.1" }], ExternalId: "3" }
        ];

        mocks.$rootScope.$broadcast("productContextChanged", { ExternalId: "3.1" });

        expect(sut.businessUnit.ExternalId).toEqual("3");
        expect(sut.package.ExternalId).toEqual("3.1");
        expect(mocks.remiapi.get.businessUnits.calls.count()).toEqual(1);
    });

    it("should not do anything, when product context has changed and businessUnits are empty", function () {
        mocks.$rootScope.$broadcast("productContextChanged", { ExternalId: "3.1" });

        expect(sut.businessUnit).toBeUndefined();
        expect(sut.package).toBeUndefined();
    });

    it("should refresh business unit, when BusinessUnitsChangedEvent is handled", function () {
        businessUnitChangedEventHandler({ name: "BusinessUnitsChangedEvent" });

        expect(mocks.remiapi.get.businessUnits).toHaveBeenCalled();
    });

    it("should unsubscribe BusinessUnitsChangedEvent, when controller destroyed", function () {
        mocks.$scope.$broadcast("$destroy");

        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith("BusinessUnitsChangedEvent");
    });
});

