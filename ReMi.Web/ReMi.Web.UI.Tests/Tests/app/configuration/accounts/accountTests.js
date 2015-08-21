describe("Accounts Controller", function () {
    var sut, mocks, logger;
    var deferred, getAccountDeferred, getRolesDeferred;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope, $timeout) {
        deferred = $q.defer();
        getAccountDeferred = $q.defer();
        getRolesDeferred = $q.defer();
        mocks = {
            $scope: $rootScope.$new(),
            $rootScope: $rootScope,
            common: {
                logger: window.jasmine.createSpyObj("logger", ["getLogger"]),
                activateController: window.jasmine.createSpy("activateController"),
                handleEvent: window.jasmine.createSpy("handleEvent"),
                showInfoMessage: window.jasmine.createSpy("showInfoMessage"),
                $q: $q
            },
            remiapi: window.jasmine.createSpyObj("remiapi", ["getRoles", "getAccounts", "createAccount", "updateAccount"]),
            authService: {
                identity: { role: "Admin" }
            },
            localData: { businessUnits: businessUnits },
            config: {
                events: {
                    notificationReceived: "testEvent",
                    businessUnitsLoaded: "businessUnitsLoaded"
                }
            },
            $timeout: $timeout
        };
        logger = window.jasmine.createSpyObj("logger", ["console", "error", "info", "warn"]);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.common.activateController.and.returnValue(deferred.promise);
        mocks.remiapi.getRoles.and.returnValue(getRolesDeferred.promise);
        mocks.remiapi.getAccounts.and.returnValue(getAccountDeferred.promise);
    }));

    it("should call initialization methods, when created", function () {
        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });

        expect(sut).toBeDefined();
        deferred.resolve();
        mocks.$scope.$digest();

        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("accounts");
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), "accounts");
        expect(mocks.remiapi.getRoles).toHaveBeenCalled();
        expect(mocks.remiapi.getAccounts).toHaveBeenCalled();
        expect(mocks.common.handleEvent).toHaveBeenCalledWith("businessUnitsLoaded", jasmine.any(Function), mocks.$scope);
        expect(sut.businessUnits).toEqual(businessUnits);
    });

    it("should hide modal, when method hideCurrentAccountModal called", function () {
        spyOn($.fn, "modal");
        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });
        sut.validator = window.jasmine.createSpyObj("validator", ["resetForm"]);

        sut.hideCurrentAccountModal();

        expect($.fn.modal).toHaveBeenCalledWith("hide");
    });

    it("should get roles, when initialize", function () {
        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });

        getRolesDeferred.resolve(roles);
        mocks.$scope.$digest();

        expect(mocks.remiapi.getRoles).toHaveBeenCalled();
        expect(sut.roles.length).toEqual(4);
    });

    it("should get accounts, when initialize", function () {
        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });

        getAccountDeferred.resolve(accounts);
        mocks.$scope.$digest();

        expect(mocks.remiapi.getAccounts).toHaveBeenCalled();
        expect(sut.accounts.length).toEqual(4);
        expect(sut.state.isBusy).toEqual(false);
    });

    it("should create new currentAccount and show modal in Add mode, when showAccountAdd method invoked", function () {
        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });
        spyOn(window, "newGuid").and.returnValue("new guid");
        spyOn(sut, "showAccountModalMode");
        sut.roles = roles.Roles;

        sut.showAccountAdd();

        expect(sut.currentAccount.Role).toEqual(roles.Roles[0]);
        expect(sut.currentAccount.RoleId).toEqual(roles.Roles[0].ExternalId);
        expect(sut.currentAccount.ExternalId).toEqual("new guid");
        expect(sut.showAccountModalMode).toHaveBeenCalledWith("add");
        expect(Enumerable.From(sut.businessUnits).All(function (x) {
            return Enumerable.From(x.Packages).All(function (p) { return !p.Checked; });
        })).toBeTruthy();
        expect(Enumerable.From(sut.businessUnits).All(function (x) {
            return Enumerable.From(x.Packages).All(function (p) { return !p.IsDefault; });
        })).toBeTruthy();
    });

    it("should fill out currentAccount and show modal in Update mode, when showAccountUpdate method invoked", function () {
        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });
        spyOn(sut, "showAccountModalMode");

        sut.showAccountUpdate(accounts.Accounts[0]);

        expect(sut.currentAccount.ExternalId).toEqual(accounts.Accounts[0].ExternalId);
        expect(sut.currentAccount.RoleId).toEqual(accounts.Accounts[0].Role.ExternalId);
        expect(sut.showAccountModalMode).toHaveBeenCalledWith("update");
        expect(Enumerable.From(sut.businessUnits).SelectMany(function (x) {
            return x.Packages;
        }).First(function (x) { return x.Name === "Lobortis Quis Corp."; }).IsDefault).toBeTruthy();
        expect(Enumerable.From(sut.businessUnits).SelectMany(function (x) {
            return x.Packages;
        }).Count(function (x) { return x.Checked; })).toEqual(1);
    });

    it("should show modal, when method showAccountModalMode called", function () {
        spyOn($.fn, "modal");
        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });

        sut.showAccountModalMode("mode");

        expect(sut.accountModalMode).toEqual("mode");
        expect($.fn.modal).toHaveBeenCalled();
    });

    it("should get current account role by RoleId, when role changed", function () {
        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });
        sut.roles = roles.Roles;

        sut.currentAccount.RoleId = roles.Roles[1].ExternalId;
        sut.roleChanged();

        expect(sut.currentAccount.Role.ExternalId).toEqual(roles.Roles[1].ExternalId);
        expect(Enumerable.From(sut.businessUnits).SelectMany(function (x) {
            return x.Packages;
        }).All(function (x) { return x.Checked; })).toBeFalsy();
    });

    it("should check all packages and leave default, when role chenged to admin and default package was selected", function () {
        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });
        sut.roles = roles.Roles;

        sut.currentAccount.RoleId = roles.Roles[4].ExternalId;
        sut.roleChanged();

        expect(Enumerable.From(sut.businessUnits).SelectMany(function (x) {
            return x.Packages;
        }).All(function (x) { return x.Checked; })).toBeTruthy();
        expect(Enumerable.From(sut.businessUnits).SelectMany(function (x) {
            return x.Packages;
        }).First(function (x) { return x.Name === "Mi Consulting"; }).IsDefault).toBeTruthy();;

    });

    it("should check all packages and select first as default, when role chenged to admin and default package was not selected", function () {
        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });
        sut.roles = roles.Roles;
        sut.businessUnits[2].Packages[0].IsDefault = false;

        sut.currentAccount.RoleId = roles.Roles[4].ExternalId;
        sut.roleChanged();

        expect(Enumerable.From(sut.businessUnits).SelectMany(function (x) {
            return x.Packages;
        }).All(function (x) { return x.Checked; })).toBeTruthy();
        expect(Enumerable.From(sut.businessUnits).SelectMany(function (x) {
            return x.Packages;
        }).First().IsDefault).toBeTruthy();
    });

    it("should create account when saveAccount called in add mode and request is valid", function () {
        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });
        sut.roles = roles.Roles;
        sut.businessUnits[2].Packages[0].Checked = true;
        sut.currentAccount = accounts.Accounts[0];
        var createAccountDeferred = mocks.common.$q.defer();
        mocks.remiapi.createAccount.and.returnValue(createAccountDeferred.promise);
        spyOn($.fn, "modal");

        sut.saveAccount("add");
        mocks.$scope.$digest();

        expect($.fn.modal).not.toHaveBeenCalled();
        expect(sut.currentAccount.Products).toEqual([sut.businessUnits[2].Packages[0]]);
        expect(sut.currentAccount.Role).toEqual(roles.Roles[3]);
        expect(mocks.remiapi.createAccount).toHaveBeenCalledWith({ Account: sut.currentAccount });
        expect(mocks.remiapi.updateAccount).not.toHaveBeenCalled();
        expect(sut.accounts.length).toEqual(0);

        createAccountDeferred.resolve();
        mocks.$scope.$digest();

        expect($.fn.modal).toHaveBeenCalledWith("hide");
        expect(sut.state.isAccountModalBusy).toBeFalsy();
        expect(logger.error).not.toHaveBeenCalled();
        expect(sut.accounts.length).toEqual(1);
    });

    it("should update account when saveAccount called in update mode and request is valid", function () {
        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });
        sut.roles = roles.Roles;
        sut.businessUnits[2].Packages[0].Checked = true;
        sut.currentAccount = accounts.Accounts[0];
        var updateAccountDeferred = mocks.common.$q.defer();
        mocks.remiapi.updateAccount.and.returnValue(updateAccountDeferred.promise);
        spyOn($.fn, "modal");

        sut.saveAccount("update");
        mocks.$scope.$digest();

        expect($.fn.modal).not.toHaveBeenCalled();
        expect(sut.currentAccount.Products).toEqual([sut.businessUnits[2].Packages[0]]);
        expect(sut.currentAccount.Role).toEqual(roles.Roles[3]);
        expect(mocks.remiapi.createAccount).not.toHaveBeenCalled();
        expect(mocks.remiapi.updateAccount).toHaveBeenCalledWith({ Account: sut.currentAccount });
        expect(sut.accounts.length).toEqual(0);

        updateAccountDeferred.resolve();
        mocks.$scope.$digest();

        expect($.fn.modal).toHaveBeenCalledWith("hide");
        expect(sut.state.isAccountModalBusy).toBeFalsy();
        expect(logger.error).not.toHaveBeenCalled();
        expect(sut.accounts.length).toEqual(1);
    });

    it("should not create account when saveAccount called and no product is checked", function () {
        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });
        sut.roles = roles.Roles;
        sut.currentAccount = accounts.Accounts[0];
        spyOn($.fn, "modal");
        spyOn(console, "log");

        sut.saveAccount("add");
        mocks.$scope.$digest();

        expect($.fn.modal).not.toHaveBeenCalled();
        expect(sut.currentAccount.Products).toEqual([]);
        expect(sut.currentAccount.Role).toEqual(roles.Roles[3]);
        expect(mocks.remiapi.createAccount).not.toHaveBeenCalled();
        expect(mocks.remiapi.updateAccount).not.toHaveBeenCalled();
        expect(sut.accounts.length).toEqual(0);
        expect(sut.state.isAccountModalBusy).toBeFalsy();
        expect(mocks.common.showInfoMessage).toHaveBeenCalledWith("Invalid data", "Please choose at least one package and select default.");
        expect(logger.error).toHaveBeenCalledWith("Can't save account");
        expect(console.log).toHaveBeenCalledWith("Request is invalid");
    });

    it("should fill out business units, when businesUnitsLoaded event handled", function () {
        var handler;
        var bus = [{ Name: "business unit" }];
        mocks.common.handleEvent.and.callFake(function (event, callback) {
            handler = callback;
        });


        inject(function ($controller) {
            sut = $controller("accounts", mocks);
        });
        handler(bus);


        expect(sut.businessUnits).toEqual(bus);
    });

});

var roles = {
    Roles: [
        { Name: "BasicUser", Description: "Basic User", ExternalId: newGuid() },
        { Name: "ProductOwner", Description: "Product Owner", ExternalId: "62f20d1a-e849-4204-b09f-c404220ab3f4" },
        { Name: "NotAuthenticated", Description: "Not Authenticated", ExternalId: newGuid() },
        { Name: "TeamMember", Description: "Team Member", ExternalId: "f9ade772-d476-4dbb-8554-ff52d493a25e" },
        { Name: "Admin", Description: "Admin", ExternalId: "75198b5f-6a40-4797-876c-91954498698e" }
    ]
};

var accounts = {
    "Accounts": [
        {
            "ExternalId": "9cdaa11c-0a87-40a2-acdb-18264a8cf548",
            "Name": "Yvonne Reid",
            "FullName": "Yvonne Reid",
            "Email": "ultricies.ornare.elit@arcu.co.uk",
            "Role": {
                "ExternalId": "f9ade772-d476-4dbb-8554-ff52d493a25e",
                "Name": "TeamMember",
                "Description": "Team member"
            },
            "IsBlocked": false,
            "Description": "From migration",
            "Products": [
                {
                    "Name": "Lobortis Quis Corp.",
                    "ExternalId": "8f100e45-bb2a-466d-a132-11e5e120defc",
                    "ReleaseTrack": 0,
                    "IsDefault": true
                }
            ],
            "CreatedOn": "2014-06-17T17:49:30.81"
        }, {
            "ExternalId": "74a0512a-92fa-4fba-b04b-422725d4aaa9",
            "Name": "Kirby White",
            "FullName": "Kirby White",
            "Email": "nascetur.ridiculus.mus@aliquet.net",
            "Role": {
                "ExternalId": "62f20d1a-e849-4204-b09f-c404220ab3f4",
                "Name": "ProductOwner",
                "Description": "Product owner"
            },
            "IsBlocked": false,
            "Description": "From migration",
            "Products": [
                {
                    "Name": "Lobortis Quis Corp.",
                    "ExternalId": "8f100e45-bb2a-466d-a132-11e5e120defc",
                    "ReleaseTrack": 0,
                    "IsDefault": true
                }
            ],
            "CreatedOn": "2014-06-17T17:49:30.907"
        }, {
            "ExternalId": "dc27705f-f9bb-44f5-b751-fde2e6f49417",
            "Name": "Isadora Winters",
            "FullName": "Isadora Winters",
            "Email": "natoque.penatibus@velitin.org",
            "Role": {
                "ExternalId": "f9ade772-d476-4dbb-8554-ff52d493a25e",
                "Name": "TeamMember",
                "Description": "Team member"
            },
            "IsBlocked": false,
            "Description": "From migration",
            "Products": [
                {
                    "Name": "Lobortis Quis Corp.",
                    "ExternalId": "8f100e45-bb2a-466d-a132-11e5e120defc",
                    "ReleaseTrack": 0,
                    "IsDefault": true
                }
            ],
            "CreatedOn": "2014-06-17T17:49:30.97"
        }, {
            "ExternalId": "8c223b90-206c-46a6-98db-daccea83db76",
            "Name": "Quemby Monroe",
            "FullName": "Quemby Monroe",
            "Email": "pellentesque.a@montesnascetur.edu",
            "Role": {
                "ExternalId": "75198b5f-6a40-4797-876c-91954498698e",
                "Name": "Admin",
                "Description": "Admin"
            },
            "IsBlocked": false,
            "Description": "From migration",
            "Products": [
                {
                    "Name": "Lobortis Quis Corp.",
                    "ExternalId": "8f100e45-bb2a-466d-a132-11e5e120defc",
                    "ReleaseTrack": 0,
                    "IsDefault": false
                }, {
                    "Name": "Adipiscing Corp.",
                    "ExternalId": "bc79f340-fd28-47f4-94c7-cb98963ba573",
                    "ReleaseTrack": 0,
                    "IsDefault": false
                }, {
                    "Name": "Mi Consulting",
                    "ExternalId": "61ee6437-4029-408c-847d-73e84cc26819",
                    "ReleaseTrack": 0,
                    "IsDefault": false
                }, {
                    "Name": "Non Inc.",
                    "ExternalId": "01aa0e1d-8da1-48d6-9483-c15cba56d666",
                    "ReleaseTrack": 0,
                    "IsDefault": false
                }, {
                    "Name": "Eu Accumsan Incorporated",
                    "ExternalId": "0f1ffaaa-48bb-432c-952f-9e1e67c6eb33",
                    "ReleaseTrack": 0,
                    "IsDefault": false
                }, {
                    "Name": "Consequat Lectus Sit PC",
                    "ExternalId": "48853fce-3371-4bf0-a332-90e26fc9804f",
                    "ReleaseTrack": 0,
                    "IsDefault": false
                }, {
                    "Name": "Ignatius Sweet",
                    "ExternalId": "46482aea-b9c6-4d17-b3ba-71765368bb19",
                    "ReleaseTrack": 0,
                    "IsDefault": false
                }, {
                    "Name": "Pede Nonummy Ut LLC",
                    "ExternalId": "2ae5175b-9e20-43c2-825a-8ea39f37edec",
                    "ReleaseTrack": 0,
                    "IsDefault": false
                }, {
                    "Name": "Ipsum Leo PC",
                    "ExternalId": "0090e2a0-4f35-456f-ce7e-7da026f1cf74",
                    "ReleaseTrack": 0,
                    "IsDefault": true
                }
            ],
            "CreatedOn": "2014-06-17T17:49:31"
        }
    ]
};

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
