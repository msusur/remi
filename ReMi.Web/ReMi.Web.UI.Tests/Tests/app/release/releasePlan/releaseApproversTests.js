describe("Release Approvers Controller", function () {
    var sut, mocks;
    var $q, $rootScope;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });

        mocks = {
            common: {
                logger: window.jasmine.createSpyObj('logger', ['getLogger']),
                $q: window.jasmine.createSpyObj('$q', ['when']),
                activateController: window.jasmine.createSpy('activateController').and.returnValue({ then: window.jasmine.createSpy('then') }),
                sendEvent: window.jasmine.createSpy('sendEvent'),
                handleEvent: window.jasmine.createSpy('handleEvent')
            },
            remiapi: window.jasmine.createSpyObj('remiapi', ['executeCommand', 'approveRelease', 'removeReleaseApprover', 'addReleaseApprovers', 'getReleaseApprovers']),
            authService: window.jasmine.createSpyObj('authService', ['identity', 'state']),
            commandPermissions: window.jasmine.createSpyObj('commandPermissions', ['readPermissions', 'checkCommand']),
            notifications: window.jasmine.createSpyObj('notifications', ['subscribe', 'unsubscribe']),
        };

        mocks.common.logger.getLogger.and.returnValue({ console: function () { }, warn: function () { }, error: function () { } });

        inject(function ($controller, _$q_, _$rootScope_) {
            $q = _$q_;
            $rootScope = _$rootScope_;
            mocks.$scope = $rootScope.$new();
            mocks.$scope.registerWidget = function (name) { return true; };
            spyOn(mocks.$scope, 'registerWidget');

            mocks.commandPermissions.readPermissions.and.returnValue($q.when());

            sut = $controller('releaseApprovers', mocks);
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('releaseApprovers');
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), 'releaseApprovers', mocks.$scope);

        expect(mocks.common.handleEvent).toHaveBeenCalledWith('release.ReleaseWindowLoadedEvent', window.jasmine.any(Function), mocks.$scope);
    });

    it("should not allow add approvers for not accepeted role", function () {
        sut.manageOptions.allowAdd = false;
        sut.manageOptions.allowApprove = false;
        sut.manageOptions.allowRemove = false;

        mocks.commandPermissions.checkCommand.and.returnValue({ result: true });

        sut.adjustManageOptions();

        expect(sut.manageOptions.allowAdd).toBe(true);
        expect(sut.manageOptions.allowApprove).toBe(true);
        expect(sut.manageOptions.allowRemove).toBe(true);
    });

    it("should fail approve when called without signature", function () {
        var res = sut.approve();
        expect(res).toBe(null);
    });

    it("should call remiapi.approveRelease and resolve differ, when approve pass", function () {
        var deferred = $q.defer();
        var signatureDeferred = $q.defer();
        sut.releaseWindow = { ExternalId: 'Window' };
        sut.releaseApprovers = [
            { Account: { ExternalId: 'a1' } },
            { Account: { ExternalId: 'a2' } }
        ];

        mocks.remiapi.approveRelease.and.returnValue(deferred.promise);
        var resolved = false, rejected = false;
        signatureDeferred.promise.then(function () { resolved = true; }, function () { rejected = true; });

        sut.approve({ deferred: signatureDeferred, userName: 'user Name', password: 'Password', Comment: 'smth' },
            { Account: { ExternalId: 'a2' } });
        deferred.resolve();
        mocks.$scope.$digest();

        expect(resolved).toBeTruthy();
        expect(rejected).toBeFalsy();
        expect(mocks.remiapi.approveRelease).toHaveBeenCalledWith({
            ReleaseWindowId: 'Window',
            AccountId: 'a2',
            Comment: 'smth',
            Password: 'Password',
            UserName: 'user Name'
        });
        expect(sut.releaseApprovers[1].IsApproved).toBeTruthy();
        expect(sut.releaseApprovers[1].ApprovedOn).toBeDefined();
    });

    it("should call remiapi.approveRelease and reject differ, when approve fail", function () {
        var deferred = $q.defer();
        var signatureDeferred = $q.defer();
        sut.releaseWindow = { ExternalId: 'Window' };
        mocks.remiapi.approveRelease.and.returnValue(deferred.promise);
        var resolved = false, rejected = false;
        signatureDeferred.promise.then(function () { resolved = true; }, function () { rejected = true; });

        sut.approve({ deferred: signatureDeferred, userName: 'user Name', password: 'Password', Comment: 'smth' },
            { Account: { ExternalId: 'AccountId' } });
        deferred.reject();
        mocks.$scope.$digest();

        expect(resolved).toBeFalsy();
        expect(rejected).toBeTruthy();
        expect(mocks.remiapi.approveRelease).toHaveBeenCalledWith({
            ReleaseWindowId: 'Window',
            AccountId: 'AccountId',
            Comment: 'smth',
            Password: 'Password',
            UserName: 'user Name'
        });
    });

    it("should not send approve command, when accountId is empty", function () {
        var deferred = $q.defer();
        var signatureDeferred = $q.defer();
        sut.releaseWindow = { ExternalId: 'Window' };
        mocks.remiapi.approveRelease.and.returnValue(deferred.promise);

        sut.approve({ deferred: signatureDeferred, userName: 'user Name', password: 'Password', Comment: 'smth' },
            { Account: {} });
        deferred.reject();

        expect(mocks.remiapi.approveRelease).not.toHaveBeenCalled();
    });

    it("should return existign release approvers when getSelectedAccounts invoked", function () {
        sut.releaseWindow = { ExternalId: 'Window' };
        sut.releaseApprovers = [
            { Account: { 'ExternalId': 'a1' } },
            { Account: { 'ExternalId': 'a2' } }
        ];
        var res = sut.getSelectedAccounts();

        expect(res.length).toBe(2);
        expect(res[0].ExternalId).toBe('a1');
        expect(res[1].ExternalId).toBe('a2');
    });

    it("should bind to release when releaseWindowLoadedEventHandler invoked with not emapty release", function () {
        spyOn(sut, 'adjustManageOptions');
        sut.releaseWindow = null;

        var deferred = $q.defer();
        mocks.remiapi.getReleaseApprovers.and.returnValue(deferred.promise);

        deferred.resolve({ Approvers: [] });

        sut.releaseWindowLoadedEventHandler({ ExternalId: 'Window', Status: 'Opened' });

        mocks.$scope.$digest();

        expect(sut.releaseWindow).not.toBeNull();
        expect(sut.releaseWindow.ExternalId).toBe('Window');
        expect(sut.releaseWindow.Status).toBe('Opened');
        expect(sut.state.bindedToReleaseWindow).toBe(true);
        expect(sut.isReleaseOpened).toBe(true);
        expect(sut.adjustManageOptions).toHaveBeenCalled();
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ReleaseStatusChangedEvent', { 'ReleaseWindowId': 'Window' });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ApprovementEvent', { 'ReleaseWindowId': 'Window' });
    });

    it("should unbinded from release when releaseWindowLoadedEventHandler invoked without parameters", function () {
        spyOn(sut, 'adjustManageOptions');
        sut.releaseWindow = { ExternalId: 'Window', Status: 'Opened' };

        sut.releaseWindowLoadedEventHandler();

        expect(sut.releaseWindow).toBeNull();
        expect(sut.state.bindedToReleaseWindow).toBe(false);
        expect(sut.isReleaseOpened).toBe(false);
        expect(sut.adjustManageOptions).toHaveBeenCalled();
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseStatusChangedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ApprovementEvent');
    });

    it("should mark approver when approvement event comes from server", function () {
        sut.releaseWindow = { ExternalId: 'Window', Status: 'Opened' };
        sut.releaseApprovers = [{ ExternalId: 'id' }, { ExternalId: 'id1' }];

        sut.serverNotificationHandler({ name: 'ApprovementEvent', data: { ApproverId: 'id', Comment: 'desc' } });

        expect(sut.releaseApprovers[0].IsApproved).toBe(true);
        expect(sut.releaseApprovers[0].Comment).toEqual('desc');
    });

    it("should change release status when change status event comes from server", function () {
        sut.releaseWindow = { ExternalId: 'Window', Status: 'Opened' };
        sut.releaseApprovers = [{ ExternalId: 'id' }, { ExternalId: 'id1' }];

        sut.serverNotificationHandler({
            name: 'ReleaseStatusChangedEvent',
            data: { ReleaseStatus: 'Approved' }
        });

        expect(sut.releaseWindow.Status).toEqual('Approved');
        expect(sut.isReleaseOpened).toBe(false);
    });

    it("should add new approver when event comes from server", function () {
        sut.releaseWindow = { ExternalId: 'Window', Status: 'Opened' };
        sut.releaseApprovers = [{ ExternalId: 'id', Account: { ExternalId: 'acc' } }, { ExternalId: 'id1', Account: { ExternalId: 'acc1' } }];

        sut.serverNotificationHandler({
            name: 'ApproverAddedToReleaseWindowEvent',
            data: { Approver: { ExternalId: 'id2', Account: { ExternalId: 'acc2' } } }
        });

        expect(sut.releaseApprovers.length).toEqual(3);
        expect(sut.releaseApprovers[2].ExternalId).toEqual('id2');
    });

    it("should remove approver when event comes from server", function () {
        sut.releaseWindow = { ExternalId: 'Window', Status: 'Opened' };
        sut.releaseApprovers = [{ ExternalId: 'id', Account: { ExternalId: 'acc' } }, { ExternalId: 'id1', Account: { ExternalId: 'acc1' } }];

        sut.serverNotificationHandler({
            name: 'ApproverRemovedFromReleaseEvent',
            data: { ApproverId: 'id', Account: { ExternalId: 'acc' } }
        });

        expect(sut.releaseApprovers.length).toEqual(1);
        expect(sut.releaseApprovers[0].ExternalId).toEqual('id1');
    });

    it("should add new approver to local collection when addApprovers invoked", function () {
        sut.releaseWindow = { ExternalId: 'Window' };

        var deferred = $q.defer();
        mocks.remiapi.addReleaseApprovers.and.returnValue(deferred.promise);

        sut.addApprovers([{ ExternalId: 'acc1' }]);

        deferred.resolve();
        mocks.$scope.$digest();

        expect(sut.releaseApprovers.length).toBe(1);
        expect(sut.releaseApprovers[0].Account.ExternalId).toBe('acc1');
    });

    it("should remove approver from local collection when removeApprover invoked", function () {
        sut.releaseWindow = { ExternalId: 'Window' };
        sut.releaseApprovers = [{ Account: { ExternalId: 'acc1' } }];

        var deferred = $q.defer();
        mocks.remiapi.removeReleaseApprover.and.returnValue(deferred.promise);

        sut.removeApprover([{ ExternalId: 'acc1' }]);

        deferred.resolve();
        mocks.$scope.$digest();

        expect(sut.releaseApprovers.length).toBe(0);
    });
});

