(function () {
    'use strict';
    var controllerId = 'release';
    angular.module('app').controller(controllerId, ['$rootScope', '$scope', '$location', '$timeout', 'common', 'config', 'remiapi', 'authService', 'notifications', 'localData', release]);

    function release($rootScope, $scope, $location, $timeout, common, config, remiapi, authService, notifications, localData) {
        var vm = this;

        var $q = common.$q;
        var logger = common.logger.getLogger(controllerId);

        vm.controllerId = controllerId;
        vm.state = {
            isBusy: true,
            isLoaded: true,
            isBindedToRelease: false
        };
        vm.showDescriptionEditor = false;
        vm.releaseTypeClass = '';

        vm.releaseSelector = {
            isOpen: false
        };

        var locationSearch = $location.search();
        vm.activeTab = {
            plan: false,
            exec: false,
            setActive: function (tab) {
                switch (tab) {
                    case 'exec':
                        $location.search('tab', 'execution');
                        vm.activeTab.plan = false;
                        vm.activeTab.exec = true;
                        break;
                    default:
                        $location.search('tab', 'plan');
                        vm.activeTab.exec = false;
                        vm.activeTab.plan = true;
                }
            }
        };
        vm.activeTab.setActive((locationSearch.tab && locationSearch.tab == 'execution') ? 'exec' : 'plan');
        vm.queryParameters = {
            releaseWindowId: locationSearch.releaseWindowId
        };

        var releaseWindowLoadedEvent = 'release.ReleaseWindowLoadedEvent';

        // server notifications
        var releaseStatusChangedEventName = 'ReleaseStatusChangedEvent';
        var releaseWindowClosedEventName = 'ReleaseWindowClosedEvent';
        var releaseDecisionChangedEventName = 'ReleaseDecisionChangedEvent';

        vm.currentReleaseWindow = undefined;
        vm.people = [];
        vm.product = null;
        vm.setProduct = setProduct;
        vm.setReleaseWindowId = setReleaseWindowId;
        vm.showEditor = false;
        vm.enableCloseRelease = false;
        vm.isClosed = false;
        vm.showDescriptionEditor = false;
        vm.disableSaveDescription = true;
        vm.closeRelease = closeRelease;
        vm.processReleaseStatus = processReleaseStatus;
        vm.handleReleaseStatusChange = handleReleaseStatusChange;
        vm.handleReleaseDecisionChange = handleReleaseDecisionChange;
        vm.handleReleaseWindowClosed = handleReleaseWindowClosed;
        vm.saveDescription = saveDescription;
        vm.releases = [];
        vm.unbindRelease = unbindRelease;
        vm.bindRelease = bindRelease;
        vm.broadcastEvents = broadcastEvents;
        vm.loadReleaseWindowById = loadReleaseWindowById;
        vm.getNearReleases = getNearReleases;
        vm.chooseToBind = chooseToBind;
        vm.initReleaseTypeClass = initReleaseTypeClass;
        vm.initReleaseDecisionClass = initReleaseDecisionClass;
        vm.getPeople = getPeople;
        vm.descriptionBackUp = '';
        vm.helpdeskTickets = [];
        vm.tickets = [];
        vm.releaseChanges = [];
        vm.loadInitialisationData = loadInitialisationData;
        vm.loadReleaseWindow = loadReleaseWindow;
        vm.hasPlugin = hasPlugin;

        vm.productContextChanged = productContextChanged;
        vm.serverNotificationHandler = serverNotificationHandler;
        vm.routeUpdateHandler = routeUpdateHandler;
        vm.ticketsLoadedHandler = ticketsLoadedHandler;
        vm.releaseChangesLoadedHandler = releaseChangesLoadedHandler;
        vm.intersectChangesAndTickets = intersectChangesAndTickets;

        common.handleEvent(config.events.productContextChanged, vm.productContextChanged, $scope);
        common.handleEvent(config.events.notificationReceived, vm.serverNotificationHandler, $scope);
        common.handleEvent(config.events.navRouteUpdate, vm.routeUpdateHandler, $scope);
        common.handleEvent(config.events.closeReleaseOnSignOffEvent, vm.closeRelease, $scope);
        common.handleEvent('releaseContent.ticketsLoaded', vm.ticketsLoadedHandler, $scope);
        common.handleEvent('releaseChanges.releaseChangesLoaded', vm.releaseChangesLoadedHandler, $scope);
        common.handleEvent(config.events.businessUnitsLoaded, businessUnitsLoadedHandler, $scope);

        $scope.$on('$destroy', scopeDestroyHandler);
        $scope.$watch('vm.showDescriptionEditor', function () {
            vm.disableSaveDescription = !vm.showDescriptionEditor;
        }, false);

        var widgets = [];
        vm.registerWidget = registerWidget;
        vm.isAllWidgetsLoaded = false;
        $scope.registerWidget = vm.registerWidget;

        activate();

        function activate() {
            common.activateController([vm.loadInitialisationData().then(vm.loadReleaseWindow)], controllerId)
                .then(function () { logger.console('Activated Release Plan View'); });
        }

        function businessUnitsLoadedHandler() {
            vm.businessUnits = localData.businessUnits;
        }

        function loadInitialisationData() {
            vm.state.isBusy = true;

            return localData.getEnum('ReleaseType')
                .then(function (data) {
                    vm.releaseTypes =
                    Enumerable.From(data)
                        .Where(function (x) { return x.Name != 'Automated'; })
                        .ToArray();;
                })
            .then(function () { return localData.businessUnitsPromise(); })
            .then(function(businessUnits) {
                vm.businessUnits = businessUnits;
            });
        }


        function loadReleaseWindow() {
            if (vm.queryParameters.releaseWindowId) {
                logger.console('All widgets are loaded. Loading release window by Id=' + vm.queryParameters.releaseWindowId);
                return vm.setReleaseWindowId(vm.queryParameters.releaseWindowId)
                    .then(
                        function () { },
                        function () { $location.path('/').search({}); }
                    );
            } else {
                logger.console('All widgets are loaded. Loading release window by Product=' + authService.identity.product.Name);
                return vm.setProduct(authService.identity.product);
            }
        }

        function scopeDestroyHandler() {
            notifications.unsubscribe(releaseStatusChangedEventName);
            notifications.unsubscribe(releaseDecisionChangedEventName);
            notifications.unsubscribe(releaseWindowClosedEventName);
            vm.tickets = [];
            vm.releaseChanges = [];
        }

        function productContextChanged(product) {
            vm.setProduct(product);
        }

        function setProduct(product) {
            if (!product) {
                logger.console('Could not use empty product context');
                return $q.when();
            }

            if (vm.product && vm.product.Name == product.Name) {
                logger.console('Product the same. Ignore product context change');
                return $q.when();
            }

            vm.product = product;

            if (!!$location.search().releaseWindowId)
                $location.search('releaseWindowId', null);

            vm.state.isBusy = true;

            logger.console('Set product to ' + product.Name);

            return vm.unbindRelease()
                .then(vm.getNearReleases)
                .then(function (firstNonClosedRelease) {
                    if (firstNonClosedRelease) {
                        return vm.loadReleaseWindowById(firstNonClosedRelease.ExternalId);
                    }
                    return $q.when();
                })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function setReleaseWindowId(releaseWindowId) {
            vm.state.isBusy = true;

            return vm.unbindRelease()
                .then(function () { return vm.loadReleaseWindowById(releaseWindowId); })
                .then(function () { return vm.getNearReleases(); })
                .finally(function () {
                    vm.state.isBusy = false;
                });
        }

        function loadReleaseWindowById(releaseWindowId) {
            var deferred = $q.defer();

            if (!authService.isLoggedIn) {
                deferred.reject();
                return deferred.promise;
            }

            if (!releaseWindowId) {
                deferred.reject();
                return deferred.promise;
            }

            remiapi.getRelease(releaseWindowId)
                .then(function (response) {
                    var r = response.ReleaseWindow;
                    var productsEnumer = Enumerable.From(r.Products);

                    if (!vm.product || !productsEnumer.FirstOrDefault(null, function (x) { return vm.product.Name == x; })) {
                        var found = Enumerable
                            .From(authService.identity.products)
                            .FirstOrDefault(null, function (x) {
                                if (productsEnumer.FirstOrDefault(null, function (y) { return y == x.Name; }))
                                    return x;

                                return null;
                            });

                        if (found) {
                            if (!vm.product || found.Name !== vm.product.Name) {
                                vm.product = found;

                                authService.selectProduct(found);
                            }
                        } else
                            vm.product = { Name: r.Products[0] };
                    }

                    vm.bindRelease(r);

                    deferred.resolve(r);

                }, function () {
                    logger.warn('Can\'t load requested release!');
                    deferred.reject();
                });

            return deferred.promise;
        }

        function unbindRelease() {
            if (vm.currentReleaseWindow) {
                logger.console('Unbind from release window Id=' + vm.currentReleaseWindow.ExternalId);

                vm.state.isBindedToRelease = false;
                vm.currentReleaseWindow = null;
                vm.releases = [];

                notifications.unsubscribe(releaseStatusChangedEventName);
                notifications.unsubscribe(releaseDecisionChangedEventName);
                notifications.unsubscribe(releaseWindowClosedEventName);
                common.sendEvent(releaseWindowLoadedEvent);

                vm.people = [];

                vm.isAllWidgetsLoaded = false;
                widgets = [];
            }

            return $q.when();
        }

        function bindRelease(rel) {
            if (!rel) {
                logger.console('Could\'t bind empty release!');
                return $q.when();
            }
            vm.releaseSelector.isOpen = false;
            vm.state.isBindedToRelease = true;

            vm.currentReleaseWindow = rel;
            $scope.currentReleaseWindow = rel;

            vm.isCurrentReleaseWindowMaintenance = !!Enumerable.From(vm.releaseTypes)
                .FirstOrDefault(null, function (x) {
                    return x.IsMaintenance && x.Name.toLowerCase() == vm.currentReleaseWindow.ReleaseType.toLowerCase();
                });

            vm.descriptionBackUp = vm.currentReleaseWindow.Description;

            vm.initReleaseTypeClass(vm.currentReleaseWindow.ReleaseType);
            vm.initReleaseDecisionClass(vm.currentReleaseWindow.ReleaseDecision);
            vm.processReleaseStatus();
            logger.console('Binded to release window ' + vm.currentReleaseWindow.ExternalId);

            if (vm.businessUnits) {
                vm.releasePackage = Enumerable.From(vm.businessUnits)
                    .SelectMany(function(x) { return x.Packages; })
                    .Where(function(x) { return x.Name === rel.Products[0]; })
                    .FirstOrDefault();
            }

            return $q.when();
        }

        function broadcastEvents() {
            var deferred = $q.defer();
            deferred.resolve();

            $timeout(function () {
                if (vm.currentReleaseWindow) {
                    common.sendEvent(releaseWindowLoadedEvent, vm.currentReleaseWindow);

                    notifications.subscribe(releaseStatusChangedEventName, { 'ReleaseWindowId': vm.currentReleaseWindow.ExternalId });
                    notifications.subscribe(releaseDecisionChangedEventName, { 'ReleaseWindowId': vm.currentReleaseWindow.ExternalId });
                    notifications.subscribe(releaseWindowClosedEventName, { 'ReleaseWindowId': vm.currentReleaseWindow.ExternalId });

                    $location.search('releaseWindowId', vm.currentReleaseWindow.ExternalId);
                }
            }, 0, false);

            return deferred.promise;
        }

        function getNearReleases() {
            var deferred = $q.defer();

            if (!vm.product) {
                deferred.reject();
                logger.console('Get nearest releases for empty product');
            } else {
                logger.console('Get nearest releases for product ' + vm.product.Name);

                remiapi
                    .getNearReleases(vm.product.Name)
                    .then(function (response) {
                        vm.releases = response.ReleaseWindows;

                        var found = Enumerable
                            .From(vm.releases)
                            .LastOrDefault(null, function (x) { return !x.ClosedOn; });

                        if (found) {
                            deferred.resolve(found);
                        } else if (vm.releases.length > 0) {
                            deferred.resolve(vm.releases[0]);
                        } else {
                            deferred.reject();
                        }
                    });
            }

            return deferred.promise;
        }

        function chooseToBind(rel) {
            if (rel && rel.ExternalId)
                vm.setReleaseWindowId(rel.ExternalId);
            else
                logger.console('Couldn\'t choose invalid release for binding');
        }
        function registerWidget(name, callback) {
            if (vm.isAllWidgetsLoaded) {
                logger.console('All widgets already loaded. Ignore this call for widget=' + name);
                return null;
            }

            var found = Enumerable
                .From(widgets)
                .FirstOrDefault(null, function (x) { return x.name == name; });

            if (found) {
                logger.console('Widget already loaded (' + name + ')');
                return null;
            }

            widgets.push({ 'name': name, 'callback': callback });

            if (isAllWidgets()) {
                vm.isAllWidgetsLoaded = true;
                if (vm.currentReleaseWindow) {
                    logger.console('All widgets are loaded. Loading release window by Id=' + vm.currentReleaseWindow.releaseWindowId);
                    return vm.broadcastEvents()
                        .then(
                            function () { },
                            function () { $location.path('/').search({}); }
                        );
                } else {
                    logger.console('All widgets are loaded. Loading release window by Product=' + authService.identity.product.Name);
                    return vm.setProduct(authService.identity.product);
                }
            }

            return null;
        };

        function isAllWidgets() {
            if (vm.isAllWidgetsLoaded)
                return true;
            if (!vm.currentReleaseWindow)
                return false;

            var requiredWidgets = releaseTypeWidgets[vm.currentReleaseWindow.ReleaseType];

            for (var i = 0; i < requiredWidgets.length; i++) {
                var found = Enumerable.From(widgets)
                    .FirstOrDefault(null, function (x) { return x.name == requiredWidgets[i]; });

                if (!found) {
                    return false;
                }
            }

            return true;
        }

        function routeUpdateHandler() {
            var search = $location.search();
            if (search.releaseWindowId) {
                if (!vm.currentReleaseWindow || (vm.currentReleaseWindow.ExternalId && vm.currentReleaseWindow.ExternalId.toLowerCase() !== search.releaseWindowId.toLowerCase())) {
                    logger.console('Route updated. Loading release window ' + search.releaseWindowId);

                    vm.setReleaseWindowId(search.releaseWindowId)
                        .then(
                            function () { },
                            function () { $location.path('/').search({}); }
                        );
                }
            }
        }

        function getPeople() {
            return remiapi.getAccounts().then(function (data) {
                return vm.people = data.Accounts;
            });
        }

        function initReleaseTypeClass(releaseType) {
            if (releaseType) {
                vm.releaseTypeClass = releaseType.toLowerCase() + '-type';
            }
        }

        function initReleaseDecisionClass(releaseDecision) {
            if (releaseDecision) {
                var decision = releaseDecision.toLowerCase().replace(' ', '-');
                vm.releaseDecisionClass = {
                    css: decision + '-decision-class'
                };
                switch (decision) {
                    case 'no-go':
                        vm.releaseDecisionClass.icon = 'fa-thumbs-o-down'; break;
                    case 'go':
                        vm.releaseDecisionClass.icon = 'fa-thumbs-o-up'; break;
                    case 'undermined':
                    default: vm.releaseDecisionClass.icon = 'fa-question';
                }
            }
        }

        function handleReleaseStatusChange(data) {
            if (vm.currentReleaseWindow) {
                vm.currentReleaseWindow.Status = data.ReleaseStatus;
            }

            processReleaseStatus();
        }

        function handleReleaseWindowClosed(data) {
            if (vm.currentReleaseWindow) {
                vm.currentReleaseWindow.IsFailed = data.IsFailed;
            }
        }

        function handleReleaseDecisionChange(data) {
            if (vm.currentReleaseWindow) {
                vm.currentReleaseWindow.ReleaseDecision = data.ReleaseDecision;
                vm.initReleaseDecisionClass(data.ReleaseDecision);
            }
        }

        function processReleaseStatus() {
            var releaseStatus = '';
            if (vm.currentReleaseWindow)
                releaseStatus = vm.currentReleaseWindow.Status;

            vm.enableCloseRelease = releaseStatus == 'Signed off';

            vm.isClosed = (releaseStatus == 'Closed');
        }

        function closeRelease() {
            var showModal = function () {
                $('#closeReleaseModal').modal('show');
                return $q.when();
            };

            if (!vm.people || vm.people.length == 0) {
                return getPeople()
                    .then(function () {
                        showModal();
                    });
            }

            return showModal();
        }

        function serverNotificationHandler(notification) {
            if (notification.name == releaseStatusChangedEventName) {
                vm.handleReleaseStatusChange(notification.data);
            }
            else if (notification.name == releaseWindowClosedEventName) {
                vm.handleReleaseWindowClosed(notification.data);
            }
            else if (notification.name == releaseDecisionChangedEventName) {
                vm.handleReleaseDecisionChange(notification.data);
            }

            return $q.when();
        };

        function saveDescription() {
            if (vm.descriptionBackUp == vm.currentReleaseWindow.Description) {
                logger.console('Release description was not changed');
                return null;
            }

            var commandId = newGuid();
            vm.state.isBusy = true;

            return remiapi.executeCommand('UpdateReleaseWindowCommand', commandId, { ReleaseWindow: vm.currentReleaseWindow, AllowUpdateInPast: true })
                .then(function () {
                    vm.descriptionBackUp = vm.currentReleaseWindow.Description;
                }, function (error) {
                    logger.error('Cannot update release description');
                    console.log(error);
                }).finally(function () {
                    vm.currentReleaseWindow.Description = vm.descriptionBackUp;
                    vm.state.isBusy = false;
                    vm.showDescriptionEditor = false;
                });
        }

        function ticketsLoadedHandler(tickets) {
            vm.tickets = tickets;
            if (vm.releaseChanges)
                vm.intersectChangesAndTickets();
        }

        function releaseChangesLoadedHandler(releaseChanges) {
            vm.releaseChanges = releaseChanges;
            if (vm.tickets)
                vm.intersectChangesAndTickets();
        }

        function intersectChangesAndTickets() {
            var ticketEnumerable = Enumerable.From(vm.tickets);

            var ticketExpr = /\b[A-Z]{1,10}-\d{1,10}/g;
            for (var i = 0; i < vm.releaseChanges.length; i++) {
                var change = vm.releaseChanges[i];

                var matches = change.Description.match(ticketExpr);
                if (matches && matches.length > 0)
                    for (var j = 0; j < matches.length; j++) {
                        var ticketName = matches[j];
                        var found = ticketEnumerable.FirstOrDefault(null, function (x) { return x.TicketName === ticketName; });
                        if (found) {
                            if (!change.tickets) change.tickets = [];
                            change.tickets.push({ name: found.TicketName, url: found.TicketUrl });

                            if (!found.releaseChanges) found.releaseChanges = [];
                            found.releaseChanges.push({ identifier: change.Identifier });
                        }
                    }
            }
        }

        function hasPlugin(pluginType) {
            if (!vm.currentReleaseWindow
                || !vm.currentReleaseWindow.Plugins
                || vm.currentReleaseWindow.Plugins.length === 0)
            return false;

            return Enumerable.From(vm.currentReleaseWindow.Plugins)
                .Any(function (x) { return x.PluginType && x.PluginType.indexOf(pluginType) >= 0; });
        }

        var releaseTypeWidgets = {
            Scheduled: [
                'checkList',
                'releaseApprovers',
                'releaseContent',
                'releaseParticipant',
                'releaseProcess',
                'signOff'
            ],
            Hotfix: [
                'checkList',
                'releaseApprovers',
                'releaseParticipant',
                'releaseProcess',
                'signOff'
            ],
            ChangeRequest: [
                'checkList',
                'releaseApprovers',
                'releaseParticipant',
                'releaseProcess',
                'signOff'
            ],
            Pci: [
                'checkList',
                'releaseApprovers',
                'releaseContent',
                'releaseParticipant',
                'signOff'
            ],
            SystemMaintenance: [
                'checkList',
                'releaseApprovers',
                'releaseContent',
                'releaseParticipant',
                'signOff'
            ],
            CorpIT: [
                'checkList',
                'releaseApprovers',
                'releaseContent',
                'releaseParticipant',
                'signOff'
            ],
            ThirdParty: [
                'checkList',
                'releaseApprovers',
                'releaseContent',
                'releaseParticipant',
                'signOff'
            ],
            Automated: [
                'releaseContent',
                'releaseProcess',
                'signOff'
            ],
        };
    }
})();
