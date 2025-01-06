const noResultsFoundMessage = 'Няма намерени резултати!';
$(function () {
    initializeElements($(document));
    //Tooltip-a при чекбокс да не излиза от очертанията на елемента
    $('.checkbox-tooltip').popup({
        boundary: '.ui checkbox'
    })

    $('.person-tooltip').popup({
        boundary: '.person-fieldset'
    })

    $('.company-tooltip').popup({
        boundary: '.company-fieldset'
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

function frontEndValidation(checkbox) {
    if (!checkbox.checked) {
        $('#submit').attr("novalidate", true);
    } else {
        $('#submit').removeAttr("novalidate");
    }
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

// спира auto-fill в Chrome
$('input').attr('autocomplete', 'one-time-code');

//преведено съобщение за не намерени резултати при autocomplete елементите
$.fn.search.settings.templates.message = function () {
    return '<div class="message"><div class="header">' + noResultsFoundMessage + '</div><div class="description">Пробвайте с друг текст!</div></div>';
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


//#region За FileUpload
$('.upload-file-input').change(function () {
    let textForLabel;
    var selectedFiles = $(this).prop("files");
    if (selectedFiles.length === 0) {
        textForLabel = "Изберете файл";
        $(this).parent().find('.remove-file').parent().hide();
    }
    else {
        var filenames = [];

        if (selectedFiles.length > 0) {
            for (var i = 0; i < selectedFiles.length; i++) {
                filenames.push(selectedFiles[i].name);
            }
            textForLabel = filenames.join("; ");
        }

        let label = $(this).parent().find('.selected-file');
        label.text(textForLabel);
        label.attr('title', textForLabel.replace(/; /g, "\n"));
        $(this).parent().find('.remove-file').parent().show();
    }
});

//#endregion

$('.remove-file').click(function () {

    $(this).closest('.action').find('.upload-file-input').val('');    
    $('.selected-file').text('');
    $('.selected-file').removeAttr('title');
});

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
                        url: '/Admin/Nomenclature/GetEkatteValues?query={query}',
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

    //Autocomplete initialize
    $('.autocomplete')
        .each(function () {
            $(this).search(
                {
                    apiSettings: {
                        url: '/Admin/Nomenclature/GetAutocompleteValues?query={query}&nomenclatureType=' + $(this).data('nomenclatureType'),
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

    $('.pid').find('.ui.dropdown').change(function () {
        generatePIDValue($(this).closest('.pid'));
    });

    $('.pid').find(':input[type=text]').on('input', function () {
        generatePIDValue($(this).closest('.pid'));
    });   
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

function actionWithConfirmation(actionUrl, id, confirmDeleteText = "Сигурни ли сте, че искате да изтриете елемента?", callback = null) {
    $('#confirmActionText').text(confirmDeleteText);
    $('.confirm-action')
        .modal({
            centered: true,
            closable: false,
            onApprove: function () {
                let url = actionUrl;
                post_async(url, {
                    id: id,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                )
                    .then((result) => {
                        if (callback !== null) {
                            callback();
                        }
                        else if (result === null) {
                            window.location.reload();
                        }
                        else if (result.redirectUrl) {
                            if (result.redirectUrl !== '#') {
                                window.location.href = result.redirectUrl;
                            }
                        }
                        else {
                            window.location.reload();
                        }
                    })
                    .catch((error) => {
                        console.error('Грешка при URL ' + actionUrl + " : " + error.statusText);
                    });
            }
        })
        .modal('show');
};

function editButton(url) {
    return `<a href="${url}" data-tooltip="Редакция" class="ui tertiary icon button">
               <i class="edit icon"></i>
            </a>`;
}

function InitForm() {
    $('.ui.dropdown').dropdown();
    $('.ui.accordion').accordion();
    initDynamicForms(function () {
        InitForm();
    })
}