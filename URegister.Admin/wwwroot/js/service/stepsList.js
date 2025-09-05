$(function () {
    LoadSteps();
});

function LoadSteps() {
    const tableId = '#steps';
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
                    name: 'name',
                    data: 'name',
                    title: 'Име',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'roleName',
                    data: 'roleName',
                    title: 'Потребителска роля',
                    sortable: false,
                    searchable: false
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
                        return editButton(`/Service/EditStep/${data}`) +
                            "<a href='javascript:actionWithConfirmation(\"/Service/DeleteStep\", " +
                            data + ", \"Сигурни ли сте, че искате да изтриете \\\"" +
                            row.name +
                            "\\\"?\", null)' type='button' class='ui tertiary icon button' data-tooltip='Изтрий'><i class='red trash alternate icon'></i></a>";
                    }
                }
            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}