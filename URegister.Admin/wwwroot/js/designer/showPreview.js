$(function () {
    $('.repeat-field').on('click', function (event) {
        let parentForm = $(this).siblings('.ui.form').parent();
        let fieldToClone = $(this).siblings('.ui.form').first();

        // Destroy calendar instances on the first field
        fieldToClone.find('.dateonly-calendar, .datetime-calendar').calendar('destroy');

        let clonedField = fieldToClone.clone(false); // Clone without data and events
        clonedField.find("span.validation-error").remove();

        AddThrashIconToClone(clonedField);

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

        // Clear input values in the cloned field if needed
        clonedField.find('input').val('');
        clonedField.find('.ui.dropdown').dropdown('clear');

        if (clonedField.find('.pid-text').length === 1) {
            clonedField.find('.pid-text').closest('.dropdown.label').dropdown('set text', 'Тип');
        }

        // Insert the cloned field right after the last field
        //parentForm.append(clonedField);
        $(this).before(clonedField);

        initializeElements(parentForm);
        clonedField.find('.checkbox-template').trigger("change");
        subscribeAfterCloning();

        $(this).siblings('.delete-field').show();
    });

    $('.repeat-field').closest('.ui.form').find('.ui.form')
        .filter(function () {
            return $(this).find('[name*="#"]').length > 0;
        })
        .each(function () {
            AddThrashIconToClone($(this));
        });

    $('.delete-field').on('click', function () {
        let fields = $(this).siblings('.ui.form');
        // Remove only if more than one field exists
        if (fields.length > 1) {
            fields.last().remove();
        }

        // Hide the remove button if no clones are left
        if (fields.length <= 2) { // 1 original + 1 clone = 2 fields
            $(this).hide();
        }
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
            const settlementEkatte = $(this).val(); // Get the current value of the input
            if (settlementEkatte === '' || settlementEkatte === '0') {
                $(this).closest('.person-fieldset').find("[name*='regionImmutable']").val('');
                $(this).closest('.person-fieldset').find("[name*='regionImmutable']").siblings('.text').text('');
                return;
            }
            if (settlementEkatte) {
                loadRegionsByEkatte(settlementEkatte, $(this)); // Call function to load regions
                loadStreetsByEkatte(settlementEkatte, $(this)); // Call function to load streets
                loadDistrictsByEkatte(settlementEkatte, $(this)); // Call function to load districts
            } else {
                clearStreets(); // Clear streets if the input is empty
            }
        });
    }

    //#region За идентификатор на физическо лице

    function showHideAddressSettlement(caller) {
        if (caller.val() === "BG") {
            caller.closest('.ui.form').parent().closest('.ui.form').find("[name*='birthPlaceBgImmutable']").closest('.ui.form').show();
            caller.closest('.ui.form').parent().closest('.ui.form').find("[name*='birthPlaceAbroadImmutable']").closest('.ui.form').hide();
        } else {
            caller.closest('.ui.form').parent().closest('.ui.form').find("[name*='birthPlaceBgImmutable']").closest('.ui.form').hide();
            caller.closest('.ui.form').parent().closest('.ui.form').find("[name*='birthPlaceAbroadImmutable']").closest('.ui.form').show();
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
        const settlementEkatte = $(this).val(); // Get the current value of the input
        if (settlementEkatte === '' || settlementEkatte === '0') {
            //TODO clearraion!
            return;
        }
        if (settlementEkatte) {
            loadRegionsByEkatte(settlementEkatte, $(this)); // Call function to load regions
            loadStreetsByEkatte(settlementEkatte, $(this), false); // Call function to load streets
            loadDistrictsByEkatte(settlementEkatte, $(this), false); // Call function to load districts
        } else {
            clearStreets(); // Clear streets if the input is empty
        }
    });

    function loadRegionsByEkatte(settlementEkatte, changedElement) {
        $.ajax({
            url: '/Nomenclature/GetNomenclatureValues', // Backend endpoint to fetch
            method: 'GET',
            data: { nomenclatureCode: 'EK007', holderCode: settlementEkatte }, // Pass the input value to the server
            success: function (data) {
                if (data) {
                    let populatableMenuElement = changedElement.closest('.ui.form').parent().closest('.ui.form').find("[name*='regionImmutable']").siblings('.menu');
                    let dropdownElement = changedElement.closest('.ui.form').parent().closest('.ui.form').find("[name*='regionImmutable']");
                    populateSelectOptions(populatableMenuElement, dropdownElement, data);
                } else {
                    return showToast('warning', 'Проблем при зареждане на райони');
                }
            },
            error: function () {
                return showToast('warning', 'Проблем при зареждане на райони');
            }
        });
    }

    function loadStreetsByEkatte(settlementEkatte, changedElement, deleteCurrent = true) {
        let populatableElement = changedElement.closest('.ui.form').parent().closest('.ui.form').find("[name*='streetImmutable']").closest('.ui.search');
        let hiddenElement = populatableElement.find('input[type="hidden"]');
        let searchInput = populatableElement.find('input.prompt');

        if (deleteCurrent) {

            hiddenElement.val('');
            hiddenElement.trigger('change');

            searchInput.val('');

            // Reset the search component value and clear cache
            populatableElement.search('set value', ''); // Reset visible value
            populatableElement.search('clear cache');   // Clear cached results
        }

        populatableElement.search(
            {
                apiSettings: {
                    url: `/Nomenclature/GetNomenclatureValuesForAutocomplete?query={query}&nomenclatureCode=EK008&holderCode=${settlementEkatte}`,
                    cache: false
                },
                minCharacters: 3,
                onSelect: function (result) {
                    hiddenElement.val(result.id);
                    hiddenElement.trigger('change');
                }
            });
    }

    function loadDistrictsByEkatte(settlementEkatte, changedElement, deleteCurrent = true) {
        let populatableElement = changedElement.closest('.ui.form').parent().closest('.ui.form').find("[name*='districtImmutable']").closest('.ui.search');
        let hiddenElement = populatableElement.find('input[type="hidden"]');
        let searchInput = populatableElement.find('input.prompt');

        if (deleteCurrent) {

            hiddenElement.val('');
            hiddenElement.trigger('change');

            searchInput.val('');

            // Reset the search component value and clear cache
            populatableElement.search('set value', ''); // Reset visible value
            populatableElement.search('clear cache');   // Clear cached results
        }

        populatableElement.search(
            {
                apiSettings: {
                    url: `/Nomenclature/GetNomenclatureValuesForAutocomplete?query={query}&nomenclatureCode=EK010&holderCode=${settlementEkatte}`,
                    cache: false
                },
                minCharacters: 3,
                onSelect: function (result) {
                    hiddenElement.val(result.id);
                    hiddenElement.trigger('change');
                }
            });
    }

    //#endregion

});

function AddThrashIconToClone(element) {
    element.find("label").first().append(
        $('<button>', {
            class: "ui right top floated red tertiary mini button remove-item",
            "data-tooltip": "Изтриване",
            html: '<i class="trash alternate large icon"></i>'
        }).on("click", function () {
            $(this).closest(".ui.form").remove();
        })
    );
}

function populateSelectOptions(menuElement, dropdownElement, data) {
    menuElement.empty(); // Clear current options
    data.forEach(d => {
        menuElement.append(
            `<div class="item" data-value="${d.value}">${d.text}</div>`
        );
    });
    dropdownElement.dropdown('clear').dropdown('refresh');
}