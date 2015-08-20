describe("Account Signature Directive ", function () {
    var mocks, scope, container, element, button, modal;
    var html, modalHtml, compiled;
    var $httpBackend, $compile;

    beforeEach(function () {
        modalHtml = '<div class="modal"><div>Modal window</div></div>';
        html = '<div class="container"><button data-ng-click="action(data)" data-account-signature="" data-default-user-name="@defaulUserName">Push me</button></div>';

        mocks = {
            authService: { isLoggedIn: false, identity: { name: 'name', email: 'email@email', role: '' } }
            //authService: {
            //    isLoggedIn: true,
            //    identity: { username: 'test user', email: 'email@email' }
            //}
        };

        module("app", function ($provide) {
            $provide.value('remiapi', mocks.remiapi);
            $provide.value('authService', mocks.authService);
        });


        inject(function ($rootScope, _$compile_, $templateCache, $injector, _$httpBackend_) {
            $templateCache.put("accountSignatureDialog.html", modalHtml);

            $compile = _$compile_;

            scope = $rootScope.$new();

            $httpBackend = _$httpBackend_;
            $httpBackend.when('GET', 'app/common/directives/tmpls/accountSignatureDialog.html').respond(modalHtml);
            $httpBackend.when('GET', 'app/releaseCalendar/releaseCalendar.html').respond('<div></div>');
        });
    });

    function prepareDirective(s, isLoggedIn, defaulUserName) {
        mocks.authService.isLoggedIn = isLoggedIn;

        if (!!defaulUserName) {
            html = html.replace('@defaulUserName', defaulUserName);
        } else {
            html = html.replace('@defaulUserName', '');
        }

        container = angular.element(html);
        compiled = $compile(container);
        element = compiled(s);
        s.$digest();

        button = container.find('button');
        modal = container.find('div.modal');

        $httpBackend.flush();
    }

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    /***********************************************************************************************************************/

    it('check init for logged in user', function () {
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();

        expect(container).toBeDefined();
        expect(button).toBeDefined();
        expect(container.find('div.modal').length).toBe(1);

        expect(targetScope).toBeDefined();
        expect(targetScope.sign).toBeDefined();
        expect(targetScope.cancel).toBeDefined();
        expect(targetScope.$dialog).toBeDefined();
    });

    it('check init with default user', function () {
        prepareDirective(scope, true, 'test@user.abc');

        var targetScope = button.isolateScope();

        expect(targetScope.userName).toBe('test@user.abc');
    });

    it('should show modal dialog after button click', function () {
        spyOn($.fn, 'modal');
        prepareDirective(scope, true);
        var targetScope = button.isolateScope();

        expect(modal.hasClass('in')).toBe(false);

        button.click();

        targetScope.$digest();

        expect(targetScope.$dialog.modal).toHaveBeenCalledWith('show');
        expect(targetScope.userName).toBe(mocks.authService.identity.email);
        expect(!!targetScope.password).toBe(false);
    });

    it('should populate data object, when sign', function () {
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();
        targetScope.userName = 'qwerty';
        targetScope.password = 'password';

        button.click();

        targetScope.sign();

        expect(targetScope.data.userName).toBe('qwerty');
        expect(targetScope.data.password).toBe('password');
        expect(targetScope.data.deferred).toBeDefined();
        expect(targetScope.state.isBusy).toBe(true);
    });

    it('should hide modal dialog, when check pass in external source', function () {
        spyOn($.fn, 'modal');
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();
        targetScope.userName = 'qwerty';
        targetScope.password = 'password';
        spyOn(targetScope, 'ngClick');

        button.click();

        targetScope.sign();

        targetScope.data.deferred.resolve();

        targetScope.$digest();

        expect(targetScope.$dialog.modal).toHaveBeenCalledWith('hide');
        expect(modal.hasClass('in')).toBe(false);
        expect(targetScope.userName).toBe(mocks.authService.identity.email);
        expect(targetScope.ngClick).toHaveBeenCalledWith(targetScope, { userName: 'qwerty', password: 'password', deferred: targetScope.data.deferred });
    });

    it('should not hide modal, when check fail in external source', function () {
        spyOn($.fn, 'modal');
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();
        targetScope.userName = 'qwerty';
        targetScope.password = 'password';

        button.click();

        targetScope.sign();

        targetScope.data.deferred.reject();

        targetScope.$digest();

        expect(targetScope.$dialog.modal).not.toHaveBeenCalledWith('hide');
        expect(!!targetScope.password).toBe(false);
    });

    it('should hide modal dialog when operation cancelled', function () {
        spyOn($.fn, 'modal');
        prepareDirective(scope, true);

        var targetScope = button.isolateScope();

        button.click();

        targetScope.userName = 'qwerty';

        targetScope.cancel();

        targetScope.$digest();

        expect(targetScope.$dialog.modal).toHaveBeenCalledWith('hide');
        expect(targetScope.userName).toBe(mocks.authService.identity.email);
    });

    it('should remove button from page for anonimous user', function () {
        prepareDirective(scope, false);

        expect(container.find('modal').length).toBe(0);
        expect(container.find('button').length).toBe(0);
    });

    it('should show modal dialog after button click, when beforeShow initialized and defer resolved', function () {
        spyOn($.fn, 'modal');
        html = html.replace("data-account-signature=", 'data-before-show="beforeShow(defer)" data-account-signature=');
        prepareDirective(scope, true);
        var targetScope = button.isolateScope();
        targetScope.beforeShow = function (defer) {
            defer.defer.resolve();
        };

        expect(modal.hasClass('in')).toBe(false);

        button.click();

        targetScope.$digest();

        expect(targetScope.$dialog.modal).toHaveBeenCalledWith('show');
        expect(targetScope.userName).toBe(mocks.authService.identity.email);
        expect(!!targetScope.password).toBe(false);
    });

    it('should not show modal dialog after button click, when beforeShow initialized and defer rejected', function () {
        spyOn($.fn, 'modal');
        html = html.replace("data-account-signature=", 'data-before-show="beforeShow(defer)" data-account-signature=');
        prepareDirective(scope, true);
        var targetScope = button.isolateScope();
        targetScope.beforeShow = function (defer) {
            defer.defer.reject();
        };

        expect(modal.hasClass('in')).toBe(false);

        button.click();

        targetScope.$digest();

        expect(targetScope.$dialog.modal).not.toHaveBeenCalledWith('show');
    });
});

