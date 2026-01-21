$(function () {
    LoadAdministrations();
});

function LoadAdministrations() {
    const tableId = '#administrations';
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
               },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: true,
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
                    title: 'Наименование на административния орган',
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
                        return `<a href="/Admin/Register/OpenDataAdministration?administrationId=${row.administrationId}" data-tooltip="OpenData" class="ui tertiary icon button">
                                   <i class="upload icon"></i>
                               </a>` +
                            `<a href="/Admin/Register/indexPerson?administrationId=${row.id}" data-tooltip="Оторизирани лица" class="ui tertiary icon button">
                                   <i class="tasks icon"></i>
                                </a>`;
                        //editButton(`/Admin/Register/EditAdministration?administrationId=${row.id}`)+

                    }
                }
            ]
        });

        dt.ready(function () {
           // SetAddButtonWithTitle("addAdministration", "Обновяване", "addAdministration();", '#administrationsContainer');
        });
    }
}

async function addAdministration() {
    await post_fetch_string_async('/Admin/Register/AddAdministration', { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() });
    LoadAdministrations()
}