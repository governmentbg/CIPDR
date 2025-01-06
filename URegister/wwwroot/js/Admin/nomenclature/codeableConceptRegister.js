$(function () {
    LoadCodeableConceptRegister();
});

function LoadCodeableConceptRegister() {
    const tableId = '#codeableConceptRegister';
    const isValidAllType = $('#IsValidAllType').val() == 'True';
    const isValidTitle = 'Валиден' + (isValidAllType ? "" : ' <input id = "isValid-all" class="center" type = "checkbox" onclick = "isValidClick();" /> ');
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else {
        let url = $(tableId).data('url');
        let dt = $(tableId).DataTable({
            'order': [[0, 'asc']],
            ajax: {
                "url": url,
                "type": "POST",
                "datatype": "json",
                data: function (d) {
                    d.filter = {
                        type: $('#Type').val(),
                        registerId: $('#RegisterId').val()
                    }
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            columns: [
                {
                    name: 'code',
                    data: 'code',
                    title: 'Тип',
                    sortable: true,
                    searchable: true,
                    type: 'string'
                },
                {
                    name: 'value',
                    data: 'value',
                    title: 'Стойност',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'valueEn',
                    data: 'valueEn',
                    title: 'Стойност EN',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'dateFrom',
                    data: 'dateFrom',
                    title: 'Валидно от',
                    sortable: false,
                    searchable: false,
                    "render": function (data) {
                        return JsonBGdateTS(data);
                    }
                },
                {
                    name: 'dateTo',
                    data: 'dateTo',
                    title: 'Валидно до',
                    sortable: false,
                    searchable: false,
                    "render": function (data) {
                        return JsonBGdateTS(data);
                    }
                },
                {
                    name: 'isValid',
                    data: 'isValid',
                    title: isValidTitle,
                    sortable: false,
                    searchable: false,
                    width: 120,
                    "render": function (data, type, row) {
                        if (isValidAllType) {
                            return `<input id="isValidRow${row.id}" class="center" type="checkbox" checked disabled/>`;
                        } 
                        const checked = data ? 'checked' : '';
                        return `<input id="isValidRow${row.id}" class="center" type="checkbox" onclick="isValidRowClick('${row.code}', ${row.id});" ${checked}/>`;
                    }

                },
            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}


function getUpdateData() {
    const param = $('#codeableConceptRegister').DataTable().ajax.params();
    
    console.log(param);
    debugger
    return {
        registerId: param.filter.registerId,
        type: param.filter.type,
        filter: param.search.value,
    };
}
async function updateCodeableConceptRegister(data) {
    await post_fetch_string_async('/Admin/Nomenclature/UpdateCodeableConceptRegister', data);
    LoadCodeableConceptRegister()
}

async function isValidClick(all) {
    const data = getUpdateData();
    data.isValid = $('#isValid-all').is(':checked');
    await updateCodeableConceptRegister(data);
}

async function isValidRowClick(code, id) {
    const data = getUpdateData();
    data.code = code;
    data.isValid = $(`#isValidRow${id}`).is(':checked');
    await updateCodeableConceptRegister(data);
}
