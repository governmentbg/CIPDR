var jsonFieldsModel = [];
var selectedFieldId = null;
var addAfterId = null;
const totalColumns = 4;
const columnWidthStep = 1;
const fomanticColumnsForUserColumn = 4;
var skipLoadingDefaultsOnTypeChange = false;
var loadedSubFields = [];

$(function () {
    loadConfiguration();

    $('.compact-tooltip').popup({
        boundary: '.ui responsive-tooltip'
    });

    $('#type').change(function () {
        if ($('#type').val() === '') {
            return;
        }

        // Hide all elements that don't match any of the values
        $('[data-specific-for]').each(function () {
            var specificForValue = $(this).data('specific-for').split(',');
            // Check if the specificForValue is in the list of selected values
            if (specificForValue.includes($('#type').val())) {
                $(this).show(); // Show the element if it matches one of the values
            } else {
                $(this).hide(); // Hide it otherwise
            }
        });

        if (skipLoadingDefaultsOnTypeChange) {
            skipLoadingDefaultsOnTypeChange = false;
            return;
        }

        let url = "/Admin/Designer/GetFieldDefaultConfiguration";
        showLoader('body');
        get_async(url,
            {
                type: $('#type').val()
            })
            .then((result) => {
                if (result !== '') {
                    $('#fieldProperties').find('input[id]').each(function () {
                        let fieldName = $(this).attr('id');
                        
                        if (fieldName === 'type') {
                            return;
                        }
                        if (fieldName === 'label' && $('#label').val() !== '') {
                            return;
                        }
                        if (fieldName === 'name' && $('#name').val() !== '') {
                            return;
                        }

                        if ($(this).attr('type') === 'checkbox') {
                            $(this).prop('checked', result[fieldName]);

                        }
                        else {
                            $(this).val(result[fieldName]).trigger('change');
                        }
                    });

                    if ($('#columns').val() < 1) {
                        $('#columns').val(1);
                    }

                    $('#allowedFileExtensions').trigger('change');
                    let textForNomenclatureType = $('#nomenclatureType').parent().find(`.menu .item[data-value="${$('#nomenclatureType').val()}"]`).text();                    
                    $('#nomenclatureType').parent().dropdown('set text', textForNomenclatureType);

                    loadedSubFields = result['fields'];
                    hideLoader('body');
                }
                else {
                    $('#columns').val(1);
                    hideLoader('body');
                }
            })
            .catch((error) => {
                console.error("Проблем при зареждане на конфигурацията по подразбитане" + error);               
                $('#columns').val(1);
                hideLoader('body');
            });
    });

    $('#addField').on('click', function (event) {
        selectedFieldId = null;
        $('.card').removeClass('black').addClass('grey');
        clearInputs();
        showProperties();
    });

    $('#close').on('click', function (event) {
        selectedFieldId = null;
        $('.card').removeClass('black').addClass('grey');
        hideProperties();
    });
    
    $('#ok').on('click', function () {
        createField();
    });
    
    subscribeForSelection();

    $('#previewButton').on('click', function (event) {        
        if (!$('.field-properties').hasClass('hidden') && !createField()) {
            return showToast('warning', 'Проблем при запис на текущото поле. Моля прегледайте панела с настройки.');
        }

        let url = "/Admin/Designer/SaveConfiguration";
        post_async(url, {
            jsonFieldsModel: JSON.stringify(jsonFieldsModel),
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
            formParentId: $('#FormParentId').val(),
            formTitle: $('#FormTitle').val()
        },
        )
            .then((result) => {
                if (result === true) {
                    let formParentId = $('#FormParentId').val();
                    let formTitle = $('#FormTitle').val();
                    let url = `/Admin/Designer/ShowPreview?formParentId=${formParentId}`;
                    window.location.href = url;
                }
                else {
                    showToast('error', 'Проблем при запис на конфигурацията');
                }
            })
            .catch((error) => {
                console.error("Проблем при запис на конфигурацията " + error);
            });
    });

    $('#saveButton').on('click', function (event) {        
        if (!$('.field-properties').hasClass('hidden') && !createField()) {
            return showToast('warning', 'Проблем при запис на текущото поле. Моля прегледайте панела с настройки.');
        }

        let url = "/Admin/Designer/SaveConfiguration";
        post_async(url, {
            jsonFieldsModel: JSON.stringify(jsonFieldsModel),
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
            formParentId: $('#FormParentId').val(),
            formTitle: $('#FormTitle').val()
        },
        )
            .then((result) => {
                if (result === true) {
                    showToast('success', 'Конфигурацията е записана успешно');
                }
                else {
                    showToast('error', 'Проблем при запис на конфигурацията');
                }
            })
            .catch((error) => {
                console.error("Проблем при запис на конфигурацията " + error);
            });
    });

    $('[data-specific-for]').hide();
});

function validateData() {
    let result = true;

    if (!$('#type').val()) {
        result = false;
        $('span[for="type"]').text('Изберете тип');
    }
    else {
        $('span[for="type"]').text('');
    }

    if (!$('#nomenclatureType').val() && ["Select", "MultiSelect", "Autocomplete"].includes($('#type').val())) {
        result = false;
        $('span[for="nomenclatureType"]').text('Изберете вид номенклатура');
    }
    else {
        $('span[for="nomenclatureType"]').text('');
    }

    if (!$('#label').val()) {
        result = false;
        $('span[for="label"]').text('Въведете име');
    }
    else if (jsonFieldsModel.find(obj => obj.label.toUpperCase() === $('#label').val().toUpperCase() && obj.identifier !== selectedFieldId)) {
        result = false;
        $('span[for="label"]').text('Име на поле се повтаря');
    }
    else {
        $('span[for="label"]').text('');
    }

    if ($('#minValue').val() !== '' && $('#maxValue').val() !== '') {
        const minValue = parseFloat($('#minValue').val().replace(',', '.'));
        const maxValue = parseFloat($('#maxValue').val().replace(',', '.'));

        if (!isNaN(minValue) && !isNaN(maxValue) && minValue >= maxValue) {
            result = false;
            $('span[for="minValue"]').text('Стойността не може да бъде по-малка или равна на Макс. допустимата');
        }
        else {
            $('span[for="minValue"]').text('');
        }
    }
    else {
        $('span[for="minValue"]').text('');
    }

    if (!$('#name').val()) {
        result = false;
        $('span[for="name"]').text('Въведете име');
    }
    else if (jsonFieldsModel.find(obj => obj.name.toUpperCase() === $('#name').val().toUpperCase() && obj.identifier !== selectedFieldId)) {
        result = false;
        $('span[for="name"]').text('Име на поле в базата данни се повтаря');
    }
    else {
        const regex = /^[A-Za-z][A-Za-z0-9]*$/;
        if (!regex.test($('#name').val())) {
            result = false;
            $('span[for="name"]').text('Приемат се латински букви и цифри. Започва се с буква');
        }
        else {
            $('span[for="name"]').text('');
        };
    }

    if (!isRegexPatternValid($('#pattern').val())) {
        result = false;
        $('span[for="pattern"]').text('Невалиден Regex израз');
    }
    else {
        $('span[for="pattern"]').text('');
    };

    return result;
}

function createField() {
    if (!validateData()) {
        return false;
    }

    let record;

    if (selectedFieldId === null) {
        record = {
            identifier: uuidv4(),
            columns: columnWidthStep
        };

        setFieldPropertiesToJsonRecord(record);
        record.fields = loadedSubFields;

        if (addAfterId === null) {
            jsonFieldsModel.push(record);
        }
        else {
            let insertIndex = jsonFieldsModel.findIndex(obj => obj.identifier === addAfterId);
            jsonFieldsModel.splice(insertIndex + 1, 0, record);
        }
    }
    else {
        let selectedJsonElement = jsonFieldsModel.find(obj => {
            return obj.identifier === selectedFieldId;
        });

        setFieldPropertiesToJsonRecord(selectedJsonElement);
        selectedJsonElement.fields = loadedSubFields;
    }

    selectedFieldId = null;
    hideProperties();
    clearInputs();
    renderPreview();
    return true;
}


function clearAllFields() {
    jsonFieldsModel = [];
    skipLoadingDefaultsOnTypeChange = false;
    hideProperties();
    renderPreview();
};

function subscribeForSelection() {
    $('.field-preview').on('click', function (event) {
        if (selectedFieldId === $(this).attr('id')) {
            selectedFieldId = null;
            $('.card').removeClass('black').addClass('grey');
            $(this).find(".ui.right.floated.mini.icon.button.remove-field i.icon.window.close").removeClass("inverted");
            hideProperties();
            return;
        }       

        selectedFieldId = $(this).attr('id');

        let selectedJsonElement = getSelectedJsonElement();

        $('#fieldProperties').find('input[id]').each(function () {
            if ($(this).attr('type') === 'checkbox') {
                $(this).prop('checked', selectedJsonElement[$(this).attr('id')]);

            }
            else {
                $(this).val(selectedJsonElement[$(this).attr('id')]);
            }
        });

        loadedSubFields = selectedJsonElement.fields;
        skipLoadingDefaultsOnTypeChange = true;
        $('#type').trigger('change');
        $('#nomenclatureType').trigger('change');

        $('#allowedFileExtensions').val(selectedJsonElement.allowedFileExtensions);
        $('#allowedFileExtensions').trigger('change');

        $('.card').removeClass('black').addClass('grey');
        $(".ui.right.floated.mini.icon.button.remove-field i.icon.window.close").removeClass("inverted");

        $(this).find('.card').removeClass('grey').addClass('black');
        $(this).find(".ui.right.floated.mini.icon.button.remove-field i.icon.window.close").addClass("inverted"); 

        showProperties();
    });
};

function renderPreview() {

    $('#preview-container').empty();
    $('#hidden-fields-wrapper').empty();
    $('#preview-hidden-fields').hide();

    let row = $('<div class="fields"></div>');
    let columnsFilled = 0;

    jsonFieldsModel.forEach(function (item, index) {

        if (item.isHidden) {
            let hiddenField = `
                <div class="ui column field-preview" id="${item.identifier}">                
                <div class="ui label">
                <i class="window close icon remove-field"></i>
                        ${item.label}        
                </div>             
            </div>
            `;

            $('#hidden-fields-wrapper').append(hiddenField);
            $('#preview-hidden-fields').show();

            return;
        };

        if (columnsFilled + item.columns > totalColumns) {
            $('#preview-container').append(row);
            row = $('<div class="fields"></div>');
            columnsFilled = item.columns;
        }
        else {
            columnsFilled = columnsFilled + item.columns;
        }

        if (item.isLastInRow) {
            columnsFilled = totalColumns;
        }

        let moveLeftDisabled = index > 0 ? '' : 'disabled';
        let moveRightDisabled = index + 1 < jsonFieldsModel.length ? '' : 'disabled';
        let growDisabled = item.columns + columnWidthStep <= totalColumns ? '' : 'disabled';
        let shrinkDisabled = item.columns - columnWidthStep > 0 ? '' : 'disabled';

        let newFieldPreview = `
                <div class="${numberToEnglishName(item.columns * fomanticColumnsForUserColumn)} wide field field-preview" id="${item.identifier}">
                <div class="ui basic grey mini card" style="width: 2000px;">
                    <div class="content">
                        <div class="header">${item.label}
                            <button class="ui right floated mini icon button remove-field" data-tooltip="Премахване" data-position="bottom left" data-variation="tiny">
                                <i class="icon window close"></i>
                            </button>
                        </div>
                        <div class="meta">
                            <span class="category">${item.type}</span>
                        </div>                     
                    </div>
                     <div class="extra content field-buttons">
                        <div class="ui wrapped wrapping icon buttons">
                            <button class="ui mini icon button replace-left" data-tooltip="Преместване наляво" data-position="bottom left" data-variation="tiny" ${moveLeftDisabled}>
                                <i class="icon angle left"></i>
                            </button>
                            <button class="ui mini icon button replace-right" data-tooltip="Преместване надясно" data-position="bottom left" data-variation="tiny" ${moveRightDisabled}>
                                <i class="icon angle right"></i>
                            </button>
                            <button class="ui mini icon button shrink" data-tooltip="Намаляване" data-position="bottom left" data-variation="tiny" ${shrinkDisabled}>
                                <i class="compress alternate icon"></i>
                            </button>
                            <button class="ui mini icon button grow" data-tooltip="Увеличаване" data-position="bottom left" data-variation="tiny" ${growDisabled}>
                                <i class="expand alternate icon"></i>
                            </button>
                            <button class="ui mini icon button add-after" data-tooltip="Добави след текущото поле" data-position="bottom left" data-variation="tiny">
                                <i class="icon plus"></i>
                            </button>
                            <button class="ui mini icon button is-last-in-row" data-tooltip="${item.isLastInRow ? "Продължи реда" : "Направи полето последно на реда"}" data-position="bottom left" data-variation="tiny">
                                <i class="${item.isLastInRow ? "level up alternate icon" : "level down alternate icon"}" style="${!item.isLastInRow ? "transform: rotate(90deg);" : ""}"></i>
                            </button> 
                        </div>
                      </div>
                </div>
            </div>
            `;

        row.append(newFieldPreview);
    });

    $('#preview-container').append(row);
    subscribeForSelection();
    subscribeForPreviewButtonEvents();
};

function setFieldPropertiesToJsonRecord(record) {
    $('#fieldProperties').find('input[id]').each(function () {
        if ($(this).attr('type') === 'checkbox') {
            record[$(this).attr('id')] = $(this).is(':checked');
        }
        else {
            record[$(this).attr('id')] = $(this).val();
        }
    });
    record.allowedFileExtensions = $('#allowedFileExtensions').val();
    record.columns = parseInt(record.columns, 10);    
};

function showProperties() {
    if ($('.field-properties').hasClass('hidden')) {
        $('.field-properties')
            .transition('fade left');
    };
};

function hideProperties() {
    if (!$('.field-properties').hasClass('hidden')) {
        $('.field-properties')
            .transition('fade left');
    };
    clearInputs();
};

function numberToEnglishName(num) {
    const names = [
        "one", "two", "three", "four", "five",
        "six", "seven", "eight", "nine", "ten",
        "eleven", "twelve", "thirteen", "fourteen", "fifteen",
        "sixteen"
    ];

    if (num >= 1 && num <= totalColumns * fomanticColumnsForUserColumn) {
        return names[num - 1];
    } else {
        return "Invalid number";
    }
}

function getSelectedJsonElement(selectedElement = null) {
    return jsonFieldsModel.find(obj => {
        return obj.identifier === (selectedElement === null ? selectedFieldId : selectedElement.attr('id'));
    });
}

function subscribeForPreviewButtonEvents() {
    $('.grow').on('click', function (event) {
        event.stopPropagation();
        let jsonObject = getSelectedJsonElement($(this).closest('.field-preview'));
        if (jsonObject.columns + columnWidthStep <= totalColumns) {
            jsonObject.columns = jsonObject.columns + columnWidthStep;
            renderPreview();
        }
    });

    $('.shrink').on('click', function (event) {
        event.stopPropagation();
        let jsonObject = getSelectedJsonElement($(this).closest('.field-preview'));
        if (jsonObject.columns - columnWidthStep > 0) {
            jsonObject.columns = jsonObject.columns - columnWidthStep;
            renderPreview();
        }
    });

    $('.replace-left').on('click', function (event) {
        event.stopPropagation();
        let index = jsonFieldsModel.findIndex(obj => obj.identifier === ($(this).closest('.field-preview').attr('id')));

        if (index > 0) {
            let prev = jsonFieldsModel[index - 1];
            jsonFieldsModel[index - 1] = jsonFieldsModel[index];
            jsonFieldsModel[index] = prev;
            renderPreview();
        }
    });

    $('.replace-right').on('click', function (event) {
        event.stopPropagation();
        let index = jsonFieldsModel.findIndex(obj => obj.identifier === ($(this).closest('.field-preview').attr('id')));

        if (index + 1 < jsonFieldsModel.length) {
            let next = jsonFieldsModel[index + 1];
            jsonFieldsModel[index + 1] = jsonFieldsModel[index];
            jsonFieldsModel[index] = next;
            renderPreview();
        }
    });

    $('.remove-field').on('click', function (event) {
        event.stopPropagation();
        let index = jsonFieldsModel.findIndex(obj => obj.identifier === ($(this).closest('.field-preview').attr('id')));
        let fieldName = jsonFieldsModel[index].label;
        $('#confirmModal')
            .modal({
                title: '<i class="archive icon"></i>Премахване на поле',
                content: `Сигурни ли сте, че искате да премахнете полето \"${fieldName}\" ?`
            })
            .modal({
                onApprove: function () {
                    jsonFieldsModel.splice(index, 1);
                    hideProperties();
                    renderPreview();
                }
            })
            .modal('show');
    });

    $('#clearAllButton').on('click', function (event) {
        $('#confirmModal')
            .modal({
                title: '<i class="trash icon"></i>Премахване на всички полета',
                content: 'Сигурни ли сте, че искате да изтриете всички полета?'
            })
            .modal({
                onApprove: function () {
                    clearAllFields();
                }
            })
            .modal('show');
    });

    $('#loadConfigurationButton').on('click', function (event) {
        $('#confirmModal')
            .modal({
                title: '<i class="redo icon"></i>Зареждане на последно записаната конфигурация',
                content: 'Сигурни ли сте, че искате да заредите последно записаната конфигурация?'
            })
            .modal({
                onApprove: function () {
                    loadConfiguration();
                }
            })
            .modal('show');
    });

    $('.add-after').on('click', function (event) {
        event.stopPropagation();
        clearInputs();
        addAfterId = $(this).closest('.field-preview').attr('id');
        selectedFieldId = null;
        $('.card').removeClass('black').addClass('grey');
        showProperties();
    });

    $('.is-last-in-row').on('click', function (event) {       
        event.stopPropagation();
        let index = jsonFieldsModel.findIndex(obj => obj.identifier === ($(this).closest('.field-preview').attr('id')));
        jsonFieldsModel[index].isLastInRow = !jsonFieldsModel[index].isLastInRow;       
        renderPreview();       
    });
}

function clearInputs() {
    $('#fieldProperties').find('input[id]').val('');
    $('#fieldProperties').find('input[type="checkbox"]').prop('checked', false);
    $('#fieldProperties').find('.ui.dropdown').dropdown('clear');

    $('#allowPastDates').prop('checked', true);
    $('#allowFutureDates').prop('checked', true);

    loadedSubFields = [];
    addAfterId = null;
}

function loadConfiguration() {
    let url = "/Admin/Designer/LoadConfiguration";
    showLoader('body');
    get_async(url,
        {
            formParentId: $('#FormParentId').val()
        })
        .then((result) => {
            if (result !== '') {
                jsonFieldsModel = result;
                hideLoader('body');
                renderPreview();
            }
            else {
                hideLoader('body');
                showToast('error', 'Проблем при зареждане на конфигурацията');
            }
        })
        .catch((error) => {
            hideLoader('body');
            showToast('error', 'Проблем при зареждане на конфигурацията');
            console.error("Проблем при зареждане на конфигурацията " + error);
        });
}

$('#loadConfigurationButton')
    .popup({
        position: 'bottom center',
        content: 'Зареждане на последно записана конфигурация от базата данни',
    });

$('#previewButton')
    .popup({
        position: 'bottom center',
        content: 'Запис и визуализиране на изглед на формата',
    });

$('#saveButton')
    .popup({
        position: 'bottom center',
        content: 'Запис на конфигурацията',
    });

$('#clearAllButton')
    .popup({
        position: 'bottom center',
        content: 'Изтриване на текущо създадената конфигурация',
    });

$('#addField')
    .popup({
        position: 'bottom center',
        content: 'Отваря прозорец с настройки за добавяне на поле',
    });

$('#backToFormIndex')
    .popup({
        position: 'bottom center',
        content: 'Върнете се назад към списък на формите в регистъра ',
    });

function isRegexPatternValid(pattern) {
    try {
        new RegExp(pattern);
        return true;
    } catch (e) {
        return false;
    }
}