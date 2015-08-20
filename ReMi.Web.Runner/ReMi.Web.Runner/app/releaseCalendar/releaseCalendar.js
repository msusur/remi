(function () {
    "use strict";
    var controllerId = "releaseCalendar";

    angular.module("app").controller(controllerId, ["$scope", "$location", "common", "config", "remiapi", "authService", "localData", "$timeout", releaseCalendar]);

    function releaseCalendar($scope, $location, common, config, remiapi, authService, localData, $timeout) {
        var logger = common.logger.getLogger(controllerId);
        var vm = this;
        var search = $location.search();

        vm.state = {
            isBusy: true
        };
        vm.title = "Release Calendar";

        if (search && search.returnPath) {
            logger.console("Stop page loading. The application should navigate to: " + search.returnPath);
            return;
        }

        vm.calendarData = [];
        vm.contextMenuState = { style: { left: "0px", top: "0px" }, shown: false };
        vm.timePickerViewName = "minute";

        vm.initDeferred = common.$q.defer();

        vm.filterReleaseType = filterReleaseType;
        vm.viewChanged = viewChanged;
        vm.eventClick = eventClick;
        vm.contextmenu = contextmenu;
        vm.navigateToReleasePlan = navigateToReleasePlan;
        vm.showReleaseWindowModal = showReleaseWindowModal;
        vm.hideModal = hideModal;
        vm.removeReleaseWindow = removeReleaseWindow;
        vm.addReleaseWindow = addReleaseWindow;
        vm.updateReleaseWindow = updateReleaseWindow;
        vm.startTimeChanged = startTimeChanged;

        vm.refresh = function () {
            refresh(vm.calendar.options.position.start, vm.calendar.options.position.end);
        }


        common.handleEvent(config.events.productContextChanged, productContextChangedHandler, $scope);
        common.handleEvent(config.events.businessUnitsLoaded, businessUnitsLoadedHandler, $scope);
        $(document).on("click", hideContextMenu);

        $scope.$on("$destroy", function () {
            $(document).unbind("click", hideContextMenu);
        });

        clearValidation();
        activate();

        function activate() {
            if (!vm.state.isSupressLoading) {
                common.activateController([init()], controllerId)
                    .then(function () {
                        logger.console("Activated Release Calendar View");
                        vm.state.isBusy = false;
                    });
            }
        }

        function init() {
            return localData.getEnum("ReleaseType")
                .then(function (releaseTypes) { vm.releaseTypes = releaseTypes; })
                .then(function () { return authService.identityPromise; })
                .then(function (identity) { vm.currentProduct = identity.product; })
                .then(function () { return localData.businessUnitsPromise(); })
                .then(businessUnitsLoadedHandler)
                .finally(function () { vm.initDeferred.resolve(); });
        }

        function refresh(start, end) {
            vm.state.isBusy = true;
            var format = function (date) { return date.format("YYYY-MM-DD"); };
            return vm.initDeferred.promise.then(function () {
                return remiapi.get.releaseCalendar(format(start), format(end))
                    .then(function (response) {
                        vm.calendarData.length = [];
                        angular.forEach(response.ReleaseWindows, function (releaseWindow) {
                            vm.calendarData.push(getCalendarEvent(releaseWindow));
                        });
                    }, function (error) {
                        logger.error("Cannot get releases between " + format(start) + " and " + format(end));
                        logger.console(error);
                    })
                    .finally(function () {
                        $timeout(function () { vm.state.isBusy = false; });
                    });
            });
        }

        function filterReleaseType(releaseType) {
            return releaseType.Name !== "Automated"
                && releaseType.IsMaintenance === vm.isMaintenanceWindow;
        }

        function getCalendarEvent(releaseWindow) {
            var typeClass = releaseWindow.Status === "Closed" ? "" : releaseWindow.ReleaseType.toLowerCase() + "-type";
            return {
                id: releaseWindow.ExternalId,
                title: releaseWindow.IsMaintenance ? getFirstChars(releaseWindow.Sprint) : releaseWindow.Products,
                fullTitle: releaseWindow.IsMaintenance ? releaseWindow.Sprint : releaseWindow.Products + ", " + releaseWindow.Sprint,
                start: moment(releaseWindow.StartTime),
                end: moment(releaseWindow.EndTime),
                isFailed: releaseWindow.Status === "Closed" && releaseWindow.IsFailed,
                state: getEventState(releaseWindow),
                tooltip: releaseWindow.Products,
                "class": releaseWindow.Status === "Closed"
                    ? "release-closed btn event-important"
                    : "btn event-important " + typeClass,
                releaseWindow: releaseWindow,
                typeClass: typeClass
            };
        }

        function getEventState(releaseWindow) {
            var result = { isDisabled: true, isAllowed: false };
            if (releaseWindow && vm.currentProduct) {
                var productsEnum = Enumerable.From(releaseWindow.Products);
                var packagesEnum = Enumerable.From(vm.businessUnits)
                    .SelectMany(function (x) { return x.Packages; });
                result.isDisabled = releaseWindow.Status === "Closed" || productsEnum
                    .All(function (x) { return x !== vm.currentProduct.Name; });
                result.isAllowed = packagesEnum.Any(function (x) {
                    return productsEnum.Any(function (p) { return x.Name === p; });
                });
                result.isApproved = releaseWindow.Status === "Approved";
            }

            return result;
        }

        function getFirstChars(text, len) {
            if (!text || !text.length) return "";

            var chars = len || 10;

            if (text.length > chars)
                return text.substring(0, chars) + "...";

            return text;
        }

        function viewChanged(view, calendar) {
            if (calendar) {
                if (vm.currentView
                    && (vm.currentView.view && vm.currentView.view === view)
                    && (vm.currentView.start && vm.currentView.start.isSame(calendar.options.position.start, "day"))
                    && (vm.currentView.end && vm.currentView.end.isSame(calendar.options.position.end, "day"))) {

                    vm.state.isBusy = false;
                    return;
                }

                vm.currentView = {
                    view: view,
                    start: calendar.options.position.start,
                    end: calendar.options.position.end
                };

                refresh(vm.currentView.start, vm.currentView.end);
            }
        };

        function contextmenu(date, event, eventElement, calendar) {
            if (!vm.currentProduct) {
                logger.warn("Package is not defined");
                return;
            }
            vm.eventUnderModification = event;
            if (event) {
                vm.currentReleaseWindow = angular.copy(event.releaseWindow);
            } else {
                switch (calendar.options.view) {
                    case "month": date = date.isSame(moment(), "day")
                            ? moment().startOf("hour").add(1, "hour") : date.startOf("day").add(22, "hours");
                        break;
                    case "year": date = date.isSame(moment(), "month")
                            ? moment().startOf("hour").add(1, "hour") : date.startOf("day").add(22, "hours");
                        break;
                }
                vm.currentReleaseWindow = getNewReleaseWindow(date, vm.currentProduct.Name, vm.releaseTypes[0].Name, true, vm.releaseTypes[0].IsMaintenance);
            }
            vm.allowNavigateToRelease = event && event.state.isAllowed;
            vm.allowRemoveFromContextMenu = event && !event.state.isDisabled;
            vm.allowEditFromContextMenu = vm.allowRemoveFromContextMenu
                && event.releaseWindow.ReleaseType !== "Automated"
                && event.releaseWindow.Status !== "Closed";

            vm.contextMenuState.style.top = eventElement.pageY;
            vm.contextMenuState.style.left = eventElement.pageX;
            vm.contextMenuState.shown = true;
        };

        function hideContextMenu() {
            $timeout(function () {
                vm.contextMenuState.shown = false;
            });
        }

        function navigateToReleasePlan() {
            if (vm.currentReleaseWindow && vm.currentReleaseWindow.ExternalId) {
                var modal = $("#releaseWindowModal");
                if (modal.is(":visible")) {
                    modal.on("hidden.bs.modal", function () {
                        modal.off("hidden.bs.modal");
                        $scope.$apply(function () {
                            $location.path("/releasePlan").search({ 'releaseWindowId': vm.currentReleaseWindow.ExternalId });
                        });
                    });
                    vm.hideModal();
                } else {
                    $location.path("/releasePlan").search({ 'releaseWindowId': vm.currentReleaseWindow.ExternalId });
                }
            }
        }

        function eventClick(event) {
            if (!event
                || !event.releaseWindow
                || !event.releaseWindow.Products
                || !vm.currentProduct
                || event.releaseWindow.ReleaseType === "Automated"
                || Enumerable.From(event.releaseWindow.Products).All(function (x) { return x !== vm.currentProduct.Name; })) {
                return;
            }

            vm.eventUnderModification = event;
            vm.currentReleaseWindow = angular.copy(event.releaseWindow);

            showReleaseWindowModal("update", !!event.releaseWindow.IsMaintenance);
        };

        function showReleaseWindowModal(mode, isMaintenance) {
            clearValidation();
            vm.showDescriptionEditor = false;
            vm.releseWindowModalMode = mode;

            if (mode === "add") {
                vm.currentReleaseWindow.ExternalId = newGuid();
                vm.currentReleaseWindow.Products = [vm.currentProduct.Name];
                vm.isMaintenanceWindow = vm.currentReleaseWindow.IsMaintenance = !!isMaintenance;
                vm.eventUnderModification = null;
                vm.currentReleaseWindow.ReleaseType = Enumerable.From(vm.releaseTypes)
                    .First(function (x) { return vm.isMaintenanceWindow === !!x.IsMaintenance; }).Name;
            }
            else if (mode === "update") {
                vm.isMaintenanceWindow = vm.currentReleaseWindow.IsMaintenance;
            }

            vm.allowUpdateCurrentRelease = (vm.currentReleaseWindow.Status !== "Closed");
            vm.allowRemoveCurrentRelease = (vm.currentReleaseWindow.Status === "Opened");

            updateFiledTitles();

            if (vm.isMaintenanceWindow) {
                prepareBusinessUnits(vm.currentReleaseWindow);
            }

            $("#releaseWindowModal").modal({ backdrop: "static", keyboard: true });
        }

        function prepareBusinessUnits(releaseWindow) {
            var businessUnits = Enumerable.From(vm.businessUnits);
            businessUnits.ForEach(function (bu) {
                if (bu.Packages && bu.Packages.length > 0) {
                    Enumerable.From(bu.Packages).ForEach(function ($package) {
                        $package.Checked = false;
                        var found = Enumerable.From(releaseWindow.Products).Where(function (p) {
                            return $package.Name === p;
                        }).FirstOrDefault();
                        if (found) {
                            $package.Checked = true;
                            $package.IsDefault = found.IsDefault;
                        }
                    });
                }
            });
            vm.businessUnits = businessUnits.ToArray();
        }

        function updateFiledTitles(releaseWindow) {
            var rw;
            if (releaseWindow)
                rw = releaseWindow;
            else
                rw = vm.currentReleaseWindow;

            vm.fieldTitles = {};
            if (rw.IsMaintenance) {
                vm.fieldTitles.ModalHeader = "Maintenance Window";
                vm.fieldTitles.Sprint = "Headline";
                vm.fieldTitles.Release = "";
            } else {
                vm.fieldTitles.ModalHeader = "Release Window";
                vm.fieldTitles.Sprint = "Sprint";
                vm.fieldTitles.Release = "release";
            }
        }

        function hideModal() {
            $("#releaseWindowModal").modal("hide");
        }

        function getNewReleaseWindow(startTime, product, releaseType, requiresDowntime, isMaintenance) {
            return {
                ExternalId: newGuid(),
                StartTime: moment(startTime).format(),
                EndTime: moment(startTime).add(2, "hours").format(),
                Products: [product],
                Sprint: "",
                Status: "Open",
                ReleaseType: releaseType,
                RequiresDowntime: !!requiresDowntime,
                Description: "",
                IsFailed: false,
                IsMaintenance: !!isMaintenance
            };
        }

        function startTimeChanged(newDate) {
            var newEndTime = moment(newDate).add(2, "h");
            vm.currentReleaseWindow.EndTime = newEndTime.toDate();
        }

        function validate() {
            var deferred = common.$q.defer();

            clearValidation();

            if (!vm.currentReleaseWindow) {
                deferred.reject();
            } else {
                var isValid = true;
                var css = "invalid-data";
                if (!vm.currentReleaseWindow.Sprint || vm.currentReleaseWindow.Sprint === "") {
                    vm.invalidData.sprint.css = css;
                    vm.invalidData.sprint.error = "This field is required";
                    isValid = false;
                }
                if (vm.currentReleaseWindow.IsMaintenance && getCheckedPackages().length <= 0) {
                    vm.invalidData.packages.css = css;
                    vm.invalidData.packages.error = "Please check at least one package";
                    isValid = false;
                }
                if (!vm.currentReleaseWindow.ReleaseType) {
                    vm.invalidData.releaseType.css = css;
                    vm.invalidData.releaseType.error = "Please select release type";
                    isValid = false;
                }
                if (!vm.currentReleaseWindow.StartTime
                    || !vm.currentReleaseWindow.EndTime) {
                    vm.invalidData.date.css = css;
                    vm.invalidData.date.error = "Date cannot be empty";
                    isValid = false;
                } else {
                    var start = moment(vm.currentReleaseWindow.StartTime);
                    var end = moment(vm.currentReleaseWindow.EndTime);
                    if (start.isAfter(end)) {
                        vm.invalidData.date.css = css;
                        vm.invalidData.date.error = "Start cannot be after end";
                        isValid = false;
                    }
                    else if (start.isBefore(moment())) {
                        vm.invalidData.date.css = css;
                        vm.invalidData.date.error = "Release cannot be in the past";
                        isValid = false;
                    }
                }

                if (!isValid) {
                    deferred.reject();
                } else {
                    deferred.resolve();
                }
            }
            return deferred.promise;
        }

        function clearValidation() {
            vm.invalidData = { sprint: {}, date: {}, packages: {}, releaseType: {} };
        }

        function getCheckedPackages() {
            return Enumerable.From(vm.businessUnits)
                .SelectMany(function (x) { return x.Packages; })
                .Where(function (x) { return x.Checked; })
                .Select(function (x) { return x.Name; })
                .ToArray();
        }

        function getReleaseWindowCommandData() {
            var releaseWindow = angular.copy(vm.currentReleaseWindow);
            releaseWindow.StartTime = moment(releaseWindow.StartTime).format();
            releaseWindow.EndTime = moment(releaseWindow.EndTime).format();
            if (releaseWindow.IsMaintenance) {
                releaseWindow.Products = getCheckedPackages();
            }
            if (releaseWindow.ReleaseType === "Hotfix") {
                releaseWindow.RequiresDowntime = false;
            }

            return { ReleaseWindow: releaseWindow };
        }

        function addReleaseWindow() {
            vm.state.isBusy = true;
            return validate()
                .then(function () {
                    var data = getReleaseWindowCommandData();
                    return remiapi.post.bookReleaseWindow(data)
                        .then(function () {
                            hideModal();
                            var event = getCalendarEvent(data.ReleaseWindow);
                            vm.calendarData.push(event);
                        }, function (error) {
                            logger.error(error);
                        });
                }, function () {
                    logger.error("Data are invalid");
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function updateReleaseWindow() {
            vm.state.isBusy = true;
            return validate()
                .then(function () {
                    var data = getReleaseWindowCommandData();
                    return remiapi.post.updateReleaseWindow(data)
                        .then(function () {
                            hideModal();
                            return refreshSingleRelease(data.ReleaseWindow.ExternalId, vm.eventUnderModification);
                        }, function (error) {
                            logger.error(error);
                        });
                }, function () {
                    logger.error("Data are invalid");
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function removeReleaseWindow() {
            var releaseWindow = vm.currentReleaseWindow;
            vm.state.isBusy = true;

            remiapi.post.removeReleaseWindow({ ExternalId: releaseWindow.ExternalId })
                .then(
                    function () {
                        var index = vm.calendarData.indexOf(vm.eventUnderModification);
                        if (index >= 0) {
                            vm.calendarData.splice(index, 1);
                        }
                        vm.hideModal();
                    },
                    function (statusCode) {
                        if (statusCode !== 401) {
                            logger.error(statusCode);
                        }
                    }
                )
                .finally(function () {
                    vm.state.isBusy = false;
                });
        };

        function refreshSingleRelease(releaseWindowId, event) {
            return remiapi.getRelease(releaseWindowId)
                .then(function (response) {
                    var refreshedEvent = getCalendarEvent(response.ReleaseWindow);
                    var index = vm.calendarData.indexOf(event);
                    if (index >= 0) {
                        vm.calendarData.splice(index, 1, refreshedEvent);
                    }
                }, function (error) {
                    logger.error(error);
                });
        }

        function productContextChangedHandler(product) {
            vm.currentProduct = product;
            for (var i in vm.calendarData) {
                if (vm.calendarData.hasOwnProperty(i)) {
                    var event = vm.calendarData[i];
                    event.state = getEventState(event.releaseWindow);
                }
            }
        }

        function businessUnitsLoadedHandler(data) {
            vm.businessUnits = angular.copy(data);
            vm.currentProduct = authService.identity.product;
        }
    }
})();
