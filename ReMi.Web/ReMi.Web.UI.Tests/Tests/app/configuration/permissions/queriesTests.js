describe("Queries Controller", function () {
    var sut, mocks, logger;
    var deferred, getQueryDeferred, getRolesDeferred;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope) {
        deferred = $q.defer();
        getQueryDeferred = $q.defer();
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
            remiapi: window.jasmine.createSpyObj('remiapi', ['getRoles', 'getQueries', 'addQueryToRole', 'removeQueryFromRole']),
            authService: {
                identity: { role: 'Admin' }
            },
            config: { events: { notificationReceived: 'testEvent' } },
            rulesService: jasmine.createSpyObj('rulesService', ['getPermissionRule', 'generateNewRule', 'testBusinessRule', 'savePermissionRule', 'deleteRule'])
    };
        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.common.activateController.and.returnValue(deferred.promise);
        mocks.remiapi.getRoles.and.returnValue(getRolesDeferred.promise);
        mocks.remiapi.getQueries.and.returnValue(getQueryDeferred.promise);
    }));

    it("should call initialization methods, when created", function () {
        spyOn(mocks.$scope, '$on');

        inject(function ($controller) {
            sut = $controller('queries', mocks);
        });

        expect(sut).toBeDefined();
        deferred.resolve();
        mocks.$scope.$digest();

        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('queries');
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), 'queries');
        expect(mocks.remiapi.getRoles).toHaveBeenCalled();
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('testEvent', window.jasmine.any(Function), mocks.$scope);
        expect(mocks.$scope.$on).toHaveBeenCalledWith('$destroy', window.jasmine.any(Function));
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('RoleCreatedEvent', window.jasmine.any(Object));
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('RoleUpdatedEvent', window.jasmine.any(Object));
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('RoleDeletedEvent', window.jasmine.any(Object));
    });

    it("should specify group count and define query roles, when get all query finished", function () {
        inject(function ($controller) {
            sut = $controller('queries', mocks);
        });

        expect(sut).toBeDefined();
        getRolesDeferred.resolve(rolesData);
        getQueryDeferred.resolve({
            "Queries": [{
                "QueryId": 1,
                "Name": "query",
                "Group": "Access Control",
                "Description": "Test query",
                "IsStatic": false,
                "Roles": [{
                    "ExternalId": "role1",
                    "Name": "name1",
                    "Description": "description1"
                }, {
                    "ExternalId": "role2",
                    "Name": "name2",
                    "Description": "description2"
                }, {
                    "ExternalId": "role3",
                    "Name": "name3",
                    "Description": "description3"
                }, {
                    "ExternalId": "role4",
                    "Name": "name4",
                    "Description": "description4"
                }]
            }]
        });

        mocks.$scope.$digest();

        expect(sut.queries.length).toBe(1);
        expect(sut.queries[0].Name).toBe('query');
        expect(sut.queries[0].Description).toBe('Test query');
        expect(sut.queries[0]['role1']).toBe(true);
        expect(sut.queries[0]['role2']).toBe(true);
        expect(sut.queries[0]['role3']).toBe(true);
        expect(sut.queries[0]['role4']).toBe(true);
        expect(sut.queries[0]['role5']).toBe(false);
        expect(sut.queries[0]['role6']).toBe(false);
        expect(sut.queries[0]['role7']).toBeUndefined();
    });

    it("should call remiapi.addQueryToRole, when adding permission to query", function () {
        inject(function ($controller) {
            sut = $controller('queries', mocks);
        });
        var query = { QueryId: 'queryId' };
        var role = { ExternalId: 'ExternalId' };

        mocks.remiapi.addQueryToRole.and.returnValue(deferred.promise);

        sut.changeQueryRole(query, role, true);
        deferred.resolve();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toBe(false);
        expect(mocks.remiapi.addQueryToRole).toHaveBeenCalledWith({ QueryId: 'queryId', RoleExternalId: 'ExternalId' });
    });

    it("should call remiapi.removeQueryFromRole, when removing permission from query", function () {
        inject(function ($controller) {
            sut = $controller('queries', mocks);
        });
        var query = { QueryId: 'queryId' };
        var role = { ExternalId: 'ExternalId' };
        query[role.ExternalId] = true;

        mocks.remiapi.removeQueryFromRole.and.returnValue(deferred.promise);

        sut.changeQueryRole(query, role, false);
        deferred.resolve();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toBe(false);
        expect(mocks.remiapi.removeQueryFromRole).toHaveBeenCalledWith({ QueryId: 'queryId', RoleExternalId: 'ExternalId' });
    });

    it("should revert model remove, when query execution went wrong", function () {
        inject(function ($controller) {
            sut = $controller('queries', mocks);
        });
        var query = { QueryId: 'queryId' };
        var role = { ExternalId: 'ExternalId' };
        query[role.ExternalId] = true;

        mocks.remiapi.removeQueryFromRole.and.returnValue(deferred.promise);

        sut.changeQueryRole(query, role);
        deferred.reject();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toBe(false);
        expect(query[role.ExternalId]).toBe(true);
    });

    it("should revert model add, when query execution went wrong", function () {
        inject(function ($controller) {
            sut = $controller('queries', mocks);
        });
        var query = { QueryId: 'queryId' };
        var role = { ExternalId: 'ExternalId' };
        query[role.ExternalId] = false;

        mocks.remiapi.addQueryToRole.and.returnValue(deferred.promise);

        sut.changeQueryRole(query, role);
        deferred.reject();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toBe(false);
        expect(query[role.ExternalId]).toBe(false);
    });

    it("should unsubscribe events, when controller is destroyed", function () {
        inject(function ($controller) {
            sut = $controller('queries', mocks);
        });

        mocks.$rootScope.$broadcast('$destroy', {});

        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('RoleCreatedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('RoleUpdatedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('RoleDeletedEvent');
    });

    it("should get initial data, when RoleCreatedEvent handled", function () {
        inject(function ($controller) {
            sut = $controller('queries', mocks);
        });

        sut.serverNotificationHandler({ name: 'RoleCreatedEvent' });

        expect(mocks.remiapi.getRoles.calls.count()).toBe(2);
    });

    it("should get initial data, when RoleDeletedEvent handled", function () {
        inject(function ($controller) {
            sut = $controller('queries', mocks);
        });

        sut.serverNotificationHandler({ name: 'RoleDeletedEvent' });

        expect(mocks.remiapi.getRoles.calls.count()).toBe(2);
    });

    it("should get initial data, when RoleUpdatedEvent handled", function () {
        inject(function ($controller) {
            sut = $controller('queries', mocks);
        });

        sut.serverNotificationHandler({ name: 'RoleUpdatedEvent' });

        expect(mocks.remiapi.getRoles.calls.count()).toBe(2);
    });

    it("should not get initial data, when unsubscribed event handled", function () {
        inject(function ($controller) {
            sut = $controller('queries', mocks);
        });

        sut.serverNotificationHandler({ name: 'unsubscribed' });

        expect(mocks.remiapi.getRoles.calls.count()).toBe(1);
    });

    describe("Rule Persmissions", function () {

        it("should open modal and get query rule, when invoked", function () {
            inject(function ($controller) {
                sut = $controller('queries', mocks);
            });
            var query = { Name: "queryName" };
            var rule = { data: "some data" };
            spyOn($.fn, 'modal');
            var defer = mocks.common.$q.defer();
            mocks.rulesService.getPermissionRule.and.returnValue(defer.promise);
            sut.editRule(query);
            defer.resolve(rule);
            mocks.$scope.$digest();

            expect(sut.currentRule).toEqual(rule);
            expect(mocks.rulesService.getPermissionRule).toHaveBeenCalledWith(query.Name);
            expect($.fn.modal).toHaveBeenCalledWith(jasmine.objectContaining({ backdrop: 'static', keyboard: true }));
        });

        it("should open modal, try get query rule and hide model, when failed", function () {
            inject(function ($controller) {
                sut = $controller('queries', mocks);
            });
            var query = { Name: "queryName" };
            spyOn($.fn, 'modal');
            var defer = mocks.common.$q.defer();
            mocks.rulesService.getPermissionRule.and.returnValue(defer.promise);

            sut.editRule(query);
            defer.reject();
            mocks.$scope.$digest();

            expect(mocks.rulesService.getPermissionRule).toHaveBeenCalledWith(query.Name);
            expect($.fn.modal).toHaveBeenCalledWith(jasmine.objectContaining({ backdrop: 'static', keyboard: true }));
            expect($.fn.modal).toHaveBeenCalledWith('hide');
        });

        it("should get new generated rule, when invoked", function () {
            inject(function ($controller) {
                sut = $controller('queries', mocks);
            });
            sut.currentQuery = { Name: "queryName", Namespace: "Namespace" };
            var rule = { data: "some data" };
            var defer = mocks.common.$q.defer();
            mocks.rulesService.generateNewRule.and.returnValue(defer.promise);

            sut.generateNewRule();
            defer.resolve(rule);
            mocks.$scope.$digest();

            expect(sut.currentRule).toEqual(rule);
            expect(mocks.rulesService.generateNewRule).toHaveBeenCalledWith(sut.currentQuery.Name, sut.currentQuery.Namespace);
        });

        it("should test rule, when invoked", function () {
            inject(function ($controller) {
                sut = $controller('queries', mocks);
            });
            sut.currentRule = { data: "some data" };

            sut.testBusinessRule();

            expect(mocks.rulesService.testBusinessRule).toHaveBeenCalledWith(sut.currentRule);
        });

        it("should call savePermissionRule and hide modal, when invoked", function () {
            inject(function ($controller) {
                sut = $controller('queries', mocks);
            });
            sut.currentQuery = { QueryId: "query id" };
            sut.currentRule = { data: "some data" };
            spyOn($.fn, 'modal');
            var defer = mocks.common.$q.defer();
            mocks.rulesService.savePermissionRule.and.returnValue(defer.promise);
            sut.savePermissionRule();
            defer.resolve();
            mocks.$scope.$digest();

            expect(mocks.rulesService.savePermissionRule).toHaveBeenCalledWith(sut.currentRule, 'query', sut.currentQuery.QueryId);
            expect($.fn.modal).toHaveBeenCalledWith('hide');
            expect(sut.currentQuery.HasRuleApplied).toEqual(true);
        });
        
        it("should call deleteRule and hide modal, when invoked", function () {
            inject(function ($controller) {
                sut = $controller('queries', mocks);
            });
            sut.currentQuery = { QueryId: "query id", HasRuleApplied: true };
            sut.currentRule = { data: "some data", ExternalId: "external id" };
            spyOn($.fn, 'modal');
            var defer = mocks.common.$q.defer();
            mocks.rulesService.deleteRule.and.returnValue(defer.promise);
            sut.deleteRule();
            defer.resolve();
            mocks.$scope.$digest();

            expect(mocks.rulesService.deleteRule).toHaveBeenCalledWith("external id");
            expect($.fn.modal).toHaveBeenCalledWith('hide');
            expect(sut.currentQuery.HasRuleApplied).toEqual(false);
            expect(sut.currentRule).toBeNull();
        });
    });

});

var rolesData = {
    "Roles": [
        {
            "ExternalId": "role1",
            "Name": "name1",
            "Description": "description1"
        }, {
            "ExternalId": "role2",
            "Name": "name2",
            "Description": "description2"
        }, {
            "ExternalId": "role3",
            "Name": "name3",
            "Description": "description3"
        }, {
            "ExternalId": "role4",
            "Name": "name4",
            "Description": "description4"
        }, {
            "ExternalId": "role5",
            "Name": "name5",
            "Description": "description5"
        }, {
            "ExternalId": "role6",
            "Name": "name6",
            "Description": "description6"
        }
    ]
};
