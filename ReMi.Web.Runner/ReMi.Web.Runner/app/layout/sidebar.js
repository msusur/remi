(function () {
    'use strict';

    var controllerId = 'sidebar';

    angular.module('app').controller(controllerId, ['$rootScope', '$location', 'routes', sidebar]);

    function sidebar($rootScope, $location, routes) {
        var vm = this;

        vm.autoHide = true; // turn on autohiding for child menu items 

        vm.isCurrent = isCurrent;

        vm.mouseOverRoute = mouseOverRoute;
        vm.mouseOutAllRoutes = mouseOutAllRoutes;
        vm.initRoutesState = initRoutesState;
        vm.toggleParent = toggleParent;
        vm.getNavRoutes = getNavRoutes;
        vm.locationChangeSuccessHandler = locationChangeSuccessHandler;

        vm.navRoutes = null;
        vm.navRoutesEnumerable = null;

        $rootScope.$on('$locationChangeSuccess', vm.locationChangeSuccessHandler);

        vm.activate = activate;
        vm.activate();

        function activate() { vm.getNavRoutes(); }

        function getNavRoutes() {
            vm.navRoutes = Enumerable.From(routes)
                .Where(function (x) {
                    return x.config.settings && x.config.settings.nav;
                })
                .OrderBy(function (x) { return x.config.settings.nav; })
                .Select(function (x) { return x; })
                .ToArray();

            vm.navRoutesEnumerable = Enumerable.From(vm.navRoutes);

            vm.initRoutesState();
        }

        function initRoutesState() {
            for (var i = 0; i < vm.navRoutes.length; i++) {
                var r = vm.navRoutes[i];
                r.state = {
                    isCurrent: vm.isCurrent(r)
                };

                if (r.config.parentUrl) {
                    var parent = vm.navRoutesEnumerable.FirstOrDefault(null, function (x) { return x.url == r.config.parentUrl; });
                    if (parent) {
                        parent.state.hasChildren = true;
                        parent.state.expanded = false;
                    }

                    if (vm.autoHide) {
                        r.state.hidden = true;
                    }
                }
            }

            vm.locationChangeSuccessHandler();
        }

        function isCurrent(route) {
            if (route && route.url) {
                return route.url == $location.path();
            }
            return false;
        }

        function mouseOverRoute(route) {
            if (!vm.navRoutesEnumerable) return;
            if (!vm.autoHide) return;

            var expandedParents = vm.navRoutesEnumerable
                .Where(function (x) { return x.state && x.state.hasChildren && x.state.expanded && !x.state.isCurrentChild; })
                .ToArray();

            for (var i = 0; i < expandedParents.length; i++) {
                var r = expandedParents[i];
                if (r.url != route.url) {
                    if (route.config.settings.nav < r.config.settings.nav)
                        vm.toggleParent(r.url, false);
                }
            }

            if (route.state.hasChildren) {
                vm.toggleParent(route.url, true);
            }
        }

        function mouseOutAllRoutes() {
            if (!vm.navRoutesEnumerable) return;
            if (!vm.autoHide) return;

            var expandedParents = vm.navRoutesEnumerable
                .Where(function (x) { return x.state && x.state.hasChildren && x.state.expanded && !x.state.isCurrentChild; })
                .ToArray();

            if (expandedParents && expandedParents.length > 0) {
                // collapse all expanded parents that doesn't has children
                for (var i = 0; i < expandedParents.length; i++) {
                    var r = expandedParents[i];

                    vm.toggleParent(r.url, false);
                }
            }
        }

        function toggleParent(url, show) {
            var route = vm.navRoutesEnumerable
                .FirstOrDefault(null, function (x) { return x.url == url; });
            
            if (!route) {
                console.log('Route not found: ' + url);
                return;
            }

            if (route.state.hasChildren) {
                route.state.expanded = show;

                // show/hide child items for the parent route
                var children = vm.navRoutesEnumerable
                    .Where(function (x) { return x.config.parentUrl == url; })
                    .Select(function (x) { return x; })
                    .ToArray();

                if (children && children.length > 0) {
                    for (var i = 0; i < children.length; i++) {
                        children[i].state.hidden = !show;
                    }
                }
            }
        }

        function locationChangeSuccessHandler() {
            // set isCurrent property for route or for parent route of it
            var r, i;
            for (i = 0; i < vm.navRoutes.length; i++) {
                r = vm.navRoutes[i];
                r.state.isCurrent = false;
                r.state.isCurrentChild = false;
            }

            for (i = 0; i < vm.navRoutes.length; i++) {
                r = vm.navRoutes[i];
                if (!r.state.isCurrent) {
                    r.state.isCurrent = vm.isCurrent(r);

                    if (vm.autoHide && r.state.isCurrent && r.config.parentUrl) { // find parent and highlight it too
                        var parent = vm.navRoutesEnumerable
                            .FirstOrDefault(null, function (x) { return x.url == r.config.parentUrl; });

                        if (parent) {
                            parent.state.isCurrentChild = true;

                            vm.toggleParent(parent.url, true);
                        }
                    }
                }
            }
        }
    };
})();
