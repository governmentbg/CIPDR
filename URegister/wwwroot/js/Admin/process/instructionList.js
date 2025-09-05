$(function () {
    LoadInstructions();
});

function LoadInstructions() {
    const tableId = '#instructions';
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
                    name: 'instructionDate',
                    data: 'instructionDate',
                    title: 'От дата',
                    sortable: true,
                    searchable: false,
                    "render": function (data) {
                        return JsonBGdate(data);
                    }
                },
                {
                    name: 'userName',
                    data: 'userName',
                    title: 'Дадено от',
                    sortable: true,
                    searchable: true,
                    type: 'string'
                },
                {
                    name: 'content',
                    data: 'content',
                    title: 'Указание',
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
                        let result = '';
                        if (row.hasResponse || row.canAdd) {
                            result += `<a href="/Admin/Process/InstructionResponseIndex?instructionId=${data}" data-tooltip="Отговор" class="ui tertiary icon button">
                                <i class="hand point left outline icon"></i>
                           </a>`;
                        }
                        return result;
                    }
                }
            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}
