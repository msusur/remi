(function () {
    "use strict";
    var controllerId = "releaseChanges";
    angular.module("app").controller(controllerId, ["remiapi", "common", "$rootScope", "$scope", releaseChanges]);

    function releaseChanges(remiapi, common, $rootScope, $scope) {
        var logger = common.logger.getLogger(controllerId);
        var self = this;

        self.parent = common.getParentScope($scope, function (sc) {
            return sc && sc.vm && sc.vm.controllerId === "release";
        }).vm;
        self.state = {
            isBusy: false,
            bindedToReleaseWindow: false,
            visible: true
        };
        self.currentPage = 0;
        self.totalItemsCount = 0;
        self.numPages = 0;
        self.maxSize = 5;
        self.itemsPerPage = 100;
        self.pageChanged = function () {
            self.releaseChanges = Enumerable.From(self.releaseChangesAll)
                .Skip((self.currentPage - 1) * self.itemsPerPage)
                .Take(self.itemsPerPage).ToArray();
        };

        self.releaseChanges = [];
        self.releaseChangesAll = [];
        self.canReFixChanges = false;

        self.getReleaseChanges = getReleaseChanges;
        self.releaseWindowLoadedEventHandler = releaseWindowLoadedEventHandler;
        self.openTickets = openTickets;
        self.reAssignToRelease = reAssignToRelease;
        self.updateReleaseRepository = updateReleaseRepository;
        self.repositoryChanged = repositoryChanged;
        self.index = index;
        self.hasRepositories = hasRepositories;
        self.reloadRepositories = reloadRepositories;

        common.handleEvent("release.ReleaseWindowLoadedEvent", self.releaseWindowLoadedEventHandler, $scope);

        activate();

        function activate() {
            common.activateController([releaseWindowLoadedEventHandler(self.parent.currentReleaseWindow)], controllerId, $scope);
        }

        function getReleaseChanges(releaseWindow) {
            self.releaseChanges = [];
            self.releaseChangesAll = [];
            // TODO can be taken from business rules
            if (releaseWindow.ReleaseType !== "Scheduled") {
                self.state.visible = false;
                return null;
            }

            self.state.visible = true;
            self.state.isBusy = true;
            return remiapi.releaseChanges(releaseWindow.ExternalId).then(
                function (event) {
                    self.releaseChangesAll = event.Changes;
                    self.repositories = event.Repositories;
                    self.totalItemsCount = self.releaseChangesAll.length;
                    self.numPages = Math.ceil(self.totalItemsCount / self.itemsPerPage);
                    self.currentPage = 1;
                    self.pageChanged();

                    common.sendEvent(controllerId + ".releaseChangesLoaded", self.releaseChanges);
                },
                function (response) {
                    console.log(response);
                    logger.console("error");
                    logger.error("Cannot load Release Changes");
                }).finally(function () {
                    self.state.isBusy = false;
                });
        }

        function releaseWindowLoadedEventHandler(releaseWindow) {
            if (releaseWindow) {
                self.releaseWindowId = releaseWindow.ExternalId;
                self.releaseWindow = releaseWindow;
                self.state.bindedToReleaseWindow = true;
                self.canReFixChanges = !!releaseWindow.ClosedOn;

                self.getReleaseChanges(releaseWindow);

                logger.console("Binded to release window " + releaseWindow.ExternalId);

            } else {
                self.releaseWindowId = "";
                self.state.bindedToReleaseWindow = false;

                self.releaseChanges = [];
                self.releaseChangesAll = [];

                logger.console("Unbind from release window");
            }
        }

        function openTickets(tickets) {
            if (!tickets) return;

            for (var i = 0; i < tickets.length; i++) {
                if (tickets[i].url)
                    window.open(tickets[i].url);
            }
        }

        function reAssignToRelease() {
            if (!self.releaseWindowId) return null;

            self.state.isBusy = true;

            return remiapi.post
                .reAssignReleaseChanges({ 'ReleaseWindowId': self.releaseWindowId })
                .finally(function () {
                    self.state.isBusy = false;
                });
        }

        function updateReleaseRepository(repository) {
            if (!self.releaseWindowId || !repository) return null;

            self.state.isBusy = true;
            delete repository.hasChanged;
            return remiapi.post
                .updateReleaseRepository({
                    "ReleaseWindowId": self.releaseWindowId,
                    "Repository": repository
                }).then(null, function () {
                    repository.hasChanged = true;
                }).finally(function () {
                    self.state.isBusy = false;
                });

        }

        function repositoryChanged(repository) {
            if (repository) repository.hasChanged = true;
        }

        function index($index) {
            return self.itemsPerPage * (self.currentPage - 1) + $index + 1;
        }
        function hasRepositories() {
            return !!self.repositories && self.repositories.length > 0;
        }

        function reloadRepositories() {
            if (!self.releaseWindowId) return null;

            self.state.isBusy = true;
            return remiapi.post
                .reloadRepositories({
                    "ReleaseWindowId": self.releaseWindowId
                })
                .then(function () {
                    self.getReleaseChanges(self.releaseWindow);
                }, function () {
                    self.state.isBusy = false;
                });
        }
    }
})()
