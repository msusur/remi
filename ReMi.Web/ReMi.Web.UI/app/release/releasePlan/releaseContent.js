(function () {
    'use strict';
    var controllerId = 'releaseContent';
    angular.module('app').controller(controllerId, ['remiapi', 'common', '$rootScope', '$scope', 'config', 'notifications', releaseContent]);

    function releaseContent(remiapi, common, $rootScope, $scope, config, notifications) {
        var logger = common.logger.getLogger(controllerId);
        var self = this;

        self.state = {
            isBusy: false,
            bindedToReleaseWindow: false,
            visible: true
        };

        self.tickets = [];
        self.risk = [];
        self.allTickets = [];
        self.showTicketsButtonName = 'Show selected only';
        self.contentLoaded = false;

        self.getReleaseContent = getReleaseContent;
        self.getTicketRisk = getTicketRisk;
        self.releaseWindowLoadedEventHandler = releaseWindowLoadedEventHandler;
        self.serverNotificationHandler = serverNotificationHandler;
        self.showEditCommentModal = showEditCommentModal;
        self.closeEditCommentModal = closeEditCommentModal;
        self.mapRiskStyle = mapRiskStyle;
        self.updateCommentForTicket = updateCommentForTicket;
        self.updateRiskForTicket = updateRiskForTicket;
        self.updateTicket = updateTicket;
        self.manageTicketReleaseRelation = manageTicketReleaseRelation;
        self.manageAllTicketsReleaseRelation = manageAllTicketsReleaseRelation;
        self.updateDisplayedTicketList = updateDisplayedTicketList;
        self.reapproveReleaseContent = reapproveReleaseContent;

        common.handleEvent('release.ReleaseWindowLoadedEvent', self.releaseWindowLoadedEventHandler, $scope);
        common.handleEvent(config.events.notificationReceived, self.serverNotificationHandler, $scope);

        activate();

        $scope.$on('$destroy', scopeDestroyHandler);

        function scopeDestroyHandler() {
            notifications.unsubscribe('TicketChangedEvent');
        }

        function activate() {
            common.activateController([getTicketRisk()], controllerId, $scope);
        }

        function showEditCommentModal(item) {
            self.currentItem = item;
            $('#releaseContentCommentModal').modal({ backdrop: 'static', keyboard: true });
        };

        function closeEditCommentModal() {
            $('#releaseContentCommentModal').modal('hide');
            self.updateCommentForTicket(self.currentItem);
        };

        function mapRiskStyle(risk) {
            switch (risk) {
                case 'Low': return { css: 'btn-success', color: '#419641' };
                case 'Medium': return { css: 'btn-warning', color: '#eb9316' };
                case 'High': return { css: 'btn-danger', color: '#c12e2a' };
            }
            return { css: 'btn-success', color: '#419641' };
        };

        function updateCommentForTicket(ticket) {
            self.state.isBusy = true;
            self.updateTicket('UpdateTicketCommentCommand',
                {
                    TicketId: ticket.TicketId,
                    TicketKey: ticket.TicketName,
                    Comment: ticket.Comment,
                    IncludeToReleaseNotes: ticket.IncludeToReleaseNotes,
                    ReleaseWindowId: self.releaseWindowId
                });
        };

        function updateRiskForTicket(ticket, risk) {
            self.state.isBusy = true;
            ticket.Risk = risk.Text;
            self.updateTicket('UpdateTicketRiskCommand',
                {
                    TicketId: ticket.TicketId,
                    TicketKey: ticket.TicketName,
                    Risk: ticket.Risk,
                    IncludeToReleaseNotes: ticket.IncludeToReleaseNotes,
                    ReleaseWindowId: self.releaseWindowId
                });
        };

        function updateTicket(commandName, data) {

            var commandId = newGuid();

            remiapi.executeCommand(commandName, commandId, data)
                .then(
                    function () {
                    },
                    function (statusCode) {
                        if (statusCode == 406) {
                            logger.warn('Request is invalid');
                        } else {
                            logger.error(statusCode + ' Error occured');
                        }
                        self.getReleaseContent(self.releaseWindow);
                    }
                )
                .finally(function () { self.state.isBusy = false; });
        };

        function getReleaseContent(releaseWindow) {
            self.tickets = [];

            if (releaseWindow.ReleaseType != 'Scheduled' && releaseWindow.ReleaseType != 'Automated') {
                self.state.visible = false;
                return null;
            }
            self.state.visible = true;
            self.state.isBusy = true;
            return remiapi.releaseContentData(releaseWindow.ExternalId).then(
                function (event) {
                    self.tickets = event.Content;

                    common.sendEvent(controllerId + '.ticketsLoaded', self.tickets);
                },
                function (response) {
                    console.log(response);
                    logger.console('error');
                    logger.error('Cannot load Release Content');
                }).finally(function () {
                    self.state.isBusy = false;
                });
        }

        function getTicketRisk() {
            self.state.isBusy = true;
            return remiapi.ticketRisk().then(
                function (event) {
                    self.risk = event.TicketRisk;
                },
                function (response) {
                    logger.console(response);
                    logger.console('error');
                    logger.error('Cannot load Ticket Risk types');
                }).finally(function () {
                    self.state.isBusy = false;
                    self.contentLoaded = true;
                });
        }

        function releaseWindowLoadedEventHandler(releaseWindow) {
            if (releaseWindow) {
                self.releaseWindow = releaseWindow;
                self.releaseWindowId = releaseWindow.ExternalId;
                self.state.bindedToReleaseWindow = true;

                self.getReleaseContent(releaseWindow);

                logger.console('Binded to release window ' + releaseWindow.ExternalId);

                notifications.subscribe('TicketChangedEvent', { 'ReleaseWindowId': self.releaseWindowId });
            } else {
                self.releaseWindowId = '';
                self.state.bindedToReleaseWindow = false;

                self.allTickets = [];
                self.tickets = [];

                logger.console('Fail to bind to release window ');

                notifications.unsubscribe('TicketChangedEvent');
            }
        }

        function manageTicketReleaseRelation(ticket) {
            self.state.isBusy = true;
            remiapi.executeCommand('UpdateTicketToReleaseNotesRelationCommand', newGuid(),
                {
                    'Tickets': [ticket],
                    'ReleaseWindowId': self.releaseWindowId
                }).then(function () { }, function (error) {
                    logger.error('Cannot update ticket to release relation');
                    logger.console('Cannot update ticket to release relation');
                    logger.console(error);
                    for (var counter = 0; counter < self.tickets.length; counter++) {
                        if (self.tickets[counter].TicketId == ticket.TicketId) {
                            self.tickets[counter].IncludeToReleaseNotes = !self.tickets[counter].IncludeToReleaseNotes;
                            break;
                        }
                    }
                }).finally(function () { self.state.isBusy = false; });
        }

        function manageAllTicketsReleaseRelation(flag) {
            self.state.isBusy = true;
            var backUpTickets = self.tickets;
            if (flag) {
                for (var iCounter = 0; iCounter < self.tickets.length; iCounter++) {
                    self.tickets[iCounter].IncludeToReleaseNotes = true;
                }
            } else {
                for (var eCounter = 0; eCounter < self.tickets.length; eCounter++) {
                    self.tickets[eCounter].IncludeToReleaseNotes = false;
                }
            }

            remiapi.executeCommand('UpdateTicketToReleaseNotesRelationCommand', newGuid(),
            {
                'Tickets': self.tickets,
                'ReleaseWindowId': self.releaseWindowId
            }).then(function () { }, function (error) {
                self.tickets = backUpTickets;
                logger.error('Cannot update ticket to release relation');
                logger.console('Cannot update ticket to release relation');
                logger.console(error);
            }).finally(function () { self.state.isBusy = false; });
        }

        function updateDisplayedTicketList() {
            if (self.showTicketsButtonName == 'Show selected only') {
                self.allTickets = self.tickets;
                self.tickets = self.allTickets.filter(function (item) { return item.IncludeToReleaseNotes; });
                self.showTicketsButtonName = 'Show all';
            } else {
                for (var tCounter = 0; tCounter < self.tickets.length; tCounter++) {
                    for (var aCounter = 0; aCounter < self.tickets.length; aCounter++) {
                        if (self.allTickets[aCounter].TicketId == self.tickets[tCounter].TicketId) {
                            self.allTickets[aCounter] = self.tickets[tCounter];
                        }
                    }
                }
                self.tickets = self.allTickets;
                self.showTicketsButtonName = 'Show selected only';
            }
        }

        function serverNotificationHandler(notification) {
            if (notification.name === 'TicketChangedEvent') {
                ticketChangedEventHandler(notification.data);
            }
        }

        function ticketChangedEventHandler(data) {
            if (data.Tickets.length == 1) {
                logger.info(data.Tickets[0].TicketName + ' ticket information was changed');
            } else {
                logger.info('Tickets information was changed');
            }
            $scope.$apply(function () {
                for (var counter = 0; counter < data.Tickets.length; counter++) {
                    for (var tCounter = 0; tCounter < self.tickets.length; tCounter++) {
                        if (data.Tickets[counter].TicketId == self.tickets[tCounter].TicketId) {
                            self.tickets[tCounter].IncludeToReleaseNotes = data.Tickets[counter].IncludeToReleaseNotes;
                            self.tickets[tCounter].Risk = data.Tickets[counter].Risk;
                            self.tickets[tCounter].Comment = data.Tickets[counter].Comment;
                            break;
                        }
                    }
                }
            });
        }

        function reapproveReleaseContent() {
            if (self.releaseWindowId) {
                self.state.isBusy = true;
                remiapi.post.reapproveTickets({ ReleaseWindowId: self.releaseWindowId })
                    .then(null, function (response) {
                        logger.console(response);
                        logger.error('Cannot store jire tickets in database');
                        logger.console('Cannot store jire tickets in database');
                    })
                    .finally(function() {
                        self.state.isBusy = false;
                    });
            }
        }
    }
})()
