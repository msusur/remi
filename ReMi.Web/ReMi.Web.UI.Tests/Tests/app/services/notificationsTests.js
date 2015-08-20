describe("Notifications Service", function () {
    var sut, mocks, logger;
    var $timeout;

    beforeEach(function () {
        mocks = {
            common: {
                logger: jasmine.createSpyObj('logger', ['getLogger']),
                $q: jasmine.createSpyObj('$q', ['when']),
                $timeout: jasmine.createSpy('$timeout')
            },
            config: jasmine.createSpyObj('config', ['events']),
            remiapi: jasmine.createSpyObj('remiapi', ['executeCommand', 'getAccountRoles']),
            authService: jasmine.createSpyObj('authService', ['identity', 'role'])
        };

        logger = window.jasmine.createSpyObj('logger', ['console', 'error']);
        mocks.common.logger.getLogger.and.returnValue(logger);

        module("app", function($provide) {
            $provide.value('common', mocks.common);
            $provide.value('config', mocks.config);
            $provide.value('remiapi', mocks.remiapi);
            $provide.value('authService', mocks.authService);
        });

        inject(function (_notifications_, _$timeout_) {
            $timeout = _$timeout_;

            sut = _notifications_;
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('notifications');
    });
});

