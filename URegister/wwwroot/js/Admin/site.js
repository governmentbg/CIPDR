const noResultsFoundMessage = 'Няма намерени резултати!';
$(function () {
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

    $(document).on('click', '.single-click-submit', function (e) {
        singleClickSubmitDisable(this);
        e.preventDefault();
        showLoader('body');
        //if (!$(this).parents('form:first').valid()) {
        //    singleClickSubmitEnable();
        //}
        return false;
    });
    InitForm();
});

function singleClickSubmitDisable(sender) {
    var disabled = $(sender).is(':disabled') || $(sender).attr('disabled') || $(sender).attr('data-clicked');

    if (!disabled) {
        $(sender).attr('disabled', 'disabled');
        $(sender).attr('data-clicked', 'clicked');
        $('#UserTimeZoneOffsetInMinutes').val(new Date().getTimezoneOffset());
        $(sender).parents('form:first').trigger('submit');
    }
}
function singleClickSubmitEnable() {
    $('.single-click-submit').removeAttr("disabled");
    $('.single-click-submit').removeAttr("data-clicked");
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
function initializeElements(parentForm) {    
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

function showLoader(selector) {
    $(selector)
        .dimmer({
            displayLoader: true,
            variation: 'inverted',
            loaderVariation: 'slow green double large loader',
            loaderText: 'Моля изчакайте...',
            closable: false
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
                        if (error.status === 401) {
                            window.location.reload();
                        }
                        console.error('Грешка при URL ' + actionUrl + " : " + error.statusText);
                    });
            }
        })
        .modal('show');
};

function fileActionWithConfirmation(actionUrl, data, confirmDeleteText = "Сигурни ли сте, че искате да изтриете елемента?", callback = null) {
    $('#confirmActionText').text(confirmDeleteText);
    $('.confirm-action')
        .modal({
            centered: true,
            closable: false,
            onApprove: function () {
                let url = actionUrl;
                post_async(url, data)
                    .then((result) => {                       
                        if (result.success) {
                            if (callback !== null) {
                                callback();
                            }
                            showToast('success', 'Файлът е премахнат успешно.');                           
                        }
                        else {
                            showToast('error', 'Проблем при премахване на файла.');
                            console.error(result.error);
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
    SetAttachedFiles();
    initDynamicForms(function () {
        InitForm();
    })
}

function SetAttachedFiles() {
    $('.attached-file-input').off('change');
    $('.attached-file-input').change(async function () {
        var selectedFiles = $(this).prop("files");
        const parent = $(this).parents('.fields:first');
        if (selectedFiles.length > 0) {
            parent.find('.attached-file-button').show();
            parent.find('.attached-file').text(selectedFiles[0].name);
            const response = await uploadAttachedFile('/Admin/Process/UploadAttachedFile', selectedFiles[0]);
            parent.find('.attached-file-id').val(response.metaFileId)
        } else {
            parent.find('.attached-file-button').hide();
            parent.find('.attached-file').text('');
        }
    });
}

async function downloadAttachedFile(btn) {
    const parent = $(btn).parents('.fields:first');
    const file_id = parent.find('.attached-file-id').val();
    var response = await post_fetch_json_async(`/Admin/Process/GetAttachedFileUrl?id=${file_id}`, {});
    await downloadPresignedFile(response.fileUrl, response.fileName)
}

async function downloadAttachedFileById(file_id) {
    var response = await post_fetch_json_async(`/Admin/Process/GetAttachedFileUrl?id=${file_id}`, {});
    await downloadPresignedFile(response.fileUrl, response.fileName)
}

async function uploadAttachedFile(url, file) {
    showLoader('body');
    var data = new FormData()
    data.append('file', file)
    const response = await fetch(url,
        {
            method: "POST",
            body: data,
            headers: {
                "X-CSRF-TOKEN": getRequestVerificationToken()
            }
        });
    hideLoader('body');
    return await ResolveResponseJson(response);
}

