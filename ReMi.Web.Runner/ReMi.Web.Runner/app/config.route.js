(function () {
    'use strict';

    var app = angular.module('app');

    // Collect the routes
    app.constant('routes', getRoutes());

    // Configure the routes and route resolvers
    app.config(['$routeProvider', 'routes', routeConfigurator]);
    function routeConfigurator($routeProvider, routes) {

        routes.forEach(function (r) {
            $routeProvider.when(r.url, r.config);
        });
        $routeProvider.otherwise({ redirectTo: '/' });
    }

    // Define the routes 
    function getRoutes() {
        return [
            {
                url: '/',
                config: {
                    title: 'releaseCalendar',
                    label: 'Release Calendar',
                    templateUrl: 'app/releaseCalendar/releaseCalendar.html',
                    settings: {
                        nav: 1,
                        iconCss: 'fa fa-calendar'
                    },
                    access: { allowAnonymous: true }
                }
            }, {
                url: '/release',
                config: {
                    title: 'release',
                    label: 'Release',
                    templateUrl: 'app/release/release.html',
                    settings: {
                        nav: 2,
                        iconCss: 'fa fa-check',
                    },
                    access: { allowAnonymous: false },
                    reloadOnSearch: false
                }
            }, {
                url: '/reports',
                config: {
                    title: 'metrics',
                    label: 'Release Engineering',
                    templateUrl: 'app/reports/reportsView.html',
                    settings: {
                        nav: 3,
                        iconCss: 'fa fa-line-chart'
                    },
                    access: { allowAnonymous: false },
                    reloadOnSearch: false
                }
            }, {
                url: '/reports',
                config: {
                    parentUrl: '/reports',
                    title: 'reports',
                    label: 'Reports',
                    templateUrl: 'app/reports/reportsView.html',
                    settings: {
                        nav: 3.1,
                        iconCss: 'fa fa-line-chart',
                    },
                    access: { allowAnonymous: false },
                    reloadOnSearch: false
                }
            }, {
                url: '/metrics/package',
                config: {
                    parentUrl: '/reports',
                    title: 'packageMetrics',
                    label: 'Package Metrics',
                    templateUrl: 'app/metrics/metricsByProduct.html',
                    settings: {
                        nav: 3.2,
                        iconCss: 'fa fa-line-chart'
                    },
                    access: { allowAnonymous: false },
                    reloadOnSearch: false
                }
            }, {
                url: '/productRegistration',
                config: {
                    title: 'productRegistration',
                    label: 'Product Registration',
                    templateUrl: 'app/productRegistration/productRegistration.html',
                    settings: {
                        nav: 4,
                        iconCss: 'fa fa-puzzle-piece'
                    },
                    access: { allowAnonymous: false }
                }
            }, {
                url: '/productRegistration',
                config: {
                    parentUrl: '/productRegistration',
                    title: 'productRegistration',
                    label: 'Registrations',
                    templateUrl: 'app/productRegistration/productRegistration.html',
                    settings: {
                        nav: 4.1,
                        iconCss: 'fa fa-puzzle-piece'
                    },
                    access: { allowAnonymous: false }
                }
            }, {
                url: '/productRegistrationConfig',
                config: {
                    parentUrl: '/productRegistration',
                    title: 'productRegistrationConfig',
                    label: 'Templates',
                    templateUrl: 'app/productRegistrationConfig/productRegistrationConfig.html',
                    settings: {
                        nav: 4.2,
                        iconCss: 'fa fa-puzzle-piece'
                    },
                    access: {
                        allowAnonymous: false, commands: [
                            'CreateProductRequestTaskCommand'
                        ]
                    }
                }
            }, {
                url: '/configuration/accounts',
                config: {
                    title: 'configuration',
                    label: 'Config',
                    templateUrl: 'app/configuration/accounts/accounts.html',
                    settings: {
                        nav: 5,
                        iconCss: 'fa fa-cog'
                    },
                    access: {
                        allowAnonymous: false, commands: [
                                'UpdateAccountCommand'
                        ]
                    }
                }
            }, {
                url: '/configuration/accounts',
                config: {
                    parentUrl: '/configuration/accounts',
                    title: 'accounts',
                    label: 'Accounts',
                    templateUrl: 'app/configuration/accounts/accounts.html',
                    settings: {
                        nav: 5.1,
                        iconCss: 'fa fa-cog'
                    },
                    access: {
                        allowAnonymous: false, commands: [
                                'UpdateAccountCommand'
                            ]
                    },
                    reloadOnSearch: false
                }
            }, {
                url: '/configuration/businessUnits',
                config: {
                    parentUrl: '/configuration/accounts',
                    title: 'businessUnits',
                    label: 'Business Units',
                    templateUrl: 'app/configuration/businessUnits/businessUnitsPage.html',
                    settings: {
                        nav: 5.2,
                        iconCss: 'fa fa-cog'
                    },
                    access: {
                        allowAnonymous: false, commands: [
                            'UpdateProductCommand'
                        ]
                    },
                    reloadOnSearch: false
                }
            }, {
                url: '/configuration/permissions',
                config: {
                    parentUrl: '/configuration/accounts',
                    title: 'permissions',
                    label: 'Permissions',
                    templateUrl: 'app/configuration/permissions/permissions.html',
                    settings: {
                        nav: 5.3,
                        iconCss: 'fa fa-cog'
                    },
                    access: {
                        allowAnonymous: false, commands: [
                            'AddCommandToRoleCommand',
                            'AddQueryToRoleCommand',
                            'CreateRoleCommand'
                        ]
                    },
                    reloadOnSearch: false
                }
            }, {
                url: '/configuration/plugins/:tab',
                config: {
                    parentUrl: '/configuration/accounts',
                    title: 'plugins',
                    label: 'Plugins',
                    templateUrl: 'app/configuration/plugins/plugins.html',
                    settings: {
                        nav: 5.4,
                        iconCss: 'fa fa-cog'
                    },
                    access: { allowAnonymous: false, commands: ['UpdatePluginPackageConfigurationCommand'] },
                    reloadOnSearch: false
                }
            }, {
                url: '/configuration/rules',
                config: {
                    parentUrl: '/configuration/accounts',
                    title: 'rules',
                    label: 'Rules',
                    templateUrl: 'app/configuration/rules/rules.html',
                    settings: {
                        nav: 5.5,
                        iconCss: 'fa fa-cog'
                    },
                    access: { allowAnonymous: false, commands: ['SaveRuleCommand'] },
                    reloadOnSearch: false
                }
            }, {
                url: '/login',
                config: {
                    title: 'login',
                    templateUrl: 'app/login/login.html'
                }
            }, {
                url: '/releasePlan',
                config: {
                    redirectTo: '/release'
                }
            }, {
                url: '/apiDescription',
                config: {
                    title: 'apiDescription',
                    templateUrl: 'app/apiDescription/apiDescription.html',
                    access: { allowAnonymous: true }
                }
            }, {
                url: '/acknowledge',
                config: {
                    title: 'acknowledge',
                    templateUrl: 'app/acknowledge/acknowledge.html',
                    access: { allowAnonymous: true }
                }
            }, {
                url: '/confirm',
                config: {
                    title: 'confirm',
                    templateUrl: 'app/confirm/confirm.html',
                    access: { allowAnonymous: true }
                }
            }, {
                url: '/profile',
                config: {
                    title: 'profile',
                    templateUrl: 'app/profile/profile.html',
                    access: { allowAnonymous: false }
                }
            }, {
                url: '/error',
                config: {
                    title: 'error',
                    templateUrl: 'error.html',
                    access: { allowAnonymous: true }
                }
            }
        ];
    }
})();
