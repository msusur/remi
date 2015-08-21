describe("Api Description Controller", function () {
    var sut, mocks, logger;
    var $rootScope, $q, $timeout, $httpBackend;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });

        mocks = {
            common: {
                logger: window.jasmine.createSpyObj('logger', ['getLogger']),
                activateController: window.jasmine.createSpy('activateController').and.returnValue({ then: window.jasmine.createSpy('then') })
            },
            remiapi: window.jasmine.createSpyObj('remiapi', [
                'get', 'post']),
        };

        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.remiapi.get = window.jasmine.createSpyObj('get', ['apiDescriptions']);
        mocks.remiapi.post = window.jasmine.createSpyObj('post', ['updateApiDescription']);

        inject(function ($controller, _$rootScope_, _$q_, _$timeout_, _$httpBackend_) {
            $q = _$q_;
            $timeout = _$timeout_;
            $rootScope = _$rootScope_;
            $httpBackend = _$httpBackend_;
            mocks.$scope = $rootScope.$new();

            var deferred = $q.defer();
            mocks.remiapi.get.apiDescriptions.and.returnValue(deferred.promise);

            sut = $controller('apiDescription', mocks);

            $httpBackend.when('GET', 'app/releaseCalendar/releaseCalendar.html').respond('');
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('apiDescription');
    });

    it("should get descriptions", function () {
        var deferred = $q.defer();
        mocks.remiapi.get.apiDescriptions.and.returnValue(deferred.promise);

        var data = {
            ApiDescriptions: [
                { Name: 'R1', Url: '/request1', Method: 'GET' },
                { Name: 'R2', Url: '/request2', Method: 'POST' },
                { Name: 'R3', Url: '/request3', Method: 'POST' }
            ]
        };

        sut.refreshData();

        deferred.resolve(data);
        $timeout.flush();
        mocks.$scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect(sut.descriptions.length).toEqual(3);
        expect(sut.descriptions[0].Name).toEqual('R1');
        expect(sut.descriptions[1].Name).toEqual('R2');
        expect(sut.descriptions[2].Name).toEqual('R3');
    });

    it("should initialize changing description  and backup whe call manage description", function () {
        spyOn($.fn, 'modal');

        sut.manageDescription({ Description: 'ddd' });

        expect($('#changeApiDescriptionModal').modal).toHaveBeenCalledWith('show');
        expect(sut.managingDescription).toEqual({ Description: 'ddd' });
        expect(sut.managingDescriptionBackUp).toEqual('ddd');
    });

    it("should return bak up value to description when hide modal", function () {
        spyOn($.fn, 'modal');
        sut.managingDescriptionBackUp = 'super';
        sut.managingDescription = { Description: 'not super' };

        sut.hideManageDescriptionModal();

        expect($('#changeApiDescriptionModal').modal).toHaveBeenCalledWith('hide');
        expect(sut.managingDescription.Description).toEqual('super');
    });

    it("should return bak up value to description when hide modal", function () {
        spyOn(sut, 'hideManageDescriptionModal');
        sut.managingDescriptionBackUp = 'super';
        sut.managingDescription = { Description: 'super' };

        sut.saveApiDescription();

        expect(sut.hideManageDescriptionModal).toHaveBeenCalled();
    });

    it("should save updating description", function () {
        var deferred = $q.defer();
        mocks.remiapi.post.updateApiDescription.and.returnValue(deferred.promise);
        spyOn(sut, 'hideManageDescriptionModal');
        sut.managingDescriptionBackUp = 'super';
        sut.managingDescription = { Description: 'mega' };

        sut.saveApiDescription();
        
        deferred.resolve();
        $timeout.flush();
        mocks.$scope.$digest();

        expect(sut.hideManageDescriptionModal).toHaveBeenCalled();
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.managingDescriptionBackUp).toEqual('mega');
    });

    it("should reject saving updating description", function () {
        var deferred = $q.defer();
        mocks.remiapi.post.updateApiDescription.and.returnValue(deferred.promise);
        spyOn(sut, 'hideManageDescriptionModal');
        sut.managingDescriptionBackUp = 'super';
        sut.managingDescription = { Description: 'mega' };

        sut.saveApiDescription();

        deferred.reject('error');
        $timeout.flush();
        mocks.$scope.$digest();

        expect(sut.hideManageDescriptionModal).toHaveBeenCalled();
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.managingDescriptionBackUp).toEqual('super');
        expect(logger.console).toHaveBeenCalledWith('error');
        expect(logger.error).toHaveBeenCalledWith('Cannot update API description');
    });
});

