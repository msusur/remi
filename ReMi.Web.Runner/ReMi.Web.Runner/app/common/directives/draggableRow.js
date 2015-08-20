(function () {
    'use strict';

    var directiveId = 'draggableRow';

    angular.module('app')
        .directive(directiveId, [
            function () {
                return {
                    restrict: 'A',
                    scope: {
                        onDrop: '&',
                        onDrag: '&',
                        onDragEnd: '&',
                        draggableElemSelector: '@',
                        dropAreaSelector: '@'
                    },
                    link: function (scope, element) {
                        var draggableElem = element.find(scope.draggableElemSelector);
                        var dropArea = element.find(scope.dropAreaSelector);

                        element
                            .unbind("dragover")
                            .unbind("dragenter")
                            .unbind("dragend")
                            .bind("dragover", function (event) {
                                if (allowDrop(event)) {
                                    event.preventDefault();
                                }
                            }).bind("dragenter", function (event) {
                                if (allowDrop(event)) {
                                    $(this).find(scope.dropAreaSelector).show();
                                }
                            }).bind("dragend", function (event) {
                                if (scope.onDragEnd)
                                    scope.onDragEnd();
                            });

                        draggableElem.attr("draggable", "true")
                            .unbind("dragstart")
                            .bind('dragstart', function (event) {
                                setData(event, element);
                                if (scope.onDrag)
                                    scope.onDrag();
                            });

                        dropArea
                            .unbind("dragover")
                            .unbind("dragenter")
                            .unbind("dragleave")
                            .unbind("drop")
                            .bind("dragover", function (event) {
                                if (allowDrop(event)) {
                                    event.preventDefault();
                                }
                            }).bind("dragenter", function (event) {
                                if (allowDrop(event)) {
                                    $(this).show();
                                }
                            }).bind("dragleave", function (event) {
                                if (allowDrop(event)) {
                                    $(this).hide();
                                }
                            }).bind("drop", function (event) {
                                var data = getData(event);
                                if (data) {
                                    event.preventDefault();
                                    $(this).hide();
                                }
                                if (scope.onDrop)
                                    scope.onDrop();
                            });
                    }
                };
            }
        ]);

    function setData(event, element) {
        if (event.originalEvent.dataTransfer) {
            event.originalEvent.dataTransfer.setData("draggedRow", element.attr('id'));
        }
    }

    function getData(event) {
        return event.originalEvent.dataTransfer.getData("draggedRow");
    }

    function allowDrop(event) {
        return (event.originalEvent.dataTransfer.types.indexOf && event.originalEvent.dataTransfer.types.indexOf("draggedrow") >= 0)
            || event.originalEvent.dataTransfer.getData("draggedRow");
    }
})();
