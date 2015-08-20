(function () {
    "use strict";

    var serviceId = "localData";

    angular.module("app").factory(serviceId, ["common", "remiapi", localData]);

    function localData(common, remiapi) {
        var logger = common.logger.getLogger(serviceId);
        var $q = common.$q;
        var businessUnitsDeferred = $q.defer();

        var service = {
            dataRequests: [],
            businessUnits: [],
            enums: null,
            getEnum: getEnum,
            businessUnitsPromise: function() { return businessUnitsDeferred.promise; },
            businessUnitsResolve: function (businessUnits) {
                if (businessUnitsDeferred.resolved)
                    businessUnitsDeferred = $q.defer();
                service.businessUnits = businessUnits;
                businessUnitsDeferred.resolve(businessUnits);
                businessUnitsDeferred.resolved = true;
            }
        };

        activate();

        return service;

        function activate() {
            loadEnums();
        }

        function loadEnums() {
            return remiapi.get.enums()
                .then(function (result) {
                    service.enums = result.Enums;

                    for (var i = 0; i < service.dataRequests.length; i++) {
                        service.dataRequests[i].deferred.resolve(service.enums[service.dataRequests[i].name]);
                    }
                    service.dataRequests.length = 0;

                }, function (failure) {

                    for (var i = 0; i < service.dataRequests.length; i++) {
                        service.dataRequests[i].deferred.reject(failure);
                    }
                    service.dataRequests.length = 0;

                    console.log("error");
                    console.log(failure);
                    logger.error("Can't get enums");
                });
        }

        function getEnum(name) {
            var deferred = $q.defer();

            if (service.enums) {
                deferred.resolve(service.enums[name]);
                return deferred.promise;
            }

            service.dataRequests.push({ name: name, deferred: deferred });

            return deferred.promise;
        }
    }
})();
