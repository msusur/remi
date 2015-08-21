describe("releaseChanges Controller", function () {
    var sut, mocks, deferred, logger;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope) {
        deferred = $q.defer();
        mocks = {
            $rootScope: $rootScope,
            $scope: $rootScope.$new(),
            common: {
                logger: window.jasmine.createSpyObj("logger", ["getLogger"]),
                activateController: window.jasmine.createSpy("activateController"),
                handleEvent: window.jasmine.createSpy("handleEvent"),
                sendEvent: window.jasmine.createSpy("sendEvent"),
                getParentScope: window.jasmine.createSpy("getParentScope").and.returnValue({ vm: {} })
            },
            remiapi: window.jasmine.createSpyObj("remiapi", ["releaseChanges", "post"])
        };
        mocks.remiapi.releaseChanges.and.returnValue(deferred.promise);
        logger = window.jasmine.createSpyObj("logger", ["console", "error"]);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.remiapi.post = window.jasmine.createSpyObj("post", [
            "reAssignReleaseChanges", "updateReleaseRepository", "reloadRepositories"]);

        inject(function ($controller) {
            sut = $controller("releaseChanges", mocks);
        });
    }));

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("releaseChanges");
        expect(mocks.common.handleEvent).toHaveBeenCalledWith("release.ReleaseWindowLoadedEvent", window.jasmine.any(Function), mocks.$scope);
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), "releaseChanges", mocks.$scope);
    });

    it("should get release content, when getReleaseChanges success", function () {
        sut.state = { isBusy: true };
        var releaseWindow = { ExternalId: "1", ReleaseType: "Scheduled" };

        sut.getReleaseChanges(releaseWindow);
        deferred.resolve({ Changes: [1, 2, 3, 4], Repositories: [{ RepositoryId: "Id" }] });
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(sut.releaseChanges).toEqual([1, 2, 3, 4]);
        expect(sut.repositories).toEqual([{ RepositoryId: "Id" }]);
        expect(mocks.common.sendEvent).toHaveBeenCalledWith("releaseChanges.releaseChangesLoaded", [1, 2, 3, 4]);
    });

    it("should get release content and show only first 5, when getReleaseChanges success and items per page set to 5", function () {
        sut.state = { isBusy: true };
        var releaseWindow = { ExternalId: "1", ReleaseType: "Scheduled" };
        sut.itemsPerPage = 5;

        sut.getReleaseChanges(releaseWindow);
        deferred.resolve({ Changes: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11] });
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(sut.releaseChanges).toEqual([1, 2, 3, 4, 5]);
        expect(sut.releaseChangesAll).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
        expect(mocks.common.sendEvent).toHaveBeenCalledWith("releaseChanges.releaseChangesLoaded", [1, 2, 3, 4, 5]);
        expect(sut.currentPage).toEqual(1);
        expect(sut.totalItemsCount).toEqual(11);
        expect(sut.numPages).toEqual(3);
    });

    it("should log error response, when getReleaseChanges fails", function () {
        sut.state = { isBusy: true };
        var releaseWindow = { ExternalId: "1", ReleaseType: "Scheduled" };

        spyOn(console, "log");

        sut.getReleaseChanges(releaseWindow);
        deferred.reject("error response");
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(console.log).toHaveBeenCalledWith("error response");
        expect(logger.console).toHaveBeenCalledWith("error");
        expect(logger.console).toHaveBeenCalledWith("Unbind from release window");
        expect(logger.console.calls.count()).toEqual(2);
        expect(logger.error).toHaveBeenCalledWith("Cannot load Release Changes");
        expect(logger.error.calls.count()).toEqual(1);
    });

    it("should initialize controller, when gets release window id", function () {
        spyOn(sut, "getReleaseChanges");

        var releaseWindow = { ExternalId: "external id" };
        sut.releaseWindowLoadedEventHandler(releaseWindow);

        expect(sut.releaseWindowId).toEqual(releaseWindow.ExternalId);
        expect(sut.state.bindedToReleaseWindow).toEqual(true);
        expect(sut.getReleaseChanges).toHaveBeenCalledWith(releaseWindow);
    });

    it("should switch controller to unbind state, when gets empty release window", function () {
        spyOn(sut, "getReleaseChanges");

        sut.releaseWindowLoadedEventHandler();

        expect(sut.releaseWindowId).toEqual("");
        expect(sut.state.bindedToReleaseWindow).toEqual(false);
        expect(sut.getReleaseChanges.calls.count()).toEqual(0);
    });

    it("should post remiapi.reAssignReleaseChanges command when reAssignToRelease invoked", function () {
        sut.state = { isBusy: true };
        sut.releaseWindowId = "release id";

        mocks.remiapi.post.reAssignReleaseChanges.and.returnValue(deferred.promise);

        sut.reAssignToRelease();

        deferred.resolve();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(mocks.remiapi.post.reAssignReleaseChanges).toHaveBeenCalledWith({ 'ReleaseWindowId': "release id" });
    });

    it("should update releaseChanges, when current page has changed", function () {
        sut.itemsPerPage = 5;
        sut.releaseChangesAll = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
        sut.releaseChanges = [1, 2, 3, 4, 5];

        sut.currentPage = 3;
        sut.pageChanged();

        expect(sut.releaseChanges).toEqual([11]);
    });

    describe("hasRepositories", function () {
        it("should check if repositories exist, when called", function () {
            expect(sut.hasRepositories()).toEqual(false);

            sut.repositories = [];

            expect(sut.hasRepositories()).toEqual(false);

            sut.repositories = [{}];

            expect(sut.hasRepositories()).toEqual(true);
        });
    });

    describe("repositoryChanged", function () {
        it("should add hasChanged property, when called", function () {
            var repository = {};

            sut.repositoryChanged(repository);

            expect(repository.hasChanged).toEqual(true);
        });
    });

    describe("updateReleaseRepository", function () {
        it("should post command, when save repository invoked", function () {
            sut.releaseWindowId = "release id";
            sut.releaseWindow = { ReleaseWindowId: sut.releaseWindowId }
            var repository = { RepositoryId: "repo id", hasChanged: true };

            mocks.remiapi.post.updateReleaseRepository.and.returnValue(deferred.promise);

            sut.updateReleaseRepository(repository);
            deferred.resolve();
            mocks.$scope.$digest();

            expect(sut.state.isBusy).toEqual(false);
            expect(mocks.remiapi.post.updateReleaseRepository).toHaveBeenCalledWith({
                "ReleaseWindowId": "release id",
                "Repository": { RepositoryId: "repo id" }
            });
        });

        it("should not refresh changes list and leave repository marked as changed, when command was rejected", function () {
            sut.releaseWindowId = "release id";
            var repository = { RepositoryId: "repo id", hasChanged: true };

            mocks.remiapi.post.updateReleaseRepository.and.returnValue(deferred.promise);

            sut.updateReleaseRepository(repository);
            deferred.reject();
            mocks.$scope.$digest();

            expect(sut.state.isBusy).toEqual(false);
            expect(repository.hasChanged).toEqual(true);
        });
    });

    describe("reloadRepositories", function () {
        it("should post command and refresh changes, when reload repositories invoked", function () {
            sut.releaseWindowId = "release id";
            sut.releaseWindow = { ReleaseWindowId: sut.releaseWindowId }

            spyOn(sut, "getReleaseChanges");
            mocks.remiapi.post.reloadRepositories.and.returnValue(deferred.promise);

            sut.reloadRepositories();
            deferred.resolve();
            mocks.$scope.$digest();

            expect(sut.state.isBusy).toEqual(true);
            expect(mocks.remiapi.post.reloadRepositories).toHaveBeenCalledWith({
                "ReleaseWindowId": "release id"
            });
            expect(sut.getReleaseChanges).toHaveBeenCalledWith({ ReleaseWindowId: "release id" });
        });

        it("should not refresh changes list, when command was rejected", function () {
            sut.releaseWindowId = "release id";
            var repository = { RepositoryId: "repo id", hasChanged: true };

            spyOn(sut, "getReleaseChanges");
            mocks.remiapi.post.reloadRepositories.and.returnValue(deferred.promise);

            sut.reloadRepositories(repository);
            deferred.reject();
            mocks.$scope.$digest();

            expect(sut.state.isBusy).toEqual(false);
            expect(repository.hasChanged).toEqual(true);
            expect(sut.getReleaseChanges).not.toHaveBeenCalled();
        });
    });
});
