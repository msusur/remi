describe("Commands Controller", function () {
    var sut, mocks, logger;
    var deferred, getCommandDeferred, getRolesDeferred;
   
    beforeEach(function () {
        module("app");
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope) {
        deferred = $q.defer();
        getCommandDeferred = $q.defer();
        getRolesDeferred = $q.defer();
        mocks = {
            $scope: $rootScope.$new(),
            $rootScope: $rootScope,
            common: {
                logger: window.jasmine.createSpyObj('logger', ['getLogger']),
                activateController: window.jasmine.createSpy('activateController'),
                handleEvent: window.jasmine.createSpy('handleEvent'),
                $q: $q
            },
            notifications: {
                subscribe: window.jasmine.createSpy('subscribe'),
                unsubscribe: window.jasmine.createSpy('unsubscribe')
            },
            remiapi: window.jasmine.createSpyObj('remiapi', ['getRoles', 'getCommands', 'executeCommand']),
            authService: {
                identity: { role: 'Admin' }
            },
            config: { events: { notificationReceived: 'testEvent' } },
            rulesService: jasmine.createSpyObj('rulesService', ['getPermissionRule', 'generateNewRule', 'testBusinessRule', 'savePermissionRule', 'deleteRule']),
            $window: jasmine.createSpyObj('$window', ['sessionStorage'])
    };
        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.common.activateController.and.returnValue(deferred.promise);
        mocks.remiapi.getRoles.and.returnValue(getRolesDeferred.promise);
        mocks.remiapi.getCommands.and.returnValue(getCommandDeferred.promise);
    }));

    it("should call initialization methods, when created", function () {
        spyOn(mocks.$scope, '$on');
        
        inject(function ($controller) {
            sut = $controller('commands', mocks);
        });
        
        expect(sut).toBeDefined();
        deferred.resolve();
        mocks.$scope.$digest();

        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('commands');
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), 'commands');
        expect(mocks.remiapi.getRoles).toHaveBeenCalled();
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('testEvent', window.jasmine.any(Function), mocks.$scope);
        expect(mocks.$scope.$on).toHaveBeenCalledWith('$destroy', window.jasmine.any(Function));
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('RoleCreatedEvent', window.jasmine.any(Object));
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('RoleUpdatedEvent', window.jasmine.any(Object));
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('RoleDeletedEvent', window.jasmine.any(Object));
    });

    it("should specify group count and define command roles, when get all query finished", function () {
        inject(function ($controller) {
            sut = $controller('commands', mocks);
        });

        expect(sut).toBeDefined();
        getRolesDeferred.resolve(rolesData);
        getCommandDeferred.resolve(commandsData);
        mocks.$scope.$digest();

        expect(sut.commands.length).toBe(9);
        expect(sut.commands[0].Count).toBe(6);
        expect(sut.commands[1].Count).toBeUndefined();
        expect(sut.commands[6].Count).toBe(1);
        for (var c in sut.commands) {
            for (var r in sut.roles) {
                var command = sut.commands[c];
                var role = sut.roles[r];
                expect(command[role.ExternalId]).toBeDefined();
            }
        }
        expect(sut.commands[1]['f313b7d8-2731-401d-a1d6-c89e269693b5']).toBe(true);
        expect(sut.commands[1]['45aa5e50-4a03-44fa-8044-2caf43bcf217']).toBe(false);
    });

    it("should send call AddCommandToRoleCommand, when adding permission to command", function () {
        inject(function ($controller) {
            sut = $controller('commands', mocks);
        });
        var command = { CommandId: 'commandId' };
        var role = { ExternalId: 'ExternalId' };
        command[role.ExternalId] = false;

        mocks.remiapi.executeCommand.and.returnValue(deferred.promise);

        sut.changeCommandRole(command, role);
        deferred.resolve();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toBe(false);
        expect(mocks.remiapi.executeCommand).toHaveBeenCalledWith("AddCommandToRoleCommand",
            window.jasmine.any(String),
            window.jasmine.objectContaining({ CommandId: 'commandId', RoleExternalId: 'ExternalId' }));
    });

    it("should send call RemoveCommandFromRoleCommand, when removing permission from command", function () {
        inject(function ($controller) {
            sut = $controller('commands', mocks);
        });
        var command = { CommandId: 'commandId' };
        var role = { ExternalId: 'ExternalId' };
        command[role.ExternalId] = true;

        mocks.remiapi.executeCommand.and.returnValue(deferred.promise);

        sut.changeCommandRole(command, role);
        deferred.resolve();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toBe(false);
        expect(mocks.remiapi.executeCommand).toHaveBeenCalledWith("RemoveCommandFromRoleCommand",
            window.jasmine.any(String),
            window.jasmine.objectContaining({ CommandId: 'commandId', RoleExternalId: 'ExternalId' }));
    });

    it("should revert model change, when command execution went wrong", function () {
        inject(function ($controller) {
            sut = $controller('commands', mocks);
        });
        var command = { CommandId: 'commandId' };
        var role = { ExternalId: 'ExternalId' };

        mocks.remiapi.executeCommand.and.returnValue(deferred.promise);

        sut.changeCommandRole(command, role, true);
        deferred.reject();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toBe(false);
        expect(command[role.ExternalId]).toBe(false);
    });

    it("should unsubscribe events, when controller is destroyed", function () {
        inject(function ($controller) {
            sut = $controller('commands', mocks);
        });

        mocks.$rootScope.$broadcast('$destroy', {});

        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('RoleCreatedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('RoleUpdatedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('RoleDeletedEvent');
    });

    it("should get initial data, when RoleCreatedEvent handled", function () {
        inject(function ($controller) {
            sut = $controller('commands', mocks);
        });

        sut.serverNotificationHandler({ name: 'RoleCreatedEvent' });

        expect(mocks.remiapi.getRoles.calls.count()).toBe(2);
    });

    it("should get initial data, when RoleDeletedEvent handled", function () {
        inject(function ($controller) {
            sut = $controller('commands', mocks);
        });

        sut.serverNotificationHandler({ name: 'RoleDeletedEvent' });

        expect(mocks.remiapi.getRoles.calls.count()).toBe(2);
    });

    it("should get initial data, when RoleUpdatedEvent handled", function () {
        inject(function ($controller) {
            sut = $controller('commands', mocks);
        });

        sut.serverNotificationHandler({ name: 'RoleUpdatedEvent' });

        expect(mocks.remiapi.getRoles.calls.count()).toBe(2);
    });

    it("should not get initial data, when unsubscribed event handled", function () {
        inject(function ($controller) {
            sut = $controller('commands', mocks);
        });

        sut.serverNotificationHandler({ name: 'unsubscribed' });

        expect(mocks.remiapi.getRoles.calls.count()).toBe(1);
    });

    describe("Rule Persmissions", function() {

        it("should open modal and get command rule, when invoked", function () {
            inject(function ($controller) {
                sut = $controller('commands', mocks);
            });
            var command = { Name: "commandName" };
            var rule = { data: "some data" };
            spyOn($.fn, 'modal');
            var defer = mocks.common.$q.defer();
            mocks.rulesService.getPermissionRule.and.returnValue(defer.promise);
            sut.editRule(command);
            defer.resolve(rule);
            mocks.$scope.$digest();

            expect(sut.currentRule).toEqual(rule);
            expect(mocks.rulesService.getPermissionRule).toHaveBeenCalledWith(command.Name);
            expect($.fn.modal).toHaveBeenCalledWith(jasmine.objectContaining({ backdrop: 'static', keyboard: true }));
        });

        it("should open modal, try get command rule and hide model, when failed", function () {
            inject(function ($controller) {
                sut = $controller('commands', mocks);
            });
            var command = { Name: "commandName" };
            spyOn($.fn, 'modal');
            var defer = mocks.common.$q.defer();
            mocks.rulesService.getPermissionRule.and.returnValue(defer.promise);

            sut.editRule(command);
            defer.reject();
            mocks.$scope.$digest();

            expect(mocks.rulesService.getPermissionRule).toHaveBeenCalledWith(command.Name);
            expect($.fn.modal).toHaveBeenCalledWith(jasmine.objectContaining({ backdrop: 'static', keyboard: true }));
            expect($.fn.modal).toHaveBeenCalledWith('hide');
        });

        it("should get new generated rule, when invoked", function () {
            inject(function ($controller) {
                sut = $controller('commands', mocks);
            });
            sut.currentCommand = { Name: "commandName", Namespace: "Namespace" };
            var rule = { data: "some data" };
            var defer = mocks.common.$q.defer();
            mocks.rulesService.generateNewRule.and.returnValue(defer.promise);

            sut.generateNewRule();
            defer.resolve(rule);
            mocks.$scope.$digest();

            expect(sut.currentRule).toEqual(rule);
            expect(mocks.rulesService.generateNewRule).toHaveBeenCalledWith(sut.currentCommand.Name, sut.currentCommand.Namespace);
        });

        it("should test rule, when invoked", function () {
            inject(function ($controller) {
                sut = $controller('commands', mocks);
            });
            sut.currentRule = { data: "some data" };

            sut.testBusinessRule();

            expect(mocks.rulesService.testBusinessRule).toHaveBeenCalledWith(sut.currentRule);
        });

        it("should call savePermissionRule and hide modal, when invoked", function () {
            inject(function ($controller) {
                sut = $controller('commands', mocks);
            });
            sut.currentCommand = { CommandId: "command id" };
            sut.currentRule = { data: "some data" };
            spyOn($.fn, 'modal');
            var defer = mocks.common.$q.defer();
            mocks.rulesService.savePermissionRule.and.returnValue(defer.promise);
            sut.savePermissionRule();
            defer.resolve();
            mocks.$scope.$digest();

            expect(mocks.rulesService.savePermissionRule).toHaveBeenCalledWith(sut.currentRule, 'command', sut.currentCommand.CommandId);
            expect($.fn.modal).toHaveBeenCalledWith('hide');
            expect(sut.currentCommand.HasRuleApplied).toEqual(true);
        });


        it("should call deleteRule and hide modal, when invoked", function () {
            inject(function ($controller) {
                sut = $controller('commands', mocks);
            });
            sut.currentCommand = { CommandId: "command id", HasRuleApplied: true };
            sut.currentRule = { data: "some data", ExternalId: "external id" };
            spyOn($.fn, 'modal');
            var defer = mocks.common.$q.defer();
            mocks.rulesService.deleteRule.and.returnValue(defer.promise);
            sut.deleteRule();
            defer.resolve();
            mocks.$scope.$digest();

            expect(mocks.rulesService.deleteRule).toHaveBeenCalledWith("external id");
            expect($.fn.modal).toHaveBeenCalledWith('hide');
            expect(sut.currentCommand.HasRuleApplied).toEqual(false);
            expect(sut.currentRule).toBeNull();
        });
    });
});

var commandsData = {
    "Commands": [{
        "CommandId": 4,
        "Name": "CreateAccountCommand",
        "Group": "Access Control",
        "Description": "Create Account Command",
        "IsBackground": false,
        "Roles": [{
            "ExternalId": "62f20d1a-e849-4204-b09f-c404220ab3f4",
            "Name": "ProductOwner",
            "Description": "Product owner"
        }, {
            "ExternalId": "f313b7d8-2731-401d-a1d6-c89e269693b5",
            "Name": "ExecutiveManager",
            "Description": "Executive manager"
        }, {
            "ExternalId": "75198b5f-6a40-4797-876c-91954498698e",
            "Name": "Admin",
            "Description": "Admin"
        }, {
            "ExternalId": "a68396f0-2686-412b-84d8-6303fc29ccba",
            "Name": "ReleaseEngineer",
            "Description": "Release engineer"
        }
        ]
    }, {
        "CommandId": 54,
        "Name": "CreateRoleCommand",
        "Group": "Access Control",
        "Description": "Create Role",
        "IsBackground": false,
        "Roles": [{
            "ExternalId": "f313b7d8-2731-401d-a1d6-c89e269693b5",
            "Name": "ExecutiveManager",
            "Description": "Executive manager"
        }, {
            "ExternalId": "75198b5f-6a40-4797-876c-91954498698e",
            "Name": "Admin",
            "Description": "Admin"
        }, {
            "ExternalId": "a68396f0-2686-412b-84d8-6303fc29ccba",
            "Name": "ReleaseEngineer",
            "Description": "Release engineer"
        }
        ]
    }, {
        "CommandId": 52,
        "Name": "DeleteRoleCommand",
        "Group": "Access Control",
        "Description": "Delete Role",
        "IsBackground": false,
        "Roles": [{
            "ExternalId": "f313b7d8-2731-401d-a1d6-c89e269693b5",
            "Name": "ExecutiveManager",
            "Description": "Executive manager"
        }, {
            "ExternalId": "75198b5f-6a40-4797-876c-91954498698e",
            "Name": "Admin",
            "Description": "Admin"
        }, {
            "ExternalId": "a68396f0-2686-412b-84d8-6303fc29ccba",
            "Name": "ReleaseEngineer",
            "Description": "Release engineer"
        }
        ]
    }, {
        "CommandId": 51,
        "Name": "RemoveCommandFromRoleCommand",
        "Group": "Access Control",
        "Description": "Remove command from role",
        "IsBackground": false,
        "Roles": [{
            "ExternalId": "75198b5f-6a40-4797-876c-91954498698e",
            "Name": "Admin",
            "Description": "Admin"
        }
        ]
    }, {
        "CommandId": 5,
        "Name": "UpdateAccountCommand",
        "Group": "Access Control",
        "Description": "Update Account",
        "IsBackground": false,
        "Roles": [{
            "ExternalId": "62f20d1a-e849-4204-b09f-c404220ab3f4",
            "Name": "ProductOwner",
            "Description": "Product owner"
        }, {
            "ExternalId": "f313b7d8-2731-401d-a1d6-c89e269693b5",
            "Name": "ExecutiveManager",
            "Description": "Executive manager"
        }, {
            "ExternalId": "75198b5f-6a40-4797-876c-91954498698e",
            "Name": "Admin",
            "Description": "Admin"
        }, {
            "ExternalId": "a68396f0-2686-412b-84d8-6303fc29ccba",
            "Name": "ReleaseEngineer",
            "Description": "Release engineer"
        }
        ]
    }, {
        "CommandId": 53,
        "Name": "UpdateRoleCommand",
        "Group": "Access Control",
        "Description": "Update Role",
        "IsBackground": false,
        "Roles": [{
            "ExternalId": "f313b7d8-2731-401d-a1d6-c89e269693b5",
            "Name": "ExecutiveManager",
            "Description": "Executive manager"
        }, {
            "ExternalId": "75198b5f-6a40-4797-876c-91954498698e",
            "Name": "Admin",
            "Description": "Admin"
        }, {
            "ExternalId": "a68396f0-2686-412b-84d8-6303fc29ccba",
            "Name": "ReleaseEngineer",
            "Description": "Release engineer"
        }
        ]
    }, {
        "CommandId": 43,
        "Name": "AddProductCommand",
        "Group": "Configuration",
        "Description": "Add product",
        "IsBackground": false,
        "Roles": [{
            "ExternalId": "62f20d1a-e849-4204-b09f-c404220ab3f4",
            "Name": "ProductOwner",
            "Description": "Product owner"
        }, {
            "ExternalId": "f313b7d8-2731-401d-a1d6-c89e269693b5",
            "Name": "ExecutiveManager",
            "Description": "Executive manager"
        }, {
            "ExternalId": "75198b5f-6a40-4797-876c-91954498698e",
            "Name": "Admin",
            "Description": "Admin"
        }, {
            "ExternalId": "a68396f0-2686-412b-84d8-6303fc29ccba",
            "Name": "ReleaseEngineer",
            "Description": "Release engineer"
        }
        ]
    }, {
        "CommandId": 56,
        "Name": "AddPipelineCommand",
        "Group": "Go Pipelines",
        "Description": "Add new go pipeline for product",
        "IsBackground": false,
        "Roles": []
    }, {
        "CommandId": 57,
        "Name": "RemoveGoPipelineCommand",
        "Group": "Go Pipelines",
        "Description": "Remove pipeline",
        "IsBackground": false,
        "Roles": []
    }]
};

var rolesData = {
    "Roles": [
        {
            "ExternalId": "79bcb7af-0d3f-4977-a138-dfe93227ceb6",
            "Name": "NotAuthenticated",
            "Description": "Not authenticated"
        }, {
            "ExternalId": "45aa5e50-4a03-44fa-8044-2caf43bcf217",
            "Name": "BasicUser",
            "Description": "Basic user"
        }, {
            "ExternalId": "f9ade772-d476-4dbb-8554-ff52d493a25e",
            "Name": "TeamMember",
            "Description": "Team member"
        }, {
            "ExternalId": "c7bf5184-d154-4988-9f9a-2d13cf185f18",
            "Name": "ProductionSupport",
            "Description": "Production support"
        }, {
            "ExternalId": "62f20d1a-e849-4204-b09f-c404220ab3f4",
            "Name": "ProductOwner",
            "Description": "Product owner"
        }, {
            "ExternalId": "f313b7d8-2731-401d-a1d6-c89e269693b5",
            "Name": "ExecutiveManager",
            "Description": "Executive manager"
        }, {
            "ExternalId": "75198b5f-6a40-4797-876c-91954498698e",
            "Name": "Admin",
            "Description": "Admin"
        }, {
            "ExternalId": "a68396f0-2686-412b-84d8-6303fc29ccba",
            "Name": "ReleaseEngineer",
            "Description": "Release engineer"
        }
    ]
};
