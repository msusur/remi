var greaterOrEqualDate = function(first, second) {
    var firstDate = Date.parse(first);
    var secondDate = Date.parse(second);
    return firstDate >= secondDate;
};

var utcNow = function() {
    var date = new Date;
    return date;
};

var actionInFuture = function(date) {
    var utcDate = utcNow();
    return greaterOrEqualDate(date, utcDate);
};

var actionInMoreThanNMinutes = function (date, minutes) {
    date = new Date(Date.parse(date) - minutes * 60000);
    return actionInFuture(date);
};

var addZeroIfLessThenTen = function(number) {
    if (number < 10) {
        return '0' + number;
    }
    return number;
};
