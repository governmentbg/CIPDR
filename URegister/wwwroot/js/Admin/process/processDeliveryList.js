$(function () {
    LoadProcessDeliveries();
});

function LoadProcessDeliveries() {
    const tableId = '#processDeliveries';
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
                        processId: $('#ProcessId').val(),
                    }
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: false,
            columns: [
                {
                    name: 'source',
                    data: 'source',
                    title: 'Вид',
                    sortable: true,
                    searchable: true,
                    type: 'string'
                },
                {
                    name: 'channel',
                    data: 'channel',
                    title: 'Метод на връчване',
                    sortable: true,
                    searchable: true,
                },
                {
                    name: 'status',
                    data: 'status',
                    title: 'Статус',
                    sortable: true,
                    searchable: true,
                },
                {
                    name: 'deliveryDate',
                    data: 'deliveryDate',
                    title: 'Дата връчване',
                    sortable: true,
                    searchable: false,
                    "render": function (data) {
                        return JsonBGdate(data);
                    }
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
                        let result = '';
                        if (row.hasResponse || row.canAdd) {
                            result += `<a href="/Admin/Process/ProcessDelivery?processDeliveryId=${data}" data-tooltip="Редакция" class="ui tertiary icon button">
                                <i class="pen icon"></i>
                           </a>`;
                        }
                        return result;
                    }
                }
            ]
        });
    }
}
