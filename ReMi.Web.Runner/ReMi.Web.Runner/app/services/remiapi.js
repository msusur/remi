(function () {
    "use strict";

    var serviceId = "remiapi";
    angular.module("app").factory(serviceId, ["$http", "$rootScope", "$location", "$timeout", "common", "config", remiapi]);

    function remiapi($http, $rootScope, $location, $timeout, common, config) {
        var $q = common.$q;
        var logger = {
            console: function (message) { common.logger.getLogFn(serviceId, "console")(message); }
        };

        var commandPoll = {};
        var commandStates = {
            notRegistered: "NotRegistered",
            waiting: "Waiting",
            running: "Running",
            success: "Success",
            failed: "Failed"
        };

        var service = {
            isLoggedIn: false,
            getAbsolutePath: getAbsolutePath,
            apiPath: getApiPath(),

            //authentication
            checkSession: checkSession,
            defaultProductForNewUser: defaultProductForNewUser,

            //configuration
            getProducts: getProducts,
            getCommands: getCommands,
            getQueries: getQueries,
            addQueryToRole: addQueryToRole,
            removeQueryFromRole: removeQueryFromRole,

            //dictionaries
            getReleaseTaskTypes: getReleaseTaskTypes,
            getReleaseTaskRisks: getReleaseTaskRisks,

            //releases
            releaseCalendar: releaseCalendar,
            releaseEnums: releaseEnums,
            followingRelease: followingRelease,
            getNearReleases: getNearReleases,
            approveRelease: approveRelease,
            getRelease: getRelease,
            createRole: createRole,
            updateRole: updateRole,
            deleteRole: deleteRole,
            saveReleaseIssues: saveReleaseIssues,

            //release plan
            checkList: checkList,
            getReleaseTask: getReleaseTask,
            getReleaseTasks: getReleaseTasks,
            createReleaseTask: createReleaseTask,
            updateReleaseTask: updateReleaseTask,
            deleteReleaseTask: deleteReleaseTask,
            updateReleaseTasksOrder: updateReleaseTasksOrder,
            releaseParticipants: releaseParticipants,
            releaseContentData: releaseContentData,
            ticketRisk: ticketRisk,
            releaseChanges: releaseChanges,
            searchUsers: searchUsers,
            closeReleaseWindow: closeReleaseWindow,
            additionalCheckListQuestion: additionalCheckListQuestion,
            addCheckListQuestions: addCheckListQuestions,
            removeCheckListQuestion: removeCheckListQuestion,
            getReleaseApprovers: getReleaseApprovers,
            addReleaseApprovers: addReleaseApprovers,
            removeReleaseApprover: removeReleaseApprover,
            completeReleaseTask: completeReleaseTask,
            getReleaseTaskEnvironments: getReleaseTaskEnvironments,
            confirmReleaseTaskReview: confirmReleaseTaskReview,
            confirmReleaseTaskImplementation: confirmReleaseTaskImplementation,

            //command tracking
            commandStatus: commandStatus,

            // generic command execution
            executeCommand: executeCommand,

            //release execution
            signOffRelease: signOffRelease,
            addSignOffs: addSignOffs,
            removeSignOff: removeSignOff,
            getSignOffs: getSignOffs,

            //admin
            getAccounts: getAccounts,
            getAccountsByProduct: getAccountsByProduct,
            getRoles: getRoles,
            createAccount: createAccount,
            updateAccount: updateAccount,
            checkAccounts: checkAccounts,
            getReleaseTrack: getReleaseTrack,

            //metrics
            getMetrics: getMetrics,
            updateMetrics: updateMetrics,

            // business rules
            testBusinessRule: testBusinessRule,
            executeBusinessRule: executeBusinessRule
        };

        service.queryDefinitions = {
            // authentication
            session: "/session/{0}",

            //common
            enums: "/common/enums",

            //release calendar
            getRelease: "/releases/{0}",
            releaseEnums: "/releases/releaseEnums",
            releaseCalendar: "/releases/search/{0}/{1}",

            //release plan
            getReleaseJobs: "/releases/deploymentJobs/{0}",
            qaStatus: "/QaStatus/{0}",

            //release execution
            getDeploymentJobsMeasurements: "/measurements/deploymentJobs/{0}",

            //measurements
            getMeasurements: "/measurements/{0}",
            getDeploymentJobsMeasurementsByProduct: "/measurements/jobs/{0}",

            //configuration
            products: "/configuration/products",
            businessUnits: "/configuration/businessUnits/{0?}",

            commandsNyNames: "/configuration/commands/{0}",

            //continiousDelivery
            apiDescriptions: "/cd/apiDescriptions",

            //productRegistration
            productRegistrationsConfig: "/productRequests/config",
            productRequestRegistrations: "/productRequests/registrations",

            //Business Rules
            businessRuleByName: "rule/{0}/{1}",
            businessRuleById: "rule/{0}",
            businessRules: "rule/rules",
            generatedBusinessRule: "rule/generate/{0}/{1}",

            //subscriptions
            userNotificationSubscriptions: "/subscriptions/{0}",

            //permissions
            permissions: "/accounts/permissions/{0?}",

            //reports
            reportList: "/reports",
            report: "/reports/{0}",

            //plugins
            globalPlugins: "/plugins/global",
            packagePlugins: "/plugins/package",
            plugins: "/plugins",
            pluginConfituration: "/plugins/{0}",
            globalPluginConfiguration: "/plugins/global/{0}",
            packagePluginConfiguration: "/plugins/package/{0}/{1}"
        };

        service.commandDefinitions = {
            // authentication
            startSession: "StartSessionCommand",

            //products
            updateProduct: "UpdateProductCommand",
            addProduct: "AddProductCommand",

            //release calendar
            removeReleaseWindow: "CancelReleaseWindowCommand",
            bookReleaseWindow: "BookReleaseWindowCommand",
            updateReleaseWindow: "UpdateReleaseWindowCommand",

            //release plan
            reAssignReleaseChanges: "ReAssignReleaseChangesToReleaseCommand",
            reapproveTickets: "ReapproveTicketsCommand",
            updateReleaseRepository: "UpdateReleaseRepositoryCommand",
            reloadRepositories: "LoadReleaseRepositoriesCommand",
            updateReleaseTask: "UpdateReleaseTaskCommand",
            updateReleaseTasksOrder: "UpdateReleaseTasksOrderCommand",
            updateReleaseJob: "UpdateReleaseJobCommand",


            //release execution
            checkQaStatus: "CheckQaStatusCommand",
            rePopulateMeasurements: "RePopulateDeploymentMeasurementsCommand",

            //productRegistration
            createProductRequestType: "CreateProductRequestTypeCommand",
            updateProductRequestType: "UpdateProductRequestTypeCommand",
            deleteProductRequestType: "DeleteProductRequestTypeCommand",
            createProductRequestGroup: "CreateProductRequestGroupCommand",
            updateProductRequestGroup: "UpdateProductRequestGroupCommand",
            deleteProductRequestGroup: "DeleteProductRequestGroupCommand",
            createProductRequestTask: "CreateProductRequestTaskCommand",
            updateProductRequestTask: "UpdateProductRequestTaskCommand",
            deleteProductRequestTask: "DeleteProductRequestTaskCommand",
            createProductRequestRegistration: "CreateProductRequestRegistrationCommand",
            updateProductRequestRegistration: "UpdateProductRequestRegistrationCommand",
            deleteProductRequestRegistration: "DeleteProductRequestRegistrationCommand",

            //Business Rules
            saveRule: "SaveRuleCommand",
            saveCommandPermissionRule: "SaveCommandPermissionRuleCommand",
            saveQueryPermissionRule: "SaveQueryPermissionRuleCommand",
            deletePermissionRule: "DeletePermissionRuleCommand",

            //subscriptions
            updateUserNotificationSubscriptions: "UpdateNotificationSubscriptionsCommand",
            //api descriptions
            updateApiDescription: "UpdateApiDescriptionCommand",

            //access controll
            updateAccountPackages: "UpdateAccountPackagesCommand",

            //plugins
            assignGlobalPlugin: "AssignGlobalPluginCommand",
            assignPackagePlugin: "AssignPackagePluginCommand",
            updatePluginGlobalConfiguration: "UpdatePluginGlobalConfigurationCommand",
            updatePluginPackageConfiguration: "UpdatePluginPackageConfigurationCommand",
            updatePluginGlobalConfigurationEntity: "UpdatePluginGlobalConfigurationEntityCommand",
            updatePluginPackageConfigurationEntity: "UpdatePluginPackageConfigurationEntityCommand",

            //confirmation
            confirmReleaseTask: "ConfirmReleaseTaskReceiptCommand"
        };

        //usage: remiapi.get.#QUERY_NAME# ([query_arguments])
        service.get = initQueries();

        //usage: remiapi.post.#COMMAND_NAME# (data_object)
        service.post = initCommands();

        return service;

        //common
        function getApiPath() {
            var port = $location.port();
            return $location.protocol() + "://" + $location.host() + (port === 80 ? "" : ":" + port) + "/api";
        }

        function initQueries() {
            var parsed = {};
            var names = getKeys(service.queryDefinitions);
            for (var i = 0; i < names.length; i++) {
                var name = names[i];

                var queryWrapper = function (requestName) {
                    return function () {
                        var query = replaceArguments(service.queryDefinitions[requestName], arguments);

                        if (/\{\d+\?\}/.test(query))
                            query = query.replace(new RegExp("/\{\\d+\\?\\}", "g"), "");
                        if (/\{\d+\??\}/.test(query))
                            throw ("Not all arguments were passed to the request");

                        return execRequest(query);
                    };
                };
                parsed[name] = queryWrapper(name);
            }

            return parsed;
        }

        function initCommands() {
            var parsed = {};
            var names = getKeys(service.commandDefinitions);
            for (var i = 0; i < names.length; i++) {
                var name = names[i];

                var commandWrapper = function (requestName) {
                    return function () {
                        var command = service.commandDefinitions[requestName];

                        var commandId = newGuid();
                        if (arguments && arguments.length === 1)
                            return executeCommand(command, commandId, arguments[0]);

                        throw ("Invalid arguments were passed to the command");
                    };
                };

                parsed[name] = commandWrapper(name);
            }

            return parsed;
        }

        function replaceArguments(query, args) {
            var q = query;
            for (var j = 0; j < args.length; j++) {
                var expression = "\\{" + j + "\\??\\}";
                q = q.replace(new RegExp(expression, "g"), args[j]);
            }
            return q;
        }

        function getKeys(obj) {
            if (!obj) return [];

            var keys = [];
            for (var key in obj) {
                if (obj.hasOwnProperty(key)) {
                    keys.push(key);
                }
            }
            return keys;
        }

        //release window

        function getRelease(releaseId) {
            return execRequest("/releases/" + releaseId);
        }

        function getSignOffs(releaseId) {
            return execRequest("releases/signers/" + releaseId);
        }

        function releaseContentData(releaseId) {
            return execRequest("/releases/" + releaseId + "/content");
        }

        function ticketRisk() {
            return execRequest("/releases/ticketRisk");
        }

        function releaseChanges(releaseId) {
            return execRequest("/releases/" + releaseId + "/changes");
        }

        function releaseCalendar(startDate, endDate) {
            return execRequest("/releases/search/" + startDate + "/" + endDate);
        }

        function releaseEnums() {
            return execRequest("/releases/releaseEnums");
        }

        function getMetrics(windowId) {
            return execRequest("/metrics/release/execution/" + windowId);
        }

        function followingRelease(product) {
            return execRequest("/releases/search/upcomingRelease/" + product);
        }

        function getNearReleases(product) {
            return execRequest("/releases/search/nearReleases/" + product);
        }

        function getReleaseTask(taskId) {
            return execRequest("/releases/tasks/" + taskId);
        }

        function getReleaseTasks(releaseId) {
            return execRequest("/releases/" + releaseId + "/tasks");
        }

        function searchUsers(criteria) {
            return execRequest("/accounts/search/" + criteria);
        }

        function createReleaseTask(data) {
            var commandId = newGuid();
            return executeCommand("CreateReleaseTaskCommand", commandId, data);
        }

        function updateReleaseTask(data) {
            var commandId = newGuid();
            return executeCommand("UpdateReleaseTaskCommand", commandId, data);
        }

        function deleteReleaseTask(data) {
            var commandId = newGuid();
            return executeCommand("DeleteReleaseTaskCommand", commandId, data);
        }

        function updateReleaseTasksOrder(data) {
            var commandId = newGuid();
            return executeCommand("UpdateReleaseTasksOrderCommand", commandId, data);
        }

        function completeReleaseTask(data) {
            var commandId = newGuid();
            return executeCommand("CompleteReleaseTaskCommand", commandId, data);
        }

        function closeReleaseWindow(data) {
            var commandId = newGuid();
            return executeCommand("CloseReleaseCommand", commandId, data);
        }

        function approveRelease(data) {
            var commandId = newGuid();
            return executeCommand("ApproveReleaseCommand", commandId, data);
        }

        function signOffRelease(data) {
            var commandId = newGuid();
            return executeCommand("SignOffReleaseCommand", commandId, data);
        }

        function addSignOffs(data) {
            var commandId = newGuid();
            return executeCommand("AddPeopleToSignOffReleaseCommand", commandId, data);
        }

        function removeSignOff(data) {
            var commandId = newGuid();
            return executeCommand("RemoveSignOffCommand", commandId, data);
        }

        function updateMetrics(data) {
            var commandId = newGuid();
            return executeCommand("UpdateMetricsCommand", commandId, data);
        }

        function saveReleaseIssues(data) {
            var commandId = newGuid();
            return executeCommand("SaveReleaseIssuesCommand", commandId, data);
        }

        // Dictionaries


        function getReleaseTaskTypes() {
            return execRequest("/dictionaries/releaseTask/types");
        }
        function getReleaseTaskRisks() {
            return execRequest("/dictionaries/releaseTask/risks");
        }
        function getReleaseTaskEnvironments() {
            return execRequest("/dictionaries/releaseTask/environments");
        }

        function getReleaseTrack() {
            return execRequest("/dictionaries/product/releaseTracks");
        }

        //Configuration

        function getProducts() {
            return execRequest("/configuration/products");
        }
        function getCommands() {
            return execRequest("/configuration/commands");
        }
        function getQueries() {
            return execRequest("/configuration/queries");
        }
        function addQueryToRole(data) {
            var commandId = newGuid();
            return executeCommand("AddQueryToRoleCommand", commandId, data);
        }
        function removeQueryFromRole(data) {
            var commandId = newGuid();
            return executeCommand("RemoveQueryFromRoleCommand", commandId, data);
        }

        // Release Plan

        function checkList(releaseWindowId) {
            return execRequest("/checkList/" + releaseWindowId);
        }

        function releaseParticipants(releaseWindowId) {
            return execRequest("/releaseParticipants/" + releaseWindowId);
        }

        function additionalCheckListQuestion(releaseWindowId) {
            return execRequest("/checklist/additionalQuestions/" + releaseWindowId);
        }

        function addCheckListQuestions(data) {
            var commandId = newGuid();
            return executeCommand("AddCheckListQuestionsCommand", commandId, data);
        }

        function removeCheckListQuestion(data) {
            var commandId = newGuid();
            return executeCommand("RemoveCheckListQuestionCommand", commandId, data);
        }

        function getReleaseApprovers(releaseWindowId) {
            return execRequest("/releases/" + releaseWindowId + "/approvers");
        }

        function addReleaseApprovers(data) {
            var commandId = newGuid();
            return executeCommand("AddReleaseApproversCommand", commandId, data);
        }

        function removeReleaseApprover(data) {
            var commandId = newGuid();
            return executeCommand("RemoveReleaseApproverCommand", commandId, data);
        }

        function confirmReleaseTaskReview(data) {
            var commandId = newGuid();
            return executeCommand("ConfirmReleaseTaskReviewCommand", commandId, data);
        }

        function confirmReleaseTaskImplementation(data) {
            var commandId = newGuid();
            return executeCommand("ConfirmReleaseTaskImplementationCommand", commandId, data);

        }

        //command tracking START

        function commandStatus(commandId) {
            return execRequest("/commands/" + commandId + "/state");
        }

        function executeCommand(commandName, commandId, data) {
            return execRequest("/commands/deliver/" + commandName, data, "POST", { CommandId: commandId });
        }

        //command tracking END


        //admin START

        function checkSession() {
            return execRequest("/session/check");
        }

        function getAccounts() {
            return execRequest("/accounts");
        }

        function getAccountsByProduct(product) {
            return execRequest("/accounts/product/" + product);
        }

        function getRoles() {
            return execRequest("/accounts/roles");
        }

        function createRole(data) {
            var commandId = newGuid();
            return executeCommand("CreateRoleCommand", commandId, data);
        }

        function updateRole(data) {
            var commandId = newGuid();
            return executeCommand("UpdateRoleCommand", commandId, data);
        }

        function deleteRole(data) {
            var commandId = newGuid();
            return executeCommand("DeleteRoleCommand", commandId, data);
        }

        function createAccount(data) {
            var commandId = newGuid();
            return executeCommand("CreateAccountCommand", commandId, data);
        }

        function defaultProductForNewUser(data) {
            var commandId = newGuid();
            return executeCommand("SetDefaultProductForNewlyRegisteredUserCommand", commandId, data);
        }

        function checkAccounts(data) {
            var commandId = newGuid();
            return executeCommand("CheckAccountsCommand", commandId, data);
        }

        function updateAccount(data) {
            var commandId = newGuid();
            return executeCommand("UpdateAccountCommand", commandId, data);
        }

        //admin END

        //business rule START

        function testBusinessRule(data) {
            return execRequest("rule/test", data, "POST");
        }

        function executeBusinessRule(group, rule, parameters) {
            return execRequest("rule/" + group + "/" + rule, parameters, "POST");
        }

        //business rule END

        //internal helpers

        function execRequest(relativePath, data, method, headers) {
            var deferred = $q.defer();

            var m = method || (typeof data != "undefined" && data != null ? "POST" : "GET");
            if (!headers)
                headers = {};
            headers["Content-Type"] = "application/json; charset=utf-8";
            var request = $http({ url: getAbsolutePath(relativePath), method: m, data: data, headers: headers });

            request.success(function (response, status) {
                if (headers.CommandId) {
                    waitForCommand(deferred, headers.CommandId); // if header contains CommandId then the app performs a command state polling
                }
                else if (status !== 200) {
                    deferred.reject(status);
                } else {
                    deferred.resolve(response);
                }
            });
            request.error(function (response, status) {
                if (status === 401) {
                    common.showAccessDeniedMessage("You do not have permissions to perform this action");
                }
                else if (status === 403) {
                    common.showAccessDeniedMessage("User is not logged in or session expired");
                    $rootScope.$broadcast(config.events.sessionExpired);
                }
                else if (status === 406) {
                    common.showValidationError(response);
                }
                deferred.reject(status);
            });

            return deferred.promise;
        }

        function getAbsolutePath(relative) {
            var url = getApiPath();

            if (relative && ("" !== ("" + relative))) {
                if (relative.substr(0, 1) === "/")
                    url += relative;
                else
                    url += "/" + relative;
            }

            logger.console("API url=" + url);

            return url;
        }

        function doCommandPoll(commandId, callback, delay, immediate) {
            var defaultDelay = 1000;
            var restrictPollCount = 30;
            var restrictPollCountForNotRegister = 5;

            delay = delay || defaultDelay;
            if (commandPoll[commandId]) {
                $timeout.cancel(commandPoll[commandId].t);

                if (commandPoll[commandId].cnt >= restrictPollCount) {
                    logger.console("Forbid call (#" + commandPoll[commandId].cnt + ") for commandId=" + commandId + " (timeout)");
                    return false;

                } else if (restrictPollCountForNotRegister > 0
                    && commandPoll[commandId].cnt >= restrictPollCountForNotRegister
                    && commandPoll[commandId].lastState === commandStates.notRegistered) {
                    logger.console("Forbid call (#" + commandPoll[commandId].cnt + ") for commandId=" + commandId + " (not registered)");
                    return false;

                } else if (restrictPollCountForNotRegister > 0
                    && commandPoll[commandId].cnt >= restrictPollCountForNotRegister
                    && commandPoll[commandId].lastState === commandStates.notRegistered) {
                    logger.console("Forbid call (#" + commandPoll[commandId].cnt + ") for commandId=" + commandId + " (not registered)");
                    return false;

                } else {
                    commandPoll[commandId].cnt++;
                    commandPoll[commandId].t = $timeout(callback, delay);

                    logger.console("Check status (#" + commandPoll[commandId].cnt + ") for commandId=" + commandId);
                }
            } else {
                logger.console("Check status (#1) for commandId=" + commandId);

                commandPoll[commandId] = { cnt: 1, t: $timeout(callback, immediate ? 1 : delay) };
            }

            return true;
        }

        function cleanCommandPoll(commandId) {
            $timeout.cancel(commandPoll[commandId].t);
            commandPoll[commandId] = undefined;
        }

        function waitForCommand(deferred, commandId) {
            var returnRejected = function (data) {
                if (data && data.Details)
                    deferred.reject({ Details: data.Details });
                else
                    deferred.reject(0);
            };

            var callback = function () {
                service.commandStatus(commandId)
                    .then(function (data) {
                        if (commandPoll[commandId]) {
                            commandPoll[commandId].lastState = data.State;
                        }

                        if (data.State !== commandStates.success && data.State !== commandStates.failed) {
                            if (!doCommandPoll(commandId, callback)) {
                                returnRejected(data);
                            }
                        } else {
                            if (data.State === commandStates.success)
                                deferred.resolve(data.State);
                            else {
                                returnRejected(data);
                            }

                            cleanCommandPoll(commandId);
                        }
                    }, function () {
                        deferred.reject(0);
                    });
            };

            doCommandPoll(commandId, callback, undefined, true);
        }
    };

})();
