(function () {
    "use strict";

    var controllerId = "businessUnits";

    angular.module("app").controller(controllerId, businessUnits);

    function businessUnits($scope, common, remiapi, config, localData) {
        var logger = common.logger.getLogger(controllerId);
        var vm = this;

        vm.showModal = showModal;
        vm.hideModal = hideModal;
        vm.addBusinessUnit = addBusinessUnit;
        vm.updateBusinessUnit = updateBusinessUnit;
        vm.removeBusinessUnit = removeBusinessUnit;

        vm.businessUnitToManage = undefined;
        vm.state = { isBusy: false };
        vm.businessUnits = [];

        common.handleEvent(config.events.businessUnitsLoaded, businesUnitsLoadedHandler, $scope);

        activate();

        function activate() {
            common.activateController([getBusinessUnits()], controllerId)
                .then(function () {
                    logger.console("Activated Business Units View");
                });
        }

        function getBusinessUnits() {
            return localData.businessUnitsPromise()
                .then(function (data) {
                    if (data) {
                        vm.businessUnits = Enumerable.From(angular.copy(data))
                            .Select(function (x) {
                                return { Name: x.Name, Description: x.Description, ExternalId: x.ExternalId }
                            })
                            .ToArray();
                    } else {
                        vm.businessUnits = [];
                    }
                });
        }

        function addBusinessUnit() {
            if (vm.businessUnitToManage == undefined
                || vm.businessUnitToManage.Name === ""
                || vm.businessUnitToManage.Description === "") {
                logger.warn("Cannot add empty business unit");
                return;
            }
            if (vm.businessUnits.filter(function (item) {
                 return item.Description === vm.businessUnitToManage.Description
                || item.Name === vm.businessUnitToManage.Name;
            }).length > 0) {
                logger.warn("Business Unit already exists");
                return;
            }

            vm.state.isBusy = true;
            var businessUnit = {
                ExternalId: vm.businessUnitToManage.ExternalId,
                Description: vm.businessUnitToManage.Description,
                Name: vm.businessUnitToManage.Name
            };
            remiapi.post.addBusinessUnit(businessUnit).then(function () {
                updateLocalData(businessUnit);
                vm.hideModal();
            }, function (error) {
                logger.error("Cannot add business unit");
                logger.console(error);
            }).finally(function () {
                vm.state.isBusy = false;
            });
        }

        function updateBusinessUnit() {
            if (vm.businessUnitToManage == undefined
                || vm.businessUnitToManage.Name === ""
                || vm.businessUnitToManage.Description === "") {
                logger.warn("Cannot add empty business unit");
                return;
            }
            if (vm.businessUnits.filter(function (item) {
                 return (item.Description === vm.businessUnitToManage.Description
                        || item.Name === vm.businessUnitToManage.Name)
                    && item.ExternalId !== vm.businessUnitToManage.ExternalId;
            }).length > 0) {
                logger.warn("Business Unit already exists");
                return;
            }

            vm.state.isBusy = true;
            var businessUnit = {
                ExternalId: vm.businessUnitToManage.ExternalId,
                Description: vm.businessUnitToManage.Description,
                Name: vm.businessUnitToManage.Name
            };
            remiapi.post.updateBusinessUnit(businessUnit).then(function () {
                updateLocalData(businessUnit);
                vm.hideModal();
            }, function (error) {
                logger.error("Cannot update business unit");
                logger.console(error);
            }).finally(function () {
                vm.state.isBusy = false;
            });
        }

        function removeBusinessUnit(businessUnit) {
            remiapi.post.removeBusinessUnit({ ExternalId: businessUnit.ExternalId }).then(function () {
                updateLocalData(businessUnit, "remove");
            }, function (error) {
                logger.error("Cannot remove business unit");
                logger.console(error);
            }).finally(function () {
                vm.state.isBusy = false;
            });
        }

        function showModal(businessUnit) {
            if (businessUnit) {
                vm.operationMode = "Update";
                vm.businessUnitToManage = {
                    Description: businessUnit.Description,
                    ExternalId: businessUnit.ExternalId,
                    Name: businessUnit.Name
                };
            } else {
                vm.operationMode = "Add";
                vm.businessUnitToManage = {
                    Description: "",
                    ExternalId: newGuid(),
                    Name: ""
                };
            }

            $("#manageBusinessUnitModal").modal("show");
        }

        function hideModal() {
            $("#manageBusinessUnitModal").modal("hide");
        }

        function businesUnitsLoadedHandler() {
            getBusinessUnits();
        }

        function updateLocalData(businessUnit, mode) {
            var oldBusinessUnit = Enumerable.From(localData.businessUnits)
                .Where(function (x) { return x.ExternalId === businessUnit.ExternalId; })
                .FirstOrDefault();

            if (oldBusinessUnit && mode === "remove") {
                var index = localData.businessUnits.indexOf(oldBusinessUnit);
                localData.businessUnits.splice(index, 1);
            }
            else if (oldBusinessUnit) {
                oldBusinessUnit.Name = businessUnit.Name;
                oldBusinessUnit.Description = businessUnit.Description;
            } else {
                var newBusinessUnit = angular.copy(businessUnit);
                newBusinessUnit.Packages = [];
                localData.businessUnits.push(newBusinessUnit);
            }

            common.$broadcast(config.events.businessUnitsLoaded, localData.businessUnits);
        }
    }
})();
