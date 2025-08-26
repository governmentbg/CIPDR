$(function () {
    LoadCalendar();
});

function LoadCalendar() {
    const tableId = '#calendar_days';
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else {
        openAccordionFilter()
        let url = $(tableId).data('url');
        let dt = $(tableId).DataTable({
            'order': [[0, 'desc']],
            ajax: {
                "url": url,
                "type": "POST",
                "datatype": "json",
                data: function (d) {
                    d.filter = {
                        dateFrom: $('#DateFrom').val(),
                        dateTo: $('#DateTo').val(),
                    }
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: false,
            columns: [
                {
                    name: 'currentDate',
                    data: 'currentDate',
                    title: 'Дата',
                    sortable: true,
                    searchable: false,
                    "render": function (data) {
                        debugger
                        return JsonBGdateTS(data);
                    }
                },
                {
                    name: 'kind',
                    data: 'kind',
                    title: 'Вид',
                    sortable: true,
                    searchable: true,
                    type: 'string'
                },
                {
                    name: 'description',
                    data: 'description',
                    title: 'Описание',
                    sortable: true,
                    searchable: true,
                },
                {
                    name: 'actions',
                    data: "id",
                    title: "Действия",
                    sortable: false,
                    searchable: false,
                    className: "text-left noExport",
                    width: 150,
                    "render": function (data, type, row) {
                        return editButton(`/Calendar/Edit/${data}`);
                    }
                }
            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}

async function confirmCodeableConcept(id, code, name) {
    const result = await confirmDialog('Потвърдете номенклатурна стойност', name, 'Потвърди');
    if (result) {
        const param = $('#codeableConcepts').DataTable().ajax.params();
        const data = {
            type: param.filter.type,
            id,
            code,
            statusId: 2
        };
        await post_fetch_string_async('/Nomenclature/UpdateCodeableConceptStatus', data);
        LoadCodeableConcept();
    }
}

async function refuseCodeableConcept(id, code, name) {
    const result = await confirmDialog('Отказ номенклатурна стойност', name, 'Потвърди отказ');
    if (result) {
        const param = $('#codeableConcepts').DataTable().ajax.params();
        const data = {
            type: param.filter.type,
            id,
            statusId: 3,
            code
        };
        await post_fetch_string_async('/Nomenclature/UpdateCodeableConceptStatus', data);
        LoadCodeableConcept();
    }
}