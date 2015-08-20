describe("ReleaseContent Controller", function () {
    var sut, mocks, deferred, logger, riskDeferred;

    beforeEach(function () {
        module("app");
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope) {
        deferred = $q.defer();
        riskDeferred = $q.defer();
        mocks = {
            $rootScope: $rootScope,
            $scope: $rootScope.$new(),
            common: {
                logger: window.jasmine.createSpyObj('logger', ['getLogger']),
                activateController: window.jasmine.createSpy('activateController'),
                handleEvent: window.jasmine.createSpy('handleEvent'),
                sendEvent: window.jasmine.createSpy('sendEvent')
            },
            remiapi: window.jasmine.createSpyObj('remiapi', ['releaseContentData', 'ticketRisk', 'executeCommand']),
            notifications: window.jasmine.createSpyObj('notifications', ['subscribe', 'unsubscribe']),
            $q: $q
        };
        mocks.remiapi.post = jasmine.createSpyObj('remiapi.post', ['reapproveTickets']);
        mocks.remiapi.ticketRisk.and.returnValue(riskDeferred.promise);
        mocks.remiapi.releaseContentData.and.returnValue(deferred.promise);
        mocks.remiapi.executeCommand.and.returnValue(deferred.promise);
        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info']);
        mocks.common.logger.getLogger.and.returnValue(logger);
        inject(function ($controller) {
            sut = $controller('releaseContent', mocks);
        });
    }));

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('releaseContent');
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('release.ReleaseWindowLoadedEvent', window.jasmine.any(Function), mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('notifications.received', window.jasmine.any(Function), mocks.$scope);
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), 'releaseContent', mocks.$scope);
    });

    it("should get release content, when getReleaseContent success", function () {
        sut.state = { isBusy: true };
        var releaseWindow = { ExternalId: '1', ReleaseType: 'Scheduled' };

        sut.getReleaseContent(releaseWindow);
        deferred.resolve({ Content: 'content' });
        mocks.$scope.$digest();

        expect(mocks.remiapi.releaseContentData).toHaveBeenCalledWith(releaseWindow.ExternalId);
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.tickets).toEqual('content');
        expect(mocks.common.sendEvent).toHaveBeenCalledWith('releaseContent.ticketsLoaded', 'content');
    });

    it("should not get release content, when release is not scheduled or automated", function () {
        sut.state = {
            isBusy: true,
            visible: true
        };
        var releaseWindow = { ExternalId: '1', ReleaseType: 'Hotfix' };

        sut.getReleaseContent(releaseWindow);

        expect(mocks.remiapi.releaseContentData.calls.any()).toEqual(false);
        expect(sut.state.visible).toEqual(false);
    });

    it("should log error response, when getReleaseContent fails", function () {
        sut.state = { isBusy: true };
        var releaseWindow = { ExternalId: '1', ReleaseType: 'Automated' };

        spyOn(console, 'log');

        sut.getReleaseContent(releaseWindow);
        deferred.reject('error response');
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(console.log).toHaveBeenCalledWith('error response');
        expect(logger.console).toHaveBeenCalledWith('error');
        expect(logger.error).toHaveBeenCalledWith('Cannot load Release Content');
    });


    it("should get ticket risk, when getReleaseContent success", function () {
        sut.state = { isBusy: true };

        sut.getTicketRisk();
        riskDeferred.resolve({ TicketRisk: 'TicketRisk' });
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(sut.risk).toEqual('TicketRisk');
    });

    it("should initialize controller, when gets release window id", function () {
        spyOn(sut, 'getReleaseContent');

        var releaseWindow = { ExternalId: 'external id' };
        sut.releaseWindowLoadedEventHandler(releaseWindow);

        expect(sut.releaseWindow).toEqual(releaseWindow);
        expect(sut.releaseWindowId).toEqual('external id');
        expect(sut.state.bindedToReleaseWindow).toEqual(true);
        expect(sut.getReleaseContent).toHaveBeenCalledWith(releaseWindow);
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith('TicketChangedEvent', { 'ReleaseWindowId': 'external id' });
        expect(sut.releaseWindowId).toEqual(releaseWindow.ExternalId);
    });

    it("should switch controller to unbind state, when gets empty release window", function () {
        spyOn(sut, 'getReleaseContent');

        sut.releaseWindowLoadedEventHandler();

        expect(sut.releaseWindowId).toEqual('');
        expect(sut.state.bindedToReleaseWindow).toEqual(false);
        expect(sut.getReleaseContent.calls.count()).toEqual(0);
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith('TicketChangedEvent');
    });

    it("should show comment modal, when invoke showEditCommentModal", function () {
        spyOn(window, '$').and.callThrough();
        spyOn($.fn, 'modal');

        var item = { Comment: 'comment' };
        sut.showEditCommentModal(item);

        expect(sut.currentItem).toEqual(item);
        expect(sut.currentItem.Comment).toEqual(item.Comment);
        expect(window.$).toHaveBeenCalledWith('#releaseContentCommentModal');
        expect($.fn.modal).toHaveBeenCalledWith(window.jasmine.objectContaining({ backdrop: 'static', keyboard: true }));
    });

    it("should close comment modal and update ticket, when invoke closeEditCommentModal", function () {
        spyOn(window, '$').and.callThrough();
        spyOn($.fn, 'modal');
        spyOn(sut, 'updateCommentForTicket');

        sut.currentItem = 'item';

        sut.closeEditCommentModal();

        expect(sut.updateCommentForTicket).toHaveBeenCalledWith('item');
        expect(window.$).toHaveBeenCalledWith('#releaseContentCommentModal');
        expect($.fn.modal).toHaveBeenCalledWith('hide');
    });

    it("should reutrn style, when call mapRiskStyle", function () {
        var result = sut.mapRiskStyle('Medium');

        expect(result.css).toEqual('btn-warning');
        expect(result.color).toEqual('#eb9316');
    });

    it("should call updateTicket with comment, when invoked", function () {
        spyOn(sut, 'updateTicket');

        var ticket = {
            TicketId: 'ticketId',
            TicketName: 'ticketName',
            Comment: 'some comment',
            Risk: 'Medium'
        };
        sut.updateCommentForTicket(ticket);

        expect(sut.updateTicket).toHaveBeenCalledWith("UpdateTicketCommentCommand", window.jasmine.objectContaining({
            TicketId: ticket.TicketId,
            TicketKey: ticket.TicketName,
            Comment: ticket.Comment
        }));
        expect(sut.updateTicket.calls.count()).toEqual(1);
        expect(sut.state.isBusy).toEqual(true);
    });

    it("should call updateTicket with risk, when invoked", function () {
        spyOn(sut, 'updateTicket');

        var ticket = {
            TicketId: 'ticketId',
            TicketName: 'ticketName',
            Comment: 'some comment',
            Risk: 'Medium',
            IncludeToReleaseNotes: true
        };
        var risk = {
            Text: 'High'
        };
        sut.updateRiskForTicket(ticket, risk);

        expect(sut.updateTicket).toHaveBeenCalledWith("UpdateTicketRiskCommand", window.jasmine.objectContaining({
            TicketId: ticket.TicketId,
            TicketKey: ticket.TicketName,
            Risk: risk.Text,
            IncludeToReleaseNotes: ticket.IncludeToReleaseNotes
        }));
        expect(sut.updateTicket.calls.count()).toEqual(1);
        expect(sut.state.isBusy).toEqual(true);
        expect(ticket.Risk).toEqual(risk.Text);
    });

    it("should send command and turn off busy state, when invoked", function () {
        var commandName = 'command name';
        var data = { data: 'data' };
        sut.state = { isBusy: true };

        spyOn(window, 'newGuid').and.returnValue('new guid');

        sut.updateTicket(commandName, data);
        deferred.resolve({ TicketRisk: 'TicketRisk' });
        mocks.$scope.$digest();

        expect(mocks.remiapi.executeCommand).toHaveBeenCalledWith(commandName, 'new guid', data);
        expect(mocks.remiapi.executeCommand.calls.count()).toEqual(1);
        expect(sut.state.isBusy).toEqual(false);
    });

    it("should refresh tickets, when updateTicket failed", function () {
        var commandName = 'command name';
        var data = { data: 'data' };
        var releaseWindow = { ExternalId: 'external id' };
        sut.state = { isBusy: true };
        sut.releaseWindow = releaseWindow;

        spyOn(window, 'newGuid').and.returnValue('new guid');
        spyOn(sut, 'getReleaseContent');

        sut.updateTicket(commandName, data);
        deferred.reject('error message');
        mocks.$scope.$digest();

        expect(mocks.remiapi.executeCommand).toHaveBeenCalledWith(commandName, 'new guid', data);
        expect(mocks.remiapi.executeCommand.calls.count()).toEqual(1);
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.getReleaseContent).toHaveBeenCalledWith(releaseWindow);
        expect(logger.error).toHaveBeenCalledWith('error message Error occured');
    });

    it("should manage ticket release relation", function () {
        var commandName = 'UpdateTicketToReleaseNotesRelationCommand';
        sut.tickets = [{ TicketId: 23, IncludeToReleaseNotes: true, Risk: 'risk' }];
        sut.releaseWindowId = 'some id';
        var ticket = { TicketId: 23, IncludeToReleaseNotes: true };
        sut.state = { isBusy: false };

        spyOn(window, 'newGuid').and.returnValue('new guid');

        sut.manageTicketReleaseRelation(ticket);
        deferred.resolve();
        mocks.$scope.$digest();

        expect(mocks.remiapi.executeCommand)
            .toHaveBeenCalledWith(commandName, 'new guid',
            { 'Tickets': [ticket], 'ReleaseWindowId': sut.releaseWindowId });
        expect(mocks.remiapi.executeCommand.calls.count()).toEqual(1);
        expect(sut.tickets[0].IncludeToReleaseNotes).toEqual(true);
        expect(sut.state.isBusy).toEqual(false);
    });

    it("should reject manage ticket release relation", function () {
        var commandName = 'UpdateTicketToReleaseNotesRelationCommand';
        sut.tickets = [{ TicketId: 23, IncludeToReleaseNotes: true, Risk: 'risk' }];
        sut.releaseWindowId = 'some id';
        var ticket = { TicketId: 23, IncludeToReleaseNotes: true };
        sut.state = { isBusy: false };

        spyOn(window, 'newGuid').and.returnValue('new guid');

        sut.manageTicketReleaseRelation(ticket);
        deferred.reject('some error');
        mocks.$scope.$digest();

        expect(mocks.remiapi.executeCommand)
            .toHaveBeenCalledWith(commandName, 'new guid',
            { 'Tickets': [ticket], 'ReleaseWindowId': sut.releaseWindowId });
        expect(logger.console).toHaveBeenCalledWith('Cannot update ticket to release relation');
        expect(logger.console).toHaveBeenCalledWith('some error');
        expect(logger.error).toHaveBeenCalledWith('Cannot update ticket to release relation');
        expect(sut.tickets[0].IncludeToReleaseNotes).toEqual(false);
        expect(sut.state.isBusy).toEqual(false);
    });

    it("should manage all tickets release relation", function () {
        var commandName = 'UpdateTicketToReleaseNotesRelationCommand';
        sut.tickets = [
            { TicketId: 23, IncludeToReleaseNotes: true, Risk: 'risk' },
            { TicketId: 24, IncludeToReleaseNotes: true, Risk: 'risk' },
            { TicketId: 25, IncludeToReleaseNotes: true, Risk: 'risk' }
        ];

        sut.releaseWindowId = 'some id';
        sut.state = { isBusy: false };

        spyOn(window, 'newGuid').and.returnValue('new guid');

        sut.manageAllTicketsReleaseRelation(false);
        deferred.resolve();
        mocks.$scope.$digest();

        expect(mocks.remiapi.executeCommand)
            .toHaveBeenCalledWith(commandName, 'new guid',
            { 'Tickets': sut.tickets, 'ReleaseWindowId': sut.releaseWindowId });
        expect(sut.tickets[0].IncludeToReleaseNotes).toEqual(false);
        expect(sut.tickets[1].IncludeToReleaseNotes).toEqual(false);
        expect(sut.tickets[2].IncludeToReleaseNotes).toEqual(false);
        expect(sut.state.isBusy).toEqual(false);
    });


    it("should reject manage all tickets release relation", function () {
        var commandName = 'UpdateTicketToReleaseNotesRelationCommand';
        sut.tickets = [
            { TicketId: 23, IncludeToReleaseNotes: true, Risk: 'risk' },
            { TicketId: 24, IncludeToReleaseNotes: false, Risk: 'risk' }
        ];

        sut.releaseWindowId = 'some id';
        sut.state = { isBusy: false };

        spyOn(window, 'newGuid').and.returnValue('new guid');

        sut.manageAllTicketsReleaseRelation(true);
        deferred.reject('some error');
        mocks.$scope.$digest();

        expect(mocks.remiapi.executeCommand)
            .toHaveBeenCalledWith(commandName, 'new guid',
            { 'Tickets': sut.tickets, 'ReleaseWindowId': sut.releaseWindowId });
        expect(logger.console).toHaveBeenCalledWith('Cannot update ticket to release relation');
        expect(logger.console).toHaveBeenCalledWith('some error');
        expect(logger.error).toHaveBeenCalledWith('Cannot update ticket to release relation');
        expect(sut.state.isBusy).toEqual(false);
    });

    it("specify to display selected only tickets", function () {
        sut.showTicketsButtonName = 'Show selected only';
        sut.tickets = [
           { TicketId: 23, IncludeToReleaseNotes: true, Risk: 'risk' },
           { TicketId: 24, IncludeToReleaseNotes: false, Risk: 'risk' }
        ];

        sut.updateDisplayedTicketList();

        expect(sut.tickets.length).toEqual(1);
        expect(sut.tickets[0].TicketId).toEqual(23);
        expect(sut.showTicketsButtonName).toEqual('Show all');
    });

    it("specify to display all tickets", function () {
        sut.showTicketsButtonName = 'Show all';
        sut.allTickets = [
           { TicketId: 23, IncludeToReleaseNotes: true, Risk: 'risk' },
           { TicketId: 24, IncludeToReleaseNotes: false, Risk: 'risk' }
        ];
        sut.tickets = [
            { TicketId: 23, IncludeToReleaseNotes: true, Risk: 'changed risk' }
        ];

        sut.updateDisplayedTicketList();

        expect(sut.tickets.length).toEqual(2);
        expect(sut.tickets[0].TicketId).toEqual(23);
        expect(sut.tickets[0].Risk).toEqual('changed risk');
        expect(sut.showTicketsButtonName).toEqual('Show selected only');
    });

    it("handle jita ticket changed notification when all tickets were changed", function () {

        mocks.$scope = window.jasmine.createSpyObj('$scope', ['$apply', '$on']);
        inject(function ($controller) {
            sut = $controller('releaseContent', mocks);
        });
        var data = {
            Tickets: [
                { TicketId: 23, IncludeToReleaseNotes: true, Risk: 'risk' },
                { TicketId: 24, IncludeToReleaseNotes: true, Risk: 'risk' }
            ]
        };

        sut.serverNotificationHandler({ name: 'TicketChangedEvent', data: data });

        expect(logger.info).toHaveBeenCalledWith('Tickets information was changed');
        expect(mocks.$scope.$apply).toHaveBeenCalled();
    });

    it("handle jita ticket changed notification when one ticket was changed", function () {

        mocks.$scope = window.jasmine.createSpyObj('$scope', ['$apply', '$on']);
        inject(function ($controller) {
            sut = $controller('releaseContent', mocks);
        });
        var data = {
            Tickets: [
                { TicketId: 23, IncludeToReleaseNotes: true, Risk: 'risk', TicketName: 'some name' }
            ]
        };

        sut.serverNotificationHandler({ name: 'TicketChangedEvent', data: data });

        expect(logger.info).toHaveBeenCalledWith('some name ticket information was changed');
        expect(mocks.$scope.$apply).toHaveBeenCalled();
    });

    describe('reapproveReleaseContent', function () {
        it("should do nothing, when method was called and release window id is empty", function () {
            sut.reapproveReleaseContent();

            expect(mocks.remiapi.post.reapproveTickets).not.toHaveBeenCalled();
        });

        it("should do reapprove command and set state to not busy, when command executed successfully", function () {
            deferred = mocks.$q.defer();
            mocks.remiapi.post.reapproveTickets.and.returnValue(deferred.promise);
            sut.releaseWindowId = newGuid();

            sut.reapproveReleaseContent();
            deferred.resolve();
            mocks.$scope.$digest();

            expect(mocks.remiapi.post.reapproveTickets).toHaveBeenCalledWith({ ReleaseWindowId: sut.releaseWindowId });
            expect(logger.error).not.toHaveBeenCalled();
            expect(sut.state.isBusy).toBeFalsy();
        });

        it("should log error message, when command execution failed", function () {
            deferred = mocks.$q.defer();
            mocks.remiapi.post.reapproveTickets.and.returnValue(deferred.promise);
            sut.releaseWindowId = newGuid();

            sut.reapproveReleaseContent();
            deferred.reject('error message');
            mocks.$scope.$digest();

            expect(mocks.remiapi.post.reapproveTickets).toHaveBeenCalledWith({ ReleaseWindowId: sut.releaseWindowId });
            expect(logger.console).toHaveBeenCalledWith('error message');
            expect(logger.console).toHaveBeenCalledWith('Cannot store jire tickets in database');
            expect(logger.error).toHaveBeenCalledWith('Cannot store jire tickets in database');
            expect(sut.state.isBusy).toBeFalsy();
        });
    });
});
