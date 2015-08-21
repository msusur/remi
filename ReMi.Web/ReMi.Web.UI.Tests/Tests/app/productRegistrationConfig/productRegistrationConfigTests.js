describe("Product Registration Config Controller", function () {
    var sut, mocks, logger;
    var $rootScope, $q, $timeout, $httpBackend;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });

        mocks = {
            common: {
                logger: window.jasmine.createSpyObj('logger', ['getLogger']),
                activateController: window.jasmine.createSpy('activateController').and.returnValue({ then: window.jasmine.createSpy('then') })
            },
            remiapi: window.jasmine.createSpyObj('remiapi', ['get', 'post'])
        };

        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);

        mocks.remiapi.get = window.jasmine.createSpyObj('get', ['productRegistrationsConfig']);
        mocks.remiapi.post = window.jasmine.createSpyObj('post', [
            'createProductRequestType', 'updateProductRequestType', 'deleteProductRequestType',
            'createProductRequestGroup', 'updateProductRequestGroup', 'deleteProductRequestGroup',
            'createProductRequestTask', 'updateProductRequestTask', 'deleteProductRequestTask']);

        inject(function ($controller, _$rootScope_, _$q_, _$timeout_, _$httpBackend_) {
            $q = _$q_;
            mocks.common.$q = $q;

            $timeout = _$timeout_;
            $rootScope = _$rootScope_;
            $httpBackend = _$httpBackend_;
            mocks.$scope = $rootScope.$new();

            var deferred = $q.defer();
            mocks.remiapi.get.productRegistrationsConfig.and.returnValue(deferred.promise);

            sut = $controller('productRegistrationConfig', mocks);

            //$httpBackend.when('GET', 'app/releaseCalendar/releaseCalendar.html').respond('');
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('productRegistrationConfig');
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), 'productRegistrationConfig');
    });

    it("should init controls on UI when initUi called", function () {
        sut.initUi();

        expect(sut.typeModalFormValidator).not.toBeNull();
        expect(sut.taskModalFormValidator).not.toBeNull();
        expect(sut.groupModalFormValidator).not.toBeNull();
    });

    it("should reset validators when resetValidaton called", function () {
        sut.typeModalFormValidator = { resetForm: function () { } };
        sut.taskModalFormValidator = { resetForm: function () { } };
        sut.groupModalFormValidator = { resetForm: function () { } };

        spyOn(sut.typeModalFormValidator, 'resetForm');
        spyOn(sut.taskModalFormValidator, 'resetForm');
        spyOn(sut.groupModalFormValidator, 'resetForm');

        sut.resetValidaton();

        expect(sut.typeModalFormValidator.resetForm).toHaveBeenCalled();
        expect(sut.taskModalFormValidator.resetForm).toHaveBeenCalled();
        expect(sut.groupModalFormValidator.resetForm).toHaveBeenCalled();
    });

    //---- types

    it("should populate currentType with empty values when showTypeModal called without parameter", function () {
        spyOn($.fn, 'modal');

        sut.showTypeModal();

        expect(sut.currentType.Name).toBe('');
        expect(sut.currentType.ExternalId).toBe('');
        expect($.fn.modal).toHaveBeenCalledWith('show');
    });

    it("should populate currentType with actual values when showTypeModal called with parameter", function () {
        spyOn($.fn, 'modal');

        sut.showTypeModal({ Name: 'name', ExternalId: 'externalid' });

        expect(sut.currentType.Name).toBe('name');
        expect(sut.currentType.ExternalId).toBe('externalid');
        expect($.fn.modal).toHaveBeenCalledWith('show');
    });

    it("should cleanup ui when hideTypeModal called", function () {
        spyOn($.fn, 'modal');
        spyOn(sut, 'resetValidaton');

        sut.hideTypeModal();

        expect(sut.resetValidaton).toHaveBeenCalled();
        expect($.fn.modal).toHaveBeenCalledWith('hide');
    });

    it("should not perform type saving when validation not passed", function () {
        sut.validateTypeModalForm = function () { return false; };

        var type = { Name: 'type', ExternalId: 'typeId' };

        sut.currentType = type;

        var deferred = $q.defer();
        mocks.remiapi.post.updateProductRequestType.and.returnValue(deferred.promise);

        sut.saveTypeModal();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.updateProductRequestType).not.toHaveBeenCalled();
    });

    it("should call createProductRequestType command when saveTypeModal invoked for new type", function () {
        sut.validateTypeModalForm = function () { return true; };
        spyOn(sut, 'updateTypeList');
        spyOn(sut, 'hideTypeModal');

        var type = { Name: 'type', ExternalId: '' };

        sut.currentType = type;

        var deferred = $q.defer();
        mocks.remiapi.post.createProductRequestType.and.returnValue(deferred.promise);

        sut.saveTypeModal();

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.createProductRequestType).toHaveBeenCalledWith({ RequestType: { ExternalId: window.jasmine.any(String), Name: 'type', RequestGroups: [] } });
        expect(sut.updateTypeList).toHaveBeenCalledWith('add', { ExternalId: window.jasmine.any(String), Name: 'type', RequestGroups: [] });
        expect(sut.hideTypeModal).toHaveBeenCalled();
    });

    it("should call updateProductRequestType command when saveTypeModal invoked for existing type", function () {
        sut.validateTypeModalForm = function () { return true; };
        spyOn(sut, 'updateTypeList');
        spyOn(sut, 'hideTypeModal');

        var type = { Name: 'type', ExternalId: 'typeId', RequestTypes: [] };

        sut.currentType = type;

        var deferred = $q.defer();
        mocks.remiapi.post.updateProductRequestType.and.returnValue(deferred.promise);

        sut.saveTypeModal();

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.updateProductRequestType).toHaveBeenCalledWith({ RequestType: { Name: 'type', ExternalId: 'typeId', RequestGroups: [] } });
        expect(sut.updateTypeList).toHaveBeenCalledWith('update', { Name: 'type', ExternalId: 'typeId', RequestGroups: [] });
        expect(sut.hideTypeModal).toHaveBeenCalled();
    });

    it("should show error when updateProductRequestType command failed", function () {
        sut.validateTypeModalForm = function () { return true; };
        spyOn(sut, 'updateTypeList');
        spyOn(sut, 'hideTypeModal');

        var type = { Name: 'type', ExternalId: 'typeId' };

        sut.currentType = type;

        var deferred = $q.defer();
        mocks.remiapi.post.updateProductRequestType.and.returnValue(deferred.promise);

        sut.saveTypeModal();

        deferred.reject('error');

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.updateProductRequestType).toHaveBeenCalledWith({ RequestType: { Name: 'type', ExternalId: 'typeId', RequestGroups: [] } });
        expect(sut.updateTypeList).not.toHaveBeenCalled();
        expect(sut.hideTypeModal).not.toHaveBeenCalled();
        expect(logger.error).toHaveBeenCalledWith('Can\'t save request type');
    });

    it("should add new type when updateType invoked for new type", function () {
        var type = { Name: 'name', ExternalId: 'externalid' };

        sut.requestConfig = [];

        sut.updateTypeList('add', type);

        expect(sut.requestConfig.length).toBe(1);
        expect(sut.requestConfig[0]).toBe(type);
    });

    it("should update existing type when updateType invoked for existing type", function () {
        sut.requestConfig = [{ Name: 'name', ExternalId: 'externalid' }];

        var type = { Name: 'name new', ExternalId: 'externalid' };

        sut.updateTypeList('update', type);

        expect(sut.requestConfig.length).toBe(1);
        expect(sut.requestConfig[0].Name).toBe('name new');
    });

    it("should populate currentType with empty values when showGroupModal called without parameter", function () {
        spyOn($.fn, 'modal');

        var type = { Name: 'type', ExternalId: 'typeid' };

        sut.showGroupModal(type);

        expect(sut.currentGroup.Name).toBe('');
        expect(sut.currentGroup.ExternalId).toBe('');
        expect(sut.currentGroup.Assignees.length).toBe(0);
        expect(sut.currentGroup.Type).toBeDefined();
        expect(sut.currentGroup.Type.Name).toBe('type');
        expect(sut.currentGroup.Type.ExternalId).toBe('typeid');
        expect($.fn.modal).toHaveBeenCalledWith('show');
    });

    it("should populate currentType with actual values when showGroupModal called with parameter", function () {
        spyOn($.fn, 'modal');

        var type = { Name: 'type', ExternalId: 'typeid' };
        var assignees = [{ FullName: 'acc1', ExternalId: 'accId' }];

        sut.showGroupModal(type, { Name: 'name', ExternalId: 'externalid', Assignees: assignees });

        expect(sut.currentGroup.Name).toBe('name');
        expect(sut.currentGroup.ExternalId).toBe('externalid');
        expect(sut.currentGroup.Assignees.length).toBe(1);
        expect(sut.currentGroup.Assignees[0].FullName).toBe('acc1');
        expect(sut.currentGroup.Assignees[0].ExternalId).toBe('accId');
        expect(sut.currentGroup.Type).toBeDefined();
        expect(sut.currentGroup.Type.Name).toBe('type');
        expect(sut.currentGroup.Type.ExternalId).toBe('typeid');
        expect($.fn.modal).toHaveBeenCalledWith('show');
    });

    it("should remove existing group when updateType invoked with delete operation type", function () {
        var type1 = { Name: 'type1', ExternalId: 'typeId1', RequestGroups: [] };
        var type2 = { Name: 'type2', ExternalId: 'typeId2', RequestGroups: [] };

        sut.requestConfig = [type1, type2];

        sut.updateTypeList('delete', type2);

        expect(sut.requestConfig.length).toBe(1);
        expect(sut.requestConfig[0].ExternalId).toBe('typeId1');
    });

    it("should not send deleteProductRequestType command when task doesn't has external id", function () {
        var type = { Name: 'type', ExternalId: 'typeId', RequestGroups: [] };

        sut.requestConfig = [type];

        var deferred = $q.defer();
        mocks.remiapi.post.deleteProductRequestType.and.returnValue(deferred.promise);

        sut.deleteType({ ExternalId: '' });

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.deleteProductRequestType).not.toHaveBeenCalled();
    });

    it("should send deleteProductRequestType command when invoked", function () {
        spyOn(sut, 'updateTypeList');
        var type = { Name: 'type', ExternalId: 'typeId', RequestTypes: [] };

        sut.requestConfig = [type];

        var deferred = $q.defer();
        mocks.remiapi.post.deleteProductRequestType.and.returnValue(deferred.promise);

        sut.deleteType(type);

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.deleteProductRequestType).toHaveBeenCalledWith({ RequestTypeId: 'typeId' });
        expect(sut.updateTypeList).toHaveBeenCalledWith('delete', type);
    });


    //---- groups

    it("should cleanup ui when hideGroupModal called", function () {
        spyOn($.fn, 'modal');
        spyOn(sut, 'resetValidaton');

        sut.currentGroup.Type = { Name: 'name' };
        sut.currentGroup.Assignees = [{ FullName: 'acc1' }];

        sut.hideGroupModal();

        expect(sut.currentGroup.Type).toBeNull();
        expect(sut.currentGroup.Assignees.length).toBe(0);
        expect(sut.resetValidaton).toHaveBeenCalled();
        expect($.fn.modal).toHaveBeenCalledWith('hide');
    });

    it("should not perform group saving when validation not passed", function () {
        sut.validateGroupModalForm = function () { return false; };

        var group = { Name: 'group', ExternalId: 'groupId' };

        sut.currentGroup = group;

        var deferred = $q.defer();
        mocks.remiapi.post.updateProductRequestGroup.and.returnValue(deferred.promise);

        sut.saveGroupModal();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.updateProductRequestGroup).not.toHaveBeenCalled();
    });

    it("should call createProductRequestGroup command when saveGroupModal invoked for new group", function () {
        sut.validateGroupModalForm = function () { return true; };
        spyOn(sut, 'updateGroupList');
        spyOn(sut, 'hideGroupModal');

        var group = { Name: 'group', ExternalId: '' };
        var type = { Name: 'type', ExternalId: 'typeId', RequestGroups: [] };

        sut.currentGroup = group;
        sut.currentGroup.Type = type;

        var deferred = $q.defer();
        mocks.remiapi.post.createProductRequestGroup.and.returnValue(deferred.promise);

        sut.saveGroupModal();

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.createProductRequestGroup).toHaveBeenCalledWith({ RequestGroup: { ExternalId: window.jasmine.any(String), Name: 'group', RequestTasks: window.jasmine.any(Array), Assignees: undefined, ProductRequestTypeId: 'typeId' } });
        expect(sut.updateGroupList).toHaveBeenCalledWith('add', type, { ExternalId: window.jasmine.any(String), Name: 'group', RequestTasks: window.jasmine.any(Array), Assignees: undefined, ProductRequestTypeId: 'typeId' });
        expect(sut.hideGroupModal).toHaveBeenCalled();
    });

    it("should call updateProductRequestGroup command when saveGroupModal invoked for existing group", function () {
        sut.validateGroupModalForm = function () { return true; };
        spyOn(sut, 'updateGroupList');
        spyOn(sut, 'hideGroupModal');

        var group = { Name: 'group', ExternalId: 'groupId' };
        var type = { Name: 'name', ExternalId: 'typeId', RequestGroups: [group] };

        sut.currentGroup = group;
        sut.currentGroup.Type = type;

        var deferred = $q.defer();
        mocks.remiapi.post.updateProductRequestGroup.and.returnValue(deferred.promise);

        sut.saveGroupModal();

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.updateProductRequestGroup).toHaveBeenCalledWith({ RequestGroup: { Name: 'group', ExternalId: 'groupId', RequestTasks: window.jasmine.any(Array), Assignees: undefined, ProductRequestTypeId: 'typeId' } });
        expect(sut.updateGroupList).toHaveBeenCalledWith('update', type, { Name: 'group', ExternalId: 'groupId', RequestTasks: window.jasmine.any(Array), Assignees: undefined, ProductRequestTypeId: 'typeId' });
        expect(sut.hideGroupModal).toHaveBeenCalled();
    });

    it("should show error when updateProductRequestGroup command failed", function () {
        sut.validateGroupModalForm = function () { return true; };
        spyOn(sut, 'updateGroupList');
        spyOn(sut, 'hideGroupModal');

        var group = { Name: 'group', ExternalId: 'groupId' };
        var type = { Name: 'name', ExternalId: 'typeId', RequestGroups: [group] };

        sut.currentGroup = group;
        sut.currentGroup.Type = type;

        var deferred = $q.defer();
        mocks.remiapi.post.updateProductRequestGroup.and.returnValue(deferred.promise);

        sut.saveGroupModal();

        deferred.reject('error');

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.updateProductRequestGroup).toHaveBeenCalledWith({ RequestGroup: { Name: 'group', ExternalId: 'groupId', RequestTasks: window.jasmine.any(Array), Assignees: undefined, ProductRequestTypeId: 'typeId' } });
        expect(sut.updateGroupList).not.toHaveBeenCalled();
        expect(sut.hideGroupModal).not.toHaveBeenCalled();
        expect(logger.error).toHaveBeenCalledWith('Can\'t save request group');
    });

    it("should add new group when updateGroup invoked for new group", function () {
        var group = { Name: 'group', ExternalId: 'groupId' };
        var type = { Name: 'type', ExternalId: 'typeId', RequestGroups: [] };

        sut.requestConfig = [type];

        sut.updateGroupList('add', type, group);

        expect(sut.requestConfig[0].RequestGroups.length).toBe(1);
        expect(sut.requestConfig[0].RequestGroups[0].Name).toBe('group');
        expect(sut.requestConfig[0].RequestGroups[0].ExternalId).toBe('groupId');
    });

    it("should update existing group when updateGroup invoked for existing group", function () {
        var group = { Name: 'group', ExternalId: 'groupId' };
        var type = { Name: 'name', ExternalId: 'typeId', RequestGroups: [group] };

        sut.requestConfig = [type];

        sut.updateGroupList('update', type, { Name: 'group new', ExternalId: 'groupId' });

        expect(sut.requestConfig[0].RequestGroups.length).toBe(1);
        expect(sut.requestConfig[0].RequestGroups[0].Name).toBe('group new');
        expect(sut.requestConfig[0].RequestGroups[0].ExternalId).toBe('groupId');
    });

    it("should add assignees into the group when addGroupAssignees invoked", function () {
        var group = { Name: 'group', ExternalId: 'groupId', Assignees: [] };

        sut.currentGroup = group;

        sut.addGroupAssignees([{ FullName: 'acc1', ExternalId: 'acc1Id' }, { FullName: 'acc2', ExternalId: 'acc2Id' }]);

        expect(sut.currentGroup.Assignees.length).toBe(2);
        expect(sut.currentGroup.Assignees[0].FullName).toBe('acc1');
        expect(sut.currentGroup.Assignees[0].ExternalId).toBe('acc1Id');
        expect(sut.currentGroup.Assignees[1].FullName).toBe('acc2');
        expect(sut.currentGroup.Assignees[1].ExternalId).toBe('acc2Id');
    });

    it("should remove account from assignees when removeGroupAssignee invoked", function () {
        var group = { Name: 'group', ExternalId: 'groupId', Assignees: [{ FullName: 'acc1', ExternalId: 'acc1Id' }, { FullName: 'acc2', ExternalId: 'acc2Id' }] };

        sut.currentGroup = group;

        sut.removeGroupAssignee(group, group.Assignees[0]);

        expect(sut.currentGroup.Assignees.length).toBe(1);
        expect(sut.currentGroup.Assignees[0].FullName).toBe('acc2');
        expect(sut.currentGroup.Assignees[0].ExternalId).toBe('acc2Id');
    });

    it("should remove existing group when updateGroup invoked with delete operation type", function () {
        var group1 = { Name: 'group1', ExternalId: 'groupId1', RequestTasks: [] };
        var group2 = { Name: 'group2', ExternalId: 'groupId2', RequestTasks: [] };
        var type = { Name: 'type', ExternalId: 'typeId', RequestGroups: [group1, group2] };

        sut.requestConfig = [type];

        sut.updateGroupList('delete', type, group2);

        expect(sut.requestConfig[0].RequestGroups.length).toBe(1);
        expect(sut.requestConfig[0].RequestGroups[0].ExternalId).toBe('groupId1');
    });

    it("should not send deleteProductRequestGroup command when task doesn't has external id", function () {
        var group = { Name: 'group', ExternalId: 'groupId', RequestTasks: [] };
        var type = { Name: 'type', ExternalId: 'typeId', RequestGroups: [group] };

        sut.requestConfig = [type];

        var deferred = $q.defer();
        mocks.remiapi.post.deleteProductRequestGroup.and.returnValue(deferred.promise);

        sut.deleteGroup(type, { ExternalId: '' });

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.deleteProductRequestGroup).not.toHaveBeenCalled();
    });

    it("should send deleteProductRequestGroup command when invoked", function () {
        spyOn(sut, 'updateGroupList');
        var group = { Name: 'group', ExternalId: 'groupId', RequestTasks: [] };
        var type = { Name: 'type', ExternalId: 'typeId', RequestGroups: [group] };

        sut.requestConfig = [type];

        var deferred = $q.defer();
        mocks.remiapi.post.deleteProductRequestGroup.and.returnValue(deferred.promise);

        sut.deleteGroup(type, group);

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.deleteProductRequestGroup).toHaveBeenCalledWith({ RequestGroupId: 'groupId' });
        expect(sut.updateGroupList).toHaveBeenCalledWith('delete', type, group);
    });

    it("should return list of assigned account when getGroupAssignees invoked", function () {
        var group = {
            Name: 'group', ExternalId: 'groupId', RequestTasks: [],
            Assignees: [
                { ExternalId: 'accId1', FullName: 'acc1' },
                { ExternalId: 'accId2', FullName: 'acc2' }
            ]
        };

        sut.currentGroup = group;

        var result = sut.getGroupAssignees();

        expect(result).not.toBeNull();
        expect(result.length).toBe(2);
        expect(result[0].ExternalId).toBe('accId1');
        expect(result[1].ExternalId).toBe('accId2');
    });

    it("should add new assignees when addGroupAssignees invoked", function () {
        var group = {
            Name: 'group', ExternalId: 'groupId', RequestTasks: [],
            Assignees: [
                { ExternalId: 'accId0', FullName: 'acc0' }
            ]
        };

        sut.currentGroup = group;

        sut.addGroupAssignees([
            { ExternalId: 'accId1', FullName: 'acc1', Email: 'em@1' },
            { ExternalId: 'accId2', FullName: 'acc2', Email: 'em@2' }
        ]);

        expect(sut.currentGroup.Assignees.length).toBe(3);
        expect(sut.currentGroup.Assignees[0].ExternalId).toBe('accId0');
        expect(sut.currentGroup.Assignees[1].ExternalId).toBe('accId1');
        expect(sut.currentGroup.Assignees[1].Email).toBe('em@1');
        expect(sut.currentGroup.Assignees[2].ExternalId).toBe('accId2');
        expect(sut.currentGroup.Assignees[2].Email).toBe('em@2');
    });

    //getGroupAssignees

    //---- tasks

    it("should populate currentTask with empty values when showTaskModal called without parameter", function () {
        spyOn($.fn, 'modal');

        var group = { Name: 'group', ExternalId: 'groupId' };
        var type = { Name: 'type', ExternalId: 'typeId', RequestGroups: [group] };

        sut.showTaskModal(type, group);

        expect(sut.currentTask.Question).toBe('');
        expect(sut.currentTask.ExternalId).toBe('');
        expect(sut.currentTask.Type).toBe(type);
        expect(sut.currentTask.Group).toBe(group);
        expect($.fn.modal).toHaveBeenCalledWith('show');
    });

    it("should populate currentTask with actual values when showTaskModal called with parameter", function () {
        spyOn($.fn, 'modal');

        var task = { Question: 'task', ExternalId: 'taskId' };
        var group = { Name: 'group', ExternalId: 'groupId' };
        var type = { Name: 'type', ExternalId: 'typeId', RequestGroups: [group] };

        sut.showTaskModal(type, group, task);

        mocks.$scope.$digest();

        expect(sut.currentTask.Question).toBe('task');
        expect(sut.currentTask.ExternalId).toBe('taskId');
        expect($.fn.modal).toHaveBeenCalledWith('show');
    });

    it("should cleanup ui when hideTaskModal called", function () {
        spyOn($.fn, 'modal');
        spyOn(sut, 'resetValidaton');

        sut.currentTask.Type = { Name: 'type' };
        sut.currentTask.Group = { Name: 'grpup' };

        sut.hideTaskModal();

        expect(sut.currentTask.Type).toBeNull();
        expect(sut.currentTask.Group).toBeNull();
        expect(sut.resetValidaton).toHaveBeenCalled();
        expect($.fn.modal).toHaveBeenCalledWith('hide');
    });

    it("should not perform task saving when validation not passed", function () {
        sut.validateTaskModalForm = function () { return false; };

        var task = { Question: 'task', ExternalId: 'taskId' };

        sut.currentTask = task;

        var deferred = $q.defer();
        mocks.remiapi.post.updateProductRequestTask.and.returnValue(deferred.promise);

        sut.saveTaskModal();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.updateProductRequestTask).not.toHaveBeenCalled();
    });

    it("should call createProductRequestTask command when saveTaskModal invoked for new task", function () {
        sut.validateTaskModalForm = function () { return true; };
        spyOn(sut, 'updateTaskList');
        spyOn(sut, 'hideTaskModal');

        var task = { Question: 'task', ExternalId: '' };
        var group = { Name: 'group', ExternalId: 'groupId', RequestTasks: [] };
        var type = { Name: 'name', ExternalId: 'typeId', RequestGroups: [group] };

        sut.currentTask = task;
        sut.currentTask.Type = type;
        sut.currentTask.Group = group;

        var deferred = $q.defer();
        mocks.remiapi.post.createProductRequestTask.and.returnValue(deferred.promise);

        sut.saveTaskModal();

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.createProductRequestTask).toHaveBeenCalledWith({ RequestTask: { ExternalId: window.jasmine.any(String), Question: 'task', ProductRequestGroupId: 'groupId' } });
        expect(sut.updateTaskList).toHaveBeenCalledWith('add', type, group, { ExternalId: window.jasmine.any(String), Question: 'task', ProductRequestGroupId: 'groupId' });
        expect(sut.hideTaskModal).toHaveBeenCalled();
    });

    it("should call updateProductRequestTask command when saveTaskModal invoked for existing task", function () {
        sut.validateTaskModalForm = function () { return true; };
        spyOn(sut, 'updateTaskList');
        spyOn(sut, 'hideTaskModal');

        var task = { Question: 'task', ExternalId: 'taskId' };
        var group = { Name: 'group', ExternalId: 'groupId', RequestTasks: [task] };
        var type = { Name: 'name', ExternalId: 'typeId', RequestGroups: [group] };

        sut.currentTask = task;
        sut.currentTask.Type = type;
        sut.currentTask.Group = group;

        var deferred = $q.defer();
        mocks.remiapi.post.updateProductRequestTask.and.returnValue(deferred.promise);

        sut.saveTaskModal();

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.updateProductRequestTask).toHaveBeenCalledWith({ RequestTask: { Question: 'task', ExternalId: 'taskId', ProductRequestGroupId: 'groupId' } });
        expect(sut.updateTaskList).toHaveBeenCalledWith('update', type, group, { Question: 'task', ExternalId: 'taskId', ProductRequestGroupId: 'groupId' });
        expect(sut.hideTaskModal).toHaveBeenCalled();
    });

    it("should show error when updateProductRequestTask command failed", function () {
        sut.validateTaskModalForm = function () { return true; };
        spyOn(sut, 'updateTaskList');
        spyOn(sut, 'hideTaskModal');

        var task = { Question: 'task', ExternalId: 'taskId' };
        var group = { Name: 'group', ExternalId: 'groupId', RequestTasks: [task] };
        var type = { Name: 'name', ExternalId: 'typeId', RequestGroups: [group] };

        sut.currentTask = task;
        sut.currentTask.Type = type;
        sut.currentTask.Group = group;

        var deferred = $q.defer();
        mocks.remiapi.post.updateProductRequestTask.and.returnValue(deferred.promise);

        sut.saveTaskModal();

        deferred.reject('error');

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.updateProductRequestTask).toHaveBeenCalledWith({ RequestTask: { Question: 'task', ExternalId: 'taskId', ProductRequestGroupId: 'groupId' } });
        expect(sut.updateTaskList).not.toHaveBeenCalled();
        expect(sut.hideTaskModal).not.toHaveBeenCalled();
        expect(logger.error).toHaveBeenCalledWith('Can\'t save request task');
    });

    it("should add new task when updateTaskList invoked for new task", function () {
        var task = { Question: 'task', ExternalId: 'taskId' };
        var group = { Name: 'group', ExternalId: 'groupId', RequestTasks: [] };
        var type = { Name: 'type', ExternalId: 'typeId', RequestGroups: [group] };

        sut.requestConfig = [type];

        sut.updateTaskList('add', type, group, task);

        expect(sut.requestConfig[0].RequestGroups[0].RequestTasks.length).toBe(1);
        expect(sut.requestConfig[0].RequestGroups[0].RequestTasks[0].Question).toBe('task');
        expect(sut.requestConfig[0].RequestGroups[0].RequestTasks[0].ExternalId).toBe('taskId');
    });

    it("should update existing task when updateTask invoked for existing task", function () {
        var task = { Question: 'task', ExternalId: 'taskId' };
        var group = { Name: 'group', ExternalId: 'groupId', RequestTasks: [task] };
        var type = { Name: 'name', ExternalId: 'typeId', RequestGroups: [group] };

        sut.requestConfig = [type];

        sut.updateTaskList('update', type, group, { Question: 'task new', ExternalId: 'taskId' });

        mocks.$scope.$digest();

        expect(sut.requestConfig[0].RequestGroups[0].RequestTasks.length).toBe(1);
        expect(sut.requestConfig[0].RequestGroups[0].RequestTasks[0].Question).toBe('task new');
        expect(sut.requestConfig[0].RequestGroups[0].RequestTasks[0].ExternalId).toBe('taskId');
    });

    it("should remove existing task when updateTask invoked with delete operation type", function () {
        var task1 = { Question: 'task', ExternalId: 'taskId1' };
        var task2 = { Question: 'task', ExternalId: 'taskId2' };
        var group = { Name: 'group', ExternalId: 'groupId', RequestTasks: [task1, task2] };
        var type = { Name: 'name', ExternalId: 'typeId', RequestGroups: [group] };

        sut.requestConfig = [type];

        sut.updateTaskList('delete', type, group, task2);

        expect(sut.requestConfig[0].RequestGroups[0].RequestTasks.length).toBe(1);
        expect(sut.requestConfig[0].RequestGroups[0].RequestTasks[0].ExternalId).toBe('taskId1');
    });

    it("should not send delete command when task doesn't has external id", function () {
        var task = { Question: 'task', ExternalId: 'taskId' };
        var group = { Name: 'group', ExternalId: 'groupId', RequestTasks: [task] };
        var type = { Name: 'name', ExternalId: 'typeId', RequestGroups: [group] };

        sut.requestConfig = [type];

        var deferred = $q.defer();
        mocks.remiapi.post.deleteProductRequestTask.and.returnValue(deferred.promise);

        sut.deleteTask(type, group, { ExternalId: '' });

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.deleteProductRequestTask).not.toHaveBeenCalled();
    });

    it("should call deleteProductRequestTask post command when invoked", function () {
        spyOn(sut, 'updateTaskList');
        var task = { Question: 'task', ExternalId: 'taskId' };
        var group = { Name: 'group', ExternalId: 'groupId', RequestTasks: [task] };
        var type = { Name: 'name', ExternalId: 'typeId', RequestGroups: [group] };

        sut.requestConfig = [type];

        var deferred = $q.defer();
        mocks.remiapi.post.deleteProductRequestTask.and.returnValue(deferred.promise);

        sut.deleteTask(type, group, task);

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.deleteProductRequestTask).toHaveBeenCalledWith({ RequestTaskId: 'taskId' });
        expect(sut.updateTaskList).toHaveBeenCalledWith('delete', type, group, task);
    });
});

