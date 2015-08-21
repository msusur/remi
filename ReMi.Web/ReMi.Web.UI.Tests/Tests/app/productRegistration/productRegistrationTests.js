describe("Product Registration Controller", function () {
    var sut, mocks, logger;
    var $rootScope, $q, $timeout, $httpBackend;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });

        mocks = {
            common: {
                logger: window.jasmine.createSpyObj('logger', ['getLogger']),
                activateController: window.jasmine.createSpy('activateController').and.returnValue({ then: window.jasmine.createSpy('then') })
            },
            remiapi: window.jasmine.createSpyObj('remiapi', ['get', 'post']),
            authService: { identity: { fullname: 'myname' } },
            localData: { enums: null }
        };

        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.remiapi.get = window.jasmine.createSpyObj('get', ['productRegistrationsConfig', 'productRequestRegistrations']);
        mocks.remiapi.post = window.jasmine.createSpyObj('post', ['createProductRequestRegistration', 'updateProductRequestRegistration', 'deleteProductRequestRegistration']);

        // ReSharper disable InconsistentNaming
        inject(function ($controller, _$rootScope_, _$q_, _$timeout_, _$httpBackend_) {
        // ReSharper restore InconsistentNaming
            $q = _$q_;
            $timeout = _$timeout_;
            $rootScope = _$rootScope_;
            $httpBackend = _$httpBackend_;
            mocks.$scope = $rootScope.$new();

            var deferred = $q.defer();
            mocks.remiapi.get.productRegistrationsConfig.and.returnValue(deferred.promise);

            sut = $controller('productRegistration', mocks);

            //$httpBackend.when('GET', 'app/releaseCalendar/releaseCalendar.html').respond('');
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith('productRegistration');
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), 'productRegistration');
    });

    it("should populate requestConfig when getRequestConfig invoked", function () {
        var deferred = $q.defer();
        mocks.remiapi.get.productRegistrationsConfig.and.returnValue(deferred.promise);

        sut.getRequestConfig();

        deferred.resolve({ ProductRequestTypes: [{ ExternalId: 'ext' }] });

        mocks.$scope.$digest();

        expect(sut.requestConfig).not.toBeNull();
        expect(sut.requestConfig.length).toBe(1);
        expect(sut.requestConfig[0].ExternalId).toBe('ext');
    });

    it("should populate registrations when getProductRequestRegistrations invoked", function () {
        var deferred = $q.defer();
        mocks.remiapi.get.productRequestRegistrations.and.returnValue(deferred.promise);

        sut.getProductRequestRegistrations();

        deferred.resolve({ Registrations: [{ ExternalId: 'ext' }] });

        mocks.$scope.$digest();

        expect(sut.registrations).not.toBeNull();
        expect(sut.registrations.length).toBe(1);
        expect(sut.registrations[0].ExternalId).toBe('ext');
    });

    it("should not call prepareCurrentRecord after showProductRegistrationModal when configuration is empty", function () {
        spyOn(sut, 'prepareCurrentRecord');
        mocks.$scope.productRegistrationModalForm = { $setPristine: function () { } };
        spyOn(mocks.$scope.productRegistrationModalForm, '$setPristine');

        var registration = {
            ExternalId: 'ext',
            Descripion: 'desc'
        };

        sut.showProductRegistrationModal(registration);

        expect(sut.prepareCurrentRecord).not.toHaveBeenCalled();
        expect(mocks.$scope.productRegistrationModalForm.$setPristine).not.toHaveBeenCalled();
    });

    it("should call prepareCurrentRecord when showProductRegistrationModal invoked", function () {
        spyOn(sut, 'prepareCurrentRecord');
        mocks.$scope.productRegistrationModalForm = { $setPristine: function () { } };
        spyOn(mocks.$scope.productRegistrationModalForm, '$setPristine');

        var registration = {
            ExternalId: 'ext',
            Descripion: 'desc'
        };

        sut.requestConfig = [{ ExternalId: 'ext' }];

        sut.showProductRegistrationModal(registration);

        expect(sut.prepareCurrentRecord).toHaveBeenCalledWith(registration);
        expect(mocks.$scope.productRegistrationModalForm.$setPristine).toHaveBeenCalled();
    });

    it("should call remiapi save method when invoked with valid data", function () {
        spyOn(sut, 'updateProductRegistration');
        spyOn(sut, 'hideProductRegistrationModal');

        mocks.$scope.productRegistrationModalForm = { $invalid: false };

        sut.currentProductRegistration = {
            ExternalId: '',
            Description: 'desc',
            RequestType: {
                ExternalId: 'typeId',
                Name: 'type',
                RequestGroups: [
                    {
                        ExternalId: 'groupId',
                        Name: 'group',
                        RequestTasks: [
                            {
                                dirty: true,
                                ExternalId: 'taskId',
                                Question: 'task',
                                IsCompleted: true,
                                LastChangedBy: 'user',
                                LastChangedByAccountId: 'userId',
                                LastChangedOn: new Date()
                            },
                            {
                                ExternalId: 'taskId2',
                                Question: 'task2',
                            }
                        ]
                    }
                ]
            }
        };

        var deferred = $q.defer();
        mocks.remiapi.post.createProductRequestRegistration.and.returnValue(deferred.promise);

        sut.saveProductRegistrationModal();

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.createProductRequestRegistration).toHaveBeenCalled();
        expect(mocks.remiapi.post.createProductRequestRegistration)
            .toHaveBeenCalledWith({ Registration :
                {
                    ExternalId: window.jasmine.any(String),
                    Description : 'desc', 
                    ProductRequestTypeId : 'typeId', 
                    ProductRequestType : 'type', 
                    Tasks : [ 
                        { 
                            ProductRequestTaskId : 'taskId', 
                            IsCompleted : true, 
                            Comment : undefined,
                            LastChangedBy : 'user', 
                            LastChangedOn: window.jasmine.any(Date)
                        },
                        {
                            ProductRequestTaskId: 'taskId2',
                            IsCompleted: undefined,
                            Comment: undefined,
                            LastChangedBy: undefined,
                            LastChangedOn: undefined
                        }
                    ],
                    CreatedOn: window.jasmine.any(Date),
                    CreatedBy: 'myname',
                    Status: 'In progress'
                }
});

        expect(sut.updateProductRegistration).toHaveBeenCalled();
        expect(sut.hideProductRegistrationModal).toHaveBeenCalled();

    });

    it("should not save data in saveProductRegistrationModal when form validator is invalid", function () {
        mocks.$scope.productRegistrationModalForm = { $invalid: true };

        sut.saveProductRegistrationModal();

        expect(mocks.remiapi.post.createProductRequestRegistration).not.toHaveBeenCalled();
    });

    it("should not delete registration when ExternalId is empty", function () {
        sut.deleteProductRegistration({ ExternalId: '' });

        expect(mocks.remiapi.post.deleteProductRequestRegistration).not.toHaveBeenCalled();
    });

    it("should call remiapi delete method when registration is valid", function () {
        spyOn(sut, 'updateProductRegistration');
        spyOn(sut, 'hideRemovingReasonModal');

        var deferred = $q.defer();
        mocks.remiapi.post.deleteProductRequestRegistration.and.returnValue(deferred.promise);
        var registration = { ExternalId: 'ext' };
        sut.removingReason = { removingReason: 'reason', comment: 'some comment' };

        sut.deleteProductRegistration(registration);

        deferred.resolve();

        mocks.$scope.$digest();

        expect(mocks.remiapi.post.deleteProductRequestRegistration).toHaveBeenCalledWith({
             RegistrationId: 'ext', RemovingReason: 'reason', Comment: 'some comment'
        });
        expect(sut.updateProductRegistration).toHaveBeenCalledWith('delete', registration);
        expect(sut.hideRemovingReasonModal).toHaveBeenCalled();
    });

    it("should show remove modal, when showRemovingReasonModal called", function () {
        spyOn($.fn, 'modal');
        var registration = { data: 'some data' };
        mocks.localData.enums = { RemovingReason: [{ Name: 'some name' }] };

        sut.showRemovingReasonModal(registration);

        expect(sut.currentProductRegistration).toEqual(registration);
        expect(sut.removingReason).toEqual({ removingReason: 'some name', comment: '' });
        expect($.fn.modal).toHaveBeenCalledWith({ backdrop: 'static', keyboard: true });
    });

    it("should hide remove modal, when hideRemovingReasonModal called", function () {
        spyOn($.fn, 'modal');
        sut.currentProductRegistration = { data: 'some data' };
        sut.removingReason = { removingReason: 'some name', comment: '' };

        sut.hideRemovingReasonModal();

        expect(sut.currentProductRegistration).toBeNull();
        expect(sut.removingReason).toBeNull();
        expect($.fn.modal).toHaveBeenCalledWith('hide');
    });
});

