$(function () {
    LoadNomenclatureTypes();
});

function LoadNomenclatureTypes() {
    const tableId = '#nomenclatureTypes';
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
                        Type: $('#Type').val(),
                        Name: $('#Name').val(),
                    }
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: false,
            columns: [
                {
                    name: 'type',
                    data: 'type',
                    title: 'Тип',
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
                    name: 'actions',
                    data: "id",
                    title: "Действия",
                    sortable: false,
                    searchable: false,
                    className: "text-left noExport",
                    width: 120,
                    "render": function (data, type, row) {
                        return editButton(`/Nomenclature/EditNomenclatureType/${data}`) +
                           `<a href="/Nomenclature/index?nomenclatureType=${row.type}" data-tooltip="Стойности" class="ui tertiary icon button">
                                <i class="tasks icon"></i>
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