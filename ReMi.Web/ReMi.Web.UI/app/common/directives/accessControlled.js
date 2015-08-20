(function () {
    'use strict';

    var directiveId = 'accessControl';

    angular.module('app').directive(directiveId, ['authService', '$window', '$rootScope',  
        function (authService, $window, $rootScope) {
            return {
                restrict: 'A',
                link: function(scope, element, attrs) {
                    var apiCall = attrs.accessControl;
                    var methods;
                    
                    $rootScope.$on('pemissions.loaded', function () {
                        if (element.hasClass('hidden')) {
                            element.removeClass('hidden');
                        }

                        if (element.attr('disabled')) {
                            element.removeAttr('disabled');
                        }

                        check();
                    });

                    check();

                    function check() {
                        if (!authService || !authService.identity || !authService.identity.role) {
                            restrict();
                            return;
                        }

                        try {
                            methods = apiCall.match('Command$') ? JSON.parse($window.sessionStorage.getItem("remiCommands"))
                                : JSON.parse($window.sessionStorage.getItem("remiQueries"));
                            if (!(methods instanceof Array)) {
                                throw null;
                            }
                        } catch (e) {
                            restrict();
                            return;
                        }
                    
                        var filteredMethods = methods.filter(function(x) {
                            return x == apiCall;
                        });
                    
                        if (!filteredMethods || !filteredMethods[0]) {
                            restrict();
                        }
                    }

                    function restrict() {
                        if (attrs.restrictionMode == 'disable') {
                            element.attr('disabled', 'disabled');
                            element.attr('title', 'Action disabled');
                            element.attr('data-toggle', 'tooltip');
                            element.attr('data-placement', 'top');
                            return;
                        }
                        if (attrs.restrictionMode == 'unclick') {
                            element.unbind('click');
                            return;
                        }
                        if (attrs.draggableElemSelector) {
                            element.unbind('dragover').unbind('dragenter').unbind('dragennd')
                                .unbind('dragstart').unbind('dragleave').unbind('drop');
                            return;
                        }

                        element.addClass('hidden');
                    }
                }
            };
        }
    ]);
})();
