var stateDateMessage;
$.validator.addMethod('ur-state-date', function (value, element) {
    const dateFromInit = moment($("#DateFromInit").val(), 'DD.MM.YYYY');
    const dateFrom = moment($("#DateFrom").val(), 'DD.MM.YYYY');
    const dateTo = moment($("#DateTo").val(), 'DD.MM.YYYY');
    if (dateFrom > dateTo && element.name == "DateTo") {
        stateDateMessage = "Началото на периода трябва да е преди края на периода";
        return false;
    }
    if (dateFrom < dateFromInit && element.name == "DateFrom") {
        stateDateMessage = "Началото на периода трябва да е след " + dateFromInit.format('DD.MM.YYYY');
        return false;
    }
    return true;
});

$.validator.unobtrusive.adapters.add('ur-state-date', function (options) {
    options.rules['ur-state-date'] = true;
    options.messages['ur-state-date'] = function () { return stateDateMessage; };
});