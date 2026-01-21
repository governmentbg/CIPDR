$(function () {

    if ($("#printButton").length === 1) {
        $("#printButton").on('click', function () {
            printSpecificArea();
        });
    }    
    
    try {
        conditionData = JSON.parse($('#ConditionTree').val());
    } catch (error) {
        console.error('Error parsing ConditionTree JSON:', error);
        conditionData = {}; // Fallback to empty object to prevent errors
    }
       
    preventTooltipOutsideOfBoundaries();
    makeComplexReadonlyFieldSubfieldsReadonly();
    initializeFormFields();
    addTimeZoneAtSubmit();
    applyFormFieldConditions();

    $('.repeat-field').on('click', function (event) {
        let parentForm = $(this).siblings('.ui.form').parent();
        let fieldToClone = $(this).siblings('.ui.form').first();

        // Destroy calendar instances on the first field
        fieldToClone.find('.dateonly-calendar, .datetime-calendar').calendar('destroy');
        
        let clonedField = fieldToClone.clone(false); // Clone without data and events
        clonedField.find("span.validation-error").remove();    

        // Remove disabled class and readonly attribute only if .load-person-info or .load-company-info exists
        if (clonedField.find('.load-person-info, .load-company-info').length > 0) {
            clonedField.find('.ui.input.disabled').removeClass('disabled');
            clonedField.find('input[readonly], textarea[readonly]').removeAttr('readonly');
            clonedField.find('.ui.dropdown.disabled').removeClass('disabled');
        }

        AddTrashIconToClone(clonedField);        

        let newId = uuidv4();

        // Track the highest suffix for the base name, initialized as 0
        let nameSuffix = 0;

        // Check if the previous clone exists and extract the last suffix
        parentForm.find('.field').each(function () {
            $(this).find('[name]').each(function () {
                let nameAttr = $(this).attr('name');
                let currentSuffix = parseInt(nameAttr.match(/#(\d+)/)?.[1] || 0);
                if (currentSuffix > nameSuffix) {
                    nameSuffix = currentSuffix;
                }
            });
        });

        nameSuffix += 1;

        clonedField.find('[id], [name], [for]').each(function () {
            // Update the element's id with the new unique ID
            if ($(this).attr('id')) {
                $(this).attr('id', newId);
            }

            if ($(this).attr('for')) {
                $(this).attr('for', newId);
            }

            // Update the `name` attribute with the incremented suffix
            if ($(this).attr('name')) {
                let originalName = $(this).attr('name');
                let nameBase;

                // Check if there's an underscore in the name
                let underscoreIndex = originalName.indexOf('_');
                if (underscoreIndex !== -1) {
                    // Insert `#index` after the first underscore
                    nameBase = originalName.slice(0, underscoreIndex) +
                        `#${nameSuffix}` +
                        originalName.slice(underscoreIndex);
                } else {
                    // No underscore, add `#index` at the end
                    nameBase = `${originalName}#${nameSuffix}`;
                }

                $(this).attr('name', nameBase);
            }

        });

        // Clear file upload title in the cloned field if needed
        clonedField.find('.selected-file').text('');
        clonedField.find('.selected-file').removeAttr('title');

        // Clear input values in the cloned field if needed
        clonedField.find('input').val('');
        clonedField.find('.ui.dropdown').dropdown('clear');

        if (clonedField.find('.pid-text').length === 1) {
            clonedField.find('.pid-text').closest('.dropdown.label').dropdown('set text', 'Тип');
        }

        if (clonedField.find('.cid-text').length === 1) {
            clonedField.find('.cid-text').closest('.dropdown.label').dropdown('set text', 'Тип');
        }

        // Insert the cloned field right after the last field
        //parentForm.append(clonedField);
        $(this).before(clonedField);

        initializeElements(parentForm);
        initializeFormFields();
        clonedField.find('.checkbox-template').trigger("change");
        subscribeAfterCloning();        
    });

    $('.repeat-field').siblings('.ui.form')
        .filter(function () {
            return $(this).find('[name*="#"]').length > 0;
        })
        .each(function () {
            AddTrashIconToClone($(this));
            return
    });

    subscribeAfterCloning();

    function subscribeAfterCloning() {
        $("[name*='birthCountryImmutable' i]").on('change', function () {
            const value = $(this).val();

            if (value === '' || value === '0') {
                return;
            }

            showHideAddressSettlement($(this));
        });

        $("[name*='birthCountryImmutable' i]").each(function () {
            if ($(this).val() === '' || $(this).val() == '0') {
                $(this).siblings('.prompt').val('България');
                $(this).val('BG');
            }

            showHideAddressSettlement($(this));
        })

        $("[name*='settlementImmutable']").on('change', function () {
            const settlementEkatte = $(this).val();
            if (settlementEkatte === '' || settlementEkatte === '0') {                
                $(this).closest('.fieldgroup-fieldset').find("[name*='regionImmutable']").val('');
                $(this).closest('.fieldgroup-fieldset').find("[name*='regionImmutable']").siblings('.text').text('');
                return;
            }
            if (settlementEkatte) {
                loadRegionsByEkatte(settlementEkatte, $(this));
                //loadStreetsByEkatte(settlementEkatte, $(this));
                //loadDistrictsByEkatte(settlementEkatte, $(this));
            } else {
                clearStreets();
            }
        });

        $("[name*='countryImmutable' i]").on('change', function () {
            const value = $(this).val();

            if (value === '' || value === '0') {
                return;
            }

            showAddressCountryDependent($(this));
        });
    }
    
    //#region за идентификатор на физическо лице

    function showHideAddressSettlement(caller) {
        if (caller.val() === "BG") {
            caller.closest('.ui.form').parent().closest('.ui.form').find("[name*='birthPlaceBgImmutable']").closest('.ui.form').show();
            caller.closest('.ui.form').parent().closest('.ui.form').find("[name*='birthPlaceAbroadImmutable']").closest('.ui.form').hide();
        } else if (caller.val() !== "") {
            caller.closest('.ui.form').parent().closest('.ui.form').find("[name*='birthPlaceBgImmutable']").closest('.ui.form').hide();
            caller.closest('.ui.form').parent().closest('.ui.form').find("[name*='birthPlaceAbroadImmutable']").closest('.ui.form').show();
        }
        else {
            caller.closest('.ui.form').parent().closest('.ui.form').find("[name*='birthPlaceBgImmutable']").closest('.ui.form').hide();
            caller.closest('.ui.form').parent().closest('.ui.form').find("[name*='birthPlaceAbroadImmutable']").closest('.ui.form').hide();
        }
    }

    $("[name*='birthCountryImmutable' i]").each(function () {
        if ($(this).val() === '' || $(this).val() == '0') {
            $(this).siblings('.prompt').val('България');
            $(this).val('BG');
        }

        showHideAddressSettlement($(this));
    })

    //#endregion

    //#region за адрес

    $("[name*='settlementImmutable']").each(function () {
        const settlementEkatte = $(this).val();
        if (settlementEkatte === '' || settlementEkatte === '0') {
            //TODO clearraion!
            return;
        }
        if (settlementEkatte) {
            loadRegionsByEkatte(settlementEkatte, $(this));
            //loadStreetsByEkatte(settlementEkatte, $(this), false);
            //loadDistrictsByEkatte(settlementEkatte, $(this), false);
        } else {
            clearStreets();
        }
    });

    function loadRegionsByEkatte(settlementEkatte, changedElement) {
        // Replace `/getStreets` with your backend endpoint
        $.ajax({
            url: '/Admin/Nomenclature/GetNomenclatureValues',
            method: 'GET',
            data: { nomenclatureCode: 'EK007', holderCode: settlementEkatte },
            success: function (data) {
                let populatableMenuElement = changedElement.closest('.ui.form').parent().closest('.ui.form').find("[name*='regionImmutable']").siblings('.menu');
                let dropdownElement = changedElement.closest('.ui.form').parent().closest('.ui.form').find("[name*='regionImmutable']");

                if (data && data.length > 0) {                
                    populateSelectOptions(populatableMenuElement, dropdownElement, data);
                } else {
                    // Clear the dropdown and hidden input for cities without regions
                    dropdownElement.dropdown('clear'); // Clear the dropdown
                    dropdownElement.val(''); // Explicitly clear the hidden input
                    dropdownElement.siblings('.text').text('Няма райони'); // Set placeholder text
                    populatableMenuElement.empty(); // Clear the menu
                    dropdownElement.dropdown('refresh'); // Refresh the dropdown
                    // Disable the dropdown to prevent interaction
                    dropdownElement.closest('.ui.dropdown').addClass('disabled');                    
                }
            },
            error: function () {
                return showToast('warning', 'Проблем при зареждане на райони');
            }
        });
    }

    //function loadStreetsByEkatte(settlementEkatte, changedElement, deleteCurrent = true) {
    //    let populatableElement = changedElement.closest('.ui.form').parent().closest('.ui.form').find("[name*='streetImmutable']").closest('.ui.search');
    //    let hiddenElement = populatableElement.find('input[type="hidden"]');
    //    let searchInput = populatableElement.find('input.prompt');

    //    if (deleteCurrent) {

    //        hiddenElement.val('');
    //        hiddenElement.trigger('change');

    //        searchInput.val('');

    //        // Reset the search component value and clear cache
    //        populatableElement.search('set value', ''); // Reset visible value
    //        populatableElement.search('clear cache');   // Clear cached results
    //    }

    //    populatableElement.search(
    //        {
    //            apiSettings: {
    //                url: `/Admin/Nomenclature/GetNomenclatureValuesForAutocomplete?query={query}&nomenclatureCode=EK008&holderCode=${settlementEkatte}`,
    //                cache: false
    //            },
    //            minCharacters: 3,
    //            onSelect: function (result) {
    //                hiddenElement.val(result.id);
    //                hiddenElement.trigger('change');
    //            }
    //        });
    //}

    //function loadDistrictsByEkatte(settlementEkatte, changedElement, deleteCurrent = true) {
    //    let populatableElement = changedElement.closest('.ui.form').parent().closest('.ui.form').find("[name*='districtImmutable']").closest('.ui.search');
    //    let hiddenElement = populatableElement.find('input[type="hidden"]');
    //    let searchInput = populatableElement.find('input.prompt');

    //    if (deleteCurrent) {

    //        hiddenElement.val('');
    //        hiddenElement.trigger('change');

    //        searchInput.val('');

    //        // Reset the search component value and clear cache
    //        populatableElement.search('set value', ''); // Reset visible value
    //        populatableElement.search('clear cache');   // Clear cached results
    //    }

    //    populatableElement.search(
    //        {
    //            apiSettings: {
    //                url: `/Admin/Nomenclature/GetNomenclatureValuesForAutocomplete?query={query}&nomenclatureCode=EK010&holderCode=${settlementEkatte}`,
    //                cache: false
    //            },
    //            minCharacters: 3,
    //            onSelect: function (result) {
    //                hiddenElement.val(result.id);
    //                hiddenElement.trigger('change');
    //            }
    //        });
    //}

    //#endregion

    //#region за компания

    $('.cid').find('.ui.dropdown').each(function () {
        let cid = $(this).closest('.cid');        
        setLegalFormVisibility(cid)
    });

    //#endregion
});

var conditionData;
function AddTrashIconToClone(element) {
    let targetElement = element.find("legend").first();

    // Check if there is no legend and fallback to label
    if (targetElement.length === 0) {
        targetElement = element.find("label").first();
    }

    if (targetElement.length > 0) {
        // Check if the label is inside a checkbox
        const isCheckbox = targetElement.closest(".ui.checkbox").length > 0;

        if (isCheckbox) {
            // Append the trash icon directly
            targetElement.append(
                $('<i>', {
                    class: "trash alternate red icon remove-item"                   
                }).on("click", function () {
                    $(this).closest(".ui.form").remove();                    
                })
            );
        } else {
            // Append the button for other cases
            targetElement.append(
                $('<div>', {
                    class: "ui right top floated red tertiary icon button remove-item",
                    "data-tooltip": "Изтриване",
                    html: '<i class="trash alternate icon"></i>'
                }).on("click", function () {
                    $(this).closest(".ui.form").remove();                    
                })
            );
        }
    }
}

function populateSelectOptions(menuElement, dropdownElement, data) {
    // Store the current selected value before clearing
    let currentValue = dropdownElement.val();

    // Clear and repopulate the menu
    menuElement.empty();
    data.forEach(d => {
        menuElement.append(
            `<div class="item" data-value="${d.value}">${d.text}</div>`
        );
    });

    // Enable the dropdown
    dropdownElement.closest('.ui.dropdown').removeClass('disabled');

    // Check if the current value is still valid in the new data
    let selectedItem = data.find(d => d.value === currentValue);
    if (selectedItem) {
        // Restore the previous selection
        dropdownElement.val(currentValue);
        dropdownElement.siblings('.text').text(selectedItem.text);
    } else {
        // No valid previous selection, clear and set placeholder
        dropdownElement.dropdown('clear');
        dropdownElement.siblings('.text').text('Изберете район...');
    }

    // Refresh the dropdown to update its state
    dropdownElement.dropdown('refresh');
}

//#region Person Identifier

function setLoadPersonInfoButtonVisibility(pid) {

    let parentFieldset = pid.closest('fieldset');
    let pidType = pid.find('.ui.dropdown').dropdown('get value');

    if ((parentFieldset.find("[name*='firstNameImmutable'], [name*='middleNameImmutable'], [name*='lastNameImmutable']").length > 0)
        && (pidType == 1))//ЕГН
    {
        pid.find('.load-person-info').show();
    }
    else {
        pid.find('.load-person-info').hide();
    }

    $('.load-person-info').off('click');
    $('.load-person-info').on("click", function () {
        showLoader('body');
        let pid = $(this).closest('.pid');
        let actionUrl = '/Admin/Integration/GetPersonData';
        get_async(actionUrl, {
            pidType: pid.find('.ui.dropdown').dropdown('get value'),
            pid: pid.find(':input[type=text]').val().trim()
        },
        )
            .then((result) => {
                if (!result.success) {
                    hideLoader('body');
                    showToast("error", result.message);
                }
                else {
                    let parentForm = pid.parent().closest('.form');

                    if (parentForm.find("[name*='firstNameImmutable']").length) {
                        parentForm.find("[name*='firstNameImmutable']").val(result.firstName);
                    }

                    if (parentForm.find("[name*='middleNameImmutable']").length) {
                        parentForm.find("[name*='middleNameImmutable']").val(result.middleName);
                    }

                    if (parentForm.find("[name*='lastNameImmutable']").length) {
                        parentForm.find("[name*='lastNameImmutable']").val(result.lastName);
                    }

                    parentForm.find("[name*='firstNameImmutable'], [name*='middleNameImmutable'], [name*='lastNameImmutable']")
                        .addClass('disabled')
                        .prop("readonly", true);

                    pid.find(".ui.dropdown.label").addClass('disabled');
                    pid.find(':input[type=text]').addClass('disabled')
                        .prop("readonly", true);

                    pid.find('.load-person-info').hide();

                    hideLoader('body');
                }
            })
            .catch((error) => {
                hideLoader('body');
                console.error('Грешка при URL ' + actionUrl + " : " + error.statusText);
            });
    });
}

//#region Pid initialize

    function pidInitialize(){
        $('.pid').find('.ui.dropdown').change(function () {
            let pid = $(this).closest('.pid');
            generatePIDValue(pid);
            setLoadPersonInfoButtonVisibility(pid);
        });

        $('.pid').find(':input[type=text]').on('input', function () {
            generatePIDValue($(this).closest('.pid'));
        });

        $('.pid').each(function () {
            setLoadPersonInfoButtonVisibility($(this));
        });
    }

//#endregion

//#region Cid initialize

function cidInitialize() {
    $('.cid').find('.ui.dropdown').change(function () {
        let cid = $(this).closest('.cid');
        generateCIDValue(cid);
        setLoadCompanyInfoButtonVisibility(cid);
        setLegalFormVisibility(cid)
    });
    
    $('.cid').find(':input[type=text]').on('input', function () {
        generateCIDValue($(this).closest('.cid'));
    });

    $('.cid').each(function () {
        setLoadCompanyInfoButtonVisibility($(this));
    });
}

//#endregion

//#endregion

//#region Company

function setLoadCompanyInfoButtonVisibility(cid) {

    let parentFieldset = cid.closest('fieldset');
    let cidType = cid.find('.ui.dropdown').dropdown('get value');

    if ((parentFieldset.find("[name*='legalFormEIKImmutable'], [name*='companyNameImmutable']").length > 0) && (cidType == 1 || cidType == 2))//ЕИК или БУЛСТАТ
    {
        cid.find('.load-company-info').show();
    }
    else {
        cid.find('.load-company-info').hide();
    }

    $('.load-company-info').off('click');
    $('.load-company-info').on("click", function () {
        showLoader('body');
        let actionUrl = '/Admin/Integration/GetCompanyData';
        let cid = $(this).closest('.cid');
        let cidType = cid.find('.ui.dropdown').dropdown('get value');
        get_async(actionUrl, {
            cidType: cidType,
            cid: cid.find(':input[type=text]').val().trim()
        },
        )
            .then((result) => {
                if (!result.success) {
                    hideLoader('body');
                    showToast("error", result.message);
                }
                else {
                    let parentForm = cid.parent().closest('.form');
                    let companyName = parentForm.find("[name*='companyNameImmutable']");
                    let legalFormEik = parentForm.find("[name*='legalFormEIKImmutable']");
                    let legalFormBulstat = parentForm.find("[name*='legalFormBulstatImmutable']");

                    let country = parentForm.find("[name*='countryImmutable']");
                    if (country.length) {                        
                        country.val(result.countryCode).trigger('change');
                        country.siblings('.prompt').val(result.countryName);
                        country.parent().addClass('disabled');
                    }

                    let settlement = parentForm.find("[name*='settlementImmutable']");
                    if (settlement.length) {     
                        settlement.val(result.settlementCode).trigger('change');
                        settlement.siblings('.prompt').val(result.settlementName);
                        settlement.parent().addClass('disabled');
                    }

                    let postalCode = parentForm.find("[name*='postalCodeImmutable']");
                    if (postalCode.length) {
                        postalCode.val(result.postCode).addClass('disabled').prop('readonly', true);
                    }

                    let region = parentForm.find("[name*='regionImmutable']");
                    if (region.length) {                        
                        region.val(result.regionCode).trigger('change');
                        region.parent().addClass('disabled');
                    }                    

                    let street = parentForm.find("[name*='streetImmutable']");
                    if (street.length) {
                        street.val(result.streetName).addClass('disabled').prop('readonly', true);
                    }

                    let buildingNumber = parentForm.find("[name*='buildingNumberImmutable']");
                    if (buildingNumber.length) {
                        buildingNumber.val(!(result.buildingNumber === null || result.buildingNumber === "") ?
                            result.buildingNumber : result.streetNumber).addClass('disabled').prop('readonly', true);
                    }

                    let entranceNumber = parentForm.find("[name*='entranceNumberImmutable']");
                    if (entranceNumber.length) {
                        entranceNumber.val(result.entranceName).addClass('disabled').prop('readonly', true);
                    }

                    let floor = parentForm.find("[name*='floorImmutable']");
                    if (floor.length) {
                        floor.val(result.floorNumber).addClass('disabled').prop('readonly', true);
                    }

                    let apartmentNumber = parentForm.find("[name*='apartmentNumberImmutable']");
                    if (apartmentNumber.length) {
                        apartmentNumber.val(result.apartmentNumber).addClass('disabled').prop('readonly', true);
                    }

                    let addressAbroad = parentForm.find("[name*='addressAbroadImmutable']");
                    if (addressAbroad.length) {
                        addressAbroad.val(result.foreignAddress).addClass('disabled').prop('readonly', true);
                    }

                    if (companyName.length) {
                        companyName.val(result.companyName).addClass('disabled').prop('readonly', true);
                    }
                    if (cidType == 1) { //ЕИК
                        if (legalFormEik.length) {
                            legalFormEik.val(result.legalFormCode).trigger('change');
                            legalFormEik.parent().addClass('disabled');
                        }
                        else if (legalFormBulstat.length) {
                            legalFormBulstat.val(-1).trigger('change');                            
                            legalFormBulstat.siblings('.prompt').val();
                        }
                    }
                    if (cidType == 2) { //БУЛСТАТ
                        if (legalFormEik.length) {
                            legalFormEik.val(-1).trigger('change');
                        }
                        if (legalFormBulstat.length) {
                            legalFormBulstat.val(result.legalFormCode).trigger('change');
                            legalFormBulstat.siblings('.prompt').val(result.legalFormName);
                            legalFormBulstat.parent().addClass('disabled');

                        }
                    }

                    cid.find(".ui.dropdown.label").addClass('disabled');
                    cid.find(':input[type=text]').addClass('disabled').prop('readonly', true);
                    cid.find('.load-company-info').hide();
                    hideLoader('body');
                }
            })
            .catch((error) => {
                hideLoader('body');
                console.error('Грешка при URL ' + actionUrl + " : " + error.statusText);
            });
    });
}

function setLegalFormVisibility(cid) {
    if (cid.closest('fieldset').find("[name*='legalFormEIKImmutable'], [name*='companyNameImmutable']").length <= 0) {
        return;
    }
    let parentForm = cid.parent().closest('.form');
    let cidType = cid.find('.ui.dropdown').dropdown('get value');

    parentForm.find("[name*='legalFormBulstatImmutable']").closest(".field").addClass("required");
    parentForm.find("[name*='legalFormEIKImmutable']").closest(".field").addClass("required");    

    parentForm.find("[name*='legalFormBulstatImmutable']").closest('.ui.form').hide();
    parentForm.find("[name*='legalFormEIKImmutable']").closest('.ui.form').hide();

    if (cidType == 1) //ЕИК
    {
        parentForm.find("[name*='legalFormEIKImmutable']").closest('.ui.form').show();
        parentForm.find("[name*='legalFormBulstatImmutable']").val('').trigger('change');
        parentForm.find("[name*='legalFormBulstatImmutable']").siblings('.prompt').val('');
    }
    else if (cidType == 2) //БУЛСТАТ
    {        
        parentForm.find("[name*='legalFormBulstatImmutable']").closest('.ui.form').show();
        parentForm.find("[name*='legalFormEIKImmutable']").val('').trigger('change');        
    }
}

//#endregion

function preventTooltipOutsideOfBoundaries() {
    $('.checkbox-tooltip').popup({
        boundary: '.ui checkbox'
    })

    $('.person-tooltip').popup({
        boundary: '#showPreviewFields'
    })

    $('.company-tooltip').popup({
        boundary: '#showPreviewFields'
    })
}
function makeComplexReadonlyFieldSubfieldsReadonly() {    
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
}

function initializeFormFields() {
    cidInitialize();
    pidInitialize();
    checkboxInitialize();
    fileUploadInitialize();
    autocompleteWithCategoriesInitialize();
    addressInitialize();
    currencyInitialize();
}
function addressInitialize() {

    $("[name*='countryImmutable' i]").each(function () {
        showAddressCountryDependent($(this));
    })
};

function clearFileInput(sender) {   
    sender.find('.upload-file-input').val('');
    sender.find('.upload-file-key').val('');
    sender.find('.selected-file').text('');
    sender.find('.selected-file').removeAttr('title');
}

function autocompleteWithCategoriesInitialize() {    
    $('.autocomplete-with-category').find('input.prompt')
        .on('input keyup', function () {
            let hiddenElement = $(this).parent().find('input[type="hidden"]');
            hiddenElement.val(0);
            hiddenElement.trigger('change');
        });

    $('.autocomplete-with-category')
        .each(function () {
            $(this).search(
                {
                    apiSettings: {
                        url: `/Admin/Nomenclature/GetAutocompleteWithCategoryValues?query={query}&nomenclatureType=${$(this).data('nomenclatureType')}`,
                    },
                    minCharacters: 5,
                    type: 'category',
                    onSelect: function (result) {
                        let hiddenElement = $(this).find('input[type="hidden"]');
                        hiddenElement.val(result.value);
                        hiddenElement.trigger('change');
                    }
                });
        });

    $('.autocomplete-with-category').find('input.prompt')
        .on('input keyup', function () {
            let hiddenElement = $(this).parent().find('input[type="hidden"]');
            hiddenElement.val(0);
            hiddenElement.trigger('change');
        });
}
function fileUploadInitialize() {    
    $('.upload-file-input').off('change');

    $('.upload-file-input').change(function () {        
        var selectedFiles = $(this).prop("files");
        if (selectedFiles.length === 0) {            
            $(this).parent().find('.selected-file').attr('title', "Изберете файл");
            $(this).parent().find('.remove-file').parent().hide();
        }
        else {            
            if (selectedFiles.length > 0) {                
                uploadFile($(this), selectedFiles[0], $(this).parent().find('.upload-file-key').attr('name'));
            }            
        }
    });    

    $('.remove-file').off('click');

    $('.remove-file').click(function () {              
        var $action = $(this).closest('.action');
        var fileKey = $action.find(".upload-file-key[hidden]").val(); // Assuming the file key is stored as a data attribute
        if (!fileKey) {           
            return;
        }
        var dontUploadFilesToStorage = $('#DontUploadFilesToStorage').val();
        var fileData = {
            key: fileKey,
            dontUploadFilesToStorage: dontUploadFilesToStorage,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        }

        fileActionWithConfirmation("Сигурни ли сте, че искате да премахнете файла?", function () {
            clearFileInput($action);
        });
    });
}

function uploadFile(sender, file, fieldName) {
    showLoader('body');
    let url = '/Admin/process/UploadFile'

    let formData = new FormData();
        
    formData.append('file', file);
    formData.append('formParentId', $('#FormParentId').val());
    formData.append('fieldName', fieldName);
    formData.append('key', sender.parent().find(".upload-file-key[hidden]").val());
    formData.append('dontUploadFilesToStorage', $('#DontUploadFilesToStorage').val());    
    formData.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val());

    //TODO : loading screen
    upload_file_async(url, formData,
    ).then((result) => {
        if (result.success === true) {
            hideLoader('body');           
            let label = sender.parent().find('.selected-file');
            let textForLabel = file.name;
            label.text(textForLabel);
            label.attr('title', textForLabel.replace(/; /g, "\n"));
            sender.parent().find(".upload-file-key[hidden]").val(result.fileKey);
            sender.parent().find('.remove-file').parent().show();
            showToast('success', 'Файлът е качен успешно');
        }
        else {
                hideLoader('body');
                showToast('error', result.error);
             }
        })
        .catch((error) => {
            hideLoader('body');
            console.error("Проблем при качване на файл " + error);        
        });
}

function checkboxInitialize() {    
    $(".checkbox-template").change(function () {
        if ($(this).is(':checked')) {
            $(this).parent().find('input:hidden').val('true');
        } else {
            $(this).parent().find('input:hidden').val('false');
        }
    });    
}

function generatePIDValue(pidContainer) {
    let pidType = pidContainer.find('.ui.dropdown').dropdown('get value');
    let pidNumber = pidContainer.find(':input[type=text]').val().trim();
    pidContainer.find("input[type='hidden']:not(.label input)").val(pidType + ':' + pidNumber);
}

function generateCIDValue(cidContainer) {
    let cidType = cidContainer.find('.ui.dropdown').dropdown('get value');
    let cidNumber = cidContainer.find(':input[type=text]').val().trim();
    cidContainer.find("input[type='hidden']:not(.label input)").val(cidType + ':' + cidNumber);
}

function addTimeZoneAtSubmit() {
    $('#submit').submit(function () {
        $('#UserTimeZoneOffsetInMinutes').val(new Date().getTimezoneOffset());
    });
}

function printSpecificArea() {
    const elementsToHide = document.querySelectorAll('.no-print');
    const originalDisplay = [];

    // Hide elements
    elementsToHide.forEach(el => {
        originalDisplay.push(el.style.display);
        el.style.display = 'none';
    });

    window.print();

    // Restore hidden elements
    elementsToHide.forEach((el, i) => {
        el.style.display = originalDisplay[i];
    });
}

function showAddressCountryDependent(caller) {

    let addressForm = caller.closest('.ui.form').parent().closest('.ui.form');

    if (caller.val() === "BG") {
        addressForm.find("[name*='settlementImmutable']").closest('.ui.form').show();
        addressForm.find("[name*='postalCodeImmutable']").closest('.ui.form').show();
        addressForm.find("[name*='regionImmutable']").closest('.ui.form').show();
        //addressForm.find("[name*='districtImmutable']").closest('.ui.form').show();
        addressForm.find("[name*='streetImmutable']").closest('.ui.form').show();
        addressForm.find("[name*='buildingNumberImmutable']").closest('.ui.form').show();
        addressForm.find("[name*='entranceNumberImmutable']").closest('.ui.form').show();
        addressForm.find("[name*='floorImmutable']").closest('.ui.form').show();
        addressForm.find("[name*='apartmentNumberImmutable']").closest('.ui.form').show();
        addressForm.find("[name*='addressAbroadImmutable']").closest('.ui.form').hide();
    } else if (caller.val() !== "") {
        addressForm.find("[name*='settlementImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='postalCodeImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='regionImmutable']").closest('.ui.form').hide();
        //addressForm.find("[name*='districtImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='streetImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='buildingNumberImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='entranceNumberImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='floorImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='apartmentNumberImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='addressAbroadImmutable']").closest('.ui.form').show();
    }
    else {
        addressForm.find("[name*='settlementImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='postalCodeImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='regionImmutable']").closest('.ui.form').hide();
        //addressForm.find("[name*='districtImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='streetImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='buildingNumberImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='entranceNumberImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='floorImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='apartmentNumberImmutable']").closest('.ui.form').hide();
        addressForm.find("[name*='addressAbroadImmutable']").closest('.ui.form').hide();
    }
}    

//#region Currency initialize

function currencyInitialize() {
    let currencyContainers = $('.currency').find(':input[type=number]').closest('.currency');

    if (currencyContainers.length == 0) {
        return;
    }

    currencyContainers.each(function () {
        generateCurrencyValue($(this));
    });
    
    $('.currency').find(':input[type=number]').on('input', function () {
        generateCurrencyValue($(this).closest('.currency'));
    });
}

//#endregion

function generateCurrencyValue(currencyContainer) {
    let isBeforeEuro = $('#isBeforeEuro').val();
    //const switchDate = new Date('2026-01-01');
    //const currentDate = new Date();
    const currencyCode = isBeforeEuro ? '1' : '2';
    let currencyValue = currencyContainer.find(':input[type=number]').val().trim();
    let currencyResult = currencyCode + ':' + currencyValue;
    if (!currencyValue) {
        currencyResult = '';
    }
    currencyContainer.find("input[type='hidden']:not(.label input)").val(currencyResult);
}

function applyFormFieldConditions() {
    // Trigger initial update for all dropdowns based on their current values
    $('.ui.dropdown').each(function () {
        const $dropdown = $(this);
        const currentValue = $dropdown.dropdown('get value');
        if (currentValue) {
            showHideFieldsAccordingToConditionTree($dropdown, currentValue);
        }
    });
    
    $('.ui.dropdown').dropdown({
        onChange: function (value, text, $selectedItem) {
            showHideFieldsAccordingToConditionTree($(this), value);
        }
    });
}

// Function to handle showing/hiding fields based on dropdown value
function showHideFieldsAccordingToConditionTree(dropdown, value) {    
    const fieldName = dropdown.find('input[type="hidden"]').attr('name');
    if (fieldName && conditionData[fieldName]) {

        let FieldsToShow = conditionData[fieldName].FieldsToShow;
        
        // Show fields
        FieldsToShow.forEach(field => {
            $(`#${field}, [name="${field}"]`).closest('.ui.form').show();
        });

        if (conditionData[fieldName].Conditions[value])
        {
            let FieldsToHide = conditionData[fieldName].Conditions[value].FieldsToHide;
            // Hide fields
            FieldsToHide.forEach(field => {
                $(`#${field}, [name="${field}"]`).closest('.ui.form').hide();
            });
        }
    }
}