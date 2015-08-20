describe("Localdate Filter", function () {
    var sut;

    beforeEach(function () {
        module("common");

        inject(function (localdateFilter) {
            sut = localdateFilter;
        });
    });

    it("should parse date and format with defaults, when no format passed", function () {
        var date = "2015-05-18T10:20:18+01:00";

        var result = sut(date);

        expect(result).toEqual(moment(date).local().format("L HH:mm"));
    });

    it("should parse date and format with passed format, when format passed", function () {
        var date = "2015-05-18T10:20:18+01:00";

        var result = sut(date, "HH:mm");

        expect(result).toEqual(moment(date).local().format("HH:mm"));
    });

    it("should not parse and format with passed format, when passed Date object and format", function () {
        var date = new Date("2015-05-18T10:20:18+01:00");

        var result = sut(date, "HH:mm");

        expect(result).toEqual(moment(date).local().format("HH:mm"));
    });

    it("should log exception, when date is invalid", function () {
        var date = "2015-ad05-1adadadsf0";
        spyOn(console, "log");

        var result = sut(date);

        expect(result).toEqual("2015-ad05-1adadadsf0");
        expect(console.log).toHaveBeenCalledWith("localdate filter failed for 2015-ad05-1adadadsf0");
    });
});

