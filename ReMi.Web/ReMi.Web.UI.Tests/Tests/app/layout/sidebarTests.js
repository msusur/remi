describe("Sidebar Controller", function () {
    var sut, mocks;

    beforeEach(function () {
        module("app");

        mocks = {
            $rootScope: window.jasmine.createSpyObj('$rootScope', ['$on']),
            $location: window.jasmine.createSpyObj('$location', ['path']),
            routes: [],
        };

        inject(function ($controller) {
            sut = $controller('sidebar', mocks);
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(sut.navRoutes).not.toBeNull();
        expect(sut.navRoutesEnumerable).not.toBeNull();
        expect(mocks.$rootScope.$on).toHaveBeenCalledWith('$locationChangeSuccess', sut.locationChangeSuccessHandler);
    });

    it("should populate routes in correct order when activated", function () {
        inject(function ($controller) {
            mocks.routes = getRoutesData();

            sut = $controller('sidebar', mocks);
        });

        expect(sut.navRoutes.length).toBe(3);
        expect(sut.navRoutes[0].url).toBe('u1');
        expect(sut.navRoutes[1].url).toBe('u2');
        expect(sut.navRoutes[2].url).toBe('u4');
    });

    it("should populate routes with children when activated", function () {
        inject(function ($controller) {
            mocks.routes = getRoutesData(true);

            sut = $controller('sidebar', mocks);
        });

        expect(sut.navRoutes.length).toBe(5);
        expect(sut.navRoutes[0].url).toBe('u1');
        expect(sut.navRoutes[1].url).toBe('u2');
        expect(sut.navRoutes[2].url).toBe('u2.1');
        expect(sut.navRoutes[3].url).toBe('u2.2');
        expect(sut.navRoutes[4].url).toBe('u4');

        expect(sut.navRoutes[2].state.hidden).toBe(true);
        expect(sut.navRoutes[3].state.hidden).toBe(true);
    });

    it("should return current route when isCurrent invoked", function () {
        inject(function ($controller) {
            mocks.$location.path.and.returnValue('u2.2');
            mocks.routes = getRoutesData(true);

            sut = $controller('sidebar', mocks);
        });

        expect(sut.isCurrent({ url: 'u1' })).toBe(false);
        expect(sut.isCurrent({ url: 'u2' })).toBe(false);
        expect(sut.isCurrent({ url: 'u2.1' })).toBe(false);
        expect(sut.isCurrent({ url: 'u2.2' })).toBe(true);
        expect(sut.isCurrent({ url: 'u4' })).toBe(false);
    });

    it("should set isCurrent and expanded states for active route and it's parent when activated", function () {
        inject(function ($controller) {
            mocks.$location.path.and.returnValue('u2.2');
            mocks.routes = getRoutesData(true);

            sut = $controller('sidebar', mocks);
        });

        expect(sut.navRoutes[0].state.isCurrent).toBe(false);

        expect(sut.navRoutes[1].state.isCurrent).toBe(false);
        expect(sut.navRoutes[1].state.isCurrentChild).toBe(true);
        expect(sut.navRoutes[1].state.expanded).toBe(true);
        expect(sut.navRoutes[2].state.hidden).toBe(false);
        expect(sut.navRoutes[3].state.hidden).toBe(false);

        expect(sut.navRoutes[2].state.isCurrent).toBe(false);
        expect(sut.navRoutes[3].state.isCurrent).toBe(true);
        expect(sut.navRoutes[4].state.isCurrent).toBe(false);
    });

    it("should set expanded state for active route when mouseOver event raised", function () {
        inject(function ($controller) {
            mocks.routes = getRoutesData(true);

            sut = $controller('sidebar', mocks);
        });

        sut.navRoutes[1].state.expanded = true;
        sut.mouseOverRoute(mocks.routes[3]);

        expect(sut.navRoutes[1].state.expanded).toBe(false);
        expect(sut.navRoutes[2].state.hidden).toBe(true);
        expect(sut.navRoutes[3].state.hidden).toBe(true);
    });

    it("should not set expanded state for active route when mouseOver event raised and target nav property grater then active nav", function () {
        inject(function ($controller) {
            mocks.routes = getRoutesData(true);

            sut = $controller('sidebar', mocks);
        });

        sut.navRoutes[1].state.expanded = true;
        sut.mouseOverRoute(mocks.routes[0]);

        expect(sut.navRoutes[1].state.expanded).toBe(true);
    });

    it("should collapse all routes when mouseOut event raised", function () {
        inject(function ($controller) {
            mocks.routes = getRoutesData(true);

            sut = $controller('sidebar', mocks);
        });

        sut.navRoutes[1].state.expanded = true;
        sut.mouseOutAllRoutes();

        expect(sut.navRoutes[1].state.expanded).toBe(false);
        expect(sut.navRoutes[2].state.hidden).toBe(true);
        expect(sut.navRoutes[3].state.hidden).toBe(true);
    });

    it("should leave current route expanded when mouseOut event raised", function () {
        inject(function ($controller) {
            mocks.$location.path.and.returnValue('u2.2');
            mocks.routes = getRoutesData(true);

            sut = $controller('sidebar', mocks);
        });

        sut.mouseOutAllRoutes();

        expect(sut.navRoutes[1].state.expanded).toBe(true);
        expect(sut.navRoutes[1].state.isCurrentChild).toBe(true);
        expect(sut.navRoutes[2].state.hidden).toBe(false);
        expect(sut.navRoutes[3].state.hidden).toBe(false);
    });

    it("should activate route by url when toggleParent called", function () {
        inject(function ($controller) {
            mocks.routes = getRoutesData(true);

            sut = $controller('sidebar', mocks);
        });

        sut.toggleParent('u2', true);

        expect(sut.navRoutes[1].state.expanded).toBe(true);
        expect(sut.navRoutes[2].state.hidden).toBe(false);
        expect(sut.navRoutes[3].state.hidden).toBe(false);
    });

    it("should not activate route by url when target route doen't has sub routes", function () {
        inject(function ($controller) {
            mocks.routes = getRoutesData(true);

            sut = $controller('sidebar', mocks);
        });

        sut.toggleParent('u1', true);

        expect(sut.navRoutes[0].state.expanded).toBeUndefined();
        expect(sut.navRoutes[1].state.expanded).toBe(false);
    });


    function getRoutesData(withChildren) {
        var routes = [
            { url: 'u4', config: { title: 't4', settings: { nav: 4 } } },
            { url: 'u2', config: { title: 't2', settings: { nav: 2 } } },
            { url: 'u3', config: { title: 't3', settings: undefined } },
            { url: 'u1', config: { title: 't1', settings: { nav: 1 } } }
        ];

        if (withChildren) {
            routes.push({ url: 'u2.1', config: { title: 't2.1', parentUrl: 'u2', settings: { nav: 2.1 } } });
            routes.push({ url: 'u2.2', config: { title: 't2.2', parentUrl: 'u2', settings: { nav: 2.2 } } });
        }

        return routes;
    }
});

