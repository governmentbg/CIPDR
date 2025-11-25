$(function () {
    $("#TriggeringFieldName").on('change', function () {
        const selectedValue = $(this).val();
        if (selectedValue === '' || selectedValue === '0') {            
            return;
        }
        if (selectedValue) {
            loadCodeableConcepts(selectedValue);
        } else {
            clearCodeableConcepts(); // Clear streets if the input is empty
        }
    });
});

function loadCodeableConcepts(selectedValue) {
    let formParentId = $('#FormParentId').val();    
    $.ajax({
        url: '/Admin/Catalog/GetNomenclatureValuesForTriggeringValue',
        method: 'GET',
        data: { formParentId: formParentId, triggeringValue: selectedValue },
        success: function (data) {
            //let dropdown = $('#TriggeringNomenclatureValue');
            const dropdown = document.getElementById('TriggeringNomenclatureValue');
            
            if (data && data.length > 0) {
                dropdown.innerHTML = '';

                // Add a default placeholder option
                const placeholderOption = document.createElement('option');
                placeholderOption.value = '';
                placeholderOption.textContent = 'Изберете';
                placeholderOption.disabled = true;
                placeholderOption.selected = true;
                dropdown.appendChild(placeholderOption);

                // Populate dropdown with AJAX response data
                data.forEach(item => {
                    const option = document.createElement('option');
                    option.value = item.value;
                    option.textContent = item.text;
                    option.disabled = item.disabled;
                    option.selected = item.selected;
                    dropdown.appendChild(option);
                });
            }
        },
        error: function () {
            return showToast('warning', 'Проблем при зареждане на райони');
        }
    });
}