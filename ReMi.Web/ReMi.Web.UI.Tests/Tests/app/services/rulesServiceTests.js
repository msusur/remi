describe("Rule Service", function () {
    var sut, mocks, logger;
    var $q;

    beforeEach(function () {
        mocks = {
            common: {
                logger: jasmine.createSpyObj("logger", ["getLogger"]),
                showErrorMessage: jasmine.createSpy("showErrorMessage"),
                showInfoMessage: jasmine.createSpy("showInfoMessage"),
                $q: jasmine.createSpyObj("$q", ["defer"])
            },
            remiapi: {
                get: jasmine.createSpyObj("remiapi.get", ["businessRuleByName", "generatedBusinessRule", "businessRules", "businessRuleById"]),
                post: jasmine.createSpyObj("remiapi.post", ["saveCommandPermissionRule", "saveQueryPermissionRule", "deletePermissionRule", "saveRule"]),
                testBusinessRule: jasmine.createSpy("testBusinessRule")
            },
            notifications: jasmine.createSpyObj("notifications", ["subscribe", "unsubscribe"])
        };
        logger = jasmine.createSpyObj("logger", ["console", "error"]);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.common.$q.defer.and.returnValue({
            promise: true,
            resolve: jasmine.createSpy("resolve")
        });

        module("app", function ($provide) {
            $provide.value("common", mocks.common);
            $provide.value("remiapi", mocks.remiapi);
            $provide.value("notifications", mocks.notifications);
        });

        inject(function (_rulesService_, _$q_, _$rootScope_) {
            $q = _$q_;
            mocks.$scope = _$rootScope_.$new(),
            mocks.$rootScope = _$rootScope_,
        mocks.common.$q.defer.and.callFake(function () { return $q.defer(); });
            sut = _rulesService_;
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("rulesService");
    });

    describe("getRules", function () {

        it("should call get businessRules from remiapi service, when getRules called", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.get.businessRules.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var expectedData = { Rules: "some data" };
            var resultData;

            var result = sut.getRules();
            queryDefer.resolve(expectedData);
            result.then(function (data) {
                resultData = data;
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(resultData).toEqual(expectedData.Rules);
            expect(resolvedCalled).toEqual(true);
            expect(rejectedCalled).toEqual(false);
        });

        it("should reject defer, when query call fail for some reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.get.businessRules.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var expectedData = { Exception: "some data" };
            var resultData;

            var result = sut.getRules();
            queryDefer.reject(expectedData);
            result.then(function (data) {
                resultData = data;
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage).toHaveBeenCalled();
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(expectedData));
            expect(resultData).toBeUndefined();
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should reject defer, when query call fail for validation reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.get.businessRules.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;

            var result = sut.getRules();
            queryDefer.reject(406);
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage.calls.any()).toEqual(false);
            expect(logger.error.calls.any()).toEqual(false);
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });
    });

    describe("getRule", function () {

        it("should getRule be rejected, when called with empty ruleId", function () {
            var result = sut.getRule();
            var resolvedCalled = false;
            var rejectedCalled = false;
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(logger.console).toHaveBeenCalledWith("ruleId cannot be null");
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should call get businessRuleById from remiapi service, when getRule called", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.get.businessRuleById.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var expectedData = { Rule: "some data" };
            var resultData;

            var result = sut.getRule("ruleId");
            queryDefer.resolve(expectedData);
            result.then(function (data) {
                resultData = data;
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(resultData).toEqual(expectedData.Rule);
            expect(resolvedCalled).toEqual(true);
            expect(rejectedCalled).toEqual(false);
        });

        it("should reject defer, when query call fail for some reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.get.businessRuleById.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var expectedData = { Exception: "some data" };
            var resultData;

            var result = sut.getRule("ruleId");
            queryDefer.reject(expectedData);
            result.then(function (data) {
                resultData = data;
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage).toHaveBeenCalled();
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(expectedData));
            expect(resultData).toBeUndefined();
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should reject defer, when query call fail for validation reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.get.businessRuleById.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;

            var result = sut.getRule("ruleId");
            queryDefer.reject(406);
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage.calls.any()).toEqual(false);
            expect(logger.error.calls.any()).toEqual(false);
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });
    });

    describe("getPermissionRule", function () {

        it("should getPermissionRule be rejected, when called with empty name", function () {
            var result = sut.getPermissionRule();
            var resolvedCalled = false;
            var rejectedCalled = false;
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(logger.console).toHaveBeenCalledWith("Name cannot be null");
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should call getBusinessRuleByName from remiapi service, when getPermissionRule called", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.get.businessRuleByName.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var expectedData = { Rule: "some data" };
            var resultData;

            var result = sut.getPermissionRule("Command");
            queryDefer.resolve(expectedData);
            result.then(function (data) {
                resultData = data;
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(resultData).toEqual(expectedData.Rule);
            expect(resolvedCalled).toEqual(true);
            expect(rejectedCalled).toEqual(false);
        });

        it("should reject defer, when query call fail for some reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.get.businessRuleByName.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var expectedData = { Exception: "some data" };
            var resultData;

            var result = sut.getPermissionRule("Command");
            queryDefer.reject(expectedData);
            result.then(function (data) {
                resultData = data;
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage).toHaveBeenCalled();
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(expectedData));
            expect(resultData).toBeUndefined();
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should reject defer, when query call fail for validation reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.get.businessRuleByName.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;

            var result = sut.getPermissionRule("Command");
            queryDefer.reject(406);
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage.calls.any()).toEqual(false);
            expect(logger.error.calls.any()).toEqual(false);
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });
    });

    describe("generateNewRule", function () {
        it("should generateNewRule be rejected, when called with empty name", function () {
            var result = sut.generateNewRule();
            var resolvedCalled = false;
            var rejectedCalled = false;
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(logger.console).toHaveBeenCalledWith("Name cannot be null");
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should generateNewRule be rejected, when called with empty namespace", function () {
            var result = sut.generateNewRule("name");
            var resolvedCalled = false;
            var rejectedCalled = false;
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(logger.console).toHaveBeenCalledWith("Namespace cannot be null");
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should call getGeneratedBusinessRule from remiapi service, when generateNewRule called", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.get.generatedBusinessRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var expectedData = { Rule: "some data" };
            var resultData;

            var result = sut.generateNewRule("Command", "Namespace");
            queryDefer.resolve(expectedData);
            result.then(function (data) {
                resultData = data;
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.remiapi.get.generatedBusinessRule).toHaveBeenCalledWith("Namespace", "Command");
            expect(resultData).toEqual(expectedData.Rule);
            expect(resolvedCalled).toEqual(true);
            expect(rejectedCalled).toEqual(false);
        });

        it("should reject defer, when query call fail for some reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.get.generatedBusinessRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var expectedData = { Exception: "some data" };
            var resultData;

            var result = sut.generateNewRule("Command", "Namespace");
            queryDefer.reject(expectedData);
            result.then(function (data) {
                resultData = data;
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage).toHaveBeenCalled();
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(expectedData));
            expect(resultData).toBeUndefined();
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should reject defer, when query call fail for validation reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.get.generatedBusinessRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;

            var result = sut.generateNewRule("Command", "Namespace");
            queryDefer.reject(406);
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage.calls.any()).toEqual(false);
            expect(logger.error.calls.any()).toEqual(false);
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });
    });

    describe("testBusinessRule", function () {

        it("should testBusinessRule be rejected, when called with empty rule", function () {
            var result = sut.testBusinessRule();
            var resolvedCalled = false;
            var rejectedCalled = false;
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(logger.console).toHaveBeenCalledWith("Rule cannot be null");
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should call testBusinessRule from remiapi service, when testBusinessRule called", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.testBusinessRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var expectedData = { Result: "result data" };
            var rule = { data: "some data" };
            var resultData;

            var result = sut.testBusinessRule(rule);
            queryDefer.resolve(expectedData);
            result.then(function (data) {
                resultData = data;
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.remiapi.testBusinessRule).toHaveBeenCalledWith(rule);
            expect(mocks.common.showInfoMessage).toHaveBeenCalledWith("Rule result", JSON.stringify(expectedData.Result, null, 4));
            expect(resultData).toEqual(expectedData.Result);
            expect(resolvedCalled).toEqual(true);
            expect(rejectedCalled).toEqual(false);
        });

        it("should reject defer, when query call fail for some reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.testBusinessRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var expectedData = { Exception: "some data" };
            var rule = { data: "some data" };
            var resultData;

            var result = sut.testBusinessRule(rule);
            queryDefer.reject(expectedData);
            result.then(function (data) {
                resultData = data;
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage).toHaveBeenCalled();
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(expectedData));
            expect(resultData).toBeUndefined();
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should reject defer, when query call fail for validation reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.testBusinessRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var rule = { data: "some data" };

            var result = sut.testBusinessRule(rule);
            queryDefer.reject(406);
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage.calls.any()).toEqual(false);
            expect(logger.error.calls.any()).toEqual(false);
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });
    });

    describe("saveRule", function () {

        it("should saveRule be rejected, when called with empty rule", function () {
            var result = sut.saveRule();
            var resolvedCalled = false;
            var rejectedCalled = false;
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(logger.console).toHaveBeenCalledWith("Rule cannot be null");
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should call saveRule from remiapi service, when saveRule called with command type", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.post.saveRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var rule = { data: "some data" };

            var result = sut.saveRule(rule);
            queryDefer.resolve();
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.remiapi.post.saveRule).toHaveBeenCalledWith(jasmine.objectContaining({
                Rule: rule
            }));
            expect(resolvedCalled).toEqual(true);
            expect(rejectedCalled).toEqual(false);
        });

        it("should reject defer, when query call fail for some reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.post.saveRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var expectedData = { Exception: "some data" };
            var rule = { data: "some data" };

            var result = sut.saveRule(rule);
            queryDefer.reject(expectedData);
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage).toHaveBeenCalled();
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(expectedData));
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should reject defer, when query call fail for validation reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.post.saveRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var rule = { data: "some data" };

            var result = sut.saveRule(rule);
            queryDefer.reject(406);
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage.calls.any()).toEqual(false);
            expect(logger.error.calls.any()).toEqual(false);
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });
    });

    describe("savePermissionRule", function () {

        it("should savePermissionRule be rejected, when called with empty rule", function () {
            var result = sut.savePermissionRule();
            var resolvedCalled = false;
            var rejectedCalled = false;
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(logger.console).toHaveBeenCalledWith("Rule and messageId cannot be null");
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });
        it("should savePermissionRule be rejected, when called with empty type", function () {
            var result = sut.savePermissionRule("rule");
            var resolvedCalled = false;
            var rejectedCalled = false;
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(logger.console).toHaveBeenCalledWith("Rule and messageId cannot be null");
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });
        it("should savePermissionRule be rejected, when called with empty messageId", function () {
            var result = sut.savePermissionRule("rule", "type");
            var resolvedCalled = false;
            var rejectedCalled = false;
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(logger.console).toHaveBeenCalledWith("Rule and messageId cannot be null");
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should call saveCommandPermissionRule from remiapi service, when savePermissionRule called with command type", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.post.saveCommandPermissionRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var rule = { data: "some data" };
            var type = "command";
            var messageId = "messgeId";

            var result = sut.savePermissionRule(rule, type, messageId);
            queryDefer.resolve();
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.remiapi.post.saveCommandPermissionRule).toHaveBeenCalledWith(jasmine.objectContaining({
                Rule: rule,
                CommandId: messageId
            }));
            expect(resolvedCalled).toEqual(true);
            expect(rejectedCalled).toEqual(false);
        });

        it("should call saveQueryPermissionRule from remiapi service, when savePermissionRule called with command type", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.post.saveQueryPermissionRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var rule = { data: "some data" };
            var type = "query";
            var messageId = "messgeId";

            var result = sut.savePermissionRule(rule, type, messageId);
            queryDefer.resolve();
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.remiapi.post.saveQueryPermissionRule).toHaveBeenCalledWith(jasmine.objectContaining({
                Rule: rule,
                QueryId: messageId
            }));
            expect(resolvedCalled).toEqual(true);
            expect(rejectedCalled).toEqual(false);
        });

        it("should reject defer, when query call fail for some reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.post.saveQueryPermissionRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var expectedData = { Exception: "some data" };
            var rule = { data: "some data" };
            var type = "query";
            var messageId = "messgeId";

            var result = sut.savePermissionRule(rule, type, messageId);
            queryDefer.reject(expectedData);
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage).toHaveBeenCalled();
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(expectedData));
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should reject defer, when query call fail for validation reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.post.saveQueryPermissionRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var rule = { data: "some data" };
            var type = "query";
            var messageId = "messgeId";

            var result = sut.savePermissionRule(rule, type, messageId);
            queryDefer.reject(406);
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage.calls.any()).toEqual(false);
            expect(logger.error.calls.any()).toEqual(false);
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });
    });

    describe("deleteRule", function () {

        it("should deleteRule be rejected, when called with empty ruleId", function () {
            var result = sut.deleteRule();
            var resolvedCalled = false;
            var rejectedCalled = false;
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(logger.console).toHaveBeenCalledWith("RuleId cannot be null");
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should call deletePermissionRule from remiapi service, when deleteRule called", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.post.deletePermissionRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var ruleId = "ruleId";

            var result = sut.deleteRule(ruleId);
            queryDefer.resolve();
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.remiapi.post.deletePermissionRule).toHaveBeenCalledWith(jasmine.objectContaining({ RuleId: ruleId}));
            expect(resolvedCalled).toEqual(true);
            expect(rejectedCalled).toEqual(false);
        });

        it("should reject defer, when query call fail for some reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.post.deletePermissionRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var expectedData = { Exception: "some data" };
            var ruleId = "ruleId";

            var result = sut.deleteRule(ruleId);
            queryDefer.reject(expectedData);
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage).toHaveBeenCalled();
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(expectedData));
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });

        it("should reject defer, when query call fail for validation reason", function () {
            var queryDefer = $q.defer();
            mocks.remiapi.post.deletePermissionRule.and.returnValue(queryDefer.promise);
            var resolvedCalled = false;
            var rejectedCalled = false;
            var ruleId = "ruleId";

            var result = sut.deleteRule(ruleId);
            queryDefer.reject(406);
            result.then(function () {
                resolvedCalled = true;
            }, function () {
                rejectedCalled = true;
            });
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage.calls.any()).toEqual(false);
            expect(logger.error.calls.any()).toEqual(false);
            expect(resolvedCalled).toEqual(false);
            expect(rejectedCalled).toEqual(true);
        });
    });
});

