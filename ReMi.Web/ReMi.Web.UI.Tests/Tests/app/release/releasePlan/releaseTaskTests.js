describe("Release Tasks Controller", function () {
    var sut, mocks, logger;
    var $q, $rootScope;

    beforeEach(function () {
        module("app");

        mocks = {
            common: {
                logger: jasmine.createSpyObj("logger", ["getLogger"]),
                $q: jasmine.createSpyObj("$q", ["when"]),
                activateController: jasmine.createSpy("activateController").and.returnValue({ then: jasmine.createSpy("then") }),
                sendEvent: window.jasmine.createSpy("sendEvent"),
                handleEvent: window.jasmine.createSpy("handleEvent"),
                getParentScope: window.jasmine.createSpy("getParentScope").and.returnValue({ vm: {} })
            },
            config: jasmine.createSpyObj("config", ["events"]),
            remiapi: jasmine.createSpyObj("remiapi", [
                "executeCommand", "getReleaseTasks", "completeReleaseTask",
                "getReleaseTaskTypes", "getReleaseTaskRisks", "getReleaseTaskEnvironments",
                "confirmReleaseTaskReview", "confirmReleaseTaskImplementation", "createReleaseTask",
                "checkAccounts", "sendAttchment", "convertUploadFilesToAttachment", "updateReleaseTasksOrder",
                "deleteReleaseTask"
            ]),

            authService: jasmine.createSpyObj("authService", ["identity"]),

            notifications: jasmine.createSpyObj("notifications", ["subscribe", "unsubscribe"]),
        };
        mocks.remiapi.post = jasmine.createSpyObj("remiapi.post", ["confirmReleaseTask"]);
        logger = {
            console: window.jasmine.createSpy("console"),
            warn: window.jasmine.createSpy("warn"),
            error: window.jasmine.createSpy("error")
        };
        mocks.common.logger.getLogger.and.returnValue(logger);

        inject(function ($controller, _$q_, _$rootScope_) {
            $q = _$q_;
            $rootScope = _$rootScope_;
            mocks.$scope = $rootScope.$new();
            mocks.$scope.registerWidget = function (name) { return true; };
            spyOn(mocks.$scope, "registerWidget");

            var getReleaseTaskTypesPromise = $q.defer();
            mocks.remiapi.getReleaseTaskTypes.and.returnValue(getReleaseTaskTypesPromise.promise);
            getReleaseTaskTypesPromise.resolve({ ReleaseTaskTypes: [] });

            var getReleaseTaskRisksPromise = $q.defer();
            mocks.remiapi.getReleaseTaskRisks.and.returnValue(getReleaseTaskRisksPromise.promise);
            getReleaseTaskRisksPromise.resolve({ Risks: [] });

            var getReleaseTaskEnvironmentsPromise = $q.defer();
            mocks.remiapi.getReleaseTaskEnvironments.and.returnValue(getReleaseTaskEnvironmentsPromise.promise);
            getReleaseTaskEnvironmentsPromise.resolve({ Environments: [] });

            var whenPromise = $q.defer();
            mocks.common.$q.when.and.returnValue(whenPromise.promise);
            whenPromise.resolve();

            sut = $controller("releaseTask", mocks);
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("releaseTask");
        expect(mocks.common.activateController).toHaveBeenCalledWith(jasmine.any(Array), "releaseTask", mocks.$scope);

        expect(mocks.common.handleEvent).toHaveBeenCalledWith("release.ReleaseWindowLoadedEvent", jasmine.any(Function), mocks.$scope);
        expect(mocks.common.getParentScope).toHaveBeenCalledWith(mocks.$scope, jasmine.any(Function));
        expect(logger.console).toHaveBeenCalledWith("Unbind release window");
    });

    it("should get read release tasks when binded to release", function () {
        var deferred = $q.defer();
        mocks.remiapi.getReleaseTasks.and.returnValue(deferred.promise);
        deferred.resolve({ ReleaseTasks: [] });

        sut.releaseWindowLoadedEventHandler({ ExternalId: "windowId", Product: "XYZ" });

        mocks.$scope.$digest();

        expect(mocks.remiapi.getReleaseTasks).toHaveBeenCalledWith("windowId");
    });

    it("should subscribe on server events when binded to release", function () {
        var deferred = $q.defer();
        mocks.remiapi.getReleaseTasks.and.returnValue(deferred.promise);

        sut.releaseWindowLoadedEventHandler({ ExternalId: "windowId", Product: "XYZ" });

        mocks.$scope.$digest();

        expect(mocks.notifications.subscribe).toHaveBeenCalledWith("HelpDeskTaskCreatedEvent", { 'ReleaseWindowId': "windowId" });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith("TaskCompletedEvent", { 'ReleaseWindowId': "windowId" });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith("ReleaseTaskUpdatedEvent", { 'ReleaseWindowId': "windowId" });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith("ReleaseTaskCreatedEvent", { 'ReleaseWindowId': "windowId" });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith("ReleaseTaskUpdatedEvent", { 'ReleaseWindowId': "windowId" });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith("ReleaseTaskDeletedEvent", { 'ReleaseWindowId': "windowId" });
    });

    it("should fill out contoller properties, when binded to release", function () {
        var deferred = $q.defer();
        mocks.remiapi.getReleaseTasks.and.returnValue(deferred.promise);

        sut.releaseWindowLoadedEventHandler({ ExternalId: "windowId", Product: "XYZ", ReleaseType: "release type", ClosedOn: "some date" });

        mocks.$scope.$digest();

        expect(sut.releaseWindowId).toEqual("windowId");
        expect(sut.releaseType).toEqual("release type");
        expect(sut.state.bindedToReleaseWindow).toBeTruthy();
        expect(sut.isClosed).toBeTruthy();
    });

    it("should unsubscribe on server events when binded to release", function () {
        sut.releaseWindowLoadedEventHandler();

        mocks.$scope.$digest();

        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith("HelpDeskTaskCreatedEvent");
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith("TaskCompletedEvent");
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith("ReleaseTaskUpdatedEvent");
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith("ReleaseTaskCreatedEvent");
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith("ReleaseTasksOrderUpdatedEvent");
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith("ReleaseTaskDeletedEvent");
    });

    it("should not complete task when user not authenticated", function () {
        mocks.authService.isLoggedIn = false;
        sut.releaseTasks = [
            {
                ExternalId: "task1",
                AssigneeExternalId: "acc1"
            }
        ];
        sut.completeReleaseTask();

        mocks.$scope.$digest();

        expect(mocks.remiapi.completeReleaseTask).not.toHaveBeenCalled();
    });

    it("should call remiapi.completeReleaseTask when current user is assignee", function () {
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity.externalId = "acc1";
        var t = {
            ExternalId: "task1",
            AssigneeExternalId: "acc1"
        };
        sut.releaseTasks = [t];

        var deferred = $q.defer();
        mocks.remiapi.completeReleaseTask.and.returnValue(deferred.promise);

        sut.completeReleaseTask(t);

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.completeReleaseTask).toHaveBeenCalledWith({ ReleaseTaskExtetnalId: "task1" });
    });

    it("should call remiapi.completeReleaseTask when task is reviewed and implemented", function () {
        mocks.authService.isLoggedIn = true;
        mocks.authService.identity.externalId = "acc2";
        var t = {
            ExternalId: "task1",
            AssigneeExternalId: "acc1",
            WhoImplementedExternalId: "implementor",
            IsImplementorConfirmed: true,
            ReviewedByExternalId: "reviewer",
            IsReviewerConfirmed: true

        };
        sut.releaseTasks = [t];

        var deferred = $q.defer();
        mocks.remiapi.completeReleaseTask.and.returnValue(deferred.promise);

        sut.completeReleaseTask(t);

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.completeReleaseTask).toHaveBeenCalledWith({ ReleaseTaskExtetnalId: "task1" });
    });

    it("should call remiapi.deleteReleaseTask, when tasks being removed", function () {
        var task = { ExternalId: "task1" };

        var deferred = $q.defer();
        mocks.remiapi.deleteReleaseTask.and.returnValue(deferred.promise);

        sut.removeReleaseTask(task);
        deferred.resolve();
        mocks.$scope.$digest();

        expect(mocks.remiapi.deleteReleaseTask).toHaveBeenCalledWith(window.jasmine.objectContaining({ ReleaseTaskId: task.ExternalId }));
        expect(mocks.common.sendEvent).toHaveBeenCalledWith("releaseTask.ReleaseTaskDeletedEvent",
            window.jasmine.objectContaining({ ExternalId: task.ExternalId }));
        expect(sut.state.isBusy).toEqual(false);
    });

    it("should call remiapi.updateReleaseTasksOrder, when tasks order was changed", function () {
        sut.releaseTasks = [
            { ExternalId: "task1" },
            { ExternalId: "task2" },
            { ExternalId: "task3" }
        ];

        var deferred = $q.defer();
        mocks.remiapi.updateReleaseTasksOrder.and.returnValue(deferred.promise);

        sut.applyReleaseTasksOrder(sut.releaseTasks);
        deferred.resolve();
        mocks.$scope.$digest();

        expect(mocks.remiapi.updateReleaseTasksOrder).toHaveBeenCalledWith(window.jasmine.objectContaining({
            ReleaseTasksOrder: {
                'task1': 1,
                'task2': 2,
                'task3': 3
            }
        }));
        expect(mocks.common.sendEvent).toHaveBeenCalledWith("releaseTask.ReleaseTasksOrderUpdatedEvent");
        expect(sut.state.isBusy).toEqual(false);
    });

    it("should init drag and drop method, when initDragging method called", function () {
        expect(sut.dragging).toBeDefined();
        expect(sut.dragging.currentDragged).toBeNull();
        expect(sut.dragging.drag).toEqual(window.jasmine.any(Function));
        expect(sut.dragging.drop).toEqual(window.jasmine.any(Function));
        expect(sut.dragging.dragEnd).toEqual(window.jasmine.any(Function));
    });

    it("should init currentDragged element, when start dragging", function () {
        sut.dragging.drag("task");
        expect(sut.dragging.currentDragged).toEqual("task");
    });

    it("should clean currentDragged element, when end dragging", function () {
        sut.dragging.currentDragged = "not null";
        sut.dragging.dragEnd();
        expect(sut.dragging.currentDragged).toBeNull();
    });

    it("should change order of release tasks, when drop on another element", function () {
        sut.releaseTasks = [
            { ExternalId: "task1" },
            { ExternalId: "task2" },
            { ExternalId: "task3" }
        ];
        sut.dragging.currentDragged = sut.releaseTasks[2];

        spyOn(mocks.$scope, "$apply").and.callFake(function (method) {
            method();
        });
        spyOn(sut, "applyReleaseTasksOrder");

        sut.dragging.drop(sut.releaseTasks[0]);

        expect(sut.releaseTasks[0].ExternalId).toEqual("task3");
        expect(sut.releaseTasks[1].ExternalId).toEqual("task1");
        expect(sut.releaseTasks[2].ExternalId).toEqual("task2");
        expect(sut.applyReleaseTasksOrder).toHaveBeenCalledWith(sut.releaseTasks);
    });

    it("should not change order of release tasks, when drop on same element", function () {
        sut.releaseTasks = [
            { ExternalId: "task1" },
            { ExternalId: "task2" },
            { ExternalId: "task3" }
        ];
        sut.dragging.currentDragged = sut.releaseTasks[2];

        spyOn(mocks.$scope, "$apply").and.callFake(function (insideApplyMethod) {
            insideApplyMethod();
        });
        spyOn(sut, "applyReleaseTasksOrder");

        sut.dragging.drop(sut.releaseTasks[2]);

        expect(sut.releaseTasks[0].ExternalId).toEqual("task1");
        expect(sut.releaseTasks[1].ExternalId).toEqual("task2");
        expect(sut.releaseTasks[2].ExternalId).toEqual("task3");
        expect(sut.applyReleaseTasksOrder.calls.count()).toEqual(0);
    });

    describe("isManualConfirmationAllowed", function () {
        it("should return true, when task is not confirmed and logged user is assigned to this task", function () {
            var task = { AssigneeExternalId: "user id", IsConfirmed: false };
            mocks.authService.identity.externalId = "user id";

            var result = sut.isManualConfirmationAllowed(task);

            expect(result).toBeTruthy();
        });
        it("should return false, when task is confirmed and logged user is assigned to this task", function () {
            var task = { AssigneeExternalId: "user id", IsConfirmed: true };
            mocks.authService.identity.externalId = "user id";

            var result = sut.isManualConfirmationAllowed(task);

            expect(result).toBeFalsy();
        });
        it("should return false, when task is not confirmed and logged user is not assigned to this task", function () {
            var task = { AssigneeExternalId: "user id", IsConfirmed: false };
            mocks.authService.identity.externalId = "user2 id";

            var result = sut.isManualConfirmationAllowed(task);

            expect(result).toBeFalsy();
        });
    });

    describe("confirmTask", function () {
        it("should do nothing, when manual task confirmation is not allowed", function () {
            spyOn(sut, "isManualConfirmationAllowed").and.returnValue(false);

            sut.confirmTask({});

            expect(mocks.remiapi.post.confirmReleaseTask).not.toHaveBeenCalled();
        });
        it("should mark task as confirmed, when command executed successfully", function () {
            var task = { ExternalId: "task id" };
            var confirmationDeferred = $q.defer();

            spyOn(sut, "isManualConfirmationAllowed").and.returnValue(true);
            mocks.remiapi.post.confirmReleaseTask.and.returnValue(confirmationDeferred.promise);

            sut.confirmTask(task);

            expect(sut.state.isBusy).toBeTruthy();

            confirmationDeferred.resolve();
            mocks.$scope.$digest();

            expect(mocks.remiapi.post.confirmReleaseTask).toHaveBeenCalledWith({ ReleaseTaskId: task.ExternalId });
            expect(task.IsConfirmed).toBeTruthy();
            expect(sut.state.isBusy).toBeFalsy();
        });
        it("should not mark task as confirmed, when command execution failed", function () {
            var task = { ExternalId: "task id", IsConfirmed: false };
            var confirmationDeferred = $q.defer();

            spyOn(sut, "isManualConfirmationAllowed").and.returnValue(true);
            mocks.remiapi.post.confirmReleaseTask.and.returnValue(confirmationDeferred.promise);

            sut.confirmTask(task);
            confirmationDeferred.reject("error message");
            mocks.$scope.$digest();

            expect(task.IsConfirmed).toBeFalsy();
            expect(sut.state.isBusy).toBeFalsy();
            expect(logger.error).toHaveBeenCalledWith("Cannot confirm release task");
            expect(logger.console).toHaveBeenCalledWith("Cannot confirm release task");
            expect(logger.console).toHaveBeenCalledWith("error message");
        });
    });
});

