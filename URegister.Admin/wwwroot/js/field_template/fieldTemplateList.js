$(function () {
    LoadServiceTypes();
});

function LoadServiceTypes() {
    debugger
    const tableId = '#field_template';
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
                    title: 'Наименование',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'fieldTypeName',
                    data: 'fieldTypeName',
                    title: 'Поле',
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
                        return editButton(`/FieldTemplate/Edit/${data}`) +
                            `<a href="/FieldTemplate/EditContent/${data}" data-tooltip="Бланка" class="ui tertiary icon button">
                               <i class="file alternate outline icon"></i>
                            </a>`+
                            "<a href='javascript:actionWithConfirmation(\"/FieldTemplate/DeleteFieldTemplate\", " +
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