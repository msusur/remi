describe("TreeGrid Directive ", function () {
    var scope, targetScope, data;
    var html, container, element, compiled;
    var $compile, $timeout;

    beforeEach(function () {
        data = {
            "GoServerUrl": "http://www.go.com/go/",
            "Measurements": [
                {
                    "Id": "661d4a0c-3265-48c5-8c19-a31565eb95ee", "StepName": "liveAuthenticationDeploy", "Locator": null, "StepId": "399194", "StartTime": "2014-08-14T18:46:07", "FinishTime": "2014-08-14T22:26:55", "CompletedOnSiteStage": "Online after", "Duration": 13248000.0, "DurationLabel": "3 h. 40 min. 48 s.",
                    "ChildSteps": [{
                        "Id": "6b235ed0-b6f3-4c4e-9f08-31e767c7a078", "StepName": "seedWebEndPoints", "Locator": "pipelines/liveAuthenticationDeploy/13/seedWebEndPoints/1", "StepId": "649309", "StartTime": "2014-08-14T18:46:07", "FinishTime": "2014-08-14T18:47:37", "CompletedOnSiteStage": "Offline", "Duration": 90000.0, "DurationLabel": "1 min. 30 s.",
                        "ChildSteps": [{
                            "Id": "53058089-84aa-4cd9-99c9-6936a5c90a96", "StepName": "AuthenticationAnywhereApiPaylaterCommands", "Locator": "tab/build/detail/liveAuthenticationDeploy/13/seedWebEndPoints/1/AuthenticationAnywhereApiPaylaterCommands", "StepId": "3210881", "StartTime": "2014-08-14T18:46:07", "FinishTime": "2014-08-14T18:47:24", "CompletedOnSiteStage": "Offline", "Duration": 77000.0, "DurationLabel": "1 min. 17 s.",
                            "ChildSteps": []
                        }, {
                            "Id": "be8cae47-8de6-4b80-aba6-4761593efbd9", "StepName": "AuthenticationAnywhereApiPaylaterQueries", "Locator": "tab/build/detail/liveAuthenticationDeploy/13/seedWebEndPoints/1/AuthenticationAnywhereApiPaylaterQueries", "StepId": "3210882", "StartTime": "2014-08-14T18:46:07", "FinishTime": "2014-08-14T18:47:25", "CompletedOnSiteStage": "Offline", "Duration": 78000.0, "DurationLabel": "1 min. 18 s.",
                            "ChildSteps": []
                        }]
                    }, {
                        "Id": "cfa80082-5ed0-458c-91f0-6218e1180c4f", "StepName": "seedHandlers", "Locator": "pipelines/liveAuthenticationDeploy/13/seedHandlers/1", "StepId": "649316", "StartTime": "2014-08-14T18:47:37", "FinishTime": "2014-08-14T18:48:47", "CompletedOnSiteStage": "Offline", "Duration": 70000.0, "DurationLabel": "1 min. 10 s.",
                        "ChildSteps": [{
                            "Id": "616eb9e2-0bef-4271-a143-08b2b22626e9", "StepName": "AuthenticationAnonymisation", "Locator": "tab/build/detail/liveAuthenticationDeploy/13/seedHandlers/1/AuthenticationAnonymisation", "StepId": "3210939", "StartTime": "2014-08-14T18:47:37", "FinishTime": "2014-08-14T18:48:43", "CompletedOnSiteStage": "Offline", "Duration": 66000.0, "DurationLabel": "1 min. 6 s.",
                            "ChildSteps": []
                        }, {
                            "Id": "dfcc3087-86e9-4899-865c-e39aafb70c69", "StepName": "AuthenticationHandlers", "Locator": "tab/build/detail/liveAuthenticationDeploy/13/seedHandlers/1/AuthenticationHandlers", "StepId": "3210938", "StartTime": "2014-08-14T18:47:37", "FinishTime": "2014-08-14T18:48:47", "CompletedOnSiteStage": "Offline", "Duration": 70000.0, "DurationLabel": "1 min. 10 s.",
                            "ChildSteps": []
                        }]
                    }]
                },
                {
                    "Id": "b88ea727-dc76-4e27-8ff4-35b026d895b7", "StepName": "liveBiDeploy", "Locator": null, "StepId": "399195", "StartTime": "2014-08-14T18:46:15", "FinishTime": "2014-08-14T22:27:37", "CompletedOnSiteStage": "Online after", "Duration": 13282000.0, "DurationLabel": "3 h. 41 min. 22 s.",
                    "ChildSteps": [{
                        "Id": "7674a780-80ae-4676-8a7d-bf05598231a0", "StepName": "seedHandlers", "Locator": "pipelines/liveBiDeploy/14/seedHandlers/1", "StepId": "649310", "StartTime": "2014-08-14T18:46:15", "FinishTime": "2014-08-14T18:47:20", "CompletedOnSiteStage": "Offline", "Duration": 65000.0, "DurationLabel": "1 min. 5 s.",
                        "ChildSteps": [{
                            "Id": "e0b6d052-75a4-42a6-8b31-6e372b6420b4", "StepName": "BiCustomerManagement", "Locator": "tab/build/detail/liveBiDeploy/14/seedHandlers/1/BiCustomerManagement", "StepId": "3210887", "StartTime": "2014-08-14T18:46:15", "FinishTime": "2014-08-14T18:47:20", "CompletedOnSiteStage": "Offline", "Duration": 65000.0, "DurationLabel": "1 min. 5 s.",
                            "ChildSteps": []
                        }]
                    }, {
                        "Id": "ddd30be8-39a3-4952-9911-4aced2494936", "StepName": "installDatabase", "Locator": "pipelines/liveBiDeploy/14/installDatabase/1", "StepId": "649381", "StartTime": "2014-08-14T22:23:18", "FinishTime": "2014-08-14T22:25:20", "CompletedOnSiteStage": "Online after", "Duration": 122000.0, "DurationLabel": "2 min. 2 s.",
                        "ChildSteps": [{
                            "Id": "fbb9b930-42b5-4cb1-b338-d564920b64ef", "StepName": "BiCustomerManagement", "Locator": "tab/build/detail/liveBiDeploy/14/installDatabase/1/BiCustomerManagement", "StepId": "3211336", "StartTime": "2014-08-14T22:23:18", "FinishTime": "2014-08-14T22:25:20", "CompletedOnSiteStage": "Online after", "Duration": 122000.0, "DurationLabel": "2 min. 2 s.",
                            "ChildSteps": []
                        }]
                    }]
                }]
        };

        html = '<div id="testEl" data-tree-grid="" data-columns="measurementColumns" data-rows="measurements" data-child-items-property="ChildSteps"></div>';

        inject(function ($rootScope, _$compile_, _$timeout_) {
            $compile = _$compile_;
            $timeout = _$timeout_;

            scope = $rootScope.$new();
            scope.measurementColumns = [
                { field: 'StepName', style: { width: '60%' }, formatCallback: function () { return "##FORMAT##"; } },
                { field: 'StartTime', style: { width: '20%', 'text-align': 'right' } },
                { field: 'FinishTime', style: { width: '20%', 'text-align': 'right' } }
            ];
            scope.measurements = data;

            prepareDirective(scope, []);
        });
    });

    function prepareDirective(s, rows) {
        container = angular.element(html);
        compiled = $compile(container);
        element = compiled(s);
        s.$digest();
    }

    /***********************************************************************************************************************/

    //it('should populate content after init', function () {
    //    scope.measurements = [];
    //    scope.$digest();
    //    //$timeout.flush();

    //    var keys = getKeys(scope);
    //    console.log(keys);

    //    targetScope = element.isolateScope();

    //    expect(targetScope.tableData).toBeDefined();
    //});

    function getKeys(obj) {
        if (!obj) return [];

        var keys = [];
        for (var key in obj) {
            keys.push(key);
        }
        return keys;
    }

});
