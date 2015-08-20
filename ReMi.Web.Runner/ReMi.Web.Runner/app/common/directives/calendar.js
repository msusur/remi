(function () {
    "use strict";

    var directiveId = "calendar";

    angular.module("app")
        .directive(directiveId, [calendar2]);

    function calendar2() {
        return {
            restrict: "A",
            templateUrl: "app/common/directives/tmpls/calendar/calendar.html",
            required: "ngModel",
            scope: {
                configuration: "=",
                data: "=",
                viewChanged: "=",
                contextmenu: "=",
                eventClick: "=",
                ngModel: "=",
                isBusy: "="
            },
            link: function ($scope) {
                $scope.range = function (n) { return new Array(isNaN(n) || n < 0 ? 0 : n); };
                $scope.weekdays = function (n) { return moment.weekdays(n); }

                var config = angular.copy($scope.configuration);
                config.viewChanged = $scope.viewChanged;
                config.eventClick = $scope.eventClick;
                config.contextmenu = $scope.contextmenu;
                $scope.cal = $scope.ngModel = new calendar(config, $scope.data);
                $scope.isBusy = true;

                $scope.$watch("data", function () {
                    $scope.cal.events = $scope.data;
                    $scope.cal.refresh();
                }, true);
            }
        };
    }

    var defaults = {
        tmplPath: "tmpls/",
        view: "month",
        position: {},
        classes: {
            months: {
                inmonth: "cal-day-inmonth",
                outmonth: "cal-day-outmonth",
                saturday: "cal-day-weekend",
                sunday: "cal-day-weekend",
                today: "cal-day-today"
            }
        }
    };

    function calendar(params, events) {
        var self = this;
        this.options = angular.copy(defaults);
        this.data = {};
        this.events = events;
        this.viewChanged = params.viewChanged;
        this.eventClick = params.eventClick;
        this.contextmenu = params.contextmenu;
        this.views = ["year", "month"];

        for (var k in params) {
            if (params.hasOwnProperty(k)) {
                self.options[k] = params[k];
            }
        }
        this.options.position.start = params.date ? params.date : new Date();
        this.setView(this.options.view);
    }

    calendar.prototype.getTitle = function () {
        var p = this.options.position.start;
        switch (this.options.view) {
            case "year":
                return p.format("[Year] YYYY");
            case "month":
                return p.format("MMMM YYYY");
        }
        return "";
    };

    calendar.prototype.setRange = function (date) {
        switch (this.options.view) {
            case "year":
                this.options.position.start = moment(date).startOf("year");
                this.options.position.end = moment(date).endOf("year");
                break;
            case "month":
                this.options.position.start = moment(date).startOf("month");
                this.options.position.end = moment(date).endOf("month");
                break;
        }
        this.invokeViewChanged();
    };

    calendar.prototype.refresh = function () {
        switch (this.options.view) {
            case "year":
                this.yearView();
                break;
            case "month":
                this.monthView();
                break;
        }
    };

    calendar.prototype.setView = function (view) {
        if (!view || this.views.indexOf(view) < 0) {
            throw "View '" + view + "' does not exist";
        }
        this.options.view = view;
        this.setRange(this.options.position.start);
    };

    calendar.prototype.navigate = function (where) {
        var date;
        switch (where) {
            case "next": date = angular.copy(this.options.position.start).add(1, this.options.view); break;
            case "prev": date = angular.copy(this.options.position.start).subtract(1, this.options.view); break;
            case "today":
            default: date = moment();
        }
        this.setRange(date);
    };

    calendar.prototype.invokeViewChanged = function () {
        if (this.viewChanged || typeof (this.viewChanged) === "function") {
            this.viewChanged(this.options.view, this);
        }
    }

    calendar.prototype.invokeEventClick = function (event, $event) {
        if (this.eventClick || typeof (this.eventClick) === "function") {
            $event.preventDefault();
            $event.stopPropagation();
            this.eventClick(event, $event, this);
        }
    }

    calendar.prototype.invokeContextmenu = function (date, event, $event) {
        if (this.contextmenu || typeof (this.contextmenu) === "function") {
            $event.preventDefault();
            $event.stopPropagation();
            this.contextmenu(angular.copy(date), event, $event, this);
        }
    }

    calendar.prototype.getEvents = function (start, end) {
        if (!this.events) return [];
        var condition;
        if (end) {
            condition = function (x) { return x.start.isBetween(start, end, "minute") };
        } else {
            condition = function (x) { return x.start.isSame(start, "day"); };
        }
        return Enumerable.From(this.events)
            .Where(condition)
            .OrderBy(function (x) { return x.start.toISOString(); })
            .ToArray();
    };


    calendar.prototype.monthView = function () {
        if (!this.monthData) {
            this.monthData = {
                dayTmpl: this.options.tmplPath + "month-day.html",
                days: {}
            }
        }
        this.tmpl = this.options.tmplPath + "month.html";
        var daysOfMonth = this.options.position.end.diff(this.options.position.start, "days") + 1;
        var dayOfWeek = this.options.position.start.isoWeekday() - 1;
        this.monthData.dayRowsCount = Math.ceil((daysOfMonth + dayOfWeek) / 7);

        var days = this.monthData.days;
        var currentDay = this.options.position.wideStart = angular.copy(this.options.position.start).subtract(dayOfWeek, "days");
        for (var i = 0; i < this.monthData.dayRowsCount; i++)
            for (var j = 0; j < 7; j++) {
                var key = i + "_" + j;
                var day = this.monthData.days[key] ? this.monthData.days[key] : {};
                day.date = currentDay;
                day.css = [];
                day.dayOfMonth = currentDay.date();
                if (currentDay.month() === this.options.position.start.month()) day.css.push(this.options.classes.months.inmonth);
                else day.css.push(this.options.classes.months.outmonth);
                if (currentDay.isSame(moment(), "day")) day.css.push(this.options.classes.months.today);
                if (currentDay.isoWeekday() === 6) day.css.push(this.options.classes.months.saturday);
                if (currentDay.isoWeekday() === 7) day.css.push(this.options.classes.months.sunday);
                day.events = this.getEvents(currentDay);
                days[key] = day;
                currentDay = angular.copy(currentDay).add(1, "day");
            }

        this.options.position.wideEnd = currentDay.subtract(1, "minutes");
    };

    calendar.prototype.yearView = function () {
        if (!this.yearData) {
            this.yearData = {
                monthTmpl: this.options.tmplPath + "year-month.html",
                eventsTmpl: this.options.tmplPath + "year-events-list.html",
                months: {},
                expandEvents: function (selectedMonth) {
                    if (selectedMonth.events.length === 0) return;
                    var self = this;
                    this.selectedMonth = undefined;
                    angular.forEach(this.months, function (month) {
                        month.showEvents = !selectedMonth.showEvents && month.name === selectedMonth.name;
                        if (month.showEvents) {
                            self.selectedMonth = month;
                        }
                    });
                },
                showEvents: function (quater) {
                    return this.selectedMonth && this.selectedMonth.quater === quater;
                }
            }
        }
        this.yearData.selectedMonth = undefined;
        this.tmpl = this.options.tmplPath + "year.html";

        var months = this.yearData.months;
        this.options.position.wideStart = this.options.position.start;
        this.options.position.wideEnd = this.options.position.end;
        var currentMonth = angular.copy(this.options.position.start).startOf("month");
        for (var i = 1; i <= 12; i++) {
            var month = this.yearData.months[i] ? this.yearData.months[i] : { showEvents: false };
            month.startDate = angular.copy(currentMonth);
            month.name = currentMonth.format("MMMM");
            month.quater = Math.floor(month.startDate.get("month") / 4);
            month.number = (month.startDate.get("month") % 4) + 1;

            month.events = this.getEvents(currentMonth, angular.copy(currentMonth).endOf("month"));
            months[i] = month;
            currentMonth = angular.copy(currentMonth).add(1, "month");
        }
    };
})()
