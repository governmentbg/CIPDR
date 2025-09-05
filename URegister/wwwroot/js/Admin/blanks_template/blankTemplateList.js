$(function () {
    LoadServiceTypes();
});

function LoadServiceTypes() {
    debugger
    const tableId = '#blank_template';
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
                //{
                //    name: 'code',
                //    data: 'code',
                //    title: 'Код',
                //    sortable: true,
                //    searchable: true
                //},
                {
                    name: 'sourceTypeName',
                    data: 'sourceTypeName',
                    title: 'Тип бланка',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'serviceName',
                    data: 'serviceName',
                    title: 'Услуга',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'formName',
                    data: 'formName',
                    title: 'Форма',
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
                        return editButton(`/Admin/BlanksTemplate/Edit/${data}`) +
                            `<a href="/Admin/BlanksTemplate/EditContent/${data}" data-tooltip="Бланка" class="ui tertiary icon button">
                               <i class="file alternate outline icon"></i>
                            </a>`+
                            "<a href='javascript:actionWithConfirmation(\"/Admin/BlanksTemplate/DeleteTemplate\", " +
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