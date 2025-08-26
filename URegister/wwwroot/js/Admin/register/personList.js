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
                {
                    name: 'email',
                    data: 'email',
                    title: 'Електронна поща',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'phone',
                    data: 'phone',
                    title: 'Телефон',
                    sortable: true,
                    searchable: true
                },
                //{
                //    name: 'actions',
                //    data: "id",
                //    title: "Действия",
                //    sortable: false,
                //    searchable: false,
                //    className: "text-left noExport",
                //    width: 100,
                //    "render": function (data, type, row) {
                //        return editButton(`/Admin/Register/EditPerson?personId=${row.id}`) +
                //            `<a href="javascript:deletePerson(${data}, '${row.firstName} ${row.middleName} ${row.lastName}')" 
                //                                  type="button" 
                //                                  class="ui tertiary icon button" 
                //                                  data-tooltip="Изтриване">
                //                                  <i class="times right icon"></i>
                //                              </a>`;
                //    }
                //}
            ]
        });

        dt.ready(function () {
           // SetAddButton($(tableId).data('add-url'));
        });
    }
}

async function deletePerson(id,  name) {
    const result = await confirmDialog('Потвърдете изтриване на оторизирано лице', name, 'Изтриване');
    if (result) {
        await post_fetch_string_async(`/Admin/Register/DeletePerson?personid=${id}`, {});
        LoadPersons();
    }
}
