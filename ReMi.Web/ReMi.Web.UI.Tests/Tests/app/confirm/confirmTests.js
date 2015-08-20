describe("Confirms Controller", function () {
    var sut, mocks, deferred;
    var logger;
    var validGuid = '00000000-0000-0000-0000-000000000000';

    beforeEach(function () {
        module("app");

        mocks = {
            $route: window.jasmine.createSpyObj('$route', ['current']),
            $location: window.jasmine.createSpyObj('$location', ['path', 'search']),
            common: {
                logger: window.jasmine.createSpyObj('logger', ['getLogger']),
                $q: window.jasmine.createSpyObj('$q', ['when']),
                activateController: window.jasmine.createSpy('activateController').and.returnValue({ then: window.jasmine.createSpy('then') }),
                sendEvent: window.jasmine.createSpy('sendEvent'),
                handleEvent: window.jasmine.createSpy('handleEvent')
            },
            remiapi: window.jasmine.createSpyObj('remiapi', ['executeCommand']),
            authService: window.jasmine.createSpyObj('authService', ['identity', 'isLoggedIn'])
        };
        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);

        mocks.$route.current = window.jasmine.createSpyObj('current', ['params']);
        mocks.$route.current.params.and.returnValue({});

        mocks.$location.search.and.returnValue({});
        mocks.$location.path.and.returnValue(mocks.$location);

        mocks.authService.isLoggedIn = true;

        inject(function ($q) {
            deferred = $q.defer();
        });
        mocks.remiapi.executeCommand.and.returnValue(deferred.promise);
        inject(function ($controller) {
            sut = $controller('confirm', mocks);
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('confirm');
    });

    it("should navigate to default page when page invoked without parameters by anonymous user", function () {
        mocks.authService.isLoggedIn = false;
        mocks.$route.current.params.and.returnValue({});

        inject(function ($controller) {
            sut = $controller('confirm', mocks);
        });

        expect(mocks.$location.path).toHaveBeenCalledWith('/login');
        expect(mocks.$location.search).toHaveBeenCalledWith({});
        expect(logger.error).toHaveBeenCalledWith('The acknowledgment is not configured');
    });

    it("should navigate to default page when page invoked without parameters by logged in user", function () {
        mocks.$route.current.params.and.returnValue({});

        inject(function ($controller) {
            sut = $controller('confirm', mocks);
        });

        expect(mocks.$location.path).toHaveBeenCalledWith('/');
        expect(mocks.$location.search).toHaveBeenCalledWith({});
    });

    it("should navigate to default page when parameter in request not implemented", function () {
        mocks.$route.current.params = { 'CommandName': 'abc' };

        inject(function ($controller) {
            sut = $controller('confirm', mocks);
        });

        expect(mocks.$location.path).toHaveBeenCalledWith('/');
        expect(mocks.$location.search).toHaveBeenCalledWith({});
    });

    it("should navigate to default page when parameter in request not implemented", function () {
        mocks.$route.current.params = { 'CommandName': 'abc' };

        inject(function ($controller) {
            sut = $controller('confirm', mocks);
        });

        expect(mocks.$location.path).toHaveBeenCalledWith('/');
        expect(mocks.$location.search).toHaveBeenCalledWith({});
        expect(mocks.remiapi.executeCommand).not.toHaveBeenCalled();
        logger.error('The acknowledgment is not configured');
    });

    it("should invoke ConfirmReleaseTaskReviewCommand when correct request received", function () {
        mocks.$route.current.params = { 'releaseTaskReviewerUpdate': validGuid };

        inject(function ($controller) {
            sut = $controller('confirm', mocks);
        });

        expect(mocks.remiapi.executeCommand).toHaveBeenCalledWith('ConfirmReleaseTaskReviewCommand', window.jasmine.any(String),
            { 'ReleaseTaskId': validGuid });
    });

    it("should invoke ConfirmReleaseTaskImplementationCommand when correct request received", function () {
        mocks.$route.current.params = { 'releaseTaskImplementorUpdate': validGuid };

        inject(function ($controller) {
            sut = $controller('confirm', mocks);
        });

        expect(mocks.remiapi.executeCommand).toHaveBeenCalledWith('ConfirmReleaseTaskImplementationCommand', window.jasmine.any(String),
            { 'ReleaseTaskId': validGuid });
    });

    it("should invoke ConfirmReleaseTaskReceiptCommand when correct request received", function () {
        mocks.$route.current.params = { 'releaseTaskUpdateAcknowledge': validGuid };

        inject(function ($controller) {
            sut = $controller('confirm', mocks);
        });

        expect(mocks.remiapi.executeCommand).toHaveBeenCalledWith('ConfirmReleaseTaskReceiptCommand', window.jasmine.any(String),
            { 'ReleaseTaskId': validGuid });
    });

    it("should navigate to default page when guid in parameter's value invalid", function () {
        mocks.$route.current.params = { 'releaseTaskReviewerUpdate': 'abc' };

        inject(function ($controller) {
            sut = $controller('confirm', mocks);
        });

        expect(logger.error).toHaveBeenCalledWith('Incorrect url format');
    });
});

