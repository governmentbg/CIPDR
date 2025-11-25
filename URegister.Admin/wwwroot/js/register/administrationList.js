$(function () {
    LoadAdministrations();
});

function LoadAdministrations() {
    const tableId = '#administrations';
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
                    width: "10%",
                    "render": function (data, type, row) {
                        return editButton(`/Register/EditAdministration?registerAdministrationId=${row.id}&registerId=${registerId}`) +
                            `<a href = "/Register/indexPerson?registerAdministrationId=${row.id}&registerId=${registerId}" data-tooltip="Оторизирани лица" class="ui tertiary icon button" >
                                   <i class="tasks icon"></i>
                                </a>` +
                            `<a href="/Register/OpenDataAdministration?administrationId=${row.administrationId}&&registerId=${registerId}" data-tooltip="OpenData" class="ui tertiary icon button">
                                 <i class="upload icon"></i>
                            </a>` +
                            "<a href='javascript:actionWithConfirmation(\"/Register/DeleteRegisterAdministration\", \"" +
                            data + "\", \"Сигурни ли сте, че искате да изтриете " +
                            row.name +
                            "?\", null)' type='button' class='ui tertiary icon button' data-tooltip='Изтрий'><i class='red trash alternate icon'></i></button>";
                    }
                }
            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}