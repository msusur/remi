describe("Business Units Controller", function () {
    var sut, mocks, logger;
    var activateDeferred, businessUnitDeferred;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope) {
        activateDeferred = $q.defer();
        businessUnitDeferred = $q.defer();
        mocks = {
            $scope: $rootScope.$new(),
            common: {
                logger: window.jasmine.createSpyObj("logger", ["getLogger"]),
                activateController: window.jasmine.createSpy("activateController"),
                handleEvent: jasmine.createSpy("handleEvent"),
                $broadcast: jasmine.createSpy("$broadcast"),
                $q: $q
            },
            config: { events: { businessUnitsLoaded: "businessUnitsLoaded" } },
            remiapi: {
                post: window.jasmine.createSpyObj("post", ["addBusinessUnit", "updateBusinessUnit", "removeBusinessUnit"])
            },
            localData: {
                businessUnitsPromise: jasmine.createSpy("businessUnitsPromise"),
                businessUnits: angular.copy(businessUnits)
            }
        };
        logger = window.jasmine.createSpyObj("logger", ["console", "error", "info", "warn"]);

        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.common.activateController.and.returnValue(activateDeferred.promise);
        mocks.localData.businessUnitsPromise.and.returnValue(businessUnitDeferred.promise);
    }));

    function prepareSut() {
        inject(function ($controller) {
            sut = $controller("businessUnits", mocks);
        });
    }

    describe("Initialization", function () {
        it("should call initialization methods", function () {
            prepareSut();

            expect(sut).toBeDefined();
            expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("businessUnits");
            expect(mocks.common.handleEvent).toHaveBeenCalledWith(mocks.config.events.businessUnitsLoaded, jasmine.any(Function), mocks.$scope);
            expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), "businessUnits");
            expect(mocks.localData.businessUnitsPromise).toHaveBeenCalled();
        });

        it("should log in console, when controller activated", function () {
            prepareSut();

            activateDeferred.resolve();
            mocks.$scope.$digest();

            expect(logger.console).toHaveBeenCalledWith("Activated Business Units View");
        });

        it("should get business units", function () {
            prepareSut();

            businessUnitDeferred.resolve(mocks.localData.businessUnits);
            mocks.$scope.$digest();

            expect(sut.businessUnits.length).toEqual(7);
            expect(sut.businessUnits[0]).toEqual({
                "ExternalId": "9f13aee7-3da1-4ceb-a25f-e320fd7c91f5",
                "Name": "Iowa",
                "Description": "Iowa"
            });
        });
    });

    describe("Add Business Unit", function () {
        it("should do nothnig, when business unit to add is undefined", function () {
            prepareSut();

            sut.addBusinessUnit();

            expect(mocks.remiapi.post.addBusinessUnit).not.toHaveBeenCalled();
            expect(logger.warn).toHaveBeenCalledWith("Cannot add empty business unit");
        });

        it("should do nothnig, when Name is empty", function () {
            prepareSut();
            sut.businessUnitToManage = { Name: "businessUnit", Description: "" };

            sut.addBusinessUnit();

            expect(mocks.remiapi.post.addBusinessUnit).not.toHaveBeenCalled();
            expect(logger.warn).toHaveBeenCalledWith("Cannot add empty business unit");
        });

        it("should do nothnig, when Description is empty", function () {
            prepareSut();
            sut.businessUnitToManage = { Name: "", Description: "business unit" };

            sut.addBusinessUnit();

            expect(mocks.remiapi.post.addBusinessUnit).not.toHaveBeenCalled();
            expect(logger.warn).toHaveBeenCalledWith("Cannot add empty business unit");
        });

        it("should do nothnig, when Name already exists", function () {
            prepareSut();
            sut.businessUnits = [{ Name: "businessUnit", Description: "desc" }];
            sut.businessUnitToManage = { Name: "businessUnit", Description: "business unit" };

            sut.addBusinessUnit();

            expect(mocks.remiapi.post.addBusinessUnit).not.toHaveBeenCalled();
            expect(logger.warn).toHaveBeenCalledWith("Business Unit already exists");
        });

        it("should do nothnig, when Description already exists", function () {
            prepareSut();
            sut.businessUnits = [{ Name: "name", Description: "business unit" }];
            sut.businessUnitToManage = { Name: "businessUnit", Description: "business unit" };

            sut.addBusinessUnit();

            expect(mocks.remiapi.post.addBusinessUnit).not.toHaveBeenCalled();
            expect(logger.warn).toHaveBeenCalledWith("Business Unit already exists");
        });

        it("should update local data business unit, when successfully added", function () {
            businessUnitDeferred.resolve(mocks.localData.businessUnits);
            prepareSut();
            sut.businessUnitToManage = {
                Name: "newBusinessUnit",
                Description: "new Business Unit",
                ExternalId: "externalId"
            };
            var remiapiPromise = mocks.common.$q.when();
            mocks.remiapi.post.addBusinessUnit.and.returnValue(remiapiPromise);
            spyOn(sut, "hideModal");

            sut.addBusinessUnit();
            mocks.$scope.$digest();

            expect(mocks.remiapi.post.addBusinessUnit).toHaveBeenCalledWith(sut.businessUnitToManage);
            expect(mocks.localData.businessUnits[mocks.localData.businessUnits.length - 1]).toEqual(jasmine.objectContaining(sut.businessUnitToManage));
            expect(mocks.localData.businessUnits.length).toEqual(8);
            expect(mocks.common.$broadcast).toHaveBeenCalledWith(mocks.config.events.businessUnitsLoaded, mocks.localData.businessUnits);
            expect(sut.state.isBusy).toBeFalsy();
            expect(sut.hideModal).toHaveBeenCalled();
        });

        it("should not update local data business unit, when failed to add", function () {
            businessUnitDeferred.resolve(mocks.localData.businessUnits);
            prepareSut();
            sut.businessUnitToManage = {
                Name: "newBusinessUnit",
                Description: "new Business Unit",
                ExternalId: "externalId"
            };
            var remiapiPromise = mocks.common.$q.reject("error message");
            mocks.remiapi.post.addBusinessUnit.and.returnValue(remiapiPromise);
            spyOn(sut, "hideModal");

            sut.addBusinessUnit();
            mocks.$scope.$digest();

            expect(mocks.common.$broadcast).not.toHaveBeenCalledWith(mocks.config.events.businessUnitsLoaded, mocks.localData.businessUnits);
            expect(mocks.localData.businessUnits.length).toEqual(7);
            expect(logger.error).toHaveBeenCalledWith("Cannot add business unit");
            expect(logger.console).toHaveBeenCalledWith("error message");
            expect(sut.state.isBusy).toBeFalsy();
            expect(sut.hideModal).not.toHaveBeenCalled();
        });
    });

    describe("Update Business Unit", function () {
        it("should do nothnig, when business unit to add is undefined", function () {
            prepareSut();

            sut.updateBusinessUnit();

            expect(mocks.remiapi.post.updateBusinessUnit).not.toHaveBeenCalled();
            expect(logger.warn).toHaveBeenCalledWith("Cannot add empty business unit");
        });

        it("should do nothnig, when Name is empty", function () {
            prepareSut();
            sut.businessUnitToManage = { Name: "businessUnit", Description: "" };

            sut.updateBusinessUnit();

            expect(mocks.remiapi.post.updateBusinessUnit).not.toHaveBeenCalled();
            expect(logger.warn).toHaveBeenCalledWith("Cannot add empty business unit");
        });

        it("should do nothnig, when Description is empty", function () {
            prepareSut();
            sut.businessUnitToManage = { Name: "", Description: "business unit" };

            sut.updateBusinessUnit();

            expect(mocks.remiapi.post.updateBusinessUnit).not.toHaveBeenCalled();
            expect(logger.warn).toHaveBeenCalledWith("Cannot add empty business unit");
        });

        it("should do nothnig, when Name already exists", function () {
            businessUnitDeferred.resolve(mocks.localData.businessUnits);
            prepareSut();
            mocks.$scope.$digest();
            sut.businessUnitToManage = { Name: "Iowa", Description: "business unit", ExternalId: "external Id" };

            sut.updateBusinessUnit();

            expect(mocks.remiapi.post.updateBusinessUnit).not.toHaveBeenCalled();
            expect(logger.warn).toHaveBeenCalledWith("Business Unit already exists");
        });

        it("should do nothnig, when Description already exists", function () {
            businessUnitDeferred.resolve(mocks.localData.businessUnits);
            prepareSut();
            mocks.$scope.$digest();
            sut.businessUnitToManage = { Name: "businessUnit", Description: "Victoria", ExternalId: "external Id" };

            sut.updateBusinessUnit();

            expect(mocks.remiapi.post.updateBusinessUnit).not.toHaveBeenCalled();
            expect(logger.warn).toHaveBeenCalledWith("Business Unit already exists");
        });

        it("should update local data business unit, when Name or Description are the same for same business unit", function () {
            businessUnitDeferred.resolve(mocks.localData.businessUnits);
            prepareSut();
            mocks.$scope.$digest();
            sut.businessUnitToManage = {
                Name: "Vi",
                Description: "Victoria",
                ExternalId: sut.businessUnits[2].ExternalId
            };
            var remiapiPromise = mocks.common.$q.when();
            mocks.remiapi.post.updateBusinessUnit.and.returnValue(remiapiPromise);

            sut.updateBusinessUnit();

            expect(mocks.remiapi.post.updateBusinessUnit).toHaveBeenCalledWith(sut.businessUnitToManage);
        });

        it("should update local data business unit, when successfully updated", function () {
            businessUnitDeferred.resolve(mocks.localData.businessUnits);
            prepareSut();
            sut.businessUnitToManage = {
                Name: "newBusinessUnit",
                Description: "new Business Unit",
                ExternalId: mocks.localData.businessUnits[2].ExternalId
            };
            var remiapiPromise = mocks.common.$q.when();
            mocks.remiapi.post.updateBusinessUnit.and.returnValue(remiapiPromise);
            spyOn(sut, "hideModal");

            sut.updateBusinessUnit();
            mocks.$scope.$digest();

            expect(mocks.remiapi.post.updateBusinessUnit).toHaveBeenCalledWith(sut.businessUnitToManage);
            expect(mocks.localData.businessUnits[2]).toEqual(jasmine.objectContaining(sut.businessUnitToManage));
            expect(mocks.localData.businessUnits.length).toEqual(7);
            expect(mocks.common.$broadcast).toHaveBeenCalledWith(mocks.config.events.businessUnitsLoaded, mocks.localData.businessUnits);
            expect(sut.state.isBusy).toBeFalsy();
            expect(sut.hideModal).toHaveBeenCalled();
        });

        it("should not update local data business unit, when failed to update", function () {
            businessUnitDeferred.resolve(mocks.localData.businessUnits);
            prepareSut();
            sut.businessUnitToManage = {
                Name: "newBusinessUnit",
                Description: "new Business Unit",
                ExternalId: mocks.localData.businessUnits[2].ExternalId
            };
            var remiapiPromise = mocks.common.$q.reject("error message");
            mocks.remiapi.post.updateBusinessUnit.and.returnValue(remiapiPromise);
            spyOn(sut, "hideModal");

            sut.updateBusinessUnit();
            mocks.$scope.$digest();

            expect(mocks.common.$broadcast).not.toHaveBeenCalledWith(mocks.config.events.businessUnitsLoaded, mocks.localData.businessUnits);
            expect(logger.error).toHaveBeenCalledWith("Cannot update business unit");
            expect(logger.console).toHaveBeenCalledWith("error message");
            expect(mocks.localData.businessUnits.length).toEqual(7);
            expect(sut.state.isBusy).toBeFalsy();
            expect(sut.hideModal).not.toHaveBeenCalled();
        });
    });

    describe("Remove Business Unit", function () {
        it("should update local data business unit, when successfully removed", function () {
            businessUnitDeferred.resolve(mocks.localData.businessUnits);
            prepareSut();
            mocks.$scope.$digest();
            var remiapiPromise = mocks.common.$q.when();
            mocks.remiapi.post.removeBusinessUnit.and.returnValue(remiapiPromise);

            sut.removeBusinessUnit(sut.businessUnits[2]);
            mocks.$scope.$digest();

            expect(mocks.remiapi.post.removeBusinessUnit).toHaveBeenCalledWith({ ExternalId: sut.businessUnits[2].ExternalId });
            expect(mocks.localData.businessUnits.length).toEqual(6);
            expect(mocks.common.$broadcast).toHaveBeenCalledWith(mocks.config.events.businessUnitsLoaded, mocks.localData.businessUnits);
            expect(sut.state.isBusy).toBeFalsy();
        });

        it("should not update local data business unit, when failed to remove", function () {
            businessUnitDeferred.resolve(mocks.localData.businessUnits);
            prepareSut();
            mocks.$scope.$digest();
            var remiapiPromise = mocks.common.$q.reject("error message");
            mocks.remiapi.post.removeBusinessUnit.and.returnValue(remiapiPromise);

            sut.removeBusinessUnit(sut.businessUnits[2]);
            mocks.$scope.$digest();

            expect(mocks.common.$broadcast).not.toHaveBeenCalledWith(mocks.config.events.businessUnitsLoaded, mocks.localData.businessUnits);
            expect(mocks.localData.businessUnits.length).toEqual(7);
            expect(logger.error).toHaveBeenCalledWith("Cannot remove business unit");
            expect(logger.console).toHaveBeenCalledWith("error message");
            expect(sut.state.isBusy).toBeFalsy();
        });
    });

    describe("Modal", function() {
        it("should show add business unit modal", function () {
            prepareSut();
            spyOn($.fn, "modal");
            spyOn(window, "newGuid");

            sut.showModal();

            expect(sut.businessUnitToManage.Description).toEqual("");
            expect(sut.businessUnitToManage.Name).toEqual("");
            expect(window.newGuid).toHaveBeenCalled();
            expect(sut.operationMode).toEqual("Add");
            expect($("#manageProductModal").modal).toHaveBeenCalledWith("show");
        });

        it("should show update business unit modal", function () {
            prepareSut();
            spyOn($.fn, "modal");

            sut.showModal({
                Description: "desc",
                Name: "name",
                ExternalId: "id"
            });

            expect(sut.businessUnitToManage.Description).toEqual("desc");
            expect(sut.businessUnitToManage.Name).toEqual("name");
            expect(sut.businessUnitToManage.ExternalId).toEqual("id");
            expect(sut.operationMode).toEqual("Update");
            expect($("#manageBusinessUnitModal").modal).toHaveBeenCalledWith("show");
        });

        it("should hide business unit modal", function () {
            prepareSut();
            spyOn($.fn, "modal");

            sut.hideModal();

            expect($("#manageBusinessUnitModal").modal).toHaveBeenCalledWith("hide");
        });
    });

    describe("Events", function() {
        it("should handle businessUnitsLoaded event, when occure", function () {
            var handler;
            mocks.common.handleEvent.and.callFake(function (event, callback) {
                handler = callback;
            });
            prepareSut();
            expect(mocks.localData.businessUnitsPromise.calls.count()).toEqual(1);

            handler(businessUnits);

            expect(mocks.localData.businessUnitsPromise.calls.count()).toEqual(2);
        });
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
