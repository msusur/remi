describe("TreeList Directive ", function () {
    var scope, container, element, html, compiled, compile;
    var templateCache, injector, httpBackend, timeout;

    beforeEach(module('app'));
    beforeEach(inject(function ($compile, $rootScope, $templateCache, $injector, _$httpBackend_, _$timeout_) {
        html = '<div data-tree-list="businessUnits" data-options="options"></div>';
        var controllerHtml = "<div class=\"container\"><div data-ng-show=\"viewMode == 'code'\" class=\"cssFade\"></div></div>";

        scope = $rootScope.$new();
        compile = $compile;
        templateCache = $templateCache;
        injector = $injector;
        httpBackend = _$httpBackend_;
        timeout = _$timeout_;

        templateCache.put("ruleEditor.html", controllerHtml);
        httpBackend.when('GET', 'app/common/directives/tmpls/treeList.html').respond(controllerHtml);
        httpBackend.when('GET', 'app/releaseCalendar/releaseCalendar.html').respond('<div></div>');
    }));
    afterEach(function () {
        httpBackend.verifyNoOutstandingExpectation();
        httpBackend.verifyNoOutstandingRequest();
    });

    function prepareDirective(s) {
        container = angular.element(html);
        compiled = compile(container);
        element = compiled(s);
        s.$digest();
        httpBackend.flush();
    }

    /***********************************************************************************************************************/

    it('should fillout element scope, when initialise', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            childCheckProperty: 'Checked',
            childSelectProperty: 'IsDefault',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true,
            allowSelect: true,
            disableSelectIfNotChecked: true,
            selectTooltip: 'Select default',
            checkTooltip: 'Assign user to package'
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();

        expect(isolatedScope.hideCheckBoxes).toEqual(false);
        expect(isolatedScope.hideRadioButtons).toEqual(false);
        expect(isolatedScope.selectTooltipText).toEqual('Select default');
        expect(isolatedScope.checkTooltipText).toEqual('Assign user to package');
        expect(isolatedScope.disableSelectIfNotChecked).toEqual(true);

    });

    it('should populate element data, when initialized', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            childCheckProperty: 'Checked',
            childSelectProperty: 'IsDefault',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true,
            allowSelect: true,
            disableSelectIfNotChecked: true,
            selectTooltip: 'Select default',
            checkTooltip: 'Assign user to package'
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();

        expect(isolatedScope.data).toBeDefined();
        expect(isolatedScope.data.length).toEqual(businessUnits.length);
        expect(isolatedScope.data[0].Expanded).toEqual(false);
        expect(isolatedScope.data[0].Name).toEqual(businessUnits[0].Description);
        expect(isolatedScope.data[0].Element).toEqual(businessUnits[0]);
        expect(isolatedScope.data[0].Children.length).toEqual(businessUnits[0].Packages.length);
        expect(isolatedScope.data[0].Children[0].Element).toEqual(businessUnits[0].Packages[0]);
        expect(isolatedScope.data[0].Children[0].Name).toEqual(businessUnits[0].Packages[0].Name);
        expect(isolatedScope.data[0].Children[0].Checked).toEqual(false);
        expect(isolatedScope.data[0].Children[0].Selected).toEqual(false);
        expect(isolatedScope.data[2].Children[0].Selected).toEqual(true);
        expect(isolatedScope.data[2].Children[0].Checked).toEqual(true);
    });

    it('should re-populate tree list data, when business units changed', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name'
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();

        expect(isolatedScope.data[0].Name).toEqual(businessUnits[0].Description);
        expect(isolatedScope.data[1].Children[1].Name).toEqual(businessUnits[1].Packages[1].Name);

        var tempBu = angular.copy(businessUnits);
        tempBu[0].Description = "New description";
        tempBu[1].Packages[1].Name = "New name";
        scope.businessUnits = tempBu;
        scope.$apply();

        expect(isolatedScope.data[0].Name).toEqual(tempBu[0].Description);
        expect(isolatedScope.data[1].Children[1].Name).toEqual(tempBu[1].Packages[1].Name);
    });

    it('should re-initialized tree list, when option changed', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            selectTooltip: 'Select default'
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();

        expect(isolatedScope.data[0].Name).toEqual(businessUnits[0].Description);
        expect(isolatedScope.selectTooltipText).toEqual('Select default');

        scope.options.parentNameProperty = "Name";
        scope.options.selectTooltip = "new tooltip";
        scope.$apply();

        expect(isolatedScope.data[0].Name).toEqual(businessUnits[0].Name);
        expect(isolatedScope.selectTooltipText).toEqual('new tooltip');
    });

    it('should expand/collapse parent node, when toggle invoked', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name'
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();

        var item1 = { Expanded: true };
        var item2 = { Expanded: false };
        var item3 = {};
        isolatedScope.toggle(item1);
        isolatedScope.toggle(item2);
        isolatedScope.toggle(item3);

        expect(item1.Expanded).toEqual(false);
        expect(item2.Expanded).toEqual(true);
        expect(item3.Expanded).toEqual(true);
    });

    it('should check parent item and all children, when parentCheck invoked', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name'
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();

        var parent = isolatedScope.data[1];
        isolatedScope.checkParent(parent);

        expect(parent.Checked).toEqual(true);
        expect(parent.Children[0].Checked).toEqual(true);
        expect(parent.Children[1].Checked).toEqual(true);
    });

    it('should return FA class for parent node check state and Check parent if all childern selected, when parentCheckStateClass called', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name'
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        isolatedScope.data[1].Children[0].Checked = true;
        isolatedScope.data[3].Checked = true;

        expect(isolatedScope.parentCheckStateClass(isolatedScope.data[0])).toEqual('');
        expect(isolatedScope.parentCheckStateClass(isolatedScope.data[1])).toEqual('fa-square');
        expect(isolatedScope.parentCheckStateClass(isolatedScope.data[2])).toEqual('fa-check');
        expect(isolatedScope.data[3].Checked).toEqual(true);
        expect(!!isolatedScope.data[4].Checked).toEqual(false);
    });

    it('should return true, when parent has children', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name'
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        var result = isolatedScope.hasChildren(isolatedScope.data[0]);

        expect(result).toEqual(true);
    });

    it('should return false, when parent has no children', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name'
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        var result = isolatedScope.hasChildren(isolatedScope.data[3]);

        expect(result).toEqual(false);
    });

    it('should return false, when not all children checked', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        var result = isolatedScope.hasAllChecked();

        expect(result).toEqual(false);
    });

    it('should return true, when all children checked', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        Enumerable.From(isolatedScope.data).ForEach(function (parent) {
            Enumerable.From(parent.Children).ForEach(function (child) {
                child.Checked = true;
            });
        });
        var result = isolatedScope.hasAllChecked();

        expect(result).toEqual(true);
    });

    it('should return "check all", when not all children checked', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        var result = isolatedScope.checkAllText();

        expect(result).toEqual("check all");
    });

    it('should return "uncheck all", when all children checked', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        Enumerable.From(isolatedScope.data).ForEach(function (parent) {
            Enumerable.From(parent.Children).ForEach(function (child) {
                child.Checked = true;
            });
        });
        var result = isolatedScope.checkAllText();

        expect(result).toEqual("uncheck all");
    });

    it('should check all, when checkAll invoked and not all checked', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        isolatedScope.checkAll();
        var result = isolatedScope.hasAllChecked();

        expect(result).toEqual(true);
    });

    it('should uncheck all, when checkAll invoked and all checked', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        Enumerable.From(isolatedScope.data).ForEach(function (parent) {
            Enumerable.From(parent.Children).ForEach(function (child) {
                child.Checked = true;
            });
        });
        isolatedScope.checkAll();
        var result = Enumerable.From(isolatedScope.data)
                        .All(function (x) {
                            return Enumerable.From(x.Children)
                                .All(function (c) {
                                    return !c.Checked;
                                });
                        });

        expect(result).toEqual(true);
    });

    it('should return false, when not all expanded', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        var result = isolatedScope.hasAllExpanded();

        expect(result).toEqual(false);
    });

    it('should return true, when all expanded', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        Enumerable.From(isolatedScope.data).ForEach(function (parent) {
            parent.Expanded = true;
        });
        var result = isolatedScope.hasAllExpanded();

        expect(result).toEqual(true);
    });

    it('should return "expand all", when not all expanded', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        var result = isolatedScope.toggleAllText();

        expect(result).toEqual("expand all");
    });

    it('should return "collapse all", when all expanded', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        Enumerable.From(isolatedScope.data).ForEach(function (parent) {
            parent.Expanded = true;
        });
        var result = isolatedScope.toggleAllText();

        expect(result).toEqual("collapse all");
    });

    it('should expand all, when toggleAll and not all expanded', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        isolatedScope.toggleAll();
        var result = isolatedScope.hasAllExpanded();

        expect(result).toEqual(true);
    });

    it('should collapse all, when toggleAll and all expanded', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        Enumerable.From(isolatedScope.data).ForEach(function (parent) {
            parent.Expanded = true;
        });
        isolatedScope.toggleAll();
        var result = Enumerable.From(isolatedScope.data).All(function (parent) {
            return !parent.Expanded;
        });

        expect(result).toEqual(true);
    });

    it('should check child item, when checkChild with value true', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        var actual = isolatedScope.data[0].Children[0];
        isolatedScope.checkChild(actual, true);

        expect(actual.Checked).toEqual(true);
        expect(actual.Element.Checked).toEqual(true);
    });

    it('should uncheck child item, when checkChild with value undefined', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true,
            allowSelect: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        var actual = isolatedScope.data[0].Children[0];
        actual.Checked = true;
        isolatedScope.checkChild(actual);

        expect(actual.Checked).toEqual(false);
        expect(actual.Element.Checked).toEqual(false);
    });

    it('should uncheck child item and unselect, when checkChild with value false and disableSelectIfNotChecked is true', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true,
            allowSelect: true,
            disableSelectIfNotChecked: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        var actual = isolatedScope.data[0].Children[0];
        actual.Checked = true;
        actual.Selected = true;
        isolatedScope.checkChild(actual);

        expect(actual.Checked).toEqual(false);
        expect(actual.Element.Checked).toEqual(false);
        expect(actual.Selected).toEqual(false);
        expect(actual.Element.Selected).toEqual(false);
    });

    it('should not allow select, when allowSelect is false', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowSelect: false
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        var actual = isolatedScope.data[0].Children[0];
        isolatedScope.selectChild(actual);

        expect(actual.Selected).toEqual(false);
    });

    it('should not allow select, when allowSelect is true, disableSelectIfNotChecked is true and Checked is false', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowCheck: true,
            allowSelect: true,
            disableSelectIfNotChecked: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        var actual = isolatedScope.data[0].Children[0];
        actual.Checked = false;
        isolatedScope.selectChild(actual);

        expect(actual.Selected).toEqual(false);
    });

    it('should select child and unselect the rest, when selectChild called', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowSelect: true,
            childSelectProperty: 'IsDefault'
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        var actual = isolatedScope.data[0].Children[0];
        isolatedScope.selectChild(actual);

        expect(actual.Selected).toEqual(true);
        expect(actual.Element.IsDefault).toEqual(true);
        expect(Enumerable.From(isolatedScope.data)
            .SelectMany(function (x) { return x.Children; })
            .Count(function (x) { return x.Selected; })).toEqual(1);
        expect(Enumerable.From(isolatedScope.data)
            .SelectMany(function (x) { return x.Children; })
            .Count(function (x) { return x.Element.IsDefault; })).toEqual(1);
    });

    it('should return false, when no child is selected', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowSelect: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        var result = isolatedScope.parentSelected(isolatedScope.data[0]);

        expect(result).toEqual(false);
    });

    it('should return true, when one child is selected', function () {
        scope.businessUnits = angular.copy(businessUnits);
        scope.options = {
            childrenArrayProperty: 'Packages',
            parentNameProperty: 'Description',
            childNameProperty: 'Name',
            allowSelect: true
        };

        prepareDirective(scope);
        var isolatedScope = element.isolateScope();
        isolatedScope.data[1].Children[1].Selected = true;
        var result = isolatedScope.parentSelected(isolatedScope.data[1]);

        expect(result).toEqual(true);
    });
});

var businessUnits = [
    {
        "ExternalId": "9f13aee7-3da1-4ceb-a25f-e320fd7c91f5",
        "Name": "Iowa",
        "Description": "Iowa",
        "Packages": [
            { "Name": "Eu Accumsan Incorporated", "ExternalId": "0f1ffaaa-48bb-432c-952f-9e1e67c6eb33", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": true, "IsDefault": false }
        ]
    },
    {
        "ExternalId": "823ff88d-3b0d-4f58-b976-fbe6ecfbdcbe",
        "Name": "NI",
        "Description": "North Island",
        "Packages": [
            { "Name": "Adipiscing Corp.", "ExternalId": "bc79f340-fd28-47f4-94c7-cb98963ba573", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Adipiscing Corp. Demeter", "ExternalId": "c4979b08-2cd8-4111-8253-6b35766ba96f", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false }
        ]
    },
    {
        "ExternalId": "9246cf63-5d00-429a-a114-8db53ee375eb",
        "Name": "Vi",
        "Description": "Victoria",
        "Packages": [
            { "Name": "Mi Consulting", "ExternalId": "61ee6437-4029-408c-847d-73e84cc26819", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": true, "IsDefault": true, "Checked": true }
        ]
    },
    {
        "ExternalId": "ed47c52a-39ed-4fb7-8b69-31f715023c2f",
        "Name": "Adana",
        "Description": "Adana",
        "Packages": []
    },
    {
        "ExternalId": "86ed308c-1b91-46bb-b6c3-5e689db7b2df",
        "Name": "Wie",
        "Description": "Waals-Brabant",
        "Packages": [
            { "Name": "Non Inc.", "ExternalId": "01aa0e1d-8da1-48d6-9483-c15cba56d666", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": true, "IsDefault": false }
        ]
    },
    {
        "ExternalId": "29be4364-c96b-4f1f-bc80-3ef40c016ffe",
        "Name": "SA",
        "Description": "Saskatchewan",
        "Packages": [
            { "Name": "Consequat Lectus Sit PC", "ExternalId": "48853fce-3371-4bf0-a332-90e26fc9804f", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false }
        ]
    },
    {
        "ExternalId": "6c84143d-8b15-4168-b9cc-7ace6c7a98aa",
        "Name": "HE-GE",
        "Description": "Henegouwen",
        "Packages": [
            { "Name": "Felis Ltd", "ExternalId": "c6f3e15c-1f54-469a-3826-eddc3403cc75", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "KY", "ExternalId": "78d33ba2-39f9-40d6-8367-3638a70d8a92", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Auctor Limited", "ExternalId": "c7d9fd7f-7dd7-4962-4a1b-bb18a0c1a44b", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": true, "IsDefault": false },
            { "Name": "Ignatius Sweet", "ExternalId": "46482aea-b9c6-4d17-b3ba-71765368bb19", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Payroll", "ExternalId": "890011ae-06fa-4576-891b-8edfe71281e0", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": true, "IsDefault": false },
            { "Name": "Remediation", "ExternalId": "3c5ae728-befb-43a8-c70e-5ea1b9812b43", "ReleaseTrack": "Automated", "ChooseTicketsByDefault": true, "IsDefault": false },
            { "Name": "Pede Nonummy Ut LLC", "ExternalId": "2ae5175b-9e20-43c2-825a-8ea39f37edec", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Rhoncus Associates", "ExternalId": "d32ac36d-822d-4048-2296-b61a3f7e3384", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Queensland", "ExternalId": "e289a382-82d8-4a40-a336-3e7f4b14ce76", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Connacht", "ExternalId": "ef02646c-4390-4a59-be66-9ccf0885491e", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Ullamcorper", "ExternalId": "d3b8900b-d73b-49fe-bf32-bd16fc558f89", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false },
            { "Name": "Lobortis Quis Corp.", "ExternalId": "8f100e45-bb2a-466d-a132-11e5e120defc", "ReleaseTrack": "Manual", "ChooseTicketsByDefault": false, "IsDefault": false }
        ]
    }
];
