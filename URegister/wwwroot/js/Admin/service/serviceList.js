$(function () {
    LoadServiceTypes();
});

function LoadServiceTypes() {
    debugger
    const tableId = '#services';
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
                    d.filter = {}
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: false,
            columns: [
                {
                    name: 'title',
                    data: 'title',
                    title: 'Услуга',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'serviceType',
                    data: 'serviceType',
                    title: 'Тип услуга',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'actions',
                    data: "id",
                    title: "Действия",
                    sortable: false,
                    searchable: false,
                    className: "text-left noExport",
                    width: 120,
                    "render": function(data, type, row) {
                        return editButton(`/Admin/Service/Edit/${data}`);
                    }
                }
            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}