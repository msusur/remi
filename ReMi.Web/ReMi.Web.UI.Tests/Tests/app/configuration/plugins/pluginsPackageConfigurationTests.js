describe("PluginsPackageConfiguration Controller", function () {
    var sut, mocks, logger, parent;
    var activateDeferred, initDeferred;

    beforeEach(function () {
        module("app", function ($provide) { $provide.value("authService", {}) });
    });

    beforeEach(angular.mock.inject(function ($q, $rootScope) {
        activateDeferred = $q.defer();
        initDeferred = $q.defer();
        mocks = {
            $scope: $rootScope.$new(),
            $rootScope: $rootScope,
            common: {
                logger: window.jasmine.createSpyObj("logger", ["getLogger"]),
                activateController: window.jasmine.createSpy("activateController"),
                getParentScope: jasmine.createSpy("getParentScope")
            },
            remiapi: {
                get: jasmine.createSpyObj("remiapi.get", ["packagePlugins"]),
                post: jasmine.createSpyObj("remiapi.post", ["assignPackagePlugin"])
            },
            $q: $q
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
            sut = $controller("pluginsPackageConfiguration", mocks);
        });
    }

    it("should call activateController method, when created", function () {
        prepareSut();
        activateDeferred.resolve();
        mocks.$scope.$digest();

        expect(sut).toBeDefined();

        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("pluginsPackageConfiguration");
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), "pluginsPackageConfiguration");
        expect(logger.console).toHaveBeenCalledWith("Activated PluginsPackageConfiguration Controller");
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.parent).toEqual(parent);
        expect(mocks.remiapi.get.packagePlugins).not.toHaveBeenCalled();
    });

    describe("initialize", function () {
        it("should get package plugins with configuration after parent is initialized, when called", function () {
            prepareSut();

            var packagePlugins = {
                PackagePlugins: [
                        { PluginTypes: ["Test1", "Test3"] },
                        { PluginTypes: ["Test4"] },
                        { PluginTypes: ["Test2", "Test3"] },
                        { PluginTypes: ["Test1"] }
                ],
                PackagePluginConfiguration: {
                    Package1: {
                        Test1: { PackageId: "Package1", PackageName: "PackageName1", BusinessUnit: "businessUnit1" },
                        Test2: { PackageId: "Package1", PackageName: "PackageName1", BusinessUnit: "businessUnit1" },
                        Test3: { PackageId: "Package1", PackageName: "PackageName1", BusinessUnit: "businessUnit1" },
                        Test4: { PackageId: "Package1", PackageName: "PackageName1", BusinessUnit: "businessUnit1" }
                    },
                    Package2: {
                        Test1: { PackageId: "Package2", PackageName: "PackageName2", BusinessUnit: "businessUnit2" },
                        Test2: { PackageId: "Package2", PackageName: "PackageName2", BusinessUnit: "businessUnit2" },
                        Test3: { PackageId: "Package2", PackageName: "PackageName2", BusinessUnit: "businessUnit2" },
                        Test4: { PackageId: "Package2", PackageName: "PackageName2", BusinessUnit: "businessUnit2" }
                    },
                }
            };
            var packagesDeferred = mocks.$q.defer();
            var packagePluginsDeferred = mocks.$q.defer();
            var packages;
            packagesDeferred.promise.then(function(p) { packages = p; });
            mocks.remiapi.get.packagePlugins.and.returnValue(packagePluginsDeferred.promise);
            sut.parent.packagesDeferred = packagesDeferred;

            initDeferred.resolve({});
            mocks.$scope.$digest();

            expect(sut.state.isBusy).toEqual(true);
            expect(mocks.remiapi.get.packagePlugins).toHaveBeenCalled();

            packagePluginsDeferred.resolve(packagePlugins);
            mocks.$scope.$digest();

            expect(sut.parent.packagePlugins).toEqual(packagePlugins);
            expect(sut.parent.packagePlugins.Packages).toEqual([
                { ExternalId: "Package1", Name: "PackageName1", BusinessUnit: "businessUnit1" },
                { ExternalId: "Package2", Name: "PackageName2", BusinessUnit: "businessUnit2" }]);
            expect(sut.parent.packagePlugins.PluginTypes).toEqual({
                Test1: [{ PluginTypes: ["Test1", "Test3"] }, { PluginTypes: ["Test1"] }],
                Test2: [{ PluginTypes: ["Test2", "Test3"] }],
                Test3: [{ PluginTypes: ["Test1", "Test3"] }, { PluginTypes: ["Test2", "Test3"] }],
                Test4: [{ PluginTypes: ["Test4"] }]
            });
            expect(sut.state.isBusy).toEqual(false);
            expect(packages).toEqual(sut.parent.packagePlugins.Packages);
        });

        it("should log error, when failed to initialize global plugins", function () {
            prepareSut();

            var error = { Message: "Some error" };
            var packagePluginsDeferred = mocks.$q.defer();
            mocks.remiapi.get.packagePlugins.and.returnValue(packagePluginsDeferred.promise);

            initDeferred.resolve({});
            packagePluginsDeferred.reject(error);
            mocks.$scope.$digest();

            expect(sut.parent.packagePlugins).toEqual([]);
            expect(logger.error).toHaveBeenCalledWith("Unable to load package plugin configuration");
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(error));
            expect(sut.state.isBusy).toEqual(false);
        });
    });

    describe("packagePluginChanged", function () {
        it("should send assignPlugin command, when package plugin has changed", function () {
            prepareSut();

            var $package = { ExternalId: "package id" };
            var pluginType = "plugin type";
            var pluginConfig = { PluginId: "plugin id", ExternalId: "config id" };
            var commandDeferred = mocks.$q.defer();
            mocks.remiapi.post.assignPackagePlugin.and.returnValue(commandDeferred.promise);
            sut.parent.packagePlugins = { PackagePluginConfiguration: {} };
            sut.parent.packagePlugins.PackagePluginConfiguration[$package.ExternalId] = {};
            sut.parent.packagePlugins.PackagePluginConfiguration[$package.ExternalId][pluginType] = pluginConfig;

            sut.packagePluginChanged($package, pluginType);

            expect(sut.state.isBusy).toEqual(true);

            commandDeferred.resolve();
            mocks.$scope.$digest();

            expect(mocks.remiapi.post.assignPackagePlugin).toHaveBeenCalledWith({ PluginId: "plugin id", ConfigurationId: "config id" });
            expect(sut.state.isBusy).toEqual(false);
            expect(logger.error).not.toHaveBeenCalled();
        });

        it("should log error and refresh package plugins, when assignPlugin command failed", function () {
            prepareSut();

            var error = { Message: "Some error" };
            var $package = { ExternalId: "package id" };
            var pluginType = "plugin type";
            var pluginConfig = { PluginId: "plugin id", ExternalId: "config id" };
            var commandDeferred = mocks.$q.defer();
            var packagePluginsDeferred = mocks.$q.defer();
            mocks.remiapi.get.packagePlugins.and.returnValue(packagePluginsDeferred.promise);
            mocks.remiapi.post.assignPackagePlugin.and.returnValue(commandDeferred.promise);
            sut.parent.packagePlugins = { PackagePluginConfiguration: {} };
            sut.parent.packagePlugins.PackagePluginConfiguration[$package.ExternalId] = {};
            sut.parent.packagePlugins.PackagePluginConfiguration[$package.ExternalId][pluginType] = pluginConfig;

            sut.packagePluginChanged($package, pluginType);

            expect(sut.state.isBusy).toEqual(true);

            commandDeferred.reject(error);
            mocks.$scope.$digest();

            expect(sut.state.isBusy).toEqual(false);
            expect(logger.error).toHaveBeenCalledWith("Unable to update configuration");
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(error));
            expect(mocks.remiapi.get.packagePlugins).toHaveBeenCalled();
        });
    });
});
