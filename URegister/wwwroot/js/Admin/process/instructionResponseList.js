$(function () {
    LoadInstructions();
});

function LoadInstructions() {
    const tableId = '#instructionResponses';
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
                        id: $('#Id').val(),
                    }
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: false,
            columns: [
                {
                    name: 'modifiedOn',
                    data: 'modifiedOn',
                    title: 'От дата',
                    sortable: true,
                    searchable: false,
                    "render": function (data) {
                        return JsonBGdate(data);
                    }
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
                        if (row.canEdit) {
                            result += `<a href="/Admin/Process/InstructionResponseEdit?id=${data}" data-tooltip="Редакция на изпълнение на указание" class="ui tertiary icon button">
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
