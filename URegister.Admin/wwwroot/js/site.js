const noResultsFoundMessage = 'Няма намерени резултати!';

// спира auto-fill в Chrome
$('input').attr('autocomplete', 'one-time-code');

$(function () {
    //преведено съобщение за не намерени резултати при autocomplete елементите
    $.fn.search.settings.templates.message = function () {
        return '<div class="message"><div class="header">' + noResultsFoundMessage + '</div><div class="description">Пробвайте с друг текст!</div></div>';
    }

    initializeElements($(document));

    InitForm();

    //Tooltip-a при чекбокс да не излиза от очертанията на елемента
    $('.checkbox-tooltip').popup({
        boundary: '.ui checkbox'
    })

    $('.person-tooltip').popup({
        boundary: '.person-fieldset'
    })

    $('input:not([type="hidden"]), textarea').each(function () {
        $(this).attr('oninvalid', 'setCustomValidity(getErrorMessage(this))');
        $(this).attr('oninput', 'setCustomValidity("")');

        if ($(this).closest('.datetime-calendar').length || $(this).closest('.dateonly-calendar').length) {
            $(this).attr('onchange', 'this.setCustomValidity("")');
        }
    });

    //При readonly сложен контрол тип fieldset прави inputs readonly
    if ($('fieldset.readonly-fieldset').length > 0) {
        // Make all inputs and textareas inside the readonly fieldset readonly and add classes
        $('fieldset.readonly-fieldset').find('input, textarea').each(function () {
            $(this).prop('readonly', true).addClass('ui disabled input');
        });

        // Add the `disabled` class to any `.ui.calendar` divs within `.readonly-fieldset` fieldsets
        $('fieldset.readonly-fieldset').find('div.ui.calendar').addClass('disabled');

        // Add the `disabled` class to any `.ui fluid search selection dropdown` divs within `.readonly-fieldset` fieldsets
        $('fieldset.readonly-fieldset').find('div.ui.fluid.search.selection.dropdown').addClass('disabled');
    }
});

function generatePIDValue(pidContainer) {
    let pidType = pidContainer.find('.ui.dropdown').dropdown('get value');
    let pidNumber = pidContainer.find(':input[type=text]').val().trim();    
    pidContainer.find("input[type='hidden']:not(.label input)").val(pidType + ':' + pidNumber);
}

function getErrorMessage(sender) {
    let value = $(sender).val();

    if (value === '' && $(sender).attr('required')) {
        return `Моля въведете стойност`;
    }

    if ($(sender).attr('pattern')) {
        let regexPattern = new RegExp($(sender).attr('pattern'));

        if (!regexPattern.test($(sender).val())) {
            return 'Стойността не съвпада с регулярния израз.';
        }
    }

    if ($(sender).attr('type') === 'number') {
        let number = new Number(value);
        var min = parseFloat($(sender).attr('min').replace(',', '.'));
        var max = parseFloat($(sender).attr('max').replace(',', '.'));
        if (!isNaN(min) && !isNaN(max)) {
            return `Моля въведете стойност между ${min} и ${max}.`;
        } else if (!isNaN(min)) {
            return `Моля въведете стойност по-голяма или равна на ${min}.`;
        } else if (!isNaN(max)) {
            return `Моля въведете стойност по-малка или равна на ${max}.`;
        }
    }

    if ($(sender).attr('type') === 'checkbox') {
        return 'Моля изберете, ако искате да продължите.'
    }

    return '';
}

// за Calendar

var calendarTextConfig = {
    days: ['Н', 'П', 'В', 'С', 'Ч', 'П', 'С'],
    months: ['Януари', 'Февруари', 'Март', 'Април', 'Май', 'Юни', 'Юли', 'Август', 'Септември', 'Октомври', 'Ноември', 'Декември'],
    monthsShort: ['Яну', 'Фев', 'Мар', 'Апр', 'Май', 'Юни', 'Юли', 'Авг', 'Сеп', 'Окт', 'Ное', 'Дек'],
    today: 'Днес',
    now: 'Сега',
    am: 'AM',
    pm: 'PM'
};

function initializeElements(parentForm) {
    //#region За checkbox

    $(".checkbox-template").change(function () {
        if ($(this).is(':checked')) {
            $(this).parent().find('input:hidden').val('true');
        } else {
            $(this).parent().find('input:hidden').val('false');
        }
    });

    //#endregion

    $('.ekatte')
        .each(function () {
            $(this).search(
                {
                    apiSettings: {
                        url: '/Nomenclature/GetEkatteValues?query={query}',
                    },
                    minCharacters: 3,
                    type: 'category',
                    onSelect: function (result) {
                        let hiddenElement = $(this).find('input[type="hidden"]');
                        hiddenElement.val(result.value);
                        hiddenElement.trigger('change');
                    }
                });
        });

    $('.ekatte').find('input.prompt')
        .on('input keyup', function () {
            let hiddenElement = $(this).parent().find('input[type="hidden"]');
            hiddenElement.val(0);
            hiddenElement.trigger('change');
        });

    //Autocomplete initialize
    $('.autocomplete')
        .each(function () {
            $(this).search(
                {
                    apiSettings: {
                        url: '/Nomenclature/GetAutocompleteValues?query={query}&nomenclatureType=' + $(this).data('nomenclatureType'),
                    },
                    minCharacters: 3,
                    onSelect: function (result) {
                        let hiddenElement = $(this).find('input[type="hidden"]');
                        hiddenElement.val(result.id);
                        hiddenElement.trigger('change');
                    }
                });
        });

    $('.autocomplete').find('input.prompt')
        .on('input keyup', function () {
            let hiddenElement = $(this).parent().find('input[type="hidden"]');
            hiddenElement.val(0);
            hiddenElement.trigger('change');
        });

    //Date and DateTime initialize
    $('.dateonly-calendar').not(function () {
        return $(this).has('.ui.disabled').length > 0;
    }).calendar({
        type: 'date',
        monthFirst: false,
        formatter: {
            date: 'DD.MM.YYYY'
        },
        text: calendarTextConfig
    });

    $('.datetime-calendar').not(function () {
        return $(this).has('.ui.disabled').length > 0;
    }).calendar({
        type: 'datetime',
        monthFirst: false,
        formatter: {
            datetime: 'DD.MM.YYYY HH:mm',
            cellTime: 'H:mm'
        },
        text: calendarTextConfig
    });

    //Dropdown initialize
    $('.ui.dropdown').dropdown();
    $('.ui.accordion').accordion();

    $('.pid').find('.ui.dropdown').change(function () {
        generatePIDValue($(this).closest('.pid'));
    });

    $('.pid').find(':input[type=text]').on('input', function () {
        generatePIDValue($(this).closest('.pid'));
    });   
}
function InitForm() {
    $('.ui.dropdown').dropdown();
    $('.ui.accordion').accordion();
    initDynamicForms(function () {
        InitForm();
    })
}
function editButton(url) {
    return `<a href="${url}" data-tooltip="Редакция" class="ui tertiary icon button">
               <i class="edit icon"></i>
            </a>`;
}

function deleteItemWithConfirmationButton(url, reload) {
    return `<a href="javascript:deleteItemWithConfirmation('${url}', ${reload})" data-tooltip="Изтриване" class="ui red tertiary button">
       <i class="trash alternate icon"></i>
   </a>`;
}

function showLoader(selector) {
    $(selector)
        .dimmer({
            displayLoader: true,
            variation: 'inverted',
            loaderVariation: 'slow green double large loader',
            loaderText: 'Моля изчакайте...'
        })
        .dimmer('show');
}

function hideLoader(selector) {
    $(selector).dimmer('hide');
}

function frontEndValidation(checkbox) {
    if (!checkbox.checked) {
        $('#submit').attr("novalidate", true);
    } else {
        $('#submit').removeAttr("novalidate");
    }
}

function jsonBGdate(value) {
    if (!value || moment(value).year() < 1800) {
        return '';
    }
    return moment(value).format("DD.MM.YYYY");
}

function jsonBGdatetime(value) {
    if (!value || moment(value).year() < 1800) {
        return '';
    }
    return moment(value).format("DD.MM.YYYY г. HH:mm:ss");
}
