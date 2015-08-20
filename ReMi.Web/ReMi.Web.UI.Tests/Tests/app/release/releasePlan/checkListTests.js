describe("CheckList Controller", function () {
    var sut, mocks, logger;
    var activateDeferred, updateDeferred, getCheckListDeferred,
        additionalQuestionsDeferred, addQuestionsDeferred, removeQuestionDeferred;

    beforeEach(function () {
        module("app");
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope) {
        activateDeferred = $q.defer();
        mocks = {
            $scope: $rootScope.$new(),
            $rootScope: $rootScope,
            common: {
                logger: window.jasmine.createSpyObj("logger", ["getLogger"]),
                activateController: window.jasmine.createSpy("activateController"),
                handleEvent: window.jasmine.createSpy("handleEvent")
            },
            config: { events: { notificationReceived: "notifications.received" } },
            remiapi: window.jasmine.createSpyObj("remiapi", ["checkList", "additionalCheckListQuestion", "executeCommand", "addCheckListQuestions", "removeCheckListQuestion"]),
            authService: { identity: { fullname: "fullname" } },
            notifications: window.jasmine.createSpyObj("notifications", ["subscribe", "unsubscribe"])
        };
        logger = window.jasmine.createSpyObj("logger", ["console", "error", "info", "warn"]);
        mocks.common.logger.getLogger.and.returnValue(logger);

        mocks.common.activateController.and.returnValue(activateDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
    }));

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("checkList");
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), "checkList", mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith("notifications.received", window.jasmine.any(Function), mocks.$scope);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith("release.ReleaseWindowLoadedEvent", window.jasmine.any(Function), mocks.$scope);
    });

    it("toggle should update checklist answer", function () {
        inject(function ($q) {
            updateDeferred = $q.defer();
        });
        mocks.remiapi.executeCommand.and.returnValue(updateDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
        var item = {};

        sut.toggle(item);
        updateDeferred.resolve();
        mocks.$scope.$digest();

        expect(item.LastChangedBy).toEqual("fullname");
        expect(sut.isBusy).toEqual(false);
    });

    it("toggle should reject update checklist answer with 406", function () {
        inject(function ($q) {
            updateDeferred = $q.defer();
        });
        mocks.remiapi.executeCommand.and.returnValue(updateDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
        var item = {};

        sut.toggle(item);
        updateDeferred.reject(406);
        mocks.$scope.$digest();

        expect(logger.warn).toHaveBeenCalledWith("Request is invalid");
        expect(sut.isBusy).toEqual(false);
    });

    it("toggle should reject update checklist answer with not 406", function () {
        inject(function ($q) {
            updateDeferred = $q.defer();
        });
        mocks.remiapi.executeCommand.and.returnValue(updateDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
        var item = {};

        sut.toggle(item);
        updateDeferred.reject(500);
        mocks.$scope.$digest();

        expect(logger.error).toHaveBeenCalledWith("500 Error occured");
        expect(sut.isBusy).toEqual(false);
    });

    it("should update checklist comment", function () {
        inject(function ($q) {
            updateDeferred = $q.defer();
        });
        mocks.remiapi.executeCommand.and.returnValue(updateDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
        sut.currentItem = { Comment: "some comment" };
        spyOn($.fn, "modal");

        sut.closeEditCommentModal();
        updateDeferred.resolve();
        mocks.$scope.$digest();

        expect(sut.currentItem.LastChangedBy).toEqual("fullname");
        expect(sut.isBusy).toEqual(false);
        expect($("#checkListCommentModal").modal).toHaveBeenCalledWith("hide");
    });

    it("should reject update checklist comment with 406", function () {
        inject(function ($q) {
            updateDeferred = $q.defer();
        });
        mocks.remiapi.executeCommand.and.returnValue(updateDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
        sut.currentItem = { Comment: "some comment" };
        spyOn($.fn, "modal");

        sut.closeEditCommentModal();
        updateDeferred.reject(406);
        mocks.$scope.$digest();

        expect(logger.warn).toHaveBeenCalledWith("Request is invalid");
        expect(sut.isBusy).toEqual(false);
        expect($("#checkListCommentModal").modal).toHaveBeenCalledWith("hide");
    });

    it("should reject update checklist comment with not 406", function () {
        inject(function ($q) {
            updateDeferred = $q.defer();
        });
        mocks.remiapi.executeCommand.and.returnValue(updateDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
        sut.currentItem = { Comment: "some comment" };
        spyOn($.fn, "modal");

        sut.closeEditCommentModal();
        updateDeferred.reject(500);
        mocks.$scope.$digest();

        expect(logger.error).toHaveBeenCalledWith("500 Error occured");
        expect(sut.isBusy).toEqual(false);
        expect($("#checkListCommentModal").modal).toHaveBeenCalledWith("hide");
    });

    it("should hide checklist modal", function () {
        spyOn($.fn, "modal");
        var item = { Comment: "some comment" };

        sut.showEditCommentModal(item);

        expect($("#checkListCommentModal").modal).toHaveBeenCalledWith("show");
        expect(sut.currentItem).toEqual(item);
        expect(sut.currentItem.Comment).toEqual(item.Comment);
    });

    it("should show comment modal", function () {
        spyOn($.fn, "modal");

        sut.hideCurrentChecklistQuestionModal();

        expect($("#checkListQuestionModal").modal).toHaveBeenCalledWith("hide");
    });

    it("should get checklist", function () {
        inject(function ($q) {
            getCheckListDeferred = $q.defer();
        });
        mocks.remiapi.checkList.and.returnValue(getCheckListDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
        sut.releaseWindowId = "window id";

        sut.getCheckList();
        getCheckListDeferred.resolve({ CheckList: "checklist" });
        mocks.$scope.$digest();

        expect(logger.console).toHaveBeenCalledWith("Check List Loaded");
        expect(sut.isBusy).toEqual(false);
        expect(sut.list).toEqual("checklist");
    });

    it("should get empty checklist", function () {
        inject(function ($q) {
            getCheckListDeferred = $q.defer();
        });
        mocks.remiapi.checkList.and.returnValue(getCheckListDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
        sut.releaseWindowId = "window id";

        sut.getCheckList();
        getCheckListDeferred.resolve({});
        mocks.$scope.$digest();

        expect(logger.warn).toHaveBeenCalledWith("Error check list is empty");
        expect(sut.isBusy).toEqual(false);
    });

    it("should reject getting checklist", function () {
        inject(function ($q) {
            getCheckListDeferred = $q.defer();
        });
        mocks.remiapi.checkList.and.returnValue(getCheckListDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
        sut.releaseWindowId = "window id";

        sut.getCheckList();
        getCheckListDeferred.reject("error response");
        mocks.$scope.$digest();

        expect(logger.error).toHaveBeenCalledWith("Error occured during check list loading");
        expect(logger.console).toHaveBeenCalledWith("Error occured during check list loading");
        expect(logger.console).toHaveBeenCalledWith("error response");
        expect(sut.isBusy).toEqual(false);
    });

    it("should get additional questions", function () {
        inject(function ($q) {
            additionalQuestionsDeferred = $q.defer();
        });
        mocks.remiapi.additionalCheckListQuestion.and.returnValue(additionalQuestionsDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
        sut.releaseWindowId = "window id";
        sut.possibleQuestionsDisplayed = [];
        spyOn($.fn, "modal");

        sut.addChecklistQuestions();
        additionalQuestionsDeferred.resolve({
            Questions: [
                { Question: "question1", ExternalId: "id1" },
                { Question: "question2", ExternalId: "id2" }]
        });
        mocks.$scope.$digest();

        expect($("#checkListQuestionModal").modal).toHaveBeenCalledWith("show");
        expect(sut.possibleQuestions.length).toEqual(2);
        expect(sut.possibleQuestionsDisplayed.length).toEqual(2);
        expect(sut.possibleQuestions[0].content).toEqual("question1");
        expect(sut.possibleQuestions[1].content).toEqual("question2");
        expect(sut.possibleQuestions[0].externalId).toEqual("id1");
        expect(sut.possibleQuestions[1].externalId).toEqual("id2");
        expect(sut.possibleQuestions[0].isIncluded).toEqual(false);
        expect(sut.possibleQuestions[1].isIncluded).toEqual(false);
        expect(sut.possibleQuestions[0].isNew).toEqual(false);
        expect(sut.possibleQuestions[1].isNew).toEqual(false);
        expect(sut.possibleQuestionsDisplayed[0].content).toEqual("question1");
        expect(sut.possibleQuestionsDisplayed[1].content).toEqual("question2");
        expect(sut.possibleQuestionsDisplayed[0].isIncluded).toEqual(false);
        expect(sut.possibleQuestionsDisplayed[1].isIncluded).toEqual(false);
    });

    it("should search checklist questions with empty filter", function () {
        sut.possibleQuestions = [{ content: "question1", toBeAdded: false },
            { content: "question2", toBeAdded: false }];
        sut.possibleQuestionsFilter = "";

        sut.searchCheckListQuestion();

        expect(sut.possibleQuestionsDisplayed.length).toEqual(2);
        expect(sut.possibleQuestionsDisplayed[0].content).toEqual("question1");
        expect(sut.possibleQuestionsDisplayed[1].content).toEqual("question2");
        expect(sut.possibleQuestionsDisplayed[0].toBeAdded).toEqual(false);
        expect(sut.possibleQuestionsDisplayed[1].toBeAdded).toEqual(false);
    });

    it("should search checklist questions with existing filter", function () {
        sut.possibleQuestions = [{ content: "question1", toBeAdded: false },
            { content: "question2", toBeAdded: false }];
        sut.possibleQuestionsFilter = "1";

        sut.searchCheckListQuestion();

        expect(sut.possibleQuestionsDisplayed.length).toEqual(1);
        expect(sut.possibleQuestionsDisplayed[0].content).toEqual("question1");
        expect(sut.possibleQuestionsDisplayed[0].toBeAdded).toEqual(false);
    });

    it("should search checklist questions with not existing filter", function () {
        sut.possibleQuestions = [{ content: "question1", toBeAdded: false },
            { content: "question2", toBeAdded: false }];
        sut.possibleQuestionsFilter = "ahaha";

        sut.searchCheckListQuestion();

        expect(sut.possibleQuestionsDisplayed.length).toEqual(0);
    });

    it("should not add empty string as a question", function () {
        sut.possibleQuestionsFilter = "";

        sut.addQuestionToAddToChecklist();

        expect(logger.warn).toHaveBeenCalledWith("Cannot add empty question");
    });

    it("should not existing question", function () {
        sut.possibleQuestionsFilter = "test question";
        sut.possibleQuestions = [{ content: "ahaha" }, { content: "test question" }];

        sut.addQuestionToAddToChecklist();

        expect(logger.warn).toHaveBeenCalledWith("This question already exists");
    });

    it("should not add existing in current checklist question", function () {
        sut.possibleQuestionsFilter = "test question";
        sut.possibleQuestions = [{ content: "ahaha" }, { content: "alala" }];
        sut.list = [{ CheckListQuestion: "test question" },
            { CheckListQuestion: "eeeeee" }];

        sut.addQuestionToAddToChecklist();

        expect(logger.warn).toHaveBeenCalledWith("This question already exists");
    });

    it("should not add question to ready for adding", function () {
        sut.possibleQuestionsFilter = "test question";
        sut.possibleQuestions = [{ content: "question", externalId: "external id", isIncluded: false, isNew: false }];
        sut.list = [{ CheckListQuestion: "eeeeee" }];

        sut.addQuestionToAddToChecklist();

        expect(sut.possibleQuestionsFilter).toEqual("");
        expect(sut.possibleQuestions.length).toEqual(2);
        expect(sut.possibleQuestionsDisplayed.length).toEqual(2);
        expect(sut.possibleQuestions[0].content).toEqual("question");
        expect(sut.possibleQuestions[1].content).toEqual("test question");
        expect(sut.possibleQuestions[0].isIncluded).toEqual(false);
        expect(sut.possibleQuestions[1].isIncluded).toEqual(true);
        expect(sut.possibleQuestionsDisplayed[0].content).toEqual("question");
        expect(sut.possibleQuestionsDisplayed[1].content).toEqual("test question");
        expect(sut.possibleQuestionsDisplayed[0].isIncluded).toEqual(false);
        expect(sut.possibleQuestionsDisplayed[1].isIncluded).toEqual(true);
    });

    it("should not add question to ready for adding", function () {
        sut.possibleQuestions = [{ content: "question", toBeAdded: false }];
        spyOn($.fn, "modal");

        sut.submitQuestions();

        expect($("#checkListQuestionModal").modal).toHaveBeenCalledWith("hide");
    });

    it("should add questions", function () {
        inject(function ($q) {
            addQuestionsDeferred = $q.defer();
        });
        mocks.remiapi.addCheckListQuestions.and.returnValue(addQuestionsDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
        sut.possibleQuestions = [
            { content: "question0", externalId: "external id0", isNew: false, isIncluded: false },
            { content: "question1", externalId: "external id1", isNew: false, isIncluded: true },
            { content: "question2", externalId: "external id2", isNew: true, isIncluded: true }];
        sut.list = [];
        sut.releaseWindowId = "release window id";
        spyOn($.fn, "modal");
        spyOn(window, "newGuid");
        window.newGuid.and.returnValue("guid");

        sut.submitQuestions();
        addQuestionsDeferred.resolve();
        mocks.$scope.$digest();

        expect(sut.state.isAddingBusy).toEqual(false);
        expect($("#checkListQuestionModal").modal).toHaveBeenCalledWith("hide");
        expect(mocks.remiapi.addCheckListQuestions).toHaveBeenCalledWith({
            QuestionsToAdd: [{ Question: "question2", ExternalId: "external id2", CheckListId: "guid" }],
            QuestionsToAssign: [{ Question: "question1", ExternalId: "external id1", CheckListId: "guid" }],
            ReleaseWindowId: "release window id"
        });
        expect(sut.list.length).toEqual(2);
    });

    it("should reject adding questions", function () {
        inject(function ($q) {
            addQuestionsDeferred = $q.defer();
        });
        mocks.remiapi.addCheckListQuestions.and.returnValue(addQuestionsDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
        sut.possibleQuestions = [
            { content: "question0", externalId: "external id0", isNew: false, isIncluded: false },
            { content: "question1", externalId: "external id1", isNew: false, isIncluded: true },
            { content: "question2", externalId: "external id2", isNew: true, isIncluded: true }];
        spyOn($.fn, "modal");

        sut.submitQuestions();
        addQuestionsDeferred.reject("adding rejected");
        mocks.$scope.$digest();

        expect(sut.state.isAddingBusy).toEqual(false);
        expect($("#checkListQuestionModal").modal).toHaveBeenCalledWith("hide");
        expect(logger.error).toHaveBeenCalledWith("Cannot Add Question To CheckList");
        expect(logger.console).toHaveBeenCalledWith("Cannot Add Question To CheckList");
        expect(logger.console).toHaveBeenCalledWith("adding rejected");
    });

    it("should map question", function () {
        sut.possibleQuestions = [
            { content: "question", isIncluded: false, externalId: "external id0", isNew: false },
            { content: "question1", isIncluded: true, externalId: "external id1", isNew: false },
            { content: "question2", isIncluded: true, externalId: "external id2", isNew: false }];

        sut.mapQuestion({ content: "question1", externalId: "external id1", isNew: false, isIncluded: false });

        expect(sut.possibleQuestions.length).toEqual(3);
        expect(sut.possibleQuestions[1].content).toEqual("question1");
        expect(sut.possibleQuestions[1].isIncluded).toEqual(false);
    });

    it("should remove question", function () {
        inject(function ($q) {
            removeQuestionDeferred = $q.defer();
        });
        mocks.remiapi.removeCheckListQuestion.and.returnValue(removeQuestionDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });
        sut.list = [{ ExternalId: "external id", CheckListQuestion: "question" }];

        sut.removeCheckListQuestion({ ExternalId: "external id" }, true);
        removeQuestionDeferred.resolve();
        mocks.$scope.$digest();

        expect(sut.list.length).toEqual(0);
        expect(logger.info).toHaveBeenCalledWith("Question 'question' was removed from checklist");
    });

    it("should reject removing question", function () {
        inject(function ($q) {
            removeQuestionDeferred = $q.defer();
        });
        mocks.remiapi.removeCheckListQuestion.and.returnValue(removeQuestionDeferred.promise);
        inject(function ($controller) {
            sut = $controller("checkList", mocks);
        });

        sut.removeCheckListQuestion("question", true);
        removeQuestionDeferred.reject("remove rejected");
        mocks.$scope.$digest();

        expect(logger.error).toHaveBeenCalledWith("Cannot remove question");
        expect(logger.console).toHaveBeenCalledWith("Cannot remove question");
        expect(logger.console).toHaveBeenCalledWith("remove rejected");
    });

    it("should handle release loaded window event", function () {
        sut.getCheckList = function () { };

        sut.releaseWindowLoadedEventHandler({ ExternalId: "external id" });

        expect(logger.console).toHaveBeenCalledWith("Binded to release window external id");
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith("CheckListAnswerUpdatedEvent", { 'ReleaseWindowId': "external id" });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith("CheckListCommentUpdatedEvent", { 'ReleaseWindowId': "external id" });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith("CheckListQuestionRemovedEvent", { 'ReleaseWindowId': "external id" });
        expect(mocks.notifications.subscribe).toHaveBeenCalledWith("CheckListQuestionsAddedEvent", { 'ReleaseWindowId': "external id" });
    });

    it("should handle empty release loaded window event", function () {
        sut.releaseWindowLoadedEventHandler();

        expect(logger.console).toHaveBeenCalledWith("Unbind release window");
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith("CheckListAnswerUpdatedEvent");
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith("CheckListCommentUpdatedEvent");
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith("CheckListQuestionRemovedEvent");
        expect(mocks.notifications.unsubscribe).toHaveBeenCalledWith("CheckListQuestionsAddedEvent");
    });

    it("should handle question answered server notificartion", function () {
        sut.list = [
            { CheckListQuestion: "How are you?", ExternalId: "other id", Checked: true },
            { CheckListQuestion: "How do you do?", ExternalId: "id", Checked: false }
        ];

        sut.handleCheckListAnswerUpdated({ Checked: true, CheckListId: "id" });

        expect(sut.list[1].CheckListQuestion).toEqual("How do you do?");
        expect(sut.list[1].Checked).toEqual(true);
        expect(logger.info).toHaveBeenCalledWith("Question How do you do? was answered");
    });

    it("should handle question marked as not answered server notificartion", function () {
        sut.list = [
                    { CheckListQuestion: "How are you?", ExternalId: "other id", Checked: true },
                    { CheckListQuestion: "How do you do?", ExternalId: "id", Checked: true }
        ];

        sut.handleCheckListAnswerUpdated({ Checked: false, CheckListId: "id" });

        expect(sut.list[1].CheckListQuestion).toEqual("How do you do?");
        expect(sut.list[1].Checked).toEqual(false);
        expect(logger.info).toHaveBeenCalledWith("Question How do you do? was marked as not answered");
    });

    it("should handle comment updated server notificartion", function () {
        sut.list = [
            { CheckListQuestion: "How are you?", ExternalId: "other id" },
            { CheckListQuestion: "How do you do?", ExternalId: "id" }
        ];

        sut.handleCheckListCommentUpdated({ Comment: "yahoo!", CheckListId: "id" });

        expect(sut.list[1].CheckListQuestion).toEqual("How do you do?");
        expect(sut.list[1].Comment).toEqual("yahoo!");
        expect(logger.info).toHaveBeenCalledWith("Comment for question How do you do? was changed to: yahoo!");
    });

    it("should handle question removed server notificartion", function () {
        sut.list = [{ CheckListQuestion: "How are you?" },
            { CheckListQuestion: "How do you do?" }];

        sut.handleCheckListQuestionRemoved({ Question: "How are you?" });

        expect(sut.list.length).toEqual(1);
        expect(sut.list[0].CheckListQuestion).toEqual("How do you do?");
        expect(logger.info).toHaveBeenCalledWith("Question How are you? was removed from checklist");
    });

    it("should handle questions added server notificartion", function () {
        sut.list = [];

        sut.handleCheckListQuestionsAdded({
            Questions:
                [{ CheckListId: "xxx", Question: "ahaha" }]
        });

        expect(sut.list.length).toEqual(1);
        expect(sut.list[0].CheckListQuestion).toEqual("ahaha");
        expect(logger.info).toHaveBeenCalledWith("New check list questions were added");
    });
});

