describe("AceEditor Directive ", function () {
    var scope, container, element, html, compiled, compile;
    var templateCache, injector, httpBackend, timeout;

    beforeEach(module("app", function ($provide) { $provide.value("authService", {}) }));
    beforeEach(inject(function ($compile, $rootScope, $templateCache, $injector, _$httpBackend_, _$timeout_) {
        html = '<div data-ace-editor="" data-ng-model="model" data-mode="csharp" data-ng-change="onChange(data)" data-ng-disabled="isReadOnly"></div>';

        spyOn(ace, "edit");

        scope = $rootScope.$new();
        compile = $compile;
        templateCache = $templateCache;
        injector = $injector;
        httpBackend = _$httpBackend_;
        timeout = _$timeout_;

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
        var editor = jasmine.createSpyObj("editor", ["setTheme", "getSession", "setValue", "clearSelection", "on", "setMode", "setReadOnly"]);
        editor.getSession.and.returnValue({
            setMode: editor.setMode
        });
        return editor;
    }
    /***********************************************************************************************************************/

    it("should fillout element scope, when initialise", function () {
        scope.model = { ExternalId: "ExternalId " };
        scope.onChange = function (data) { };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();

        expect(isolatedScope.ngModel).toEqual(scope.model);
        expect(isolatedScope.mode).toEqual("csharp");
        expect(isolatedScope.ngChange).toEqual(jasmine.any(Function));
        expect(isolatedScope.init).toEqual(jasmine.any(Function));
        expect(isolatedScope.destroy).toEqual(jasmine.any(Function));
    });

    it("should destroy editor, when destroy directive invoked", function () {
        prepareDirective(scope);
        var editor = jasmine.createSpyObj("editor", ["destroy"]);

        var isolatedScope = element.isolateScope();
        isolatedScope.editor = editor;

        isolatedScope.$broadcast("$destroy");

        expect(isolatedScope.editor).toBeUndefined();
        expect(editor.destroy).toHaveBeenCalled();
    });

    it("should initialize editors, when timeout invoked", function () {
        scope.model = "{}";
        ace.edit.and.callFake(function () {
            return buildEditorSpyObject();
        });

        prepareDirective(scope);
        timeout.flush();
        var isolatedScope = element.isolateScope();

        expect(isolatedScope.editor).toBeDefined();
        expect(isolatedScope.editor.setTheme).toHaveBeenCalledWith("ace/theme/github");
        expect(isolatedScope.editor.getSession).toHaveBeenCalled();
        expect(isolatedScope.editor.setMode).toHaveBeenCalledWith("ace/mode/csharp");
        expect(isolatedScope.editor.setValue).toHaveBeenCalled();
        expect(isolatedScope.editor.clearSelection).toHaveBeenCalled();
        expect(isolatedScope.editor.on).toHaveBeenCalledWith("change", jasmine.any(Function));
        expect(isolatedScope.editor.setReadOnly).toHaveBeenCalledWith(false);
    });

    it("should update model, when model has changed in scope", function () {
        scope.model = "{}";
        prepareDirective(scope);
        var editor = jasmine.createSpyObj("editor", ["destroy"]);
        var isolatedScope = element.isolateScope();
        isolatedScope.editor = editor;
        spyOn(isolatedScope, "init");

        scope.$apply(function () {
            scope.model = "class TestClass {}";
        });

        expect(isolatedScope.ngModel).toEqual("class TestClass {}");
        expect(isolatedScope.editor).toBeUndefined();
        expect(editor.destroy).toHaveBeenCalled();
        expect(isolatedScope.init).toHaveBeenCalled();
    });

    it("should set read-only mode, when read-only is set at start", function () {
        scope.model = "{}";
        scope.isReadOnly = true;
        ace.edit.and.callFake(function () {
            return buildEditorSpyObject();
        });

        prepareDirective(scope);
        timeout.flush();
        var isolatedScope = element.isolateScope();

        expect(isolatedScope.editor.setReadOnly).toHaveBeenCalledWith(true);
    });

    it("should set read-only mode, when isReadOnly property has changed in parent scope", function () {
        scope.model = "{}";
        scope.isReadOnly = false;
        ace.edit.and.callFake(function () {
            return buildEditorSpyObject();
        });

        prepareDirective(scope);
        timeout.flush();
        var isolatedScope = element.isolateScope();

        expect(isolatedScope.editor.setReadOnly).toHaveBeenCalledWith(false);
        expect(isolatedScope.editor.setReadOnly).not.toHaveBeenCalledWith(true);

        scope.isReadOnly = true;
        scope.$digest();

        expect(isolatedScope.editor.setReadOnly).toHaveBeenCalledWith(true);
    });
});

