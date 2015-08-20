describe("Access Control Directive ", function () {
    var mocks, scope, container, button;
    var compiled;
    var $httpBackend, $compile;

    function testSetup(testRole) {
        mocks = {
            authService: { isLoggedIn: true, identity: { name: 'name', email: 'email@email', role: testRole } },
            notifications: jasmine.createSpyObj('notifications', ['subscribe', 'unsubscribe']),
            $window: jasmine.createSpyObj('$window', ['sessionStorage'])
        };

        mocks.$window.sessionStorage = jasmine.createSpyObj('sessionStorage', ['getItem']);
        mocks.$window.sessionStorage.getItem.and.returnValue(JSON.stringify(['smthCommand']));
        
        module("app", function ($provide) {
            $provide.value('remiapi', mocks.remiapi);
            $provide.value('authService', mocks.authService);
            $provide.value('notifications', mocks.notifications);
            $provide.value('$window', mocks.$window);
        });

        inject(function (_$compile_, _$httpBackend_, _$rootScope_) {
            $compile = _$compile_;
            scope = _$rootScope_;
            $httpBackend = _$httpBackend_;
            $httpBackend.when('GET', 'app/releaseCalendar/releaseCalendar.html').respond('<div></div>');
        });
    };

    function prepareDirective(html) {
        container = angular.element(html);
        compiled = $compile(container)(scope);

        button = container.find('button');

        $httpBackend.flush();
    }

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    /***********************************************************************************************************************/
    it('check button will access control initialization', function () {
        testSetup('azaza');
        var html = '<div class="container"><button data-ng-click="action(data)" data-access-control="smthCommand" data-restriction-mode="hide">Push me</button></div>';
        prepareDirective(html);

        expect(button).toBeDefined();
        expect(button.hasClass('hidden')).toBe(false);
    });

    it('should check hiding element', function () {
        testSetup('azaz');
        var html = '<div class="container"><button onclick="console.log(123)" data-access-control="smthCommand1" data-restriction-mode="hide">Push me</button></div>';
        prepareDirective(html);

        expect(button).toBeDefined();
        expect(button.hasClass('hidden')).toBe(true);
    });

    it('should check disabling', function () {
        testSetup('azaz');
        var html = '<div class="container"><button data-ng-click="action(data)" data-access-control="smthCommand1" data-restriction-mode="disable">Push me</button></div>';
        prepareDirective(html);

        expect(button).toBeDefined();
        expect(button.attr('disabled')).toEqual('disabled');
    });

    it('should check removing click handler', function () {
        testSetup('azaz');
        var html = '<div class="container"><button data-ng-click="action(data)" data-access-control="smthCommand1" data-restriction-mode="unclick">Push me</button></div>';
        prepareDirective(html);

        expect(button).toBeDefined();
        expect(button.onclick).toBe(undefined);
    });
});

