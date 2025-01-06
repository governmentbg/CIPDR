$(function () {
    LoadNomenclatureTypeRegisters();
    $('#RegisterId').on("change", function () { filterChange() });
    $('#Type').on("change", function () { filterChange() });
    $('#Name').on("change", function () { filterChange() })
});

function LoadNomenclatureTypeRegisters() {
    const tableId = '#nomenclatureTypeRegister';
    const registerId = $('#RegisterId').val();
    $('#nomenclatureTypeRegisterContainer').show();
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
                        Type: $('#Type').val(),
                        Name: $('#Name').val(),
                        RegisterId: $('#RegisterId').val(),
                    }
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: false,
            columns: [
                {
                    name: 'type',
                    data: 'type',
                    title: 'Тип',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'name',
                    data: 'name',
                    title: 'Име',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'isValid',
                    data: 'isValid',
                    title: 'Валиден  <input id="isValid-all" class="center" type="checkbox" onclick="isValidClick(false);"/>',
                    sortable: false,
                    searchable: false,
                    width: 120,
                    "render": function (data, type, row) {
                        const checked = data ? 'checked' : '';
                        return `<input id="isValidRow${row.id}" class="center" type="checkbox" onclick="isValidRowClick('${row.type}', ${row.id}, false);" ${checked}/>`;
                    }

                },
                {
                    name: 'isValidAll',
                    data: 'isValidAll',
                    title: 'Всички стойности <input id="isValidAll-all" class="center" type="checkbox" onclick="isValidClick(true);"/>',
                    sortable: false,
                    searchable: false,
                    width: 200,
                    "render": function (data, type, row) {
                        const checked = data ? 'checked' : '';
                        return `<input id="isValidAllRow${row.id}" class="center" type="checkbox" onclick="isValidRowClick('${row.type}', ${row.id}, true);" ${checked}/>`;
                    }
                },
                {
                    name: 'actions',
                    data: "id",
                    title: "Стойности",
                    sortable: false,
                    searchable: false,
                    className: "text-left noExport",
                    width: 120,
                    "render": function (data, type, row) {
                        return `<a href="/Nomenclature/indexRegister?nomenclatureType=${row.type}&registerId=${registerId}" data-tooltip="Стойности" class="ui tertiary icon button">
                                <i class="tasks icon"></i>
                           </a>`;
                    }
                }

            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}
function getUpdateData() {
    return {
        registerId: $('#RegisterId').val(),
        filterType: $('#Type').val(),
        filterName: $('#Name').val(),
    };
}
async function updateNomenclatureTypeRegister(data) {
    await post_fetch_string_async('/Nomenclature/UpdateNomenclatureTypeRegister', data);
    LoadNomenclatureTypeRegisters()
}

async function isValidClick(all) {
    const data = getUpdateData();
    if (all) {
        data.isValidAll = $('#isValidAll-all').is(':checked');
    } else {
        data.isValid = $('#isValid-all').is(':checked');
    }

    await updateNomenclatureTypeRegister(data);
}

async function isValidRowClick(type, id, all) {
    const data = getUpdateData();
    data.type = type;
    if (all) {
        data.isValidAll = $(`#isValidAllRow${id}`).is(':checked');

    } else {
        data.isValid = $(`#isValidRow${id}`).is(':checked');
    }
    await updateNomenclatureTypeRegister(data);
}

function filterChange() {
    $('#nomenclatureTypeRegisterContainer').hide();
}
