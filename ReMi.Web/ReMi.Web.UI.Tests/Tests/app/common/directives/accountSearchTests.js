describe("Account Search Directive ", function () {
    var mocks, scope, container, element, button, modal;
    var html, modalHtml, compiled;
    var $httpBackend, $compile, $timeout, $q;
    var deferred, deferredRoles;

    beforeEach(function () {
        modalHtml = '<div class="modal"><div>Modal window</div></div>';
        html = '<div class="container"><button data-account-search="" data-get-selected="getSelected()" data-on-submit="onSubmit(data)" data-dialog-title="TTL">Test button</button></div>';

        mocks = {
            remiapi: jasmine.createSpyObj('remiapi', ['executeCommand', 'getAccounts', 'getAccountRoles', 'searchUsers']),
            authService: { isLoggedIn: false, identity: { name: 'name', email: 'email@email', role: 'Product owner' } }
            //authService: {
            //    isLoggedIn: true,
            //    identity: { username: 'test user', email: 'email@email' }
            //}
        };

        module("app", function ($provide) {
            $provide.value('remiapi', mocks.remiapi);
            $provide.value('authService', mocks.authService);
        });


        inject(function ($rootScope, _$compile_, $templateCache, $injector, _$q_, _$httpBackend_, _$timeout_) {
            $timeout = _$timeout_;
            $compile = _$compile_;
            $q = _$q_;

            $templateCache.put("accountSearchDialog.html", modalHtml);

            scope = $rootScope.$new();
            scope.getSelected = function () { return []; };

            deferred = $q.defer();
            deferredRoles = $q.defer();

            $httpBackend = _$httpBackend_;
            $httpBackend.when('GET', 'app/common/directives/tmpls/accountSearchDialog.html').respond(modalHtml);
            $httpBackend.when('GET', 'app/releaseCalendar/releaseCalendar.html').respond('<div></div>');
        });
    });

    function prepareDirective(s, isLoggedIn, role) {
        mocks.authService.isLoggedIn = isLoggedIn;
        mocks.authService.identity.role = role || mocks.authService.identity.role;

        mocks.remiapi.getAccountRoles.and.returnValue(deferredRoles.promise);
        deferredRoles.resolve({ Roles: ['role1', 'role2', 'role3'] });

        mocks.remiapi.getAccounts.and.returnValue(deferred.promise);
        deferred.resolve({ Accounts: [{ ExternalId: 'acc1', Email: 'acc1@emai.com', FullName: '' }, { ExternalId: 'acc2', Email: 'acc2@emai.com', FullName: '' }] });

        container = angular.element(html);
        compiled = $compile(container);
        element = compiled(s);
        s.$digest();

        button = container.find('button');
        modal = container.find('div.modal');

        $timeout.flush();

        $httpBackend.flush();
    }

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    /***********************************************************************************************************************/

    it('should leave button from page when role is ProductOwner', function () {
        prepareDirective(scope, true, 'Product owner');
        expect(container.find('button').length).toBe(1);
    });

    it('should leave button from page when role is ReleaseEngineer', function () {
        prepareDirective(scope, true, 'Release engineer');
        expect(container.find('button').length).toBe(1);
    });

    it('should leave button from page when role is ExecutiveManager', function () {
        prepareDirective(scope, true, 'Executive manager');
        expect(container.find('button').length).toBe(1);
    });

    it('should show modal dialog after button click', function () {
        spyOn($.fn, 'modal');
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();

        button.click();

        expect(targetScope.$dialog.modal).toHaveBeenCalledWith('show');
        expect(targetScope.filterCriteria).toBe('');
    });
    
    it('should populate accounts after init', function () {
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();

        button.click();

        expect(targetScope.accounts.length).toBe(2);
    });

    it('should show all accounts after button click', function () {
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();

        button.click();

        expect(targetScope.showAccounts.length).toBe(2);
    });

    it('should show only nonselected accounts after button click', function () {
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();

        targetScope.getSelected = function () {
            return [{ ExternalId: 'acc1', Email: 'acc1@emai.com', Fullname: '' }];
        };

        button.click();

        expect(targetScope.showAccounts.length).toBe(1);
    });

    it('should all accounts when filter criteria is common', function () {
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();

        targetScope.filterCriteria = 'acc';
        targetScope.searchByFilter();

        targetScope.$digest();

        expect(targetScope.showAccounts.length).toBe(2);
        expect(targetScope.serviceCalled).toBe(false);
    });

    it('should all accounts when filter criteria is common', function () {
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();

        targetScope.filterCriteria = 'cc1';
        targetScope.searchByFilter();

        targetScope.$digest();

        expect(targetScope.showAccounts.length).toBe(1);
        expect(targetScope.serviceCalled).toBe(false);
    });

    it('should add accounts when filter criteria length is four', function () {
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();

        var defer = $q.defer();
        mocks.remiapi.searchUsers.and.returnValue(defer.promise);
        defer.resolve({ Accounts: [{ ExternalId: 'acc13', Email: 'acc13@emai.com', FullName: '' }, { ExternalId: 'acc3', Email: 'acc3@emai.com', FullName: '' }] });

        targetScope.filterCriteria = 'acc1';
        targetScope.searchByFilter();

        targetScope.$digest();

        expect(targetScope.accounts.length).toBe(4);
        expect(targetScope.showAccounts.length).toBe(2);
        expect(targetScope.serviceCalled).toBe(true);
    });

    it('should reset serviceCalled var when filter criteria length less then four', function () {

        prepareDirective(scope, true);

        var targetScope = button.isolateScope();

        var defer = $q.defer();
        mocks.remiapi.searchUsers.and.returnValue(defer.promise);
        defer.resolve({ Accounts: [{ ExternalId: 'acc13', Email: 'acc13@emai.com', FullName: '' }, { ExternalId: 'acc3', Email: 'acc3@emai.com', FullName: '' }] });

        targetScope.filterCriteria = 'acc1';
        targetScope.searchByFilter();

        targetScope.$digest();

        targetScope.filterCriteria = 'acc';
        targetScope.searchByFilter();

        targetScope.$digest();

        expect(targetScope.accounts.length).toBe(4);
        expect(targetScope.showAccounts.length).toBe(4);
        expect(targetScope.serviceCalled).toBe(false);
    });

    it('should hide modal dialog when cancelled', function () {
        spyOn($.fn, 'modal');
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();

        button.click();
        targetScope.cancel();

        expect(targetScope.$dialog.modal).toHaveBeenCalledWith('show');
        expect(targetScope.filterCriteria).toBe('');
    });

    it('should call onSubmit action when submit clicked', function () {
        spyOn($.fn, 'modal');
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();
        spyOn(targetScope, 'onSubmit');

        button.click();

        targetScope.showAccounts[1].selected = true;

        targetScope.submit();

        targetScope.$digest();

        expect(targetScope.onSubmit).toHaveBeenCalledWith({ data: [{ ExternalId: 'acc2', Email: 'acc2@emai.com', FullName: '' }] });
        expect(targetScope.$dialog.modal).toHaveBeenCalledWith('hide');
    });

});

