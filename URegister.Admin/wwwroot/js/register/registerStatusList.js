$(function () {
    LoadRegisterStatus();
});

function LoadRegisterStatus() {
    const tableId = '#registerStatus';
    const registerId = $('#Id').val();
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
                        registerId
                    }
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: true,
            columns: [
                {
                    name: 'modifiedOn',
                    data: 'modifiedOn',
                    title: 'Променено',
                    sortable: false,
                    searchable: false,
                    "render": function (data) {
                        return JsonBGDateTime(data);
                    }
                },
                {
                    name: 'status',
                    data: 'status',
                    title: 'Статус',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'remark',
                    data: 'remark',
                    title: 'Забележка',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'modifiedBy',
                    data: 'modifiedBy',
                    title: 'Променено от',
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
                    width: "10%",
                    "render": function (data, type, row) {
                        return `<a href="/Register/PreviewStatus?registerStatusId=${data}" data-tooltip="Преглед" class="ui tertiary icon button">
                                <i class="info circle icon"></i>
                           </a>`;
                    }
                }
            ]
        });
    }
}