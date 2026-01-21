const noResultsFoundMessage = 'Няма намерени резултати!';

// спира auto-fill в Chrome
$('input').attr('autocomplete', 'one-time-code');

$(function () {
    //преведено съобщение за не намерени резултати при autocomplete елементите
    $.fn.search.settings.templates.message = function () {
        return '<div class="message"><div class="header">' + noResultsFoundMessage + '</div><div class="description">Пробвайте с друг текст!</div></div>';
    }

    initializeElements($(document));

    //Horizontal scroll for datatables
    $(document).on('init.dt', function (e, settings) {
        var $table = $(settings.nTable);

        // Check for specific classes in the table (ignoring dynamic classes like 'dataTable')
        if ($table.hasClass('ui') && $table.hasClass('celled') && $table.hasClass('table')) {
            // Apply overflow-x: auto to the nearest parent with .ui.padded.grid.row
            $table.closest('.ui.padded.grid.row').css('overflow-x', 'auto');
        }
    });

    InitForm();        
});

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

    //Date, DateTime and Time initialize
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

    $('.timeonly-calendar').not(function () {
        return $(this).has('.ui.disabled').length > 0;
    }).calendar({
            type: 'time',
            formatter: {
                time: 'HH:mm',
                cellTime: 'HH:mm'
            }
        });

    //#region Dropdown initialize

    $('.ui.dropdown').dropdown();
    $('.ui.accordion').accordion();

    //#endregion
    
    preventFormSubmissionOnEnter();

    $('input:not([type="hidden"]), textarea').each(function () {
        $(this).attr('oninvalid', 'setCustomValidity(getErrorMessage(this))');
        $(this).attr('oninput', 'setCustomValidity("")');

        if ($(this).closest('.datetime-calendar').length || $(this).closest('.dateonly-calendar').length) {
            $(this).attr('onchange', 'this.setCustomValidity("")');
        }
    });        
}

function preventFormSubmissionOnEnter() {
    $('input').on('keydown', function (event) {
        if (event.key === "Enter") {
            event.preventDefault();
        }
    });
}

function SetRegisterFiles() {
    $('.upload-file-input').off('change');
    $('.upload-file-input').change(async function () {
        var selectedFiles = $(this).prop("files");
        const parent = $(this).parents('.fields:first');
        if (selectedFiles.length > 0) {
            parent.find('.selected-file-button').show();
            parent.find('.selected-file').text(selectedFiles[0].name);
            const response = await uploadRegisterFile('/Register/UploadFile', selectedFiles[0]);
            parent.find('.upload-file-id').val(response.metaFileId)
        } else {
            parent.find('.selected-file-button').hide();
            parent.find('.selected-file').text('');
        }
    });
}

async function downloadRegisterFile(btn) {
    const parent = $(btn).parents('.fields:first');
    const file_id = parent.find('.upload-file-id').val();
    await downloadFile(`/Register/DownloadFile?id=${file_id}`, {})
}
async function downloadRegisterFileById(file_id) {
    await downloadFile(`/Register/DownloadFile?id=${file_id}`, {})
}

function InitForm() {
    $('.ui.dropdown').dropdown();
    $('.ui.accordion').accordion();
    SetRegisterFiles();
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

function fileActionWithConfirmation(confirmDeleteText = "Сигурни ли сте, че искате да изтриете елемента?", callback = null) {
    $('#confirmActionText').text(confirmDeleteText);
    $('.confirm-action')
        .modal({
            centered: true,
            closable: false,
            onApprove: function () {                
                if (callback !== null)
                {
                    callback();
                }
                showToast('success', 'Файлът е премахнат успешно.');
            }
        })
        .modal('show');
};