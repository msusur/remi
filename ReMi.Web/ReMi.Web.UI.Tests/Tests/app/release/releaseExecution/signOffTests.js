describe("Sign Offs Controller", function () {
    var sut, mocks, logger;
    var $q, $rootScope;

    beforeEach(function () {
        module("app");

        mocks = {
            common: {
                logger: jasmine.createSpyObj('logger', ['getLogger']),
                activateController: jasmine.createSpy('activateController').and.returnValue({ then: jasmine.createSpy('then') }),
                handleEvent: window.jasmine.createSpy('handleEvent'),
                sendEvent: window.jasmine.createSpy('sendEvent')
            },
            config: { events: { notificationReceived: 'notifications.received', loggedIn: 'auth.loggedIn' } },
            remiapi: jasmine.createSpyObj('remiapi', ['signOffRelease', 'getSignOffs', 'addSignOffs', 'removeSignOff', 'executeBusinessRule']),
            authService: { identity: { email: 'first' }, state: {} },
            notifications: jasmine.createSpyObj('notifications', ['subscribe', 'unsubscribe'])
        };

        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);

        inject(function ($controller, _$q_, _$rootScope_) {
            $q = _$q_;
            $rootScope = _$rootScope_;
            mocks.$scope = $rootScope.$new();
            mocks.$scope.registerWidget = function (name) { return true; };
            spyOn(mocks.$scope, 'registerWidget');

            sut = $controller('signOff', mocks);
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('signOff');
        expect(mocks.common.activateController).toHaveBeenCalledWith(jasmine.any(Array), 'signOff', mocks.$scope);

        expect(mocks.common.handleEvent).toHaveBeenCalledWith('release.ReleaseWindowLoadedEvent', jasmine.any(Function), mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('auth.loggedIn', jasmine.any(Function), mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('notifications.received', jasmine.any(Function), mocks.$scope);
    });

    it("should not allow manage sign offs for not logged in users", function () {
        mocks.authService.isLoggedIn = false;

        sut.updateAllowManageSignOff();

        expect(sut.allowManageSignOff).toBe(false);
    });

    it("should not allow manage sign offs for not accepeted role", function () {
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity.role = 'Team member';

        sut.updateAllowManageSignOff();

        expect(sut.allowManageSignOff).toBe(true);
    });

    it("should allow manage sign offs for Admin", function () {
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity.role = 'Admin';

        sut.updateAllowManageSignOff();

        expect(sut.allowManageSignOff).toBe(true);
    });

    it("should allow manage sign offs for ReleaseEngineer", function () {
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity.role = 'Release engineer';

        sut.updateAllowManageSignOff();

        expect(sut.allowManageSignOff).toBe(true);
    });

    it("should allow manage sign offs for ExecutiveManager", function () {
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity.role = 'Executive manager';

        sut.updateAllowManageSignOff();

        expect(sut.allowManageSignOff).toBe(true);
    });

    it("should allow manage sign offs for PO", function () {
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity.role = 'Product owner';

        sut.updateAllowManageSignOff();

        expect(sut.allowManageSignOff).toBe(true);
    });


    it("should initialize controller, when gets release window id", function () {
        spyOn(sut, 'updateAllowManageSignOff');
        spyOn(sut, 'getSigners');
        var releaseWindow = { ExternalId: 'external id' };

        sut.releaseWindowLoadedEventHandler(releaseWindow);

        expect(sut.isReleaseClosed).toEqual(false);
        expect(sut.state.bindedToReleaseWindow).toEqual(true);
        expect(sut.getSigners).toHaveBeenCalledWith(releaseWindow.ExternalId);
        expect(sut.updateAllowManageSignOff).toHaveBeenCalled();
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ReleaseStatusChangedEvent', { 'ReleaseWindowId': 'external id' });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ReleaseSignersAddedEvent', { 'ReleaseWindowId': 'external id' });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ReleaseSignedOffBySignerEvent', { 'ReleaseWindowId': 'external id' });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('RemoveSignOffEvent', { 'ReleaseWindowId': 'external id' });
    });

    it("should initialize controller, when gets release window id and window was already loaded", function () {
        spyOn(sut, 'updateAllowManageSignOff');
        spyOn(sut, 'getSigners');
        var releaseWindow = { ExternalId: 'external id' };
        sut.releaseWindow = { ExternalId: '1' };

        sut.releaseWindowLoadedEventHandler(releaseWindow);

        expect(sut.isReleaseClosed).toEqual(false);
        expect(sut.state.bindedToReleaseWindow).toEqual(true);
        expect(sut.getSigners).toHaveBeenCalledWith(releaseWindow.ExternalId);
        expect(sut.updateAllowManageSignOff).toHaveBeenCalled();
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseStatusChangedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseSignersAddedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseSignedOffBySignerEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('RemoveSignOffEvent');
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ReleaseStatusChangedEvent', { 'ReleaseWindowId': 'external id' });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ReleaseSignersAddedEvent', { 'ReleaseWindowId': 'external id' });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ReleaseSignedOffBySignerEvent', { 'ReleaseWindowId': 'external id' });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('RemoveSignOffEvent', { 'ReleaseWindowId': 'external id' });
    });

    it("should switch controller to unbind state, when gets empty release window", function () {
        sut.releaseWindowLoadedEventHandler();

        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseStatusChangedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseSignersAddedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseSignedOffBySignerEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('RemoveSignOffEvent');
        expect(sut.signers.length).toEqual(0);
        expect(sut.state.bindedToReleaseWindow).toEqual(false);
    });

    it("should fail sign off when called without signature", function () {
        var res = sut.sign();

        expect(res).toBe(null);
    });

    it("should not sign without signature", function () {
        sut.sign();

        expect(logger.warn).toHaveBeenCalledWith('Please provide credentials');
    });

    it("should not sign without account id", function () {
        sut.sign({});

        expect(logger.warn).toHaveBeenCalledWith('Please provide credentials');
    });

    it("should not sign without release window", function () {
        var deferred = $q.defer(), resolved = false, rejected = false;
        deferred.promise.then(function () { resolved = true; }, function () { rejected = true; });

        sut.sign({ deferred: deferred, userName: 'user name', password: 'password' });
        mocks.$scope.$digest();

        expect(logger.warn).toHaveBeenCalledWith('Release window not selected');
        expect(rejected).toBeTruthy();
        expect(resolved).toBeFalsy();
    });

    it("should not sign, when Signer is empty", function () {
        var deferred = $q.defer(), resolved = false, rejected = false;
        deferred.promise.then(function () { resolved = true; }, function () { rejected = true; });
        sut.releaseWindow = { ExternalId: 'smth' };

        sut.sign({ deferred: deferred, userName: 'user name', password: 'password' }, {});
        mocks.$scope.$digest();

        expect(logger.warn).not.toHaveBeenCalled();
        expect(rejected).toBeTruthy();
        expect(resolved).toBeFalsy();
    });

    it("should not sign, when Signer.ExternalId is empty", function () {
        var deferred = $q.defer(), resolved = false, rejected = false;
        deferred.promise.then(function () { resolved = true; }, function () { rejected = true; });
        sut.releaseWindow = { ExternalId: 'smth' };

        sut.sign({ deferred: deferred, userName: 'user name', password: 'password' }, { Signer: {} });
        mocks.$scope.$digest();

        expect(logger.warn).not.toHaveBeenCalled();
        expect(rejected).toBeTruthy();
        expect(resolved).toBeFalsy();
    });

    it("should sign release", function () {
        sut.releaseWindow = { ExternalId: 'smth' };
        var deferred = $q.defer();
        var deferred2 = $q.defer(), resolved = false, rejected = false;
        mocks.remiapi.signOffRelease.and.returnValue(deferred.promise);
        deferred2.promise.then(function () { resolved = true; }, function () { rejected = true; });

        sut.sign({ deferred: deferred2, userName: 'user name', password: 'password', Comment: 'super' }, { Signer: { ExternalId: 'external id' } });
        deferred.resolve();
        mocks.$scope.$digest();

        expect(mocks.remiapi.signOffRelease).toHaveBeenCalledWith({
            AccountId: 'external id',
            ReleaseWindowId: 'smth',
            UserName: 'user name',
            Password: 'password',
            Comment: 'super'
        });
        expect(sut.state.isBusy).toEqual(false);
        expect(rejected).toBeFalsy();
        expect(resolved).toBeTruthy();

        expect(mocks.common.sendEvent).toHaveBeenCalledWith('signOff.ReleaseSignedEvent', { AccountId: 'external id' });
    });

    it("should reject signing release", function () {
        sut.releaseWindow = { ExternalId: 'smth' };
        var deferred = $q.defer();
        var deferred2 = $q.defer(), resolved = false, rejected = false;
        mocks.remiapi.signOffRelease.and.returnValue(deferred.promise);
        deferred2.promise.then(function () { resolved = true; }, function () { rejected = true; });

        sut.sign({ deferred: deferred2, userName: 'user name', password: 'password', Comment: 'super' }, { Signer: { ExternalId: 'external id' } });
        deferred.reject('error');
        mocks.$scope.$digest();

        expect(mocks.remiapi.signOffRelease).toHaveBeenCalledWith({
            AccountId: 'external id',
            ReleaseWindowId: 'smth',
            UserName: 'user name',
            Password: 'password',
            Comment: 'super'
        });
        expect(sut.state.isBusy).toEqual(false);
        expect(logger.error).toHaveBeenCalledWith('Cannot sign off the release');
        expect(logger.console).toHaveBeenCalledWith('error');
        expect(rejected).toBeTruthy();
        expect(resolved).toBeFalsy();
    });

    it("should get signers", function () {
        sut.releaseWindow = { ExternalId: 'smth' };
        var deferred = $q.defer();
        mocks.remiapi.getSignOffs.and.returnValue(deferred.promise);
        mocks.authService = {
            identity: { email: 'first' }
        }
        spyOn(sut, 'allowSign');
        var signers = {
            SignOffs: [
                { Signer: { Email: 'first' } },
                { Signer: { Email: 'second' } }
            ]
        };

        sut.init(true);
        sut.getSigners();
        deferred.resolve(signers);
        mocks.$scope.$digest();

        expect(mocks.common.sendEvent).toHaveBeenCalledWith('releaseProcess.isParticipantEvent');
        expect(sut.allowSign).toHaveBeenCalledWith({ Signer: { Email: 'first' }, AllowSign: undefined });
        expect(sut.allowSign).toHaveBeenCalledWith({ Signer: { Email: 'second' }, AllowSign: undefined });
        expect(mocks.remiapi.getSignOffs).toHaveBeenCalledWith('smth');
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.signers.length).toEqual(2);
        expect(sut.signers[0].Signer).toEqual({ Email: 'first' });
        expect(sut.signers[1].Signer).toEqual({ Email: 'second' });
    });

    it("should reject getting signers", function () {
        sut.releaseWindow = { ExternalId: 'smth' };
        var deferred = $q.defer();
        mocks.remiapi.getSignOffs.and.returnValue(deferred.promise);

        sut.getSigners();
        deferred.reject('error');
        mocks.$scope.$digest();

        expect(mocks.remiapi.getSignOffs).toHaveBeenCalledWith('smth');
        expect(sut.state.isBusy).toEqual(false);
        expect(logger.error).toHaveBeenCalledWith('Cannot get sign offs');
        expect(logger.console).toHaveBeenCalledWith('error');
    });

    it("should add signers", function () {
        sut.releaseWindow = { ExternalId: 'smth' };
        var deferred = $q.defer();
        mocks.remiapi.addSignOffs.and.returnValue(deferred.promise);
        var accounts = ['first', 'second'];

        sut.addSigners(accounts);
        deferred.resolve();
        mocks.$scope.$digest();

        expect(mocks.remiapi.addSignOffs).toHaveBeenCalled();
        expect(sut.state.isBusy).toEqual(false);
        expect(mocks.common.sendEvent).toHaveBeenCalledWith('signOff.SignerAddedEvent', { SignOff: { Signer: 'first', ExternalId : jasmine.any(String) } });
        expect(mocks.common.sendEvent).toHaveBeenCalledWith('signOff.SignerAddedEvent', { SignOff: { Signer: 'second', ExternalId: jasmine.any(String) } });
    });

    it("should reject adding signers", function () {
        sut.releaseWindow = { ExternalId: 'smth' };
        var deferred = $q.defer();
        mocks.remiapi.addSignOffs.and.returnValue(deferred.promise);
        var accounts = ['first', 'second'];

        sut.addSigners(accounts);
        deferred.reject('error');
        mocks.$scope.$digest();

        expect(mocks.remiapi.addSignOffs).toHaveBeenCalled();
        expect(sut.state.isBusy).toEqual(false);
        expect(logger.error).toHaveBeenCalledWith('Cannot add release sign off');
        expect(logger.console).toHaveBeenCalledWith('error');
        expect(mocks.common.sendEvent).not.toHaveBeenCalled();
        expect(mocks.common.sendEvent).not.toHaveBeenCalled();
    });

    it("should remove signer", function () {
        sut.releaseWindow = { ExternalId: 'smth' };
        sut.signers = [{ ExternalId: '1', Signer: { ExternalId: 'acc1' } }, { ExternalId: '2', Signer: { ExternalId: 'acc2' } }];
        var deferred = $q.defer();
        mocks.remiapi.removeSignOff.and.returnValue(deferred.promise);
        var signer = { ExternalId: '1', Signer: { ExternalId: 'acc1' } };

        sut.removeSigner(signer);
        deferred.resolve();
        mocks.$scope.$digest();

        expect(mocks.remiapi.removeSignOff).
            toHaveBeenCalledWith({
                'AccountId': 'acc1',
                'SignOffId': '1',
                'ReleaseWindowId': 'smth'
            });
        expect(sut.state.isBusy).toEqual(false);
        expect(mocks.common.sendEvent).toHaveBeenCalledWith('signOff.SignerRemovedEvent', {SignOff: { ExternalId: '1', Signer: { ExternalId: 'acc1' } }});
    });

    it("should reject removing signer", function () {
        sut.releaseWindow = { ExternalId: 'smth' };
        var deferred = $q.defer();
        mocks.remiapi.removeSignOff.and.returnValue(deferred.promise);
        sut.signers = [{ ExternalId: '1' }, { ExternalId: '2' }];
        var signer = { ExternalId: '1', Signer: { ExternalId: 'accountid' } };

        sut.removeSigner(signer);
        deferred.reject('error');
        mocks.$scope.$digest();

        expect(mocks.remiapi.removeSignOff).
            toHaveBeenCalledWith({
                'AccountId': 'accountid',
                'SignOffId': '1',
                'ReleaseWindowId': 'smth'
            });
        expect(sut.state.isBusy).toEqual(false);
        expect(logger.error).toHaveBeenCalledWith('Cannot remove release sign off');
        expect(logger.console).toHaveBeenCalledWith('error');
    });

    it("should not remove last signer", function () {
        sut.releaseWindow = { ExternalId: 'smth' };
        sut.signers = [{ ExternalId: '1' }];
        var signer = { ExternalId: '1', Signer: { ExternalId: 'accountid' } };

        sut.removeSigner(signer);

        expect(logger.warn).toHaveBeenCalledWith('Release must have at least one user to sign off');
    });

    it("should handle status changed server event", function () {
        sut.releaseWindow = { ExternalId: 'smth', Status: 'none' };
        sut.signers = [{ ExternalId: '1' }, { ExternalId: '2' }];
        var n = {
            name: 'ReleaseStatusChangedEvent',
            data: {
                ReleaseStatus: 'changedstatus'
            }
        };
        spyOn(sut, 'allowSign');
        spyOn(sut, 'updateAllowManageSignOff');
        spyOn(sut, 'getSigners');

        sut.serverNotificationHandler(n);

        expect(sut.allowSign).toHaveBeenCalledWith({ ExternalId: '1', AllowSign: undefined });
        expect(sut.allowSign).toHaveBeenCalledWith({ ExternalId: '2', AllowSign: undefined });
        expect(sut.getSigners).not.toHaveBeenCalled();
        expect(sut.updateAllowManageSignOff).toHaveBeenCalled();
        expect(sut.releaseWindow.Status).toEqual('changedstatus');
    });

    it("should refresh sign off list, when release status changes to Closed", function () {
        sut.releaseWindow = { ExternalId: 'smth', Status: 'none' };
        sut.signers = [{ ExternalId: '1' }, { ExternalId: '2' }];
        var n = {
            name: 'ReleaseStatusChangedEvent',
            data: {
                ReleaseStatus: 'Closed'
            }
        };
        spyOn(sut, 'allowSign');
        spyOn(sut, 'updateAllowManageSignOff');
        spyOn(sut, 'getSigners');

        sut.serverNotificationHandler(n);

        expect(sut.allowSign).toHaveBeenCalledWith({ ExternalId: '1', AllowSign: undefined });
        expect(sut.allowSign).toHaveBeenCalledWith({ ExternalId: '2', AllowSign: undefined });
        expect(sut.getSigners).toHaveBeenCalledWith(sut.releaseWindow.ExternalId);
        expect(sut.updateAllowManageSignOff).toHaveBeenCalled();
    });

    it("should handle signers added server event", function () {
        sut.releaseWindow = { ExternalId: 'smth', Status: 'none' };
        sut.signers = [{ ExternalId: '1', Signer: { ExternalId: 'acc1' } }];
        var signers = {
            SignOffs: [
                { ExternalId: '1', Signer: { ExternalId: 'acc1' } },
                { ExternalId: '2', Signer: { ExternalId: 'acc2' } }
            ]
        };
        var n = {
            name: 'ReleaseSignersAddedEvent',
            data: signers
        };
        spyOn(sut, 'allowSign');

        sut.serverNotificationHandler(n);

        expect(sut.allowSign).toHaveBeenCalledWith({ ExternalId: '2', Signer: { ExternalId: 'acc2' }, AllowSign: undefined });
        expect(sut.signers.length).toEqual(2);
        expect(sut.signers[0].ExternalId).toEqual('1');
        expect(sut.signers[1].ExternalId).toEqual('2');
    });

    it("should handle signed off server event", function () {
        spyOn(sut, 'releaseSignedOffEventHandler');
        sut.releaseWindow = { ExternalId: 'smth', Status: 'none' };
        sut.signers = [{ Signer: { ExternalId: '1' } }, { Signer: { ExternalId: '2' } }];
        var n = {
            name: 'ReleaseSignedOffBySignerEvent',
            data: {
                AccountId: '1'
            }
        };

        sut.serverNotificationHandler(n);

        expect(sut.releaseSignedOffEventHandler).toHaveBeenCalled();
    });

    it("should update signer list when signed off handler invoked", function () {
        sut.releaseWindow = { ExternalId: 'smth', Status: 'none' };
        sut.signers = [{ Signer: { ExternalId: '1' } }, { Signer: { ExternalId: '2' } }];
        var data = {
            AccountId: '1'
        };

        sut.releaseSignedOffEventHandler(data);

        expect(sut.signers.length).toEqual(2);
        expect(sut.signers[0].SignedOff).toEqual(true);
    });

    it("should handle remove signer server event", function () {
        sut.releaseWindow = { ExternalId: 'smth', Status: 'none' };
        sut.signers = [{ ExternalId: '1', Signer: { ExternalId: 'acc1' } }, { ExternalId: '2', Signer: { ExternalId: 'acc2' } }];
        var n = {
            name: 'RemoveSignOffEvent',
            data: {
                SignOffId: '1',
                AccountId: 'acc1'
            }
        };

        sut.serverNotificationHandler(n);

        expect(sut.signers.length).toEqual(1);
        expect(sut.signers[0].ExternalId).toEqual('2');
    });

    it("should allow show sign off signature form, when AllowCloseAfterSignOffRule returns false", function () {
        sut.releaseWindow = { ExternalId: 'smth', Status: 'none' };
        var remiapiDefer = $q.defer();
        var defer = $q.defer();
        mocks.remiapi.executeBusinessRule.and.returnValue(remiapiDefer.promise);
        var resolved = false, rejected = false;
        defer.promise.then(function () { resolved = true; }, function () { rejected = true; });

        sut.beforeShowSignatureForm(defer);
        remiapiDefer.resolve({ Result: false });
        mocks.$scope.$digest();

        expect(rejected).toBeFalsy();
        expect(resolved).toBeTruthy();
    });

    it("should allow show sign off signature form, when AllowCloseAfterSignOffRule fails", function () {
        sut.releaseWindow = { ExternalId: 'smth', Status: 'none' };
        var remiapiDefer = $q.defer();
        var defer = $q.defer();
        mocks.remiapi.executeBusinessRule.and.returnValue(remiapiDefer.promise);
        var resolved = false, rejected = false;
        defer.promise.then(function () { resolved = true; }, function () { rejected = true; });

        sut.beforeShowSignatureForm(defer);
        remiapiDefer.reject();
        mocks.$scope.$digest();

        expect(rejected).toBeFalsy();
        expect(resolved).toBeTruthy();
    });

    it("should reject showing sign off signature form and send close release event, when AllowCloseAfterSignOffRule returns true", function () {
        sut.releaseWindow = { ExternalId: 'smth', Status: 'none' };
        var remiapiDefer = $q.defer();
        var defer = $q.defer();
        mocks.remiapi.executeBusinessRule.and.returnValue(remiapiDefer.promise);
        var resolved = false, rejected = false;
        defer.promise.then(function () { resolved = true; }, function () { rejected = true; });

        sut.beforeShowSignatureForm(defer);
        remiapiDefer.resolve({ Result: true });
        mocks.$scope.$digest();

        expect(rejected).toBeTruthy();
        expect(resolved).toBeFalsy();
        expect(mocks.common.sendEvent).toHaveBeenCalledWith(mocks.config.events.closeReleaseOnSignOffEvent);
    });
});

