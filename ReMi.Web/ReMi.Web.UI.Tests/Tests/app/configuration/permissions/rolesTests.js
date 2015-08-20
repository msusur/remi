describe("Roles Controller", function () {
    var sut, mocks, logger;
    var getRolesDefer, updateRoleDefer, createRoleDefer, deleteRoleDefer;
    var scope, $q;

    beforeEach(function () {
        module("app");

        mocks = {
            common: {
                logger: window.jasmine.createSpyObj('logger', ['getLogger']),
                activateController: window.jasmine.createSpy('activateController').and.returnValue({ then: window.jasmine.createSpy('then') })
            },
            remiapi: window.jasmine.createSpyObj('remiapi', ['createRole', 'updateRole', 'deleteRole', 'getRoles'])
        };
        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);

        inject(function ($controller, $rootScope, _$q_) {
            $q = _$q_;
            scope = $rootScope.$new();

            mocks.$scope = scope;
            mocks.common.$q = $q;

            getRolesDefer = $q.defer();
            mocks.remiapi.getRoles.and.returnValue(getRolesDefer.promise);

            deleteRoleDefer = $q.defer();
            mocks.remiapi.deleteRole.and.returnValue(deleteRoleDefer.promise);

            createRoleDefer = $q.defer();
            mocks.remiapi.createRole.and.returnValue(createRoleDefer.promise);

            updateRoleDefer = $q.defer();
            mocks.remiapi.updateRole.and.returnValue(updateRoleDefer.promise);

            sut = $controller('roles', mocks);
        });
    });

    it("should call initialization methods", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('roles');
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), 'roles');
    });

    it("should get roles from server when getRoles invoked", function () {
        sut.getRoles();

        getRolesDefer.resolve({
            Roles: [
                { ExternalId: '123' }, { ExternalId: 'abc' }
            ]
        });

        scope.$digest();

        expect(mocks.remiapi.getRoles).toHaveBeenCalled();
        expect(sut.roles.length).toBe(2);
        expect(sut.roles[0].ExternalId).toBe('123');
        expect(sut.roles[1].ExternalId).toBe('abc');
        expect(sut.state.isBusy).toBe(false);
    });

    it("should update role in local storage when updateRole invoked", function () {
        var roleOld = { 'ExternalId': '123', 'Name': 'oldName' };
        var roleNew = { 'ExternalId': '123', 'Name': 'newName' };
        sut.roles = [roleOld];

        sut.updateRole(roleNew);

        expect(sut.roles.length).toBe(1);
        expect(sut.roles[0].ExternalId).toBe('123');
        expect(sut.roles[0].Name).toBe('newName');
    });

    it("should add role to local storage when updateRole invoked with new role", function () {
        var roleNew = { 'ExternalId': '123', 'Name': 'newName' };
        sut.roles = [];

        sut.updateRole(roleNew);

        expect(sut.roles.length).toBe(1);
        expect(sut.roles[0].ExternalId).toBe('123');
        expect(sut.roles[0].Name).toBe('newName');
    });

    it("should remove role from local storage when deleteRole invoked with new role", function () {
        var roleNew = { 'ExternalId': '123', 'Name': 'newName' };
        sut.roles = [roleNew, { 'ExternalId': '456', 'Name': 'newName2' }];

        sut.removeRole(roleNew);

        expect(sut.roles.length).toBe(1);
        expect(sut.roles[0].ExternalId).toBe('456');
        expect(sut.roles[0].Name).toBe('newName2');
    });

    it("should call remiapi.deleteRole when deleteRole invoked", function () {
        var role = { 'ExternalId': '123', 'Name': 'newName' };
        sut.roles = [role];

        sut.deleteRole(role);

        expect(mocks.remiapi.deleteRole).toHaveBeenCalledWith({ Role: role });
    });

    it("should remove role from locals storage when deleteRole invoked", function () {
        var role = { 'ExternalId': '123', 'Name': 'newName' };
        sut.roles = [role];

        spyOn(sut, 'removeRole');

        sut.deleteRole(role);

        deleteRoleDefer.resolve();

        scope.$digest();

        expect(sut.removeRole).toHaveBeenCalledWith(role);
    });

    it("should validate data when saveRole invoked", function () {
        var role = { 'ExternalId': '123', 'Name': 'newName' };

        spyOn(sut, 'validate');

        var def = $q.defer();
        sut.validate.and.returnValue(def.promise);

        sut.saveRole(role);

        def.resolve();

        scope.$digest();

        expect(sut.validate).toHaveBeenCalled();
    });

    it("should call remiapi.createRole when operation is add", function () {
        var role = { 'ExternalId': '123', 'Name': 'newName' };

        spyOn(sut, 'validate');

        var def = $q.defer();
        sut.validate.and.returnValue(def.promise);
        def.resolve();

        sut.currentRole = role;
        sut.saveRole('add');

        scope.$digest();

        expect(mocks.remiapi.createRole).toHaveBeenCalled();
    });

    it("should call remiapi.createRole when operation is add", function () {
        var role = { 'ExternalId': '123', 'Name': 'newName' };

        spyOn(sut, 'validate');

        var def = $q.defer();
        sut.validate.and.returnValue(def.promise);
        def.resolve();

        sut.currentRole = role;
        sut.saveRole('update');

        scope.$digest();

        expect(mocks.remiapi.updateRole).toHaveBeenCalled();
    });
});

