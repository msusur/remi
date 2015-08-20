describe("PluginsConfiguration Controller", function () {
    var sut, mocks, logger, parent;
    var activateDeferred, initDeferred, businessUnitsDeferred;

    beforeEach(function () {
        module("app");
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope, $location) {
        activateDeferred = $q.defer();
        initDeferred = $q.defer();
        businessUnitsDeferred = $q.defer();
        mocks = {
            $scope: $rootScope.$new(),
            $rootScope: $rootScope,
            common: {
                logger: window.jasmine.createSpyObj("logger", ["getLogger"]),
                activateController: window.jasmine.createSpy("activateController"),
                getParentScope: jasmine.createSpy("getParentScope"),
                common: $q,
                handleEvent: jasmine.createSpy("handleEvent ")
            },
            remiapi: {
                get: jasmine.createSpyObj("remiapi.get", ["plugins", "pluginConfituration", "globalPluginConfiguration", "packagePluginConfiguration"]),
                post: jasmine.createSpyObj("remiapi.post", ["updatePluginGlobalConfiguration", "updatePluginPackageConfiguration"])
            },
            $q: $q,
            config: {
                events: {
                    navRouteUpdate: "navRouteUpdate"
                }
            },
            $location: $location,
            localData: {
                businessUnitsPromise: function () { return businessUnitsDeferred.promise; }
            }
        };
        parent = {
            initializationPromise: initDeferred.promise
        };
        logger = window.jasmine.createSpyObj("logger", ["console", "error", "info", "warn"]);
        mocks.common.logger.getLogger.and.returnValue(logger);
        mocks.common.activateController.and.returnValue(activateDeferred.promise);
        mocks.common.getParentScope.and.returnValue({ vm: parent });
    }));

    function prepareSut() {
        inject(function ($controller) {
            sut = $controller("pluginsConfiguration", mocks);
        });
    }

    it("should call activateController method, when created", function () {
        prepareSut();
        activateDeferred.resolve();
        mocks.$scope.$digest();

        expect(sut).toBeDefined();

        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("pluginsConfiguration");
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), "pluginsConfiguration");
        expect(logger.console).toHaveBeenCalledWith("Activated PluginsConfiguration Controller");
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.parent).toEqual(parent);
        expect(mocks.remiapi.get.plugins).not.toHaveBeenCalled();
        expect(sut.activeTab).toEqual({});
        expect(sut.packages).toEqual([]);
        expect(mocks.common.handleEvent).toHaveBeenCalledWith(mocks.config.events.navRouteUpdate, jasmine.any(Function), mocks.$scope);
    });

    describe("initialize", function () {
        it("should get plugins after parent is initialized and load first plugin configuration, when no search parameters defined", function () {
            spyOn(mocks.$location, "search");
            prepareSut();

            var plugins = {
                Plugins: [
                    { PluginKey: "key1", PluginId: "id1" },
                    { PluginKey: "key2", PluginId: "id2" },
                    { PluginKey: "key3", PluginId: "id3" }
                ]
            };
            var pluginsDeferred = mocks.$q.defer();
            mocks.remiapi.get.pluginConfituration.and.returnValue(mocks.$q.defer().promise);
            mocks.remiapi.get.plugins.and.returnValue(pluginsDeferred.promise);

            initDeferred.resolve({});
            mocks.$scope.$digest();

            expect(sut.state.isBusy).toEqual(true);
            expect(mocks.remiapi.get.plugins).toHaveBeenCalled();

            pluginsDeferred.resolve(plugins);
            mocks.$scope.$digest();

            expect(sut.parent.plugins).toEqual(plugins.Plugins);

            expect(sut.state.isBusy).toEqual(false);
            expect(sut.activeTab).toEqual({
                "key1": { isCurrent: true, loaded: true },
                "key2": { isCurrent: false, loaded: false },
                "key3": { isCurrent: false, loaded: false }
            });
            expect(mocks.$location.search).toHaveBeenCalledWith("plugin", "key1");
        });

        it("should get plugins after parent is initialized and load requested plugin configuration, when search parameters defined", function () {
            spyOn(mocks.$location, "search").and.returnValue({ plugin: "key3" });
            prepareSut();

            var plugins = {
                Plugins: [
                    { PluginKey: "key1", PluginId: "id1" },
                    { PluginKey: "key2", PluginId: "id2" },
                    { PluginKey: "key3", PluginId: "id3" }
                ]
            };
            var pluginsDeferred = mocks.$q.defer();
            mocks.remiapi.get.pluginConfituration.and.returnValue(mocks.$q.defer().promise);
            mocks.remiapi.get.plugins.and.returnValue(pluginsDeferred.promise);

            initDeferred.resolve({});
            pluginsDeferred.resolve(plugins);
            mocks.$scope.$digest();

            expect(sut.activeTab).toEqual({
                "key1": { isCurrent: false, loaded: false },
                "key2": { isCurrent: false, loaded: false },
                "key3": { isCurrent: true, loaded: true }
            });
            expect(mocks.$location.search).toHaveBeenCalledWith("plugin", "key3");
        });

        it("should log error, when failed to initialize plugins", function () {
            prepareSut();

            var error = { Message: "Some error" };
            var pluginsDeferred = mocks.$q.defer();
            mocks.remiapi.get.plugins.and.returnValue(pluginsDeferred.promise);

            initDeferred.resolve({});
            pluginsDeferred.reject(error);
            mocks.$scope.$digest();

            expect(sut.parent.plugins).toEqual([]);
            expect(logger.error).toHaveBeenCalledWith("Unable to load plugins");
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(error));
            expect(sut.state.isBusy).toEqual(false);
        });
    });

    describe("loadPluginConfiguration", function () {
        it("should load configuration for first plugin, when packages already defined", function () {
            spyOn(mocks.$location, "search");
            prepareSut();

            var plugins = {
                Plugins: [
                    { PluginKey: "key1", PluginId: "id1" },
                    { PluginKey: "key2", PluginId: "id2" },
                    { PluginKey: "key3", PluginId: "id3" }
                ]
            };
            var configuration = { Plugin: { PackageConfiguration: { "package1": { test: "package1" } } } };
            var pluginsDeferred = mocks.$q.defer();
            var pluginConfigurationDeferred = mocks.$q.defer();
            mocks.remiapi.get.pluginConfituration.and.returnValue(pluginConfigurationDeferred.promise);
            mocks.remiapi.get.plugins.and.returnValue(pluginsDeferred.promise);
            sut.packages = [
                { ExternalId: "package1" },
                { ExternalId: "package2" },
                { ExternalId: "package3" }
            ];

            initDeferred.resolve({});
            pluginsDeferred.resolve(plugins);
            businessUnitsDeferred.resolve();
            pluginConfigurationDeferred.resolve(configuration);
            mocks.$scope.$digest();

            expect(sut.parent.plugins[0].PackageConfiguration["package1"]).toEqual({ test: "package1" });
            expect(sut.parent.plugins[0].PackageConfiguration["package2"]).toEqual({});
            expect(sut.parent.plugins[0].PackageConfiguration["package3"]).toEqual({});
        });

        it("should load configuration and packages, when packages not defined", function () {
            spyOn(mocks.$location, "search");
            prepareSut();

            var plugins = {
                Plugins: [
                    { PluginKey: "key1", PluginId: "id1" },
                    { PluginKey: "key2", PluginId: "id2" },
                    { PluginKey: "key3", PluginId: "id3" }
                ]
            };
            var businessUnits = [
                { Packages: [{ ExternalId: "package1" }, { ExternalId: "package2" }] },
                { Packages: [{ ExternalId: "package3" }] }
            ];
            var configuration = { Plugin: { PackageConfiguration: { "package1": { test: "package1" } } } };
            var pluginsDeferred = mocks.$q.defer();
            var pluginConfigurationDeferred = mocks.$q.defer();
            mocks.remiapi.get.pluginConfituration.and.returnValue(pluginConfigurationDeferred.promise);
            mocks.remiapi.get.plugins.and.returnValue(pluginsDeferred.promise);

            initDeferred.resolve({});
            pluginsDeferred.resolve(plugins);
            businessUnitsDeferred.resolve(businessUnits);
            pluginConfigurationDeferred.resolve(configuration);
            mocks.$scope.$digest();

            expect(sut.parent.plugins[0].PackageConfiguration["package1"]).toEqual({ test: "package1" });
            expect(sut.parent.plugins[0].PackageConfiguration["package2"]).toEqual({});
            expect(sut.parent.plugins[0].PackageConfiguration["package3"]).toEqual({});
        });

        it("should load configuration without exception, when package configuration empty", function () {
            spyOn(mocks.$location, "search");
            prepareSut();

            var plugins = {
                Plugins: [
                    { PluginKey: "key1", PluginId: "id1" },
                    { PluginKey: "key2", PluginId: "id2" },
                    { PluginKey: "key3", PluginId: "id3" }
                ]
            };
            var businessUnits = [
                { Packages: [{ ExternalId: "package1" }, { ExternalId: "package2" }] },
                { Packages: [{ ExternalId: "package3" }] }
            ];
            var configuration = { Plugin: { } };
            var pluginsDeferred = mocks.$q.defer();
            var pluginConfigurationDeferred = mocks.$q.defer();
            mocks.remiapi.get.pluginConfituration.and.returnValue(pluginConfigurationDeferred.promise);
            mocks.remiapi.get.plugins.and.returnValue(pluginsDeferred.promise);

            initDeferred.resolve({});
            pluginsDeferred.resolve(plugins);
            businessUnitsDeferred.resolve(businessUnits);
            pluginConfigurationDeferred.resolve(configuration);
            mocks.$scope.$digest();

            expect(sut.parent.plugins[0].PackageConfiguration).toBeUndefined();
        });

    });

    describe("getGlobalPluginConfiguration", function () {
        it("should get global plugin configuration and override existing one, when called successful", function () {
            prepareSut();

            var plugin = {
                PluginId: "plugin id",
                GlobalConfiguration: { key1: "property 1 old", key2: "property 2 old" }
            };
            var globalConfiguration = {
                GlobalConfiguration: { key1: "property 1 new", key2: "property 2 new" }
            };
            var globalPluginConfigurationDeferred = mocks.$q.defer();
            mocks.remiapi.get.globalPluginConfiguration.and.returnValue(globalPluginConfigurationDeferred.promise);

            sut.getGlobalPluginConfiguration(plugin);

            expect(sut.state.isBusy).toEqual(true);
            expect(mocks.remiapi.get.globalPluginConfiguration).toHaveBeenCalledWith(plugin.PluginId);

            globalPluginConfigurationDeferred.resolve(globalConfiguration);
            mocks.$scope.$digest();

            expect(plugin.GlobalConfiguration).toEqual(globalConfiguration.GlobalConfiguration);
            expect(plugin.haveChanges).toEqual(false);
            expect(sut.state.isBusy).toEqual(false);
        });

        it("should log error message, when called failed", function () {
            prepareSut();

            var plugin = { PluginId: "plugin id" };
            var error = { Details: "Some error" };
            var globalPluginConfigurationDeferred = mocks.$q.defer();
            mocks.remiapi.get.globalPluginConfiguration.and.returnValue(globalPluginConfigurationDeferred.promise);

            sut.getGlobalPluginConfiguration(plugin);
            globalPluginConfigurationDeferred.reject(error);
            mocks.$scope.$digest();

            expect(logger.error).toHaveBeenCalledWith("Unable to load global configuration");
            expect(logger.error).toHaveBeenCalledWith(error.Details);
            expect(sut.state.isBusy).toEqual(false);
        });
    });

    describe("getPackagePluginConfiguration", function () {
        it("should get package plugin configuration and override existing one, when called successful", function () {
            prepareSut();

            var plugin = {
                PluginId: "plugin id",
                PackageConfiguration: { package1: { key1: "property 1 old", key2: "property 2 old" } }
            };
            var $package = { ExternalId: "package1" }

            var packageConfiguration = {
                PackageConfiguration: { key1: "property 1 new", key2: "property 2 new" }
            };
            var packagePluginConfigurationDeferred = mocks.$q.defer();
            mocks.remiapi.get.packagePluginConfiguration.and.returnValue(packagePluginConfigurationDeferred.promise);

            sut.getPackagePluginConfiguration(plugin, $package);

            expect(sut.state.isBusy).toEqual(true);
            expect(mocks.remiapi.get.packagePluginConfiguration).toHaveBeenCalledWith(plugin.PluginId, $package.ExternalId);

            packagePluginConfigurationDeferred.resolve(packageConfiguration);
            mocks.$scope.$digest();

            expect(plugin.PackageConfiguration.package1.haveChanges).toEqual(false);
            delete plugin.PackageConfiguration.package1.haveChanges;
            expect(plugin.PackageConfiguration.package1).toEqual(packageConfiguration.PackageConfiguration);
            expect(sut.state.isBusy).toEqual(false);
        });

        it("should log error message, when called failed", function () {
            prepareSut();

            var plugin = { PluginId: "plugin id" };
            var $package = { ExternalId: "package1" }
            var error = { Message: "Some error" };
            var packagePluginConfigurationDeferred = mocks.$q.defer();
            mocks.remiapi.get.packagePluginConfiguration.and.returnValue(packagePluginConfigurationDeferred.promise);

            sut.getPackagePluginConfiguration(plugin, $package);
            packagePluginConfigurationDeferred.reject(error);
            mocks.$scope.$digest();

            expect(logger.error).toHaveBeenCalledWith("Unable to load package configuration");
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(error));
            expect(sut.state.isBusy).toEqual(false);
        });
    });

    describe("valueHasChanged", function () {
        it("should mark plugin with haveChange flag, when package is empty", function () {
            prepareSut();

            var plugin = { PluginId: "plugin id" };

            sut.valueHasChanged(plugin);

            expect(plugin.haveChanges).toEqual(true);
        });

        it("should mark plugin package configuration with haveChange flag, when package is not empty", function () {
            prepareSut();

            var plugin = {
                PluginId: "plugin id",
                PackageConfiguration: { package1: {} }
            };
            var $package = { ExternalId: "package1" }

            sut.valueHasChanged(plugin, $package);

            expect(plugin.haveChanges).toBeUndefined();
            expect(plugin.PackageConfiguration.package1.haveChanges).toEqual(true);
        });
    });

    describe("addNameValue", function () {
        it("should should create new list, add empty record and mark plugin package configuration as been changed, when list is empty", function () {
            prepareSut();

            var plugin = {
                PluginId: "plugin id",
                PackageConfiguration: { package1: {} }
            };
            var $package = { ExternalId: "package1" }

            sut.addNameValue(undefined, plugin, $package, "property1");

            expect(plugin.PackageConfiguration.package1.property1).toEqual([{ Name: "", Value: "" }]);
            expect(plugin.PackageConfiguration.package1.haveChanges).toEqual(true);
        });

        it("should should create new list, add empty record and mark plugin global configuration as been changed, when list is empty", function () {
            prepareSut();

            var plugin = {
                PluginId: "plugin id",
                GlobalConfiguration: {}
            };

            sut.addNameValue(undefined, plugin, undefined, "property1");

            expect(plugin.GlobalConfiguration.property1).toEqual([{ Name: "", Value: "" }]);
            expect(plugin.haveChanges).toEqual(true);
        });

        it("should should add empty record and mark plugin package configuration as been changed, when list is not empty", function () {
            prepareSut();

            var plugin = {
                PluginId: "plugin id",
                PackageConfiguration: { package1: {} }
            };
            var list = [{ Name: "name1", Value: "value1" }];
            var $package = { ExternalId: "package1" }

            sut.addNameValue(list, plugin, $package, "property1");

            expect(list).toEqual([{ Name: "name1", Value: "value1" }, { Name: "", Value: "" }]);
            expect(plugin.PackageConfiguration.package1.haveChanges).toEqual(true);
        });
    });

    describe("removeNameValue", function () {
        it("should remove item from the list", function () {
            prepareSut();

            var plugin = {
                PluginId: "plugin id",
                PackageConfiguration: { package1: {} }
            };
            var $package = { ExternalId: "package1" }
            var item = { Name: "name2", Value: "value2" };
            var list = [{ Name: "name1", Value: "value1" }, item];

            sut.removeNameValue(list, item, plugin, $package);

            expect(list).toEqual([{ Name: "name1", Value: "value1" }]);
            expect(plugin.PackageConfiguration.package1.haveChanges).toEqual(true);
        });
    });

    describe("saveGlobalConfiguration", function () {
        it("should save global changes for plugin and mark plugin as not changed, when called successful", function () {
            prepareSut();

            var plugin = {
                PluginId: "plugin id",
                GlobalConfiguration: { key1: "property 1", key2: "property 2" }
            };
            var deferred = mocks.$q.defer();
            mocks.remiapi.post.updatePluginGlobalConfiguration.and.returnValue(deferred.promise);

            sut.saveGlobalConfiguration(plugin);

            expect(sut.state.isBusy).toEqual(true);
            expect(mocks.remiapi.post.updatePluginGlobalConfiguration).toHaveBeenCalledWith(
                { PluginId: plugin.PluginId, JsonValues: "{\"key1\":\"property 1\",\"key2\":\"property 2\"}" });

            deferred.resolve();
            mocks.$scope.$digest();

            expect(plugin.haveChanges).toEqual(false);
            expect(sut.state.isBusy).toEqual(false);
        });

        it("should log error message, when called failed", function () {
            prepareSut();

            var error = { Message: "Some error" };
            var plugin = {
                PluginId: "plugin id",
                GlobalConfiguration: { key1: "property 1 old", key2: "property 2 old" }
            };
            var deferred = mocks.$q.defer();
            mocks.remiapi.post.updatePluginGlobalConfiguration.and.returnValue(deferred.promise);

            sut.saveGlobalConfiguration(plugin);

            deferred.reject(error);
            mocks.$scope.$digest();

            expect(logger.error).toHaveBeenCalledWith("Unable to save plugin global configuration");
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(error));
            expect(sut.state.isBusy).toEqual(false);
        });
    });

    describe("savePackageConfiguration", function () {
        it("should save package changes for plugin and mark this package as not changed, when called successful", function () {
            prepareSut();

            var plugin = {
                PluginId: "plugin id",
                PackageConfiguration: { package1: { key1: "property 1", key2: "property 2", haveChanges: true } }
            };
            var $package = { ExternalId: "package1" }
            var deferred = mocks.$q.defer();
            mocks.remiapi.post.updatePluginPackageConfiguration.and.returnValue(deferred.promise);

            sut.savePackageConfiguration(plugin, $package);

            expect(sut.state.isBusy).toEqual(true);
            expect(mocks.remiapi.post.updatePluginPackageConfiguration).toHaveBeenCalledWith(
                { PluginId: plugin.PluginId, PackageId: $package.ExternalId, JsonValues: "{\"key1\":\"property 1\",\"key2\":\"property 2\",\"PackageId\":\"package1\"}" });

            deferred.resolve();
            mocks.$scope.$digest();

            expect(plugin.PackageConfiguration.package1.haveChanges).toBeUndefined();
            expect(sut.state.isBusy).toEqual(false);
        });

        it("should log error message, when called failed", function () {
            prepareSut();

            var error = { Message: "Some error" };
            var plugin = {
                PluginId: "plugin id",
                PackageConfiguration: { package1: { key1: "property 1", key2: "property 2" } }
            };
            var $package = { ExternalId: "package1" }
            var deferred = mocks.$q.defer();
            mocks.remiapi.post.updatePluginPackageConfiguration.and.returnValue(deferred.promise);

            sut.savePackageConfiguration(plugin, $package);

            deferred.reject(error);
            mocks.$scope.$digest();

            expect(logger.error).toHaveBeenCalledWith("Unable to save plugin package configuration");
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(error));
            expect(sut.state.isBusy).toEqual(false);
        });
    });

    describe("RouteUpdated", function () {
        it("should switch active tab, when triggered", function () {
            var eventHandler = {};
            mocks.common.handleEvent.and.callFake(function (evt, handler) {
                eventHandler[evt] = handler;
            });
            spyOn(mocks.$location, "search");
            prepareSut();

            sut.parent.plugins = [
                { PluginKey: "key1", PluginId: "id1" },
                { PluginKey: "key2", PluginId: "id2" },
                { PluginKey: "key3", PluginId: "id3" }
            ];
            sut.activeTab ={
                "key1": { isCurrent: true, loaded: true },
                "key2": { isCurrent: false, loaded: false },
                "key3": { isCurrent: false, loaded: false }
            };
            mocks.remiapi.get.pluginConfituration.and.returnValue(mocks.$q.defer().promise);

            mocks.$location.search.and.returnValue({plugin: "key3"});
            eventHandler[mocks.config.events.navRouteUpdate]();

            expect(sut.activeTab).toEqual({
                "key1": { isCurrent: false, loaded: true },
                "key2": { isCurrent: false, loaded: false },
                "key3": { isCurrent: true, loaded: true }
            });
        });
    });

});
