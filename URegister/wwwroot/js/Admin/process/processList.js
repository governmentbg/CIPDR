$(function () {
    LoadProcesses();
});

function LoadProcesses() {
    debugger
    const tableId = '#processes';
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
                    name: 'incomingNumber',
                    data: 'incomingNumber',
                    title: 'Входящ номер',
                    sortable: true,
                    searchable: true,
                    type: 'string'
                },
                {
                    name: 'registerNumber',
                    data: 'registerNumber',
                    title: 'Рег. номер',
                    sortable: true,
                    searchable: true,
                    type: 'string'
                },
                {
                    name: 'incomingDate',
                    data: 'incomingDate',
                    title: 'Входирано на',
                    sortable: false,
                    searchable: false,
                    "render": function (data) {
                        return JsonBGdate(data);
                    }
                },
                {
                    name: 'serviceName',
                    data: 'serviceName',
                    title: 'Услуга',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'stepName',
                    data: 'stepName',
                    title: 'Стъпка',
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
                        if (row.stepId == 3)
                            return "";
                        return `<a href="/Admin/Process/AddStep?processId=${data}" data-tooltip="Стъпка" class="ui tertiary icon button">
                                <i class="angle double right icon"></i>
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