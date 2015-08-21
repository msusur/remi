describe("Release Participants Controller", function () {
    var sut, mocks, logger;
    var activateDeferred, addParticipantsDeferred,
        removeParticipantDeferred, getParticipantsDeferred, confirmDeferred;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope) {
        activateDeferred = $q.defer();
        mocks = {
            $scope: $rootScope.$new(),
            $rootScope: $rootScope,
            common: {
                logger: window.jasmine.createSpyObj('logger', ['getLogger']),
                activateController: window.jasmine.createSpy('activateController'),
                handleEvent: window.jasmine.createSpy('handleEvent'),
                sendEvent: window.jasmine.createSpy('sendEvent'),
                $q: window.jasmine.createSpyObj('$q', ['all', 'when', 'defer']),
            },
            config: { events: { notificationReceived: 'notifications.received' } },
            remiapi: window.jasmine.createSpyObj('remiapi', ['searchUsers', 'releaseParticipants', 'executeCommand']),
            authService: window.jasmine.createSpyObj('authService', ['identity', 'state']),
            notifications: window.jasmine.createSpyObj('notifications', ['subscribe', 'unsubscribe']),
        };
        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info']);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.$scope.registerWidget = function (a, b) {
            return true;
        };
        mocks.common.activateController.and.returnValue(activateDeferred.promise);
        spyOn(mocks.$scope, 'registerWidget');

        inject(function ($controller) {
            sut = $controller('releaseParticipant', mocks);
        });
    }));

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('releaseParticipant');
        expect(mocks.common.activateController).toHaveBeenCalledWith(jasmine.any(Array), 'releaseParticipant', mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('notifications.received', jasmine.any(Function), mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('release.ReleaseWindowLoadedEvent', jasmine.any(Function), mocks.$scope);
    });

    it("should add release participants", function () {
        inject(function ($q) {
            addParticipantsDeferred = $q.defer();
        });
        mocks.remiapi.executeCommand.and.returnValue(addParticipantsDeferred.promise);
        inject(function ($controller) {
            sut = $controller('releaseParticipant', mocks);
        });
        sut.releaseParticipants = [];
        sut.releaseWindowId = 'window id';
        sut.state.isBusy = false;
        mocks.authService.identity.role = 'Basic user';
        mocks.authService.identity.email = 'b@b.com';

        sut.addReleaseParticipants([
            { Email: 'a@a.com', ExternalId: '1', Role: { Name: 'Admin' } },
            { Email: 'b@b.com', ExternalId: '2', Role: { Name: 'TeamMember' } },
            { Email: 'c@c.com', ExternalId: '3', Role: { Name: 'BasicUser' } }
        ]);
        addParticipantsDeferred.resolve();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
    });

    it("should not call api when no accounts were chosen to add to participants", function () {
        sut.state.isBusy = false;

        sut.addReleaseParticipants();

        expect(sut.state.isBusy).toEqual(false);
        expect(mocks.remiapi.executeCommand).not.toHaveBeenCalled();
    });

    it("should remove participant", function () {
        inject(function ($q) {
            removeParticipantDeferred = $q.defer();
        });
        mocks.remiapi.executeCommand.and.returnValue(removeParticipantDeferred.promise);
        inject(function ($controller) {
            sut = $controller('releaseParticipant', mocks);
        });
        sut.releaseParticipants = [
            { Account: { Email: 'a@a.com', ExternalId: '1', Role: 'Executive manager', FullName: 'a' } },
            { Account: { Email: 'b@b.com', ExternalId: '2', Role: 'Team member', FullName: 'b' } },
            { Account: { Email: 'c@c.com', ExternalId: '3', Role: 'Team member', FullName: 'c' } }
        ];
        sut.releaseWindowId = 'window id';
        sut.state.isBusy = false;

        sut.removeReleaseParticipant({
            Account:
                { Email: 'a@a.com', ExternalId: '1', Role: 'Executive manager', FullName: 'a' }
        });
        removeParticipantDeferred.resolve();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
    });
     
    it("should get release participants", function () {
        inject(function ($q) {
            getParticipantsDeferred = $q.defer();
        });
        mocks.remiapi.releaseParticipants.and.returnValue(getParticipantsDeferred.promise);
        inject(function ($controller) {
            sut = $controller('releaseParticipant', mocks);
        });
        var participants = {
            Participants:
                [
                    { Account: { Role: 'Basic user', ExternalId: '1', Email: 'test@email.com' } },
                    { Account: { Role: 'Team member', ExternalId: '2' } }
                ]
        };
        var releaseWindow = 'window id';
        sut.releaseParticipants = [];

        sut.getReleaseParticipants(releaseWindow);
        getParticipantsDeferred.resolve(participants);
        mocks.$scope.$digest();

        expect(sut.releaseParticipants.length).toEqual(2);
        expect(sut.releaseParticipants[0].manualConfirmationAllowed).toEqual(false);
        expect(sut.releaseParticipants[1].manualConfirmationAllowed).toEqual(true);
    });

    it("should reject getting release participants", function () {
        inject(function ($q) {
            getParticipantsDeferred = $q.defer();
        });
        mocks.authService.identity.role = 'Admin';
        mocks.remiapi.releaseParticipants.and.returnValue(getParticipantsDeferred.promise);
        inject(function ($controller) {
            sut = $controller('releaseParticipant', mocks);
        });

        sut.getReleaseParticipants('atata');
        getParticipantsDeferred.reject('some error');
        mocks.$scope.$digest();

        expect(logger.error).toHaveBeenCalledWith('Cannot get release participants');
        expect(logger.console).toHaveBeenCalledWith('some error');
    });

    it("should confirm participation", function () {
        inject(function ($q) {
            confirmDeferred = $q.defer();
        });
        mocks.remiapi.executeCommand.and.returnValue(confirmDeferred.promise);
        inject(function ($controller) {
            sut = $controller('releaseParticipant', mocks);
        });

        sut.confirmParticipation({ ReleaseParticipantId: 'id' });
        confirmDeferred.resolve();
        mocks.$scope.$digest();

        expect(logger.info).toHaveBeenCalledWith('Your participation was successfully confirmed');
    });

    it("should reject confirmation", function () {
        inject(function ($q) {
            confirmDeferred = $q.defer();
        });
        mocks.remiapi.executeCommand.and.returnValue(confirmDeferred.promise);
        inject(function ($controller) {
            sut = $controller('releaseParticipant', mocks);
        });

        sut.confirmParticipation({ ReleaseParticipantId: 'id' });
        confirmDeferred.reject('some error');
        mocks.$scope.$digest();

        expect(logger.error).toHaveBeenCalledWith('Your participation was not confirmed');
        expect(logger.console).toHaveBeenCalledWith('some error');
    });

    it("should handle release window loaded event", function () {
        spyOn(sut, 'getReleaseParticipants');

        sut.releaseWindowLoadedEventHandler({ ExternalId: 'external id', Product: 'super product' });

        expect(logger.console).toHaveBeenCalledWith('Binded to release window external id');
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ReleaseParticipationConfirmedEvent', { 'ReleaseWindowId': 'external id' });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ReleaseParticipantsAddedEvent', { 'ReleaseWindowId': 'external id' });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('ReleaseParticipantRemovedEvent', { 'ReleaseWindowId': 'external id' });
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.product).toEqual('super product');
        expect(sut.releaseWindowId).toEqual('external id');
        expect(sut.state.bindedToReleaseWindow).toEqual(true);
        expect(sut.getReleaseParticipants).toHaveBeenCalledWith('external id');
    });

    it("should not handle release window loaded event with empty release window", function () {
        sut.releaseWindowLoadedEventHandler();

        expect(logger.console).toHaveBeenCalledWith('Unbind release window');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseParticipationConfirmedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseParticipantRemovedEvent');
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('ReleaseParticipantsAddedEvent');
        expect(sut.releaseWindowId).toEqual('');
        expect(sut.state.bindedToReleaseWindow).toEqual(false);
    });

    it("should handle participation confirmed event", function () {
        mocks.$scope = window.jasmine.createSpyObj('$scope', ['$apply', '$on']);

        sut.releaseParticipants = [
            { ReleaseParticipantId: 'abc', Account: { Email: 'a@a.com', ExternalId: '1', Role: 'Executive manager', FullName: 'a' } },
        ];
        sut.serverNotificationHandler({ name: 'ReleaseParticipationConfirmedEvent', data: { ReleaseParticipantId: 'abc' } });

        expect(sut.releaseParticipants[0].IsParticipationConfirmed).toBe(true);
    });

    it("should handle participant added event", function () {
        spyOn(sut, 'releaseParticipantAddedEventHandler');

        sut.serverNotificationHandler({ name: 'ReleaseParticipantsAddedEvent', data: 'x' });

        expect(sut.releaseParticipantAddedEventHandler).toHaveBeenCalledWith('x');
    });

    it("should handle participant removed event", function () {
        spyOn(sut, 'releaseParticipantRemovedEventHandler');

        sut.serverNotificationHandler({ name: 'ReleaseParticipantRemovedEvent', data: 'x' });

        expect(sut.releaseParticipantRemovedEventHandler).toHaveBeenCalledWith('x');
    });

    it("should add participants on participant added event", function () {
        sut.releaseParticipants = [];
        spyOn(sut, 'isAllowManualConfirm');

        sut.releaseParticipantAddedEventHandler({
            Participants: [
                { Account: { ExternalId:'acc1', Role: { Name: 'a' } }, ReleaseParticipantId: '1', ReleaseWindowId: 'aaa1' },
                { Account: { ExternalId: 'acc2', Role: { Name: 'BasicUser' } }, ReleaseParticipantId: '2', ReleaseWindowId: 'aaa1' }
            ]
        });

        expect(sut.releaseParticipants.length).toEqual(2);
        expect(sut.isAllowManualConfirm).toHaveBeenCalled();
        expect(sut.releaseParticipants[0].ReleaseParticipantId).toEqual('1');
        expect(sut.releaseParticipants[1].ReleaseParticipantId).toEqual('2');
        expect(sut.releaseParticipants[1].Account.Role.Name).toEqual('TeamMember');
    });

    it("should remove participant on participant removed event", function () {
        sut.releaseParticipants = [{ ReleaseParticipantId: '1', Account: { ExternalId: 'acc1' } }, { ReleaseParticipantId: '2', Account: { ExternalId: 'acc2' } }];

        sut.releaseParticipantRemovedEventHandler({
            Participant: {
                Account: {
                    Role: { Name: 'a', ExternalId: 'acc1' }
                },
                ReleaseParticipantId: '1',
                ReleaseWindowId: 'aaa1'
            }
        });

        expect(sut.releaseParticipants.length).toEqual(1);
        expect(sut.releaseParticipants[0].ReleaseParticipantId).toEqual('2');
    });
});

