describe("Dragable Row Directive ", function () {
    var scope, container, element, html, compiled, compile;

    beforeEach(module('app'));
    beforeEach(inject(function ($compile, $rootScope) {
        html = '<div id="element-id" data-draggable-row=""'
            + ' data-draggable-elem-selector=".draggable"'
            + ' data-drop-area-selector=".drop-area">'
                + '<div class="draggable"></div>'
                + '<div class="drop-area" style="display: none;"></div>'
            + '</div>';
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

    it('should add draggable attribute to draggable element, when initialise', function () {
        prepareDirective(scope);
        expect(element.find('.draggable').attr('draggable')).toBe('true');
    });

    it('should prevent default, when dragging over allowed element', function () {
        prepareDirective(scope);
        var event = $.Event('dragover');
        event.preventDefault = window.jasmine.createSpy('preventDefault');
        event.originalEvent = {
            dataTransfer: {
                types: {},
                getData: window.jasmine.createSpy('getData').and.returnValue("element-id")
            }
        };
        element.trigger(event);
        expect(event.preventDefault).toHaveBeenCalled();
    });

    it('should show drop area, when drag enter allowed element', function () {
        prepareDirective(scope);
        var event = $.Event('dragenter');
        event.originalEvent = {
            dataTransfer: {
                types: {},
                getData: window.jasmine.createSpy('getData').and.returnValue("element-id")
            }
        };
        element.trigger(event);
        expect(element.find('.drop-area').css('display')).not.toEqual('none');
        expect(event.originalEvent.dataTransfer.getData).toHaveBeenCalledWith('draggedRow');
    });

    it('should call scope onDragEnd, when dragging ends', function () {
        prepareDirective(scope);

        var event = $.Event('dragend');
        var isolateScope = element.isolateScope();
        spyOn(isolateScope, 'onDragEnd');

        element.trigger(event);

        expect(isolateScope.onDragEnd).toHaveBeenCalled();
    });

    it('should set drag data and call scope onDrag, when drag starts', function () {
        prepareDirective(scope);

        var event = $.Event('dragstart');
        event.originalEvent = {
            dataTransfer: {
                setData: window.jasmine.createSpy('setData')
            }
        };
        var isolateScope = element.isolateScope();
        spyOn(isolateScope, 'onDrag');

        element.find('.draggable').trigger(event);

        expect(isolateScope.onDrag).toHaveBeenCalled();
        expect(event.originalEvent.dataTransfer.setData).toHaveBeenCalledWith('draggedRow', 'element-id');
    });

    it('should prevent default, when dragging over allowed drop area', function () {
        prepareDirective(scope);
        var event = $.Event('dragover');
        event.preventDefault = window.jasmine.createSpy('preventDefault');
        event.originalEvent = {
            dataTransfer: {
                types: {},
                getData: window.jasmine.createSpy('getData').and.returnValue("element-id")
            }
        };
        element.find('.drop-area').trigger(event);
        expect(event.preventDefault).toHaveBeenCalled();
    });

    it('should show drop area, when drag enter allowed drop area', function () {
        prepareDirective(scope);
        var event = $.Event('dragenter');
        event.originalEvent = {
            dataTransfer: {
                types: {},
                getData: window.jasmine.createSpy('getData').and.returnValue("element-id")
            }
        };
        element.find('.drop-area').trigger(event);
        expect(element.find('.drop-area').css('display')).not.toEqual('none');
        expect(event.originalEvent.dataTransfer.getData).toHaveBeenCalledWith('draggedRow');
    });

    it('should hide drop area, when drag leave drop area', function () {
        prepareDirective(scope);
        var event = $.Event('dragleave');
        event.originalEvent = {
            dataTransfer: {
                types: {},
                getData: window.jasmine.createSpy('getData').and.returnValue("element-id")
            }
        };
        element.find('.drop-area').trigger(event);
        expect(element.find('.drop-area').css('display')).toEqual('none');
        expect(event.originalEvent.dataTransfer.getData).toHaveBeenCalledWith('draggedRow');
    });

    it('should hide drop area and call scope onDrop, when drop on drop area', function () {
        prepareDirective(scope);
        var event = $.Event('drop');
        event.preventDefault = window.jasmine.createSpy('preventDefault');
        event.originalEvent = {
            dataTransfer: {
                types: {},
                getData: window.jasmine.createSpy('getData').and.returnValue("element-id")
            }
        };
        var isolateScope = element.isolateScope();
        spyOn(isolateScope, 'onDrop');

        element.find('.drop-area').trigger(event);

        expect(event.originalEvent.dataTransfer.getData).toHaveBeenCalledWith('draggedRow');
        expect(event.preventDefault).toHaveBeenCalled();
        expect(element.find('.drop-area').css('display')).toEqual('none');
        expect(isolateScope.onDrop).toHaveBeenCalled();
    });
});

