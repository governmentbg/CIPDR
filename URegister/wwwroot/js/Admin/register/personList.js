$(function () {
    LoadPersons();
});

function LoadPersons() {
    const tableId = '#persons';
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
                        administrationId: $('#AdministrationId').val(),
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
                    title: 'Вид',
                    sortable: false,
                    searchable: false
                },
                {
                    name: 'position',
                    data: 'position',
                    title: 'Длъжност',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'firstName',
                    data: 'firstName',
                    title: 'Име',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'middleName',
                    data: 'middleName',
                    title: 'Презиме',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'lastName',
                    data: 'lastName',
                    title: 'Фамилия',
                    sortable: true,
                    searchable: true
                },
            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}