describe("FocusModalEditor Directive ", function () {
    var scope, container, element, html, compiled, compile;

    beforeEach(module("app", function ($provide) { $provide.value("authService", {}) }));
    beforeEach(inject(function ($compile, $rootScope) {
        html = '<div id="element-id" data-focus-modal-editor="textarea"</div>';
        scope = $rootScope.$new();
        compile = $compile;
    }));

    function prepareDirective(s) {
        container = angular.element(html);
        compiled = compile(container);
        element = compiled(s);
        s.$digest();
    }

    /***********************************************************************************************************************/

    it("should init modal shown event, when activated", function () {
        spyOn($.fn, 'on');
        spyOn(window, '$');
        $.and.callThrough();

        prepareDirective(scope);

        expect($.calls.count()).toEqual(1);
        expect($.fn.on).toHaveBeenCalledWith('shown.bs.modal', jasmine.any(Function));
    });

    it("should focus textarea, when modal shown", function () {
        spyOn($.fn, 'on');
        spyOn($.fn, 'focus');
        spyOn($.fn, 'find');
        spyOn(window, '$');
        $.and.callThrough();
        $.fn.find.and.callThrough();
        $.fn.on.and.callFake(function (name, func) {
            func({ currentTarger: {} });
        });

        prepareDirective(scope);

        expect($.calls.count()).toEqual(2);
        expect($.fn.find).toHaveBeenCalledWith('textarea');
        expect($.fn.focus).toHaveBeenCalled();
    });

    it("should handle destroy event, when initialized", function () {
        spyOn($.fn, 'off');

        prepareDirective(scope);
        scope.$broadcast('$destroy', {});

        expect($.fn.off.calls.count()).toEqual(1);
        expect($.fn.off).toHaveBeenCalledWith('shown.bs.modal');
    });
});

