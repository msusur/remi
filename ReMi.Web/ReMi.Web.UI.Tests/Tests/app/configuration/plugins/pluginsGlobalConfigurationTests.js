describe("PluginsGlobalConfiguration Controller", function () {
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
                get: jasmine.createSpyObj("remiapi.get", ["globalPlugins"]),
                post: jasmine.createSpyObj("remiapi.post", ["assignGlobalPlugin"])
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
            sut = $controller("pluginsGlobalConfiguration", mocks);
        });
    }

    it("should call activateController method, when created", function () {
        prepareSut();
        activateDeferred.resolve();
        mocks.$scope.$digest();

        expect(sut).toBeDefined();

        expect(mocks.common.logger.getLogger).toHaveBeenCalledWith("pluginsGlobalConfiguration");
        expect(mocks.common.activateController).toHaveBeenCalledWith(window.jasmine.any(Array), "pluginsGlobalConfiguration");
        expect(logger.console).toHaveBeenCalledWith("Activated PluginsGlobalConfiguration Controller");
        expect(sut.state.isBusy).toEqual(false);
        expect(sut.parent).toEqual(parent);
        expect(mocks.remiapi.get.globalPlugins).not.toHaveBeenCalled();
    });

    describe("initialize", function () {
        it("should get global plugins with configuration after parent is initialized, when called", function () {
            prepareSut();

            var globalPlugins = {
                GlobalPlugins: [
                    { PluginTypes: ["Test1", "Test3"] },
                    { PluginTypes: ["Test4"] },
                    { PluginTypes: ["Test2", "Test3"] },
                    { PluginTypes: ["Test1"] }
                ],
                GlobalPluginConfiguration: [
                    { PluginType: "Test1" },
                    { PluginType: "Test2" },
                    { PluginType: "Test5" }
                ]
            };
            var globalPluginsDeferred = mocks.$q.defer();
            mocks.remiapi.get.globalPlugins.and.returnValue(globalPluginsDeferred.promise);

            initDeferred.resolve({});
            mocks.$scope.$digest();

            expect(sut.state.isBusy).toEqual(true);
            expect(mocks.remiapi.get.globalPlugins).toHaveBeenCalled();

            globalPluginsDeferred.resolve(globalPlugins);
            mocks.$scope.$digest();

            expect(sut.parent.globalPlugins).toEqual(globalPlugins);
            expect(sut.parent.globalPlugins.GlobalPluginConfiguration[0].Plugins.length).toEqual(2);
            expect(sut.parent.globalPlugins.GlobalPluginConfiguration[1].Plugins.length).toEqual(1);
            expect(sut.parent.globalPlugins.GlobalPluginConfiguration[2].Plugins.length).toEqual(0);
            expect(sut.state.isBusy).toEqual(false);
        });

        it("should log error, when failed to initialize global plugins", function () {
            prepareSut();

            var error = { Message: "Some error" };
            var globalPluginsDeferred = mocks.$q.defer();
            mocks.remiapi.get.globalPlugins.and.returnValue(globalPluginsDeferred.promise);

            initDeferred.resolve({});
            globalPluginsDeferred.reject(error);
            mocks.$scope.$digest();

            expect(sut.parent.globalPlugins).toEqual([]);
            expect(logger.error).toHaveBeenCalledWith("Unable to load global plugin configuration");
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(error));
            expect(sut.state.isBusy).toEqual(false);
        });
    });

    describe("globalPluginChanged", function () {
        it("should send assignPlugin command, when global plugin has changed", function () {
            prepareSut();

            var pluginConfig = { PluginId: "plugin id", ExternalId: "config id" };
            var commandDeferred = mocks.$q.defer();
            mocks.remiapi.post.assignGlobalPlugin.and.returnValue(commandDeferred.promise);

            sut.globalPluginChanged(pluginConfig);

            expect(sut.state.isBusy).toEqual(true);

            commandDeferred.resolve();
            mocks.$scope.$digest();

            expect(mocks.remiapi.post.assignGlobalPlugin).toHaveBeenCalledWith({ PluginId: "plugin id", ConfigurationId: "config id" });
            expect(sut.state.isBusy).toEqual(false);
            expect(logger.error).not.toHaveBeenCalled();
        });

        it("should log error and refresh global plugins, when assignPlugin command failed", function () {
            prepareSut();

            var error = { Message: "Some error" };
            var pluginConfig = { PluginId: "plugin id", ExternalId: "config id" };
            var commandDeferred = mocks.$q.defer();
            var globalPluginsDeferred = mocks.$q.defer();
            mocks.remiapi.post.assignGlobalPlugin.and.returnValue(commandDeferred.promise);
            mocks.remiapi.get.globalPlugins.and.returnValue(globalPluginsDeferred.promise);

            sut.globalPluginChanged(pluginConfig);

            expect(sut.state.isBusy).toEqual(true);

            commandDeferred.reject(error);
            mocks.$scope.$digest();

            expect(sut.state.isBusy).toEqual(false);
            expect(logger.error).toHaveBeenCalledWith("Unable to update configuration");
            expect(logger.error).toHaveBeenCalledWith(JSON.stringify(error));
            expect(mocks.remiapi.get.globalPlugins).toHaveBeenCalled();
        });
    });
});
