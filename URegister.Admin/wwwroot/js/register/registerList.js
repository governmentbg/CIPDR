$(function () {
    LoadRegisters();
});

function LoadRegisters() {
    const tableId = '#registers';
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
                    name: 'code',
                    data: 'code',
                    title: 'Код',
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
                    name: 'description',
                    data: 'description',
                    title: 'Описание',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'type',
                    data: 'type',
                    title: 'Вид',
                    sortable: false,
                    searchable: false
                },
                {
                    name: 'entryType',
                    data: 'entryType',
                    title: 'Начин на вписване',
                    sortable: false,
                    searchable: false
                },
                {
                    name: 'identitySecurityLevel',
                    data: 'identitySecurityLevel',
                    title: 'Ниво на осигуреност',
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
                    "render": function (data, type, row) {
                        return `<a href="/Register/indexAdministration?registerId=${row.id}" data-tooltip="Администрации" class="ui tertiary icon button">
                                  <i class="tasks icon"></i>
                               </a>`;  //+ 
                              //`<a href="/Nomenclature/IndexTypeRegister?registerId=${row.id}" data-tooltip="Номенклатури за регистър" class="ui tertiary icon button">
                              //    <i class="object ungroup outline icon"></i>
                              //</a>`
                    }
                }
            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}