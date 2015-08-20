(function () {
    'use strict';

    var controllerId = 'apiDescription';

    angular.module('app').controller(controllerId,
        ['$scope', '$timeout', 'common', 'remiapi', apiDescription]);

    function apiDescription($scope, $timeout, common, remiapi) {
        var logger = common.logger.getLogger(controllerId);

        var vm = this;

        vm.state = {
            isBusy: false
        };

        vm.descriptions = [];
        vm.displayOptions = {
            expandAll: false
        };
        vm.filterByName = '';
        vm.apiPath = remiapi.apiPath;

        vm.refreshData = refreshData;
        vm.fillParsed = fillParsed;
        vm.descriptionClick = descriptionClick;
        vm.expandAllChanged = expandAllChanged;
        vm.filterByNameChanged = filterByNameChanged;
        vm.manageDescription = manageDescription;
        vm.saveApiDescription = saveApiDescription;
        vm.hideManageDescriptionModal = hideManageDescriptionModal;

        activate();

        return vm;

        function activate() {
            common.activateController([refreshData()], controllerId)
                .then(function () { logger.console('Activated ApiDescription View'); });
        }

        function refreshData() {
            vm.state.isBusy = true;
            vm.descriptions = [];
            vm.displayOptions.expandAll = false;
            vm.filterByName = '';

            return remiapi
                .get.apiDescriptions()
                .then(function (data) {
                    vm.descriptions = data.ApiDescriptions;
                }, function (fault) {
                    logger.error('Request failed!');
                    logger.console('error');
                    console.log(fault);
                }).finally(function () {
                    vm.state.isBusy = false;

                    $timeout(function () {
                        for (var i = 0; i < vm.descriptions.length; i++) {
                            var desc = vm.descriptions[i];
                            vm.fillParsed(desc);
                        }
                    }, 0);
                });
        }

        function fillParsed(desc) {
            if (desc.OutputFormat)
                try {
                    var objOut = JSON.parse(desc.OutputFormat);
                    if (objOut != null) {
                        desc.OutputFormatParsed = objOut;
                    }
                } catch (ex) {
                    console.log('error on parsing ' + desc.Name, ex);
                    console.log(desc.OutputFormat);
                }

            if (desc.InputFormat)
                try {
                    var objIn = JSON.parse(desc.InputFormat);
                    if (objIn != null) {
                        desc.InputFormatParsed = objIn;
                    }
                } catch (ex) {
                    console.log('error on parsing ' + desc.Name, ex);
                    console.log(desc.InputFormat);
                }
        }

        function descriptionClick(item) {
            item.isExpanded = !item.isExpanded;
        }

        function expandAllChanged() {
            if (vm.displayOptions.expandAll) return;

            Enumerable.From(vm.descriptions)
                .Where(function (x) { return x.isExpanded; })
                .ForEach(function (x) {
                    x.isExpanded = false;
                });
        }

        function filterByNameChanged() {
            for (var i = 0; i < vm.descriptions.length; i++) {
                var dn = vm.descriptions[i];

                if (vm.filterByName) {
                    dn.isHidden = (dn.Name && dn.Name.toLowerCase().indexOf(vm.filterByName.toLowerCase()) == -1 && !dn.Description)
                        || (dn.Name && dn.Name.toLowerCase().indexOf(vm.filterByName.toLowerCase()) == -1 && dn.Description && dn.Description.toLowerCase().indexOf(vm.filterByName.toLowerCase()) == -1);
                } else {
                    if (dn.isHidden)
                        dn.isHidden = false;
                }
            }
        }

        function manageDescription(desc) {
            vm.managingDescription = desc;
            vm.managingDescriptionBackUp = vm.managingDescription.Description;
            $('#changeApiDescriptionModal').modal('show');
        }

        function hideManageDescriptionModal() {
            vm.managingDescription.Description = vm.managingDescriptionBackUp;
            $('#changeApiDescriptionModal').modal('hide');
        }

        function saveApiDescription() {
            if (vm.managingDescription.Description == vm.managingDescriptionBackUp) {
                vm.hideManageDescriptionModal();
                return null;
            }
            
            vm.state.isBusy = true;
            return remiapi.post.updateApiDescription({ApiDescription: vm.managingDescription})
                .then(function() {
                        vm.managingDescriptionBackUp = vm.managingDescription.Description;
                    },
                    function (error) {
                        logger.error('Cannot update API description');
                        logger.console(error);
                    })
                .finally(function () {
                vm.state.isBusy = false;
                vm.hideManageDescriptionModal();
            });
        }
    }
})();
