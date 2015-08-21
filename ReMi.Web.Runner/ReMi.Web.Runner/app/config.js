(function () {
    'use strict';

    var app = angular.module('app');

    // Configure Toastr
    toastr.options.timeOut = 4000;
    toastr.options.positionClass = 'toast-bottom-right';

    var remoteServiceName = '';

    var apis = {
        base: ''
    };

    var events = {
        controllerActivateSuccess: 'controller.activateSuccess',
        controllerActivateError: 'controller.activateError',
        controllerDeactivateError: 'controller.deactivateError',
        controllerDeactivate: 'controller.deactivate',
        controllerAccepted: 'controller.activateAccepted',
        spinnerToggle: 'spinner.toggle',
        authContextChanged: 'auth.ContextChanged',
        authStateChanged: 'auth.StateChanged',
        loggedIn: 'auth.loggedIn',
        loggedOut: 'auth.loggedOut',
        notificationReceived: 'notifications.received',
        notificationConnected: 'notifications.connected',
        notificationDisconnected: 'notifications.disconnected',
        notificationRegistered: 'notifications.registered',
        productContextChanged: 'auth.ProductContextChanged',
        navRouteUpdate: 'navigation.routeUpdate',
        navLocationChange: 'navigation.locationChange',
        productsAddedForUser: 'auth.ProductsAddedForUser',
        sessionExpired: 'auth.sessionExpired',
        closeReleaseOnSignOffEvent: 'release.closeReleaseOnSignOffEvent',
        businessUnitsLoaded: 'auth.businessUnitsLoaded',
        locationChangeSuccess: "$locationChangeSuccess"
    };

    var config = {
        appErrorPrefix: '[RM Error] ', //Configure the exceptionHandler decorator
        docTitle: 'ReMi: ',
        events: events,
        apis: apis,
        remoteServiceName: remoteServiceName,
        version: '1.0.0'
    };

    app.value('config', config);
    
    app.config(['$logProvider', function ($logProvider) {
        // turn debugging off/on (no info or warn)
        if ($logProvider.debugEnabled) {
            $logProvider.debugEnabled(true);
        }
    }]);
    
    //#region Configure the common services via commonConfig
    app.config(['commonConfigProvider', function (cfg) {
        cfg.config.controllerActivateSuccessEvent = config.events.controllerActivateSuccess;
        cfg.config.controllerDeactivateEvent = config.events.controllerDeactivate;
        cfg.config.controllerActivateErrorEvent = config.events.controllerActivateError;
        cfg.config.controllerDeactivateErrorEvent = config.events.controllerDeactivateError;
        cfg.config.controllerAccceptedEvent = config.events.controllerAccepted;
        cfg.config.spinnerToggleEvent = config.events.spinnerToggle;

        cfg.config.loggedInEvent = config.events.loggedIn;
        cfg.config.loggedOutEvent = config.events.loggedOut;
        cfg.config.authContextChangedEvent = config.events.authContextChanged;
        cfg.config.authStateChangedEvent = config.events.authStateChanged;
        cfg.config.productContextChangedEvent = config.events.productContextChanged;
        cfg.config.productsAddedForUser = config.events.productsAddedForUser;

        cfg.config.notificationReceivedEvent = config.events.notificationReceived;
        cfg.config.notificationConnectedEvent = config.events.notificationConnected;
        cfg.config.notificationDisconnectedEvent = config.events.notificationDisconnected;
        cfg.config.notificationRegisteredEvent = config.events.notificationRegistered;
    }]);
    //#endregion

    app.config(['$httpProvider', function () {
    }]);

})();
