describe("RuleEditor Directive ", function () {
    var scope, container, element, html, compiled, compile;
    var templateCache, httpBackend, timeout;

    beforeEach(module('app'));
    beforeEach(inject(function ($compile, $rootScope, $templateCache, _$httpBackend_, _$timeout_) {
        html = '<div data-rule-editor="" data-rule="currentRule" data-is-busy="isBusy"></div>';
        var controllerHtml = "<div class=\"container\"><div data-ng-show=\"viewMode == 'code'\" class=\"cssFade\"></div></div>";

        spyOn(ace, 'edit');

        scope = $rootScope.$new();
        compile = $compile;
        templateCache = $templateCache;
        httpBackend = _$httpBackend_;
        timeout = _$timeout_;

        templateCache.put("ruleEditor.html", controllerHtml);
        httpBackend.when('GET', 'app/common/directives/tmpls/ruleEditor.html').respond(controllerHtml);
        httpBackend.when('GET', 'app/releaseCalendar/releaseCalendar.html').respond('<div></div>');
    }));
    afterEach(function () {
        httpBackend.verifyNoOutstandingExpectation();
        httpBackend.verifyNoOutstandingRequest();
    });

    function prepareDirective(s) {
        container = angular.element(html);
        compiled = compile(container);
        element = compiled(s);
        s.$digest();
        httpBackend.flush();
    }

    function buildEditorSpyObject() {
        var editor = jasmine.createSpyObj('editor', ['setTheme', 'getSession', 'setValue', 'clearSelection', 'on', 'setMode']);
        editor.getSession.and.returnValue({
            setMode: editor.setMode
        });
        return editor;
    }
    /***********************************************************************************************************************/

    it('should fillout element scope, when initialise', function () {
        scope.currentRule = { ExternalId: 'ExternalId ' };
        scope.isBusy = false;

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();

        expect(isolatedScope.isBusy).toEqual(true);
        expect(isolatedScope.ruleDesc).toEqual(scope.currentRule);
        expect(isolatedScope.viewMode).toEqual('code');
    });

    it('should fillout element scope, when initialise and Rule is wrapped in element', function () {
        scope.currentRule = { Rule : { ExternalId: 'ExternalId ' } };
        scope.isBusy = false;

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();

        expect(isolatedScope.isBusy).toEqual(true);
        expect(isolatedScope.ruleDesc).toEqual(scope.currentRule.Rule);
        expect(isolatedScope.viewMode).toEqual('code');
    });

    it('should not be busy, when rule is empty', function () {
        scope.isBusy = false;

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();

        expect(isolatedScope.isBusy).toEqual(false);
    });


    it('should desctroy all editors, when destroy invoked', function () {
        scope.currentRule = { ExternalId: 'ExternalId ' };
        scope.isBusy = false;

        prepareDirective(scope);
        var editor = jasmine.createSpyObj('editor', ['destroy']);
        var accountTestDataEditor = jasmine.createSpyObj('accountTestDataEditor', ['destroy']);
        var paramEditors = [
            jasmine.createSpyObj('paramEditors1', ['destroy']),
            jasmine.createSpyObj('paramEditors2', ['destroy']),
            jasmine.createSpyObj('paramEditors3', ['destroy'])
        ];
        var isolatedScope = element.isolateScope();
        isolatedScope.editor = editor;
        isolatedScope.accountTestDataEditor = accountTestDataEditor;
        isolatedScope.editors = paramEditors;

        isolatedScope.$broadcast('$destroy');

        expect(isolatedScope.editor).toBeUndefined();
        expect(isolatedScope.accountTestDataEditor).toBeUndefined();
        expect(isolatedScope.editors).toBeUndefined();
        expect(editor.destroy).toHaveBeenCalled();
        expect(accountTestDataEditor.destroy).toHaveBeenCalled();
        expect(paramEditors[0].destroy).toHaveBeenCalled();
        expect(paramEditors[1].destroy).toHaveBeenCalled();
        expect(paramEditors[2].destroy).toHaveBeenCalled();
    });

    it('should initialize editors, when timeout invoked', function () {
        scope.currentRule = {
            ExternalId: 'ExternalId ',
            Script: 'code',
            AccountTestData: { JsonData: '{ "data": "account test data" }' },
            Parameters: [
                { Name: 'parameter1', TestData: { JsonData: '{ "data": "test data 1" }' } },
                { Name: 'parameter2', TestData: { JsonData: '{ "data": "test data 2" }' } },
                { Name: 'parameter3', TestData: { JsonData: '{ "data": "test data 3" }' } }
            ]
        };
        scope.isBusy = false;
        ace.edit.and.callFake(function () {
            return buildEditorSpyObject();
        });

        prepareDirective(scope);
        timeout.flush();
        var isolatedScope = element.isolateScope();

        expect(isolatedScope.editor).toBeDefined();
        expect(isolatedScope.accountTestDataEditor).toBeDefined();
        expect(isolatedScope.editors).toBeDefined();
        expect(isolatedScope.editor.setTheme).toHaveBeenCalledWith("ace/theme/github");
        expect(isolatedScope.editor.getSession).toHaveBeenCalled();
        expect(isolatedScope.editor.setMode).toHaveBeenCalledWith("ace/mode/csharp");
        expect(isolatedScope.editor.setValue).toHaveBeenCalled();
        expect(isolatedScope.editor.clearSelection).toHaveBeenCalled();
        expect(isolatedScope.editor.on).toHaveBeenCalledWith("change", jasmine.any(Function));
        expect(isolatedScope.accountTestDataEditor.setTheme).toHaveBeenCalledWith("ace/theme/github");
        expect(isolatedScope.accountTestDataEditor.setMode).toHaveBeenCalledWith("ace/mode/json");
        expect(isolatedScope.editors.parameter1.setTheme).toHaveBeenCalledWith("ace/theme/github");
        expect(isolatedScope.editors.parameter1.setMode).toHaveBeenCalledWith("ace/mode/json");
        expect(isolatedScope.editors.parameter2.setTheme).toHaveBeenCalledWith("ace/theme/github");
        expect(isolatedScope.editors.parameter2.setMode).toHaveBeenCalledWith("ace/mode/json");
        expect(isolatedScope.editors.parameter3.setTheme).toHaveBeenCalledWith("ace/theme/github");
        expect(isolatedScope.editors.parameter3.setMode).toHaveBeenCalledWith("ace/mode/json");
        expect(isolatedScope.isBusy).toEqual(false);
    });

    it('should switch view mode, when switchViewMode called', function () {
        scope.currentRule = { ExternalId: 'ExternalId ' };
        scope.isBusy = false;
        prepareDirective(scope);
        var isolatedScope = element.isolateScope();

        isolatedScope.switchViewMode();

        expect(isolatedScope.viewMode).toEqual('testData');
    });

    it('should update ruleDesc, when rule has changed in scope', function () {
        scope.currentRule = { ExternalId: 'ExternalId ' };
        scope.isBusy = false;
        var newRule = { ExternalId: 'new external id' };
        prepareDirective(scope);
        var isolatedScope = element.isolateScope();

        scope.$apply(function () {
            scope.currentRule = newRule;
        });

        isolatedScope.switchViewMode();

        expect(isolatedScope.ruleDesc).toEqual(newRule);
    });
});

