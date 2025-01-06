$(function () {
    LoadAdministrations();
});

function LoadAdministrations() {
    const tableId = '#administrations';
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
               },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: false,
            columns: [
                {
                    name: 'uic',
                    data: 'uic',
                    title: 'ЕИК/БУЛСТАТ',
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
                    name: 'legalBasis',
                    data: 'legalBasis',
                    title: 'Правно основание',
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
                    "render": function (data, type, row) {
                        return editButton(`/Admin/Register/EditAdministration?administrationId=${row.id}`);
                    }
                }
            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}