$(function () {
    LoadCodeableConcept();
});

function LoadCodeableConcept() {
    const tableId = '#codeableConcepts';
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
                    name: 'actions',
                    data: "id",
                    title: "Действия",
                    sortable: false,
                    searchable: false,
                    className: "text-left noExport",
                    width: 120,
                    "render": function (data, type, row) {
                        return editButton(`/Nomenclature/Edit/${data}`);
                    }
                }
            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}