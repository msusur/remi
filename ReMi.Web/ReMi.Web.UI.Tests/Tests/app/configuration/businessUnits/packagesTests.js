describe("Products Controller", function () {
    var sut, mocks, logger;
    var activateDeferred, executeRequestDeferred, getProductsDeferred,
         getReleaseTrackDeferred, releaseTracksDeferred;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope) {
        activateDeferred = $q.defer();
        getProductsDeferred = $q.defer();
        executeRequestDeferred = $q.defer();
        getReleaseTrackDeferred = $q.defer();
        releaseTracksDeferred = $q.defer();
        var mockedPromise = { then: function (callback) { callback(mocks.localData.businessUnits); } };
        mocks = {
            $scope: $rootScope.$new(),
            $rootScope: $rootScope,
            common: {
                logger: window.jasmine.createSpyObj("logger", ["getLogger"]),
                activateController: window.jasmine.createSpy("activateController"),
                handleEvent: jasmine.createSpy("handleEvent"),
                $q: $q
            },
            config: { events: { businessUnitsLoaded: "businessUnitsLoaded" } },
            remiapi: window.jasmine.createSpyObj("remiapi", ["getProducts", "getReleaseTrack", "executeCommand"]),
            localData: {
                businessUnitsPromise: function () {
                    return mockedPromise;
                },
                businessUnits: angular.copy(businessUnits),
                getEnum: jasmine.createSpy("getEnum").and.returnValue(releaseTracksDeferred.promise)
            }
        };
        logger = window.jasmine.createSpyObj("logger", ["console", "error", "info", "warn"]);

        mocks.remiapi.post = window.jasmine.createSpyObj("post", [
            "addProduct", "updateProduct"
        ]);
        mocks.remiapi.post.addProduct.and.returnValue(executeRequestDeferred.promise);
        mocks.remiapi.post.updateProduct.and.returnValue(executeRequestDeferred.promise);
        mocks.remiapi.executeCommand.and.returnValue(executeRequestDeferred.promise);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.common.activateController.and.returnValue(activateDeferred.promise);
        mocks.remiapi.getProducts.and.returnValue(getProductsDeferred.promise);
        mocks.remiapi.getReleaseTrack.and.returnValue(getReleaseTrackDeferred.promise);
        inject(function ($controller) {
            sut = $controller("products", mocks);
        });
    }));

    it("should call initialization methods", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("products");
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), "products");
    });

    it("should activate view", function () {
        sut.activate();
        activateDeferred.resolve();
        mocks.$scope.$digest();

        expect(logger.console).toHaveBeenCalledWith("Activated Products View");
    });

    it("should get products", function () {
        sut.track = releaseTracks;

        sut.getProducts();

        expect(sut.products.length).toEqual(19);
        expect(sut.businessUnits.length).toEqual(7);
        expect(sut.products[0]).toEqual({
            Name: "Eu Accumsan Incorporated",
            ExternalId: "0f1ffaaa-48bb-432c-952f-9e1e67c6eb33",
            ReleaseTrack: "Manual",
            ChooseTicketsByDefault: true,
            IsDefault: false,
            BusinessUnit: "Iowa",
            BusinessUnitDesc: "Iowa",
            BusinessUnitId: "9f13aee7-3da1-4ceb-a25f-e320fd7c91f5",
            ReleaseTrackDescription: "Manual"
        });
    });

    it("should set empty products and business units, when local data business units are empty", function () {
        sut.track = releaseTracks;
        mocks.localData.businessUnitsPromise().then = function (callback) { callback(undefined); };

        sut.getProducts();

        expect(sut.products).toEqual([]);
        expect(sut.businessUnits).toEqual([]);
    });

    it("should avoid adding empty product", function () {
        sut.productToManage = { name: "" };

        sut.addProduct();

        expect(logger.warn).toHaveBeenCalledWith("Cannot add empty product");
    });

    it("should avoid adding existing product", function () {
        sut.productToManage = { name: "xxx" };
        sut.products = [{ Name: "test" }, { Name: "xxx" }];

        sut.addProduct();

        expect(logger.warn).toHaveBeenCalledWith("Product already exists");
    });

    it("should add product to controler list, to internal and local business unit, when called", function () {
        sut.track = releaseTracks;
        sut.getProducts();
        sut.productToManage = {
            Description: "new name",
            ExternalId: "new id",
            BusinessUnitId: sut.businessUnits[0].ExternalId,
            ReleaseTrack: releaseTracks[0],
            ChooseTicketsByDefault: true
        };
        spyOn($.fn, "modal");

        sut.addProduct();
        executeRequestDeferred.resolve();
        mocks.$scope.$digest();

        expect(sut.products.length).toEqual(20);
        expect(sut.products[19].Name).toEqual("new name");
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.products[0].BusinessUnitId).toEqual(sut.businessUnits[0].ExternalId);
        expect(sut.products[0].BusinessUnit).toEqual(sut.businessUnits[0].Name);
        expect(sut.products[0].BusinessUnitDesc).toEqual(sut.businessUnits[0].Description);
        expect(mocks.remiapi.post.addProduct).toHaveBeenCalledWith({
            ExternalId: "new id",
            Description: "new name",
            ChooseTicketsByDefault: true,
            ReleaseTrack: "Manual",
            BusinessUnitId: "9f13aee7-3da1-4ceb-a25f-e320fd7c91f5"
        });
        expect($("#manageProductModal").modal).toHaveBeenCalledWith("hide");
        expect(sut.businessUnits[0].Packages.length).toEqual(2);
        expect(sut.businessUnits[0].Packages[1].ExternalId).toEqual("new id");
    });

    it("should reject adding products", function () {
        sut.productToManage = {
            Description: "sdasd",
            ReleaseTrack: { Name: "sa", Description: "sa" }
        };
        sut.products = [{ Description: "test", ReleaseTrack: "sa" }];
        spyOn($.fn, "modal");

        sut.addProduct();
        executeRequestDeferred.reject("some error");
        mocks.$scope.$digest();

        expect(sut.products.length).toEqual(1);
        expect(logger.error).toHaveBeenCalledWith("Cannot add product");
        expect(logger.console).toHaveBeenCalledWith("some error");
        expect(sut.state.isBusy).toEqual(false);
        expect($("#manageProductModal").modal).toHaveBeenCalledWith("hide");
    });

    it("should show add product modal", function () {
        spyOn($.fn, "modal");
        sut.track = releaseTracks;
        sut.getProducts();

        sut.showManageProductModal();

        expect(sut.productToManage.Description).toEqual("");
        expect(sut.productToManage.ReleaseTrack).toEqual(sut.track[0]);
        expect($("#manageProductModal").modal).toHaveBeenCalledWith("show");
    });


    it("should show update product modal", function () {
        spyOn($.fn, "modal");
        sut.track = releaseTracks;
        sut.getProducts();

        sut.showManageProductModal({
            ReleaseTrack: "Manual",
            Name: "smth",
            ExternalId: "id"
        });

        expect(sut.productToManage.Description).toEqual("smth");
        expect(sut.productToManage.ReleaseTrack).toEqual(sut.track[0]);
        expect(sut.productToManage.ExternalId).toEqual("id");
        expect($("#manageProductModal").modal).toHaveBeenCalledWith("show");
    });

    it("should not show add product modal, when no business units", function () {
        spyOn($.fn, "modal");

        sut.showManageProductModal();

        expect(logger.warn).toHaveBeenCalledWith("No Business Units available");
        expect($("#manageProductModal").modal).not.toHaveBeenCalledWith("show");
    });

    it("should update product and not change business units, when called and business unit has not changed", function () {
        sut.track = releaseTracks;
        sut.getProducts();
        sut.productToManage = {
            Description: "new name",
            ExternalId: sut.products[0].ExternalId,
            BusinessUnitId: sut.businessUnits[0].ExternalId,
            ReleaseTrack: releaseTracks[0],
            ChooseTicketsByDefault: true
        };
        spyOn($.fn, "modal");

        sut.updateProduct();
        executeRequestDeferred.resolve();
        mocks.$scope.$digest();

        expect(sut.products.length).toEqual(19);
        expect(sut.products[0].Name).toEqual("new name");
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.products[0].BusinessUnitId).toEqual(sut.businessUnits[0].ExternalId);
        expect(sut.products[0].BusinessUnit).toEqual(sut.businessUnits[0].Name);
        expect(sut.products[0].BusinessUnitDesc).toEqual(sut.businessUnits[0].Description);
        expect(mocks.remiapi.post.updateProduct).toHaveBeenCalledWith({
            ExternalId: sut.products[0].ExternalId,
            Description: "new name",
            ChooseTicketsByDefault: true,
            ReleaseTrack: "Manual",
            BusinessUnitId: "9f13aee7-3da1-4ceb-a25f-e320fd7c91f5"
        });
        expect($("#manageProductModal").modal).toHaveBeenCalledWith("hide");
        expect(sut.businessUnits[0].Packages.length).toEqual(1);
        expect(mocks.localData.businessUnits[0].Packages.length).toEqual(1);
    });

    it("should update product and change business units, when called and business unit has changed", function () {
        sut.track = releaseTracks;
        sut.getProducts();
        sut.productToManage = {
            Description: "new name",
            ExternalId: sut.products[0].ExternalId,
            BusinessUnitId: sut.businessUnits[1].ExternalId,
            ReleaseTrack: releaseTracks[0],
            ChooseTicketsByDefault: true
        };
        spyOn($.fn, "modal");

        sut.updateProduct();
        executeRequestDeferred.resolve();
        mocks.$scope.$digest();

        expect(sut.products.length).toEqual(19);
        expect(sut.products[0].Name).toEqual("new name");
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.products[0].BusinessUnitId).toEqual(sut.businessUnits[1].ExternalId);
        expect(sut.products[0].BusinessUnit).toEqual(sut.businessUnits[1].Name);
        expect(sut.products[0].BusinessUnitDesc).toEqual(sut.businessUnits[1].Description);
        expect(mocks.remiapi.post.updateProduct).toHaveBeenCalledWith({
            ExternalId: sut.products[0].ExternalId,
            Description: "new name",
            ChooseTicketsByDefault: true,
            ReleaseTrack: "Manual",
            BusinessUnitId: sut.businessUnits[1].ExternalId
        });
        expect($("#manageProductModal").modal).toHaveBeenCalledWith("hide");
        expect(sut.businessUnits[0].Packages.length).toEqual(0);
        expect(sut.businessUnits[1].Packages.length).toEqual(3);
        expect(mocks.localData.businessUnits[0].Packages.length).toEqual(0);
        expect(mocks.localData.businessUnits[1].Packages.length).toEqual(3);
    });

    it("should reject updating products", function () {
        sut.productToManage = {
            Description: "xxx", ReleaseTrack: {
                Name: "b", Description: "b"
            },
            ExternalId: "id",
            ChooseTicketsByDefault: true
        };
        sut.products = [{
            Description: "xxx", ReleaseTrack: {
                Name: "b", Description: "b"
            },
            ExternalId: "id",
            ChooseTicketsByDefault: false
        }];
        spyOn($.fn, "modal");

        sut.updateProduct();
        executeRequestDeferred.reject("some error");
        mocks.$scope.$digest();

        expect(sut.products.length).toEqual(1);
        expect(sut.products[0].Description).toEqual("xxx");
        expect(sut.products[0].ChooseTicketsByDefault).toBe(false);
        expect(logger.error).toHaveBeenCalledWith("Cannot update product");
        expect(logger.console).toHaveBeenCalledWith("some error");
        expect(sut.state.isBusy).toEqual(false);
        expect($("#manageProductModal").modal).toHaveBeenCalledWith("hide");
    });

    it("should fill out business units, when businesUnitsLoaded event handled", function () {
        var handler;
        mocks.common.handleEvent.and.callFake(function (event, callback) {
            handler = callback;
        });
        inject(function ($controller) {
            sut = $controller("products", mocks);
        });
        sut.getProducts();
        expect(sut.businessUnits.length).toEqual(7);
        mocks.localData.businessUnits = [{
            "ExternalId": "9f13aee7-3da1-4ceb-a25f-e320fd7c91f5",
            "Name": "Iowa",
            "Description": "Iowa",
            "Packages": [
                { "Name": "Eu Accumsan Incorporated", "ExternalId": "0f1ffaaa-48bb-432c-952f-9e1e67c6eb33", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": true, "IsDefault": false }
            ]
        }];

        handler(businessUnits);


        expect(sut.businessUnits.length).toEqual(1);
        expect(sut.products.length).toEqual(1);
    });
});

var businessUnits = [
    {
        "ExternalId": "9f13aee7-3da1-4ceb-a25f-e320fd7c91f5",
        "Name": "Iowa",
        "Description": "Iowa",
        "Packages": [
            { "Name": "Eu Accumsan Incorporated", "ExternalId": "0f1ffaaa-48bb-432c-952f-9e1e67c6eb33", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": true, "IsDefault": false }
        ]
    },
    {
        "ExternalId": "823ff88d-3b0d-4f58-b976-fbe6ecfbdcbe",
        "Name": "NI",
        "Description": "North Island",
        "Packages": [
            { "Name": "Adipiscing Corp.", "ExternalId": "bc79f340-fd28-47f4-94c7-cb98963ba573", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Adipiscing Corp. Demeter", "ExternalId": "c4979b08-2cd8-4111-8253-6b35766ba96f", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false }
        ]
    },
    {
        "ExternalId": "9246cf63-5d00-429a-a114-8db53ee375eb",
        "Name": "Vi",
        "Description": "Victoria",
        "Packages": [
            { "Name": "Mi Consulting", "ExternalId": "61ee6437-4029-408c-847d-73e84cc26819", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": true, "IsDefault": true }
        ]
    },
    {
        "ExternalId": "ed47c52a-39ed-4fb7-8b69-31f715023c2f",
        "Name": "Adana",
        "Description": "Adana",
        "Packages": [
            { "Name": "Ipsum Leo PC", "ExternalId": "0090e2a0-4f35-456f-ce7e-7da026f1cf74", "ReleaseTrack": "Automated", "ChooseTicketsByDefault": true, "IsDefault": false }
        ]
    },
    {
        "ExternalId": "86ed308c-1b91-46bb-b6c3-5e689db7b2df",
        "Name": "Wie",
        "Description": "Waals-Brabant",
        "Packages": [
            { "Name": "Non Inc.", "ExternalId": "01aa0e1d-8da1-48d6-9483-c15cba56d666", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": true, "IsDefault": false }
        ]
    },
    {
        "ExternalId": "29be4364-c96b-4f1f-bc80-3ef40c016ffe",
        "Name": "SA",
        "Description": "Saskatchewan",
        "Packages": [
            { "Name": "Consequat Lectus Sit PC", "ExternalId": "48853fce-3371-4bf0-a332-90e26fc9804f", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false }
        ]
    },
    {
        "ExternalId": "6c84143d-8b15-4168-b9cc-7ace6c7a98aa",
        "Name": "HE-GE",
        "Description": "Henegouwen",
        "Packages": [
            { "Name": "Felis Ltd", "ExternalId": "c6f3e15c-1f54-469a-3826-eddc3403cc75", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "KY", "ExternalId": "78d33ba2-39f9-40d6-8367-3638a70d8a92", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Auctor Limited", "ExternalId": "c7d9fd7f-7dd7-4962-4a1b-bb18a0c1a44b", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": true, "IsDefault": false },
            { "Name": "Ignatius Sweet", "ExternalId": "46482aea-b9c6-4d17-b3ba-71765368bb19", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Payroll", "ExternalId": "890011ae-06fa-4576-891b-8edfe71281e0", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": true, "IsDefault": false },
            { "Name": "Remediation", "ExternalId": "3c5ae728-befb-43a8-c70e-5ea1b9812b43", "ReleaseTrack": "Automated", "ChooseTicketsByDefault": true, "IsDefault": false },
            { "Name": "Pede Nonummy Ut LLC", "ExternalId": "2ae5175b-9e20-43c2-825a-8ea39f37edec", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Rhoncus Associates", "ExternalId": "d32ac36d-822d-4048-2296-b61a3f7e3384", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Queensland", "ExternalId": "e289a382-82d8-4a40-a336-3e7f4b14ce76", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Connacht", "ExternalId": "ef02646c-4390-4a59-be66-9ccf0885491e", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Ullamcorper", "ExternalId": "d3b8900b-d73b-49fe-bf32-bd16fc558f89", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Lobortis Quis Corp.", "ExternalId": "8f100e45-bb2a-466d-a132-11e5e120defc", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false }
        ]
    }
];
var releaseTracks = [{ "Id": 1, "Name": "Manual", "Description": "Manual" },
            { "Id": 2, "Name": "PreApproved", "Description": "Pre-approved" },
            { "Id": 3, "Name": "Automated", "Description": "Automated" }];
