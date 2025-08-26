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

$.validator.addMethod('ur-eik', function (value, element) {
    if (!value) {
        return true;
    }
    return CheckEIK(value);
});

$.validator.unobtrusive.adapters.add('ur-eik', function (options) {
    options.rules['ur-eik'] = true;
    options.messages['ur-eik'] = options.message;
});

$.validator.addMethod('ur-egn', function (value, element) {
    if (!value) {
        return true;
    }
    return CheckEGN(value);
});

$.validator.unobtrusive.adapters.add('ur-egn', function (options) {
    options.rules['ur-egn'] = true;
    options.messages['ur-egn'] = options.message;
});

function CheckEGN(egn) {
    if (!egn) {
        return false;
    }
    if (egn.length != 10) {
        return false;
    }
    if (egn == "0000000000") {
        return false;
    }
    let a = 0;
    let valEgn = 0;
    const multipliers = [2, 4, 8, 5, 10, 9, 7, 3, 6];
    for (var i = 0; i < egn.length - 1; i++) {
        a = parseInt(egn.substring(i, i + 1))
        if (isNaN(a)) {
            return false;
        }
        valEgn += parseInt(multipliers[i]) * a;
    }
    let chkSum = valEgn % 11;
    if (chkSum == 10)
        chkSum = 0;
    if (chkSum != parseInt(egn.substring(egn.length - 1, egn.length))) {
        return false;
    }
    return true;
}

function CheckLNCH(personalId) {
    if (!personalId) {
        return false;
    }
    if (!Is10NumbersRegEx(personalId)) {
        return false;
    }

    let lastNumber = 0;
    let sum = 0;
    const multipliers = [21, 19, 17, 13, 11, 9, 7, 3, 1];

    for (var i = 0; i < personalId.length - 1; i++) {
        lastNumber = parseInt(personalId.substring(i, i + 1));
        sum += lastNumber * multipliers[i];
    }

    lastNumber = parseInt(personalId.substring(personalId.length - 1, personalId.length));
    return (sum % 10) == lastNumber;
}

function Is10NumbersRegEx(uic) {
    var match;
    let params = "^\\d{10}$";
    match = new RegExp(params).exec(uic);
    return (match && (match.index === 0) && (match[0].length === uic.length));
}

function CheckSum9EIK(eik) {
    let sum = 0, a = 0, chkSum = 0;
    for (var i = 0; i < 8; i++) {
        a = parseInt(eik.substring(i, i + 1))
        if (isNaN(a)) {
            return null;
        }
        sum += a * (i + 1);
    }
    chkSum = sum % 11;
    if (chkSum == 10) {
        sum = 0;
        a = 0;
        chkSum = 0;
        for (var i = 0; i < 8; i++) {
            a = parseInt(eik.substring(i, i + 1))
            if (isNaN(a)) {
                return null;
            }
            sum += a * (i + 3);
        }
        chkSum = sum % 11;
        if (chkSum == 10) {
            chkSum = 0;
        }
    }
    return chkSum;
}

function CheckSum13EIK(eik) {
    let sum = 0, a = 0, chkSum = 0;
    for (var i = 8; i < 12; i++) {
        a = parseInt(eik.substring(i, i + 1))
        if (isNaN(a)) {
            return null;
        }
        switch (i) {
            case 8:
                sum = a * 2;
                continue;
            case 9:
                sum += a * 7;
                continue;
            case 10:
                sum += a * 3;
                continue;
            case 11:
                sum += a * 5;
                continue;
        }
    }
    chkSum = sum % 11;
    if (chkSum == 10) {
        for (var i = 8; i < 12; i++) {
            a = parseInt(eik.substring(i, i + 1))
            if (isNaN(a)) {
                return null;
            }
            switch (i) {
                case 8:
                    sum = a * 4;
                    continue;
                case 9:
                    sum += a * 9;
                    continue;
                case 10:
                    sum += a * 5;
                    continue;
                case 11:
                    sum += a * 7;
                    continue;
            }
        }
        chkSum = sum % 11;
        if (chkSum == 10) chkSum = 0;
    }
    return chkSum;
}

function CheckEIK(eik) {
    if (!eik) {
        return false;
    }
    if ((eik.length != 9) && (eik.length != 13)) {
        return false;
    }

    if (CheckSum9EIK(eik) == eik.substring(8, 9)) {
        if (eik.length == 9) {
            return true;
        }
        else {
            return CheckSum13EIK(eik) == eik.substring(12, 13);
        }
    }
    return false;
}