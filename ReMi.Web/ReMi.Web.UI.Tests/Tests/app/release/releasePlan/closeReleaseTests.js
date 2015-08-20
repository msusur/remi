describe("Close Release Controller", function () {
    var sut, mocks, deferred, scope, fakeLogger, q;

    beforeEach(function () {
        module("app");

        fakeLogger = { warn: function () { }, error: function () { }};

        mocks = {
            $scope: jasmine.createSpyObj('$scope', ['$emit', '$on', 'registerWidget']),
            common: {
                logger: { getLogger: function () { return fakeLogger; } },
                $q: jasmine.createSpyObj('$q', ['when']),
                activateController: jasmine.createSpy('activateController').and.returnValue({ then: jasmine.createSpy('then') }),
                sendEvent: function (name, cb) { return cb; },
                handleEvent: function (name, cb) { return cb; },
            },
            remiapi: jasmine.createSpyObj('remiapi', ['closeReleaseWindow', 'searchUsers'])
        };

        inject(function (_$q_, _$rootScope_, $controller) {
            q = _$q_;
            deferred = _$q_.defer();
            scope = _$rootScope_.$new();
            sut = $controller('closeRelease', mocks);
        });
    });

    it("should call initialization methods when activated", function () {
        expect(sut).toBeDefined();
    });

    it("should hide close releasse modal", function () {
        spyOn($.fn, 'modal');

        sut.hideCurrentCloseReleaseModal();

        expect($('#closeReleaseModal').modal).toHaveBeenCalledWith('hide');
    });

    it("should hide addressee modal after adding attemp", function () {
        sut.addressees = [];

        sut.addAddressee(['item']);

        expect(sut.addressees.length).toEqual(1);
        expect(sut.addressees[0]).toEqual('item');
    });

    it("should remove addressee", function () {
        sut.addressees = [{ ExternalId: '2' }, { ExternalId: '3' }];
        var toDelete = { ExternalId: '2' };

        sut.removeAddressee(toDelete);

        expect(sut.addressees.length).toEqual(1);
        expect(sut.addressees[0].ExternalId).toEqual('3');
    });

    it("should sign off release", function () {
        var deferred2 = q.defer();
        var resolved = false, rejected = false;
        deferred2.promise.then(function () { resolved = true; }, function () { rejected = true; });
        mocks.remiapi.closeReleaseWindow.and.returnValue(deferred.promise);
        mocks.$scope = scope;
        inject(function ($controller) {
            sut = $controller('closeRelease', mocks);
        });
        sut.state = { isBusy: false };
        var windowId = '1';
        sut.releaseNotes = 'notes';
        sut.addressees = [{ ExternalId: '2' }];
        //sut.$scope.ReleaseClosedEvent = 'someEvent';
        spyOn($.fn, 'modal');
        spyOn(mocks.common, 'sendEvent');

        sut.signOffRelease({ deferred: deferred2, userName: 'user name', password: 'password' }, windowId);
        deferred.resolve();
        scope.$digest();

        expect(sut.state.isBusy).toEqual(false);
        expect($('#closeReleaseModal').modal).toHaveBeenCalledWith('hide');
        expect(mocks.common.sendEvent).toHaveBeenCalledWith('closeRelease.ReleaseClosedEvent');
        expect(resolved).toBeTruthy();
        expect(rejected).toBeFalsy();
    });

    it("should reject deferred, when fail to close release", function () {
        var deferred2 = q.defer();
        var resolved = false, rejected = false;
        deferred2.promise.then(function () { resolved = true; }, function () { rejected = true; });
        mocks.remiapi.closeReleaseWindow.and.returnValue(deferred.promise);
        mocks.$scope = scope;
        inject(function ($controller) {
            sut = $controller('closeRelease', mocks);
        });
        sut.state = { isBusy: false };
        var windowId = '1';
        sut.releaseNotes = 'notes';
        sut.addressees = [{ ExternalId: '2' }];
        //sut.$scope.ReleaseClosedEvent = 'someEvent';
        spyOn($.fn, 'modal');
        spyOn(mocks.common, 'sendEvent');

        sut.signOffRelease({ deferred: deferred2, userName: 'user name', password: 'password' }, windowId);
        deferred.reject();
        scope.$digest();

        expect(resolved).toBeFalsy();
        expect(rejected).toBeTruthy();
    });

    it("should return null when is not signed with digital signature", function () {
        var windowId = '1';
        var result = sut.signOffRelease({ someId: 'testId' }, windowId);

        expect(result).toBeNull();
    });

    it("should get addressees as selected accounts", function () {
        sut.addressees = 'super';

        var result = sut.getSelectedAccounts();

        expect(result).toEqual(sut.addressees);
    });
});
