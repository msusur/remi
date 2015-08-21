describe("Profile Controller", function () {
    var sut, mocks, logger, afterActivate;
    var getDeferred;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });
    });

    beforeEach(inject(function ($rootScope, $q) {
        mocks = {
            common: {
                logger: jasmine.createSpyObj('logger', ['getLogger']),
                activateController: jasmine.createSpy('activateController'),
                $broadcast: jasmine.createSpy('$broadcast'),
                handleEvent: jasmine.createSpy('handleEvent'),
                showWarnMessage: jasmine.createSpy('showWarnMessage'),
                showErrorMessage: jasmine.createSpy('showErrorMessage')
            },
            config: {
                events: {
                    businessUnitsLoaded: 'businessUnitsLoaded',
                    productsAddedForUser: 'productsAddedForUser'
                }
            },
            authService: {
                identity: {},
                isLoggedIn: true
            },
            localData: { businessUnits: businessUnits },
            $rootScope: $rootScope,
            $scope: $rootScope.$new(),
            $q: $q,
            remiapi: {
                post: window.jasmine.createSpyObj('post', ['updateAccountPackages']),
                get: window.jasmine.createSpyObj('get', ['businessUnits'])
            }
        };

        logger = window.jasmine.createSpyObj('logger', ['console', 'error', 'info', 'warn']);
        mocks.common.logger.getLogger.and.returnValue(logger);
        afterActivate = { then: jasmine.createSpy('then') };
        mocks.common.activateController.and.returnValue(afterActivate);
        getDeferred = $q.defer();
        mocks.remiapi.get.businessUnits.and.returnValue(getDeferred.promise);
    }));

    function prepareSystemUnderTest() {
        inject(function ($controller) {
            sut = $controller('profile', mocks);
        });
    }

    it("should initialize controller, when created", function () {
        prepareSystemUnderTest();

        expect(sut.account).toEqual(mocks.authService.identity);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith('businessUnitsLoaded', jasmine.any(Function), mocks.$scope);
        expect(sut.state.isBusy).toEqual(true);
        expect(mocks.common.activateController).toHaveBeenCalledWith(jasmine.any(Array), 'profile');
        expect(afterActivate.then).toHaveBeenCalledWith(jasmine.any(Function));
        expect(mocks.remiapi.get.businessUnits).toHaveBeenCalledWith(true);

        //check is account copied
        mocks.authService.identity.test = {};
        expect(sut.account).not.toEqual(mocks.authService.identity);
    });

    it("should fill out business units, when businesUnitsLoaded event handled", function () {
        var handler;
        var businessUnits = [{ Name: 'business unit' }];
        mocks.common.handleEvent.and.callFake(function (event, callback) {
            handler = callback;
        });
        prepareSystemUnderTest();
        handler(businessUnits);


        expect(sut.userBusinessUnits).toEqual(businessUnits);
    });

    describe('readAccountInfo', function () {
        it('should init business units, when resolved successfully', function () {
            prepareSystemUnderTest();

            mocks.localData.businessUnits = [
                angular.copy(businessUnits[0]),
                angular.copy(businessUnits[1])
            ];

            getDeferred.resolve({ BusinessUnits: businessUnits });
            mocks.$scope.$digest();

            expect(sut.userBusinessUnits).toEqual(mocks.localData.businessUnits);
            expect(sut.businessUnits).toEqual(businessUnits);
            expect(sut.businessUnits[0].Packages[0].Checked).toEqual(true);
            expect(sut.businessUnits[1].Packages[0].Checked).toEqual(true);
            expect(sut.businessUnits[1].Packages[1].Checked).toEqual(true);
            expect(sut.businessUnits[2].Packages[0].Checked).toEqual(false);
            expect(sut.businessUnits[0].Packages[0].IsDefault).toEqual(false);
            expect(sut.businessUnits[1].Packages[0].IsDefault).toEqual(true);

            expect(sut.state.isBusy).toEqual(false);
        });

        it('should log error, when rejected', function () {
            prepareSystemUnderTest();

            getDeferred.reject();
            mocks.$scope.$digest();

            expect(logger.error).toHaveBeenCalledWith('Cannot load business units');
            expect(sut.state.isBusy).toEqual(false);
        });

        it('should throw exception, when user is not logged in', function () {
            prepareSystemUnderTest();

            mocks.authService.isLoggedIn = false;

            expect(function () { sut.readAccountInfo(); }).toThrow('No active logged in account');
        });
    });

    describe('updateAccountPackages', function () {
        it('should show warrning, when acocuntId is empty', function () {
            prepareSystemUnderTest();
            sut.state.isBusy = false;
            sut.account = {};

            sut.updateAccountPackages();

            expect(mocks.common.showWarnMessage).toHaveBeenCalledWith("AccountId is empty");

            expect(sut.state.isBusy).toEqual(false);
        });

        it('should show warrning, when there is no checked packages', function () {
            prepareSystemUnderTest();
            sut.state.isBusy = false;
            sut.account = { externalId: 'external id' };

            sut.updateAccountPackages();

            expect(mocks.common.showWarnMessage).toHaveBeenCalledWith("Please check at least one packages");

            expect(sut.state.isBusy).toEqual(false);
        });

        it('should show warrning, when there is no default package', function () {
            prepareSystemUnderTest();
            sut.state.isBusy = false;
            sut.account = { externalId: 'external id' };
            prepareBusinessUnits();
            sut.businessUnits[1].Packages[0].IsDefault = false;

            sut.updateAccountPackages();

            expect(mocks.common.showWarnMessage).toHaveBeenCalledWith("Please select default package");

            expect(sut.state.isBusy).toEqual(false);
        });

        it('should show be busy, until promise has not finished working', function () {
            prepareSystemUnderTest();
            sut.state.isBusy = false;
            sut.account = { externalId: 'external id' };
            prepareBusinessUnits();
            mocks.remiapi.post.updateAccountPackages.and.returnValue(mocks.$q.defer().promise);

            sut.updateAccountPackages();

            expect(sut.state.isBusy).toEqual(true);
        });

        it("should send command and brodcast 'productsAddedForUser', when command executed successfully", function () {
            prepareSystemUnderTest();
            sut.state.isBusy = false;
            sut.account = { externalId: 'external id' };
            prepareBusinessUnits();
            var deferred = mocks.$q.defer();
            mocks.remiapi.post.updateAccountPackages.and.returnValue(deferred.promise);

            sut.updateAccountPackages();
            deferred.resolve();
            mocks.$scope.$digest();

            expect(mocks.remiapi.post.updateAccountPackages).toHaveBeenCalledWith({
                AccountId: 'external id',
                PackageIds: ["0f1ffaaa-48bb-432c-952f-9e1e67c6eb33",
                    "bc79f340-fd28-47f4-94c7-cb98963ba573", "c4979b08-2cd8-4111-8253-6b35766ba96f"],
                DefaultPackageId: "bc79f340-fd28-47f4-94c7-cb98963ba573"
            });
            expect(mocks.common.$broadcast).toHaveBeenCalledWith('productsAddedForUser', {});
            expect(sut.state.isBusy).toEqual(false);
        });

        it("should show error message and refresh user packages, when command execution failed", function () {
            prepareSystemUnderTest();
            sut.state.isBusy = false;
            sut.account = { externalId: 'external id' };
            prepareBusinessUnits();
            var deferred = mocks.$q.defer();
            mocks.remiapi.post.updateAccountPackages.and.returnValue(deferred.promise);
            spyOn(sut, 'readAccountInfo');

            sut.updateAccountPackages();
            deferred.reject('error');
            mocks.$scope.$digest();

            expect(mocks.common.showErrorMessage).toHaveBeenCalledWith("Could not update user packages");
            expect(logger.error).toHaveBeenCalledWith("error");
            expect(sut.readAccountInfo).toHaveBeenCalled();
            expect(sut.state.isBusy).toEqual(false);
        });

        function prepareBusinessUnits() {
            sut.userBusinessUnits = [
                angular.copy(businessUnits[0]),
                angular.copy(businessUnits[1])
            ];
            sut.businessUnits = businessUnits;
            Enumerable.From(sut.businessUnits)
                .SelectMany(function (x) { return x.Packages; })
                .ForEach(function (x) {
                    x.Checked = true;
                    x.IsDefault = false;
                });
            sut.businessUnits[2].Packages[0].Checked = false;
            sut.businessUnits[1].Packages[0].IsDefault = true;
        }
    });
});

var businessUnits = [
    {
        "ExternalId": "9f13aee7-3da1-4ceb-a25f-e320fd7c91f5",
        "Name": "Iowa",
        "Description": "Iowa",
        "Packages": [
            { "Name": "Eu Accumsan Incorporated", "ExternalId": "0f1ffaaa-48bb-432c-952f-9e1e67c6eb33", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": true, "IsDefault": false }
        ]
    },
    {
        "ExternalId": "823ff88d-3b0d-4f58-b976-fbe6ecfbdcbe",
        "Name": "NI",
        "Description": "North Island",
        "Packages": [
            { "Name": "Adipiscing Corp.", "ExternalId": "bc79f340-fd28-47f4-94c7-cb98963ba573", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": true },
            { "Name": "Adipiscing Corp. Demeter", "ExternalId": "c4979b08-2cd8-4111-8253-6b35766ba96f", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false }
        ]
    },
    {
        "ExternalId": "9246cf63-5d00-429a-a114-8db53ee375eb",
        "Name": "Vi",
        "Description": "Victoria",
        "Packages": [
            { "Name": "Mi Consulting", "ExternalId": "61ee6437-4029-408c-847d-73e84cc26819", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": true, "IsDefault": true }
        ]
    }
];
