describe("Rules Controller", function () {
    var sut, mocks, logger, activateDefer, getRulesDefer;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope, $controller) {
        activateDefer = $q.defer();
        getRulesDefer = $q.defer();
        mocks = {
            $scope: $rootScope.$new(),
            $rootScope: $rootScope,
            common: {
                logger: window.jasmine.createSpyObj('logger', ['getLogger']),
                activateController: window.jasmine.createSpy('activateController'),
                handleEvent: window.jasmine.createSpy('handleEvent'),
                $q: $q
            },
            rulesService: jasmine.createSpyObj('rulesService', ['getRules', 'getRule', 'testBusinessRule', 'saveRule'])
        };
        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.common.activateController.and.returnValue(activateDefer.promise);
        mocks.rulesService.getRules.and.returnValue(getRulesDefer.promise);

        sut = $controller('rules', mocks);
    }));

    it("should call initialization methods, when created", function () {

        var result = { data: 'some data' };
        expect(sut).toBeDefined();
        activateDefer.resolve();
        getRulesDefer.resolve(result);
        mocks.$scope.$digest();

        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('rules');
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), 'rules');
        expect(mocks.rulesService.getRules).toHaveBeenCalled();
        expect(logger.console).toHaveBeenCalledWith('Activated Rules View');
        expect(sut.ruleGroups).toEqual(result);
    });

    it("should open modal and get rule, when rule is edited", function () {
        var rule = { ExternalId: "rule id" };
        var ruleDesc = { data: 'some data' };
        spyOn($.fn, 'modal');
        var defer = mocks.common.$q.defer();
        mocks.rulesService.getRule.and.returnValue(defer.promise);

        sut.editRule(rule);
        defer.resolve(ruleDesc);
        mocks.$scope.$digest();

        expect(sut.currentRuleView).toEqual(rule);
        expect(sut.currentRule).toEqual(ruleDesc);
        expect(mocks.rulesService.getRule).toHaveBeenCalledWith(rule.ExternalId);
        expect($.fn.modal).toHaveBeenCalledWith(jasmine.objectContaining({ backdrop: 'static', keyboard: true }));
    });

    it("should open modal, try get query rule and hide model, when failed", function () {
        var rule = { ExternalId: "rule id" };
        spyOn($.fn, 'modal');
        var defer = mocks.common.$q.defer();
        mocks.rulesService.getRule.and.returnValue(defer.promise);

        sut.editRule(rule);
        defer.reject();
        mocks.$scope.$digest();

        expect(sut.currentRuleView).toEqual(rule);
        expect(sut.currentRule).toBeUndefined();
        expect(mocks.rulesService.getRule).toHaveBeenCalledWith(rule.ExternalId);
        expect($.fn.modal).toHaveBeenCalledWith(jasmine.objectContaining({ backdrop: 'static', keyboard: true }));
        expect($.fn.modal).toHaveBeenCalledWith('hide');
    });

    it("should test rule, when invoked", function () {
        sut.currentRule = { data: "some data" };

        sut.testBusinessRule();

        expect(mocks.rulesService.testBusinessRule).toHaveBeenCalledWith(sut.currentRule);
    });

    it("should call saveRule and hide modal and update code beggining, when invoked", function () {
        var currentRule = { Script: "01234567890123456789012345678901234567890123456789" };
        var currentRuleView = { CodeBeggining: "some data" };
        sut.currentRule = currentRule;
        sut.currentRuleView = currentRuleView;
        spyOn($.fn, 'modal');
        var defer = mocks.common.$q.defer();
        mocks.rulesService.saveRule.and.returnValue(defer.promise);

        sut.saveRule();
        defer.resolve();
        mocks.$scope.$digest();

        expect(mocks.rulesService.saveRule).toHaveBeenCalledWith(currentRule);
        expect($.fn.modal).toHaveBeenCalledWith('hide');
        expect(sut.currentRuleView).toBeUndefined();
        expect(sut.currentRule).toBeUndefined();
        expect(currentRuleView.CodeBeggining).toEqual('012345678901234567890123456789 ...');
    });
});

var rolesData = {
    "Roles": [
        {
            "ExternalId": "role1",
            "Name": "name1",
            "Description": "description1"
        }, {
            "ExternalId": "role2",
            "Name": "name2",
            "Description": "description2"
        }, {
            "ExternalId": "role3",
            "Name": "name3",
            "Description": "description3"
        }, {
            "ExternalId": "role4",
            "Name": "name4",
            "Description": "description4"
        }, {
            "ExternalId": "role5",
            "Name": "name5",
            "Description": "description5"
        }, {
            "ExternalId": "role6",
            "Name": "name6",
            "Description": "description6"
        }
    ]
};
