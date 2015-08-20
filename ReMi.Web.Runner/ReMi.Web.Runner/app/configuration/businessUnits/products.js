(function () {
    "use strict";

    var controllerId = "products";

    angular.module("app").controller(controllerId,
        ["$scope", "$rootScope", "common", "remiapi", "localData", "config", products]);

    function products($scope, $rootScope, common, remiapi, localData, config) {
        var logger = common.logger.getLogger(controllerId);
        var $q = common.$q;
        var vm = this;

        vm.getProducts = getProducts;
        vm.addProduct = addProduct;
        vm.showManageProductModal = showManageProductModal;
        vm.hideAddProductModal = hideAddProductModal;
        vm.activate = activate;
        vm.getEnums = getEnums;
        vm.updateProduct = updateProduct;

        vm.products = [];
        vm.track = [];
        vm.qaModes = [];
        vm.productToManage = undefined;
        vm.state = { isBusy: false };

        common.handleEvent(config.events.businessUnitsLoaded, businesUnitsLoadedHandler, $scope);

        vm.activate();

        function activate() {
            common.activateController([vm.getEnums()], controllerId)
                .then(function () {
                    logger.console("Activated Products View");
                });
        }

        function getProducts() {
            return localData.businessUnitsPromise()
                .then(function (data) {
                    if (data) {
                        var businessUnits = Enumerable.From(angular.copy(data));
                        var tracks = Enumerable.From(vm.track || []);
                        businessUnits.ForEach(function (bu) {
                            Enumerable.From(bu.Packages).ForEach(function (p) {
                                p.BusinessUnit = bu.Name;
                                p.BusinessUnitDesc = bu.Description;
                                p.BusinessUnitId = bu.ExternalId;
                                if (tracks.Any()) {
                                    p.ReleaseTrackDescription = tracks.First(function (t) {
                                        return t.Name === p.ReleaseTrack;
                                    }).Description;
                                }
                            });
                        });
                        vm.businessUnits = businessUnits.ToArray();
                        vm.products = businessUnits.SelectMany(function (bu) { return bu.Packages; }).ToArray();
                    } else {
                        vm.businessUnits = [];
                        vm.products = [];
                    }
                });
        }

        function getEnums() {
            return localData.getEnum('ReleaseTrack')
                .then(function (data) {
                    vm.track = data;
                }, function (error) {
                    logger.error("Cannot get release track and QA mode");
                    logger.console(error);
                }).then(getProducts);
        }

        function addProduct() {
            if (vm.productToManage == undefined || vm.productToManage.name === "") {
                logger.warn("Cannot add empty product");
                return;
            }
            if (vm.products.filter(function (item) {
                return item.Description === vm.productToManage.Description;
            }).length > 0) {
                logger.warn("Product already exists");
                return;
            }

            vm.state.isBusy = true;
            var prod = {
                ExternalId: vm.productToManage.ExternalId,
                Description: vm.productToManage.Description,
                ChooseTicketsByDefault: vm.productToManage.ChooseTicketsByDefault,
                ReleaseTrack: vm.productToManage.ReleaseTrack.Name,
                BusinessUnitId: vm.productToManage.BusinessUnitId
            };
            remiapi.post.addProduct(prod).then(function () {
                var $package = {
                    Name: prod.Description,
                    ExternalId: prod.ExternalId,
                    ReleaseTrack: vm.productToManage.ReleaseTrack.Name,
                    ReleaseTrackDescription: vm.productToManage.ReleaseTrack.Description,
                    ChooseTicketsByDefault: vm.productToManage.ChooseTicketsByDefault
                };
                var businessUnit = Enumerable.From(vm.businessUnits)
                    .First(function (x) { return x.ExternalId === prod.BusinessUnitId; });
                updateBusinessUnit(businessUnit, $package);
                vm.products.push($package);
            }, function (error) {
                logger.error("Cannot add product");
                logger.console(error);
            }).finally(function () {
                vm.state.isBusy = false;
                vm.hideAddProductModal();
            });
        }

        function updateProduct() {
            if (vm.productToManage == undefined || vm.productToManage.name === "") {
                logger.warn("Cannot update empty product");
                return;
            }

            vm.state.isBusy = true;
            var prod = {
                ExternalId: vm.productToManage.ExternalId,
                Description: vm.productToManage.Description,
                ChooseTicketsByDefault: vm.productToManage.ChooseTicketsByDefault,
                ReleaseTrack: vm.productToManage.ReleaseTrack.Name,
                BusinessUnitId: vm.productToManage.BusinessUnitId
            };
            remiapi.post.updateProduct(prod).then(function () {
                var $package = vm.products.filter(function (x) {
                    return x.ExternalId === prod.ExternalId;
                })[0];

                $package.Name = prod.Description;
                $package.ReleaseTrack = prod.ReleaseTrack;
                $package.ReleaseTrackDescription = vm.productToManage.ReleaseTrack.Description;
                $package.ChooseTicketsByDefault = prod.ChooseTicketsByDefault;
                var businessUnit = Enumerable.From(vm.businessUnits)
                    .First(function (x) { return x.ExternalId === prod.BusinessUnitId; });
                if (businessUnit.ExternalId !== $package.BusinessUnitId) {
                    updateBusinessUnit(businessUnit, $package);
                }

            }, function (error) {
                logger.error("Cannot update product");
                logger.console(error);
            }).finally(function () {
                vm.state.isBusy = false;
                vm.hideAddProductModal();
            });
        }

        function showManageProductModal(prod) {
            if (prod) {
                vm.productMode = "Update";
                vm.productToManage = {
                    Description: prod.Name,
                    ReleaseTrack: vm.track.filter(function (x) {
                        return x.Name === prod.ReleaseTrack;
                    })[0],
                    ExternalId: prod.ExternalId,
                    ChooseTicketsByDefault: prod.ChooseTicketsByDefault,
                    BusinessUnitId: prod.BusinessUnitId
                };
            } else {
                vm.productMode = "Add";
                vm.productToManage = {
                    Description: "",
                    ReleaseTrack: vm.track[0],
                    ExternalId: newGuid(),
                    ChooseTicketsByDefault: false,
                    BusinessUnitId: vm.businessUnits[0].ExternalId
                };
            }

            $("#manageProductModal").modal("show");
        }

        function hideAddProductModal() {
            $("#manageProductModal").modal("hide");
        }

        function businesUnitsLoadedHandler() {
            getProducts();
        }

        function updateBusinessUnit(newBusinessUnit, $package) {
            var oldBusinessUnit = Enumerable.From(vm.businessUnits)
                .Where(function (x) { return x.ExternalId === $package.BusinessUnitId; })
                .FirstOrDefault();

            var localPackage = undefined, index;

            if (oldBusinessUnit) {
                index = oldBusinessUnit.Packages.indexOf($package);
                oldBusinessUnit.Packages.splice(index, 1);
            } else {
                localPackage = angular.copy($package);
            }

            $package.BusinessUnitId = newBusinessUnit.ExternalId;
            $package.BusinessUnit = newBusinessUnit.Name;
            $package.BusinessUnitDesc = newBusinessUnit.Description;


            newBusinessUnit.Packages.push($package);

            // update also in localData
            var enumBusinessUnits = Enumerable.From(localData.businessUnits);
            newBusinessUnit = enumBusinessUnits.First(function (x) { return x.ExternalId === newBusinessUnit.ExternalId; });

            if (oldBusinessUnit) {
                oldBusinessUnit = enumBusinessUnits.First(function (x) { return x.ExternalId === oldBusinessUnit.ExternalId; });
                $package = Enumerable.From(oldBusinessUnit.Packages).First(function (x) { return x.ExternalId === $package.ExternalId; });
                index = oldBusinessUnit.Packages.indexOf($package);
                oldBusinessUnit.Packages.splice(index, 1);
                newBusinessUnit.Packages.push($package);
            } else {
                newBusinessUnit.Packages.push(localPackage);
            }
        }
    }
})();
