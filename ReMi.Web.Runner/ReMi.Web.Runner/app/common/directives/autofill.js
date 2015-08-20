(function() {
    'use strict';

    angular.module('app').directive('autofill', [
        '$timeout', function($timeout) {
            return {
                restrict: 'A',
                require: 'ngModel',
                link: function(scope, elem, attrs) {
                    var ownInput = false;

                    // trigger an input 500ms after loading the page (fixes chrome and safari autofill)
                    $timeout(function() {
                        $(elem[0]).trigger('input');
                    }, 500);

                    // listen for pertinent events to trigger input on form element
                    // use timeout to ensure val is populated before triggering 'input'
                    // ** 'change' event fires for Chrome
                    // ** 'DOMAttrModified' fires for Firefox 22.0
                    // ** 'keydown' for Safari  6.0.1
                    // ** 'propertychange' for IE
                    elem.on('change DOMAttrModified keydown propertychange', function() {
                        $timeout(function() {
                            $(elem[0]).trigger('input');
                        }, 0);
                    });

                    // tell other inputs to trigger (fixes firefox and ie9+ autofill)
                    elem.on('input', function() {
                        if (ownInput === false) scope.$emit('form.input');
                    });

                    // catches event and triggers if another input fired it.
                    scope.$on('form.input', function(e) {
                        e.stopPropagation();
                        ownInput = true;
                        $(elem[0]).trigger('input');
                        ownInput = false;
                    });
                }
            };
        }
    ]);
})();
