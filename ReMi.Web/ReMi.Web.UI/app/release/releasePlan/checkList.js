(function () {
    "use strict";
    var controllerId = "checkList";
    angular.module("app").controller(controllerId, ["remiapi", "common", "$scope", "authService", "config", "notifications", checkList]);

    function checkList(remiapi, common, $scope, authService, config, notifications) {
        var self = this;
        var logger = common.logger.getLogger(controllerId);
        self.isBusy = true;

        self.hideCurrentChecklistQuestionModal = hideCurrentChecklistQuestionModal;
        self.addChecklistQuestions = addChecklistQuestions;
        self.searchCheckListQuestion = searchCheckListQuestion;
        self.addQuestionToAddToChecklist = addQuestionToAddToChecklist;
        self.submitQuestions = submitQuestions;
        self.mapQuestion = mapQuestion;
        self.removeCheckListQuestion = removeCheckListQuestion;
        self.releaseWindowLoadedEventHandler = releaseWindowLoadedEventHandler;
        self.serverNotificationHandler = serverNotificationHandler;
        self.showEditCommentModal = showEditCommentModal;
        self.closeEditCommentModal = closeEditCommentModal;
        self.toggle = toggle;
        self.updateCheckListAnswer = updateCheckListAnswer;
        self.updateCheckList = updateCheckList;
        self.updateCheckListComment = updateCheckListComment;
        self.getCheckList = getCheckList;
        self.handleCheckListQuestionsAdded = handleCheckListQuestionsAdded;
        self.handleCheckListQuestionRemoved = handleCheckListQuestionRemoved;
        self.handleCheckListCommentUpdated = handleCheckListCommentUpdated;
        self.handleCheckListAnswerUpdated = handleCheckListAnswerUpdated;

        self.possibleQuestionsDisplayed = [];
        self.possibleQuestionsFilter = "";
        self.possibleQuestions = [];
        self.state = {
            isAddingBusy: false
        };

        common.handleEvent("release.ReleaseWindowLoadedEvent", self.releaseWindowLoadedEventHandler, $scope);
        common.handleEvent(config.events.notificationReceived, self.serverNotificationHandler, $scope);

        $scope.$on("$destroy", scopeDestroyHandler);

        activate();

        function toggle(item) {
            self.updateCheckListAnswer(item);
        };

        function showEditCommentModal(item) {
            self.currentItem = item;
            $("#checkListCommentModal").modal("show");
        };

        function closeEditCommentModal() {
            $("#checkListCommentModal").modal("hide");
            self.updateCheckListComment(self.currentItem);
        };

        function updateCheckListAnswer(item) {
            self.updateCheckList(item, "UpdateCheckListAnswerCommand",
                function (i) { i.Checked = !i.Checked; });
        };

        function updateCheckListComment(item) {
            self.updateCheckList(item, "UpdateCheckListCommentCommand");
        };

        function updateCheckList(item, commandName, rollback) {
            var data = { CheckListItem: item };
            var commandId = newGuid();
            self.isBusy = true;
            remiapi.executeCommand(commandName, commandId, data)
                .then(
                    function () {
                        item.LastChangedBy = authService.identity.fullname;
                    },
                    function (statusCode) {
                        if (rollback)
                            rollback(item);
                        if (statusCode === 406) {
                            logger.warn("Request is invalid");
                        } else {
                            logger.error(statusCode + " Error occured");
                        }
                    }
                )
                .finally(function () { self.isBusy = false; });
        };

        function activate() {
            common.activateController([validate()], controllerId, $scope)
                .then(function () { logger.console("Activated CheckList Widget"); });

        };

        function validate() {
            $("#checkListForm").validate({
                errorClass: "invalid-data",
                errorPlacement: function () { }
            });
        };

        function getCheckList() {
            self.isBusy = true;

            self.list = [];

            remiapi.checkList(self.releaseWindowId).then(
                function (event) {
                    if (event && event.CheckList) {
                        self.list = event.CheckList;
                        logger.console("Check List Loaded");
                    } else
                        logger.warn("Error check list is empty");
                },
                function (response) {
                    logger.console("Error occured during check list loading");
                    logger.console(response);
                    logger.error("Error occured during check list loading");
                }).finally(function () {
                    self.isBusy = false;
                });
        }

        function hideCurrentChecklistQuestionModal() {
            $("#checkListQuestionModal").modal("hide");
        }

        function addChecklistQuestions() {
            self.possibleQuestions = [];
            self.isBusy = true;
            remiapi.additionalCheckListQuestion(self.releaseWindowId).then(function (data) {
                for (var counter = 0; counter < data.Questions.length; counter++) {
                    var question = data.Questions[counter];
                    self.possibleQuestions.push({
                        content: question.Question,
                        externalId: question.ExternalId,
                        isIncluded: false,
                        isNew: false
                    });
                }
                self.possibleQuestionsDisplayed = self.possibleQuestions;
                $("#checkListQuestionModal").modal("show");
            }).finally(function () {
                self.isBusy = false;
            });
        }

        function searchCheckListQuestion() {
            if (self.possibleQuestionsFilter === "") {
                self.possibleQuestionsDisplayed = self.possibleQuestions;
            } else {
                self.possibleQuestionsDisplayed = self.possibleQuestions.filter(function (item) {
                    return (item.content.toLowerCase().indexOf(self.possibleQuestionsFilter.toLowerCase()) > -1);
                });
            }
        }

        function addQuestionToAddToChecklist() {
            if (self.possibleQuestionsFilter === "") {
                logger.warn("Cannot add empty question");
                return;
            }
            if (self.possibleQuestions.filter(function (item) {
                return item.content === self.possibleQuestionsFilter;
            }).length > 0) {
                logger.warn("This question already exists");
                return;
            }
            if (self.list.filter(function (item) {
                return item.CheckListQuestion === self.possibleQuestionsFilter;
            }).length > 0) {
                logger.warn("This question already exists");
                return;
            }


            self.possibleQuestions.push({
                content: self.possibleQuestionsFilter,
                externalId: newGuid(),
                isIncluded: true,
                isNew: true
            });
            self.possibleQuestionsFilter = "";
            self.possibleQuestionsDisplayed = self.possibleQuestions;
        }

        function submitQuestions() {
            var questionsToAdd = [];
            var questionsToAssign = [];
            for (var counter = 0; counter < self.possibleQuestions.length; counter++) {
                var question = self.possibleQuestions[counter];
                if (question.isIncluded) {
                    var questionData = {
                        Question: question.content,
                        ExternalId: question.externalId,
                        CheckListId: newGuid()
                    }
                    if (question.isNew)
                        questionsToAdd.push(questionData);
                    else
                        questionsToAssign.push(questionData);
                }
            }
            if (questionsToAdd.length === 0 && questionsToAssign.length === 0) {
                self.hideCurrentChecklistQuestionModal();
                return;
            }
            self.state.isAddingBusy = true;
            remiapi.addCheckListQuestions({
                QuestionsToAdd: questionsToAdd,
                QuestionsToAssign: questionsToAssign,
                ReleaseWindowId: self.releaseWindowId
            }).then(function () {
                angular.forEach(questionsToAdd, function (x) { addCheckListItem(x); });
                angular.forEach(questionsToAssign, function (x) { addCheckListItem(x); });
            }, function (error) {
                logger.error("Cannot Add Question To CheckList");
                logger.console("Cannot Add Question To CheckList");
                logger.console(error);
            }).finally(function () {
                self.state.isAddingBusy = false;
                self.hideCurrentChecklistQuestionModal();
            });
        }

        function mapQuestion(question) {
            for (var counter = 0; counter < self.possibleQuestions.length; counter++) {
                if (self.possibleQuestions[counter].externalId === question.externalId) {
                    self.possibleQuestions[counter].isIncluded = question.isIncluded;
                    break;
                }
            }
        }

        function removeCheckListQuestion(question, forWholeProduct) {
            self.isBusy = true;
            return remiapi.removeCheckListQuestion({
                CheckListQuestionId: question.ExternalId,
                ForWholeProduct: forWholeProduct
            }).then(function () {
                for (var counter = 0; counter < self.list.length; counter++) {
                    if (self.list[counter].ExternalId === question.ExternalId) {
                        logger.info("Question '" + self.list[counter].CheckListQuestion + "' was removed from checklist");
                        self.list.splice(counter, 1);
                        return;
                    }
                }
            }, function (error) {
                logger.error("Cannot remove question");
                logger.console("Cannot remove question");
                logger.console(error);
            }).finally(function () {
                self.isBusy = false;
            });
        }

        function releaseWindowLoadedEventHandler(releaseWindow) {
            if (releaseWindow) {
                self.releaseWindowId = releaseWindow.ExternalId;
                self.getCheckList();
                logger.console("Binded to release window " + releaseWindow.ExternalId);
                notifications.subscribe("CheckListAnswerUpdatedEvent", { 'ReleaseWindowId': self.releaseWindowId });
                notifications.subscribe("CheckListCommentUpdatedEvent", { 'ReleaseWindowId': self.releaseWindowId });
                notifications.subscribe("CheckListQuestionRemovedEvent", { 'ReleaseWindowId': self.releaseWindowId });
                notifications.subscribe("CheckListQuestionsAddedEvent", { 'ReleaseWindowId': self.releaseWindowId });
            } else {
                logger.console("Unbind release window");
                notifications.unsubscribe("CheckListAnswerUpdatedEvent");
                notifications.unsubscribe("CheckListCommentUpdatedEvent");
                notifications.unsubscribe("CheckListQuestionRemovedEvent");
                notifications.unsubscribe("CheckListQuestionsAddedEvent");
            }
        }

        function serverNotificationHandler(notification) {
            if (notification.name === "CheckListAnswerUpdatedEvent") {
                checkListAnswerUpdatedHandler(notification.data);
            }
            if (notification.name === "CheckListCommentUpdatedEvent") {
                checkListCommentUpdatedHandler(notification.data);
            }
            if (notification.name === "CheckListQuestionRemovedEvent") {
                checkListQuestionRemovedHandler(notification.data);
            }
            if (notification.name === "CheckListQuestionsAddedEvent") {
                checkListQuestionsAddedHandler(notification.data);
            }
        }

        function checkListAnswerUpdatedHandler(data) {
            $scope.$apply(self.handleCheckListAnswerUpdated(data));
        }

        function checkListCommentUpdatedHandler(data) {
            $scope.$apply(self.handleCheckListCommentUpdated(data));
        }

        function checkListQuestionRemovedHandler(data) {
            $scope.$apply(self.handleCheckListQuestionRemoved(data));
        }

        function checkListQuestionsAddedHandler(data) {
            $scope.$apply(self.handleCheckListQuestionsAdded(data));
        }

        function addCheckListItem(checkListQuestion) {
            var checkListItem = {
                ExternalId: checkListQuestion.CheckListId,
                ReleaseWindowId: self.releaseWindowId,
                CheckListQuestion: checkListQuestion.Question,
                Checked: false,
                Comment: "",
                LastChangedBy: null
            }
            self.list.push(checkListItem);
        }

        function handleCheckListQuestionsAdded(data) {
            for (var counter = 0; counter < data.Questions.length; counter++) {
                var question = data.Questions[counter];
                if (self.list.filter(function (item) {
                    return question.CheckListId === item.ExternalId;
                }).length === 0) {
                    addCheckListItem(question);
                }
            }
            if (data.Questions.length > 0) {
                logger.info("New check list questions were added");
            }
        }

        function handleCheckListQuestionRemoved(data) {
            for (var counter = 0; counter < self.list.length; counter++) {
                if (self.list[counter].ExternalId === data.CheckListId) {
                    logger.info("Question " + self.list[counter].CheckListQuestion + " was removed from checklist");
                    self.list.splice(counter, 1);
                    break;
                }
            }
        }

        function handleCheckListCommentUpdated(data) {
            for (var counter = 0; counter < self.list.length; counter++) {
                if (self.list[counter].ExternalId === data.CheckListId) {
                    self.list[counter].Comment = data.Comment;
                    self.list[counter].LastChangedBy = data.AnsweredBy;
                    logger.info("Comment for question " + self.list[counter].CheckListQuestion +
                        " was changed to: " + data.Comment);
                    break;
                }
            }
        }
        function handleCheckListAnswerUpdated(data) {
            for (var counter = 0; counter < self.list.length; counter++) {
                if (self.list[counter].ExternalId === data.CheckListId) {
                    self.list[counter].Checked = data.Checked;
                    self.list[counter].LastChangedBy = data.AnsweredBy;
                    if (data.Checked) {
                        logger.info("Question " + self.list[counter].CheckListQuestion + " was answered");
                    } else {
                        logger.info("Question " + self.list[counter].CheckListQuestion + " was marked as not answered");
                    }
                    break;
                }
            }
        }

        function scopeDestroyHandler() {
            notifications.unsubscribe("CheckListAnswerUpdatedEvent");
            notifications.unsubscribe("CheckListCommentUpdatedEvent");
            notifications.unsubscribe("CheckListQuestionRemovedEvent");
            notifications.unsubscribe("CheckListQuestionsAddedEvent");
        }
    }
})()
