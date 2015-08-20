jQuery.validator.addMethod("isFuture", function(value) {
    if (!/Invalid|NaN/.test(new Date(value))) {
        return new Date(value) > new Date();
    }
    return false;
},
"Date must be in the future");
