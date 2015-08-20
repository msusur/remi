(function() {
    "use strict";

    var app = angular.module("app");

    app.directive("remiSidebar", function () {
        // Opens and clsoes the sidebar menu.
        // Usage:
        //  <div data-remi-sidebar>
        // Creates:
        //  <div data-remi-sidebar class="sidebar">
        var directive = {
            link: link,
            restrict: "A"
        };
        return directive;

        function link(scope, element) {
            var $sidebarInner = element.find(".sidebar-inner");
            var $dropdownElement = element.find(".sidebar-dropdown a");
            element.addClass("sidebar");
            $dropdownElement.click(dropdown);

            function dropdown(e) {
                var dropClass = "dropy";
                e.preventDefault();
                if (!$dropdownElement.hasClass(dropClass)) {
                    hideAllSidebars();
                    $sidebarInner.slideDown(350);
                    $dropdownElement.addClass(dropClass);
                } else if ($dropdownElement.hasClass(dropClass)) {
                    $dropdownElement.removeClass(dropClass);
                    $sidebarInner.slideUp(350);
                }

                function hideAllSidebars() {
                    $sidebarInner.slideUp(350);
                    $(".sidebar-dropdown a").removeClass(dropClass);
                }
            }
        }
    });


    app.directive("remiWidgetClose", function () {
        // Usage:
        // <a data-remi-widget-close></a>
        // Creates:
        // <a data-remi-widget-close="" href="#" class="wclose">
        //     <i class="fa fa-remove"></i>
        // </a>
        var directive = {
            link: link,
            template: "<i class=\"fa fa-remove\"></i>",
            restrict: "A"
        };
        return directive;

        function link(scope, element, attrs) {
            attrs.$set("href", "#");
            attrs.$set("wclose");
            element.click(close);

            function close(e) {
                e.preventDefault();
                element.parent().parent().parent().hide(100);
            }
        }
    });

    app.directive("remiWidgetMinimize", ["$timeout", remiWidgetMinimize]);

    function remiWidgetMinimize($timeout) {
        // Usage:
        // <a data-remi-widget-minimize></a>
        // Creates:
        // <a data-remi-widget-minimize="" href="#"><i class="fa fa-chevron-up"></i></a>
        var directive = {
            link: link,
            template: "<i class=\"fa fa-chevron-up\"></i>",
            restrict: "A",
            scope: {
                collapsedAtStart: "@"
            }
        };
        return directive;

        function link(scope, element, attrs) {
            //$('body').on('click', '.widget .wminimize', minimize);
            attrs.$set("href", "#");
            attrs.$set("wminimize");
            element.click(minimize);

            if (attrs.collapsedAtStart !== "true")
                return;

            $timeout(function() {
                element.click();
            });

            function minimize(e) {
                e.preventDefault();
                var $wcontent = element.parent().parent().next(".widget-content");
                var iElement = element.children("i");
                if ($wcontent.is(":visible")) {
                    iElement.removeClass("fa fa-chevron-up");
                    iElement.addClass("fa fa-chevron-down");
                } else {
                    iElement.removeClass("fa fa-chevron-down");
                    iElement.addClass("fa fa-chevron-up");
                }
                $wcontent.toggle(500);
            }
        }
    }

    app.directive("remiScrollToTop", ["$window",
        // Usage:
        // <span data-remi-scroll-to-top></span>
        // Creates:
        // <span data-remi-scroll-to-top="" class="totop">
        //      <a href="#"><i class="fa fa-chevron-up"></i></a>
        // </span>
        function ($window) {
            var directive = {
                link: link,
                template: "<a href=\"#\"><i class=\"fa fa-chevron-up\"></i></a>",
                restrict: "A"
            };
            return directive;

            function link(scope, element, attrs) {
                var $win = $($window);
                element.addClass("totop");
                $win.scroll(toggleIcon);

                element.find("a").click(function (e) {
                    e.preventDefault();
                    // Learning Point: $anchorScroll works, but no animation
                    //$anchorScroll();
                    $("body").stop(true, true).animate({ scrollTop: 0 }, 500);
                });

                function toggleIcon() {
                    $win.scrollTop() > 300 ? element.stop(true, true).slideDown() : element.stop(true, true).slideUp();
                }
            }
        }
    ]);

    app.directive("remiSpinner", ["$window", function ($window) {
        // Description:
        //  Creates a new Spinner and sets its options
        // Usage:
        //  <div data-remi-spinner="vm.spinnerOptions"></div>
        var directive = {
            link: link,
            restrict: "A"
        };
        return directive;

        function link(scope, element, attrs) {
            scope.spinner = null;
            scope.$watch(attrs.ccSpinner, function (options) {
                if (scope.spinner) {
                    scope.spinner.stop();
                }
                scope.spinner = new $window.Spinner(options);
                scope.spinner.spin(element[0]);
            }, true);
        }
    }]);

    app.directive("remiWidgetHeader", function () {
        //Usage:
        //<div data-remi-widget-header title="vm.map.title"></div>
        var directive = {
            link: link,
            scope: {
                'title': "@",
                'subtitle': "@",
                'rightText': "@",
                'allowCollapse': "@",
                "collapsedAtStart": "@"
            },
            templateUrl: "/app/layout/widgetheader.html",
            restrict: "A"
        };
        return directive;

        function link(scope, element, attrs) {
            attrs.$set("class", "widget-head");
        }
    });
})();
