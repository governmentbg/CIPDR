var configuredFieldModel = { fields: [] };
var selectedFieldId = null;
var addAfterId = null;
const totalColumns = 4;
const columnWidthStep = 1;
const fomanticColumnsForUserColumn = 4;
var skipLoadingDefaultsOnTypeChange = false;
var isSubfieldEdited = false;

$(function () {

    $('#columns').on('keydown', event => {
        console.log(event.keyCode);
        const codes = [
            190, // .
            110, // . from numpad
            188, // ,
            69  // e (scientific notaion, 1e2 === 100)
        ]
        if (codes.includes(event.keyCode)) {
            event.preventDefault();
            return false;
        }
        return true;
    });

    $('#configuredType').change(function () {        
        selectedFieldId = null;

        if (isConfiguredFieldComplex()) {
            $("#addField").show();
        }
        else {
            $("#addField").hide();
        }

        clearInputs();
                
        hideSpecificFields($('#configuredType').val())

        $('#componentFieldTypeContainer').closest('.item').hide();

        if (skipLoadingDefaultsOnTypeChange) {
            skipLoadingDefaultsOnTypeChange = false;
            return;
        }

        clearAllFields();
        
        let url = "/Designer/GetFieldDefaultConfiguration";
        showLoader('body');
        get_async(url,
            {
                type: $('#configuredType').val()
            })
            .then((result) => {
                if (result !== '') {
                    $('#fieldProperties').find('input[id]').each(function () {
                        let fieldName = $(this).attr('id');
                        
                        if (fieldName === 'type') {
                            return;
                        }
                        if (fieldName === 'configuredType') {
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

                    configuredFieldModel = result;       
                    renderPreview();
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

    $('#type').change(function () {
        hideSpecificFields($('#type').val());
    });

    $('#addField').on('click', function (event) {
        selectedFieldId = null;
        $('.card').removeClass('black').addClass('grey');
        toSubfieldMode();
        clearInputs();        
    });    

    $('#close').on('click', function (event) {
        skipLoadingDefaultsOnTypeChange = true;
        toComplexFieldModel();
    });
           
    $('#ok').on('click', function () {
        if (isConfiguredFieldComplex()) {
            if ($("#type").val() === '') {
                saveField();
            }
            else {
                createField();
            }
        }
        else {
            saveField();
        }
    });
    
    subscribeForSelection();

    $('#loadConfigurationButton').on('click', function (event) {
        skipLoadingDefaultsOnTypeChange = false;
        toComplexFieldModel();
        $('#configuredType').trigger('change');
    });

    $('#previewButton').on('click', function (event) {
        if (isSubfieldEdited) {
            return showToast('warning', 'Вмомента сте в режим на редакция на подполе. Моля затворете панела с настройки.');
        }
        if (!validateData()) {
            return showToast('warning', 'Проблем при запис на текущото поле. Моля прегледайте панела с настройки.');
        }
        setFieldPropertiesToJsonRecord(configuredFieldModel, true);
        let url = "/Designer/SaveDefaults";
        post_async(url, {            
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
            jsonFieldDefaults: JSON.stringify(configuredFieldModel),
        },
        )
            .then((result) => {
                if (result === true) {
                    let fieldType = $('#configuredType').val();
                    let url = `/Designer/ShowPreview?fieldType=${fieldType}`;
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
  
    $('[data-specific-for]').hide();

    if ($('#SelectedType').val() !== '') {
        $('#configuredType').val($('#SelectedType').val()).trigger('change');
    }
});

function hideSpecificFields(selectedType) {
    // Hide all elements that don't match any of the values
    $('[data-specific-for]').each(function () {
        var specificForValue = $(this).data('specific-for').split(',');
        // Check if the specificForValue is in the list of selected values
        if (specificForValue.includes(selectedType)) {
            $(this).show(); // Show the element if it matches one of the values
        } else {
            $(this).hide(); // Hide it otherwise
        }
    });
}

function validateData() {
    let result = true;

    if (!$('#configuredType').val()) {
        result = false;
        $('span[for="configuredType"]').text('Изберете тип');
    }
    else {
        $('span[for="configuredType"]').text('');
    }

    if (!$('#type').val() && isSubfieldEdited) {
        result = false;
        $('span[for="type"]').text('Изберете тип');
    }
    else {
        $('span[for="type"]').text('');
    }

    if (!$('#nomenclatureType').val() && isSubfieldEdited && ["Select", "MultiSelect", "Autocomplete"].includes($('#type').val())) {
        result = false;
        $('span[for="nomenclatureType"]').text('Изберете вид номенклатура');
    }
    else {
        $('span[for="nomenclatureType"]').text('');
    }

    if (!$('#label').val()) {
        if (isSubfieldEdited) {
            result = false;
            $('span[for="label"]').text('Въведете име');
        }
    }
    else if ($('#label').val() && configuredFieldModel.fields.find(obj => obj.label.toUpperCase() === $('#label').val().toUpperCase() && obj.identifier !== selectedFieldId)) {
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
        if (isSubfieldEdited) {
            result = false;
            $('span[for="name"]').text('Въведете име');
        }
    }
    else if (configuredFieldModel.fields.find(obj => obj.name.toUpperCase() === $('#name').val().toUpperCase() && obj.identifier !== selectedFieldId)) {
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

    if (!$('#columns').val()) {
        result = false;
        $('span[for="columns"]').text('Въведете брой колони');
    } else if (+$('#columns').val() < 1 || +$('#columns').val() > 4) {
        result = false;
        $('span[for="columns"]').text('Въведете брой колони между 1 и 4 включително');
    }
    else {
        $('span[for="columns"]').text('');
    }

    return result;
}


function saveField() {
    if (!validateData()) {
        return false;
    }

    setFieldPropertiesToJsonRecord(configuredFieldModel, true);

    let url = "/Designer/SaveDefaults";
    post_async(url, {
        jsonFieldDefaults: JSON.stringify(configuredFieldModel),
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),        
    },
    )
        .then((result) => {
            if (result === true) {
                showToast('success', 'Успешен запис на конфигурацията');
            }
            else {
                showToast('error', 'Проблем при запис на конфигурацията');
            }
        })
        .catch((error) => {
            console.error("Проблем при запис на конфигурацията " + error);
        });

    selectedFieldId = null;
    renderPreview();
    return true;
}


function createField() {
    if (!validateData()) {
        return false;
    }

    let record = {};

    if (selectedFieldId === null) {
        record = {
            identifier: uuidv4(),
            columns: columnWidthStep
        };

        setFieldPropertiesToJsonRecord(record);

        if (addAfterId === null) {
            configuredFieldModel.fields.push(record);
        }
        else {
            let insertIndex = configuredFieldModel.fields.findIndex(obj => obj.id === addAfterId);
            configuredFieldModel.fields.splice(insertIndex + 1, 0, record);
        }
    }
    else {
        let selectedJsonElement = configuredFieldModel.fields.find(obj => {
            return obj.identifier === selectedFieldId;
        });

        setFieldPropertiesToJsonRecord(selectedJsonElement);
    }

    clearInputs();
    renderPreview();
    skipLoadingDefaultsOnTypeChange = true;
    toComplexFieldModel();
    return true;
}

function toSubfieldMode() {
    $('#type').closest('.item').show();
    $('#ok').html("Ок");
    $('#addField').hide();
    $('#configuredType').closest('.dropdown').addClass('disabled');
    $('#canBeRepeated').closest('.item').hide();
    $('#canBeRepeated').prop('checked', false);
    $('#close').show();    
    if (!isSubfieldEdited)
    {
        setFieldPropertiesToJsonRecord(configuredFieldModel)
    }
    isSubfieldEdited = true;
}

function toComplexFieldModel() {
    $('#ok').html('Запази');
    selectedFieldId = null;
    isSubfieldEdited = false;
    $('.card').removeClass('black').addClass('grey');
    $('#configuredType').closest('.dropdown').removeClass('disabled');
    $('#configuredType').trigger('change');
    $('#canBeRepeated').closest('.item').show();
    $('#close').hide();
    $('span[for]').text('');
    $('#fieldProperties').find('input[id]').each(function () {
        let fieldName = $(this).attr('id');

        if (fieldName === 'type') {
            return;
        }
        if (fieldName === 'configuredType') {
            return;
        }
        if (fieldName === 'label' && $('#label').val() !== '') {
            return;
        }
        if (fieldName === 'name' && $('#name').val() !== '') {
            return;
        }
        if ($(this).attr('type') === 'checkbox') {
            $(this).prop('checked', configuredFieldModel[fieldName]);

        }
        else {
            $(this).val(configuredFieldModel[fieldName]).trigger('change');
        }
    });
    
        $('#name').prop('disabled', false);   
}

function clearAllFields() {
    skipLoadingDefaultsOnTypeChange = false;
    configuredFieldModel = { fields: []};
    renderPreview();
};

function subscribeForSelection() {
    $('.field-preview').on('click', function (event) {
        if (selectedFieldId === $(this).attr('id')) {
            selectedFieldId = null;
            $('.card').removeClass('black').addClass('grey');
            $(this).find(".ui.right.floated.mini.icon.button.remove-field i.icon.window.close").removeClass("inverted");
            toComplexFieldModel();
            $('#type').closest('.item').hide();
            $('#ok').html("Запази");
            $('#addField').show();
            $('#configuredType').closest('.dropdown').removeClass('disabled');
            $('#canBeRepeated').closest('.item').show();
            isSubfieldEdited = false;
            return;
        }

        toSubfieldMode();
        clearInputs();
        selectedFieldId = $(this).attr('id');

        let selectedJsonElement = getSelectedJsonElement();

        $('#fieldProperties').find('input[id]').not('#configuredType').each(function () {
            if ($(this).attr('type') === 'checkbox') {
                $(this).prop('checked', selectedJsonElement[$(this).attr('id')]);

            }
            else {
                $(this).val(selectedJsonElement[$(this).attr('id')]);
            }
        });
        skipLoadingDefaultsOnTypeChange = true;
        $('#type').trigger('change');
        $('#nomenclatureType').trigger('change');

        $('#allowedFileExtensions').val(selectedJsonElement.allowedFileExtensions);
        $('#allowedFileExtensions').trigger('change');

        $('.card').removeClass('black').addClass('grey');
        $(".ui.right.floated.mini.icon.button.remove-field i.icon.window.close").removeClass("inverted");

        $(this).find('.card').removeClass('grey').addClass('black');
        $(this).find(".ui.right.floated.mini.icon.button.remove-field i.icon.window.close").addClass("inverted");
        
        if ($('#name').val().toLowerCase().includes('immutable')) {
            $('#name').prop('disabled', true);
        } else {
            $('#name').prop('disabled', false);
        }
    });
};
function renderPreview() {

    $('#preview-container').empty();
    $('#hidden-fields-wrapper').empty();
    $('#preview-hidden-fields').hide();

    let row = $('<div class="fields"></div>');
    let columnsFilled = 0;

    configuredFieldModel.fields.forEach(function (item, index) {

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
        let moveRightDisabled = index + 1 < configuredFieldModel.fields.length ? '' : 'disabled';
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


function setFieldPropertiesToJsonRecord(record, setConfiguredTypeAsType) {
    $('#fieldProperties').find('input[id]').each(function () {
        if ($(this).attr('id') === 'configuredType') {
            return;
        }
        if ($(this).attr('type') === 'checkbox') {
            record[$(this).attr('id')] = $(this).is(':checked');
        }
        else {
            record[$(this).attr('id')] = $(this).val();
        }
    });
    record.allowedFileExtensions = $('#allowedFileExtensions').val();
    record.columns = parseInt(record.columns, 10);
    record.columns = isNaN(record.columns) ? 1 : record.columns;

    if (setConfiguredTypeAsType) {
        record.type = $('#configuredType').val();
    }
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
    return configuredFieldModel.fields.find(obj => {
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
        let index = configuredFieldModel.fields.findIndex(obj => obj.identifier === ($(this).closest('.field-preview').attr('id')));

        if (index > 0) {
            let prev = configuredFieldModel.fields[index - 1];
            configuredFieldModel.fields[index - 1] = configuredFieldModel.fields[index];
            configuredFieldModel.fields[index] = prev;
            renderPreview();
        }
    });

    $('.replace-right').on('click', function (event) {
        event.stopPropagation();
        let index = configuredFieldModel.fields.findIndex(obj => obj.identifier === ($(this).closest('.field-preview').attr('id')));

        if (index + 1 < configuredFieldModel.fields.length) {
            let next = configuredFieldModel.fields[index + 1];
            configuredFieldModel.fields[index + 1] = configuredFieldModel.fields[index];
            configuredFieldModel.fields[index] = next;
            renderPreview();
        }
    });

    $('.remove-field').on('click', function (event) {
        event.stopPropagation();
        let index = configuredFieldModel.fields.findIndex(obj => obj.identifier === ($(this).closest('.field-preview').attr('id')));
        let fieldName = configuredFieldModel.fields[index].label;
        $('#confirmModal')
            .modal({
                title: '<i class="archive icon"></i>Премахване на поле',
                content: `Сигурни ли сте, че искате да премахнете полето \"${fieldName}\" ?`
            })
            .modal({
                onApprove: function () {
                    configuredFieldModel.fields.splice(index, 1);
                    skipLoadingDefaultsOnTypeChange = true;
                    toComplexFieldModel();
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

    //$('.add-after').on('click', function (event) {
    //    event.stopPropagation();
    //    clearInputs();
    //    addAfterId = $(this).closest('.field-preview').attr('id');        
    //});

    $('.add-after').on('click', function (event) {
        event.stopPropagation();
        clearInputs();
        addAfterId = $(this).closest('.field-preview').attr('id');
        selectedFieldId = null;
        $('.card').removeClass('black').addClass('grey');        
    });

    $('.is-last-in-row').on('click', function (event) {
        event.stopPropagation();
        let index = configuredFieldModel.fields.findIndex(obj => obj.identifier === ($(this).closest('.field-preview').attr('id')));
        configuredFieldModel.fields[index].isLastInRow = !configuredFieldModel.fields[index].isLastInRow;      
        renderPreview();
    });
}

function clearInputs() {
    $('#fieldProperties').find('input[id]').not('#configuredType').val('').trigger('change');
    $('#fieldProperties').find('input[type="checkbox"]').prop('checked', false);
    
    $('#allowPastDates').prop('checked', true);
    $('#allowFutureDates').prop('checked', true);
    $('#columns').val(1);
    $('#type').parent().dropdown('clear');
    addAfterId = null;
}

$('#previewButton')
    .popup({
        position: 'bottom center',
        content: 'Запис и преглед на текущо поле',
    });

$('#loadConfigurationButton')
    .popup({
        position: 'bottom center',
        content: 'Презареждане на конфигурация на текущо поле',
    });

$('#saveButton')
    .popup({
        position: 'bottom center',
        content: 'Запис на конфигурацията',
    });

$('#clearAllButton')
    .popup({
        position: 'bottom center',
        content: 'Изтриване на конфигурация на текущо поле',
    });

$('#addField')
    .popup({
        position: 'bottom center',
        content: 'Добавете подполе към конфигурираният тип поле',
    });

$('#backToFieldTypeList')
    .popup({
        position: 'bottom center',
        content: 'Върнете се назад към списък с типове полета',
    });

function isConfiguredFieldComplex() {
    let configuredType = $('#configuredType').val();
    if (configuredType === '')
        return false;

    let result = true;

    $('#type').parent().find('.item').each(function (index) {
        if (configuredType === $(this).data('value')) {
            result = false;
            return;
        }
    });

    return result;
}
function isRegexPatternValid(pattern) {
    try {
        new RegExp(pattern);
        return true;
    } catch (e) {
        return false;
    }
}