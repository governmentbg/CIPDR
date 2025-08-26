$(function () {
    LoadDeadlines();
});

function LoadDeadlines() {
    const tableId = '#deadline';
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
                    name: 'serviceName',
                    data: 'serviceName',
                    title: 'Услуга',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'deadlineType',
                    data: 'deadlineType',
                    title: 'Вид срок',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'dayType',
                    data: 'dayType',
                    title: 'Вид',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'days',
                    data: 'days',
                    title: 'Дни',
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
                    width: 140,
                    "render": function(data, type, row) {
                        return editButton(`/Admin/Deadline/Edit/${data}`) +
                            "<a href='javascript:actionWithConfirmation(\"/Admin/Deadline/DeleteDeadline\", " +
                            data + ", \"Сигурни ли сте, че искате да изтриете " +
                            row.name +
                            "?\", null)' type='button' class='ui tertiary icon button' data-tooltip='Изтрий'><i class='red trash alternate icon'></i></button>";;
                    }
                }
            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}