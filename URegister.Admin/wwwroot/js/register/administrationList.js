$(function () {
    LoadAdministrations();
});

function LoadAdministrations() {
    const tableId = '#administrations';
    const registerId = $('#RegisterId').val();
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
            filter: false,
            columns: [
                {
                    name: 'uic',
                    data: 'uic',
                    title: 'ЕИК/БУЛСТАТ',
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
                    name: 'legalBasis',
                    data: 'legalBasis',
                    title: 'Правно основание',
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
                        return `<a href="/Register/indexPerson?administrationId=${row.id}&registerId=${registerId}" data-tooltip="Оторизирани лица" class="ui tertiary icon button">
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