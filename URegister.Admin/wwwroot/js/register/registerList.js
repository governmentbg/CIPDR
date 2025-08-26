$(function () {
    LoadRegisters();
});

function LoadRegisters() {
    const tableId = '#registers';
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else {
        openAccordionFilter();
        let url = $(tableId).data('url');
        let dt = $(tableId).DataTable({
            'order': [[0, 'asc']],
            ajax: {
                "url": url,
                "type": "POST",
                "datatype": "json",
                data: function (d) {
                    d.filter = {
                        Code: $('#Code').val(),
                        Name: $('#Name').val(),
                        Description: $('#Description').val(),
                        DateFrom: $('#DateFrom').val(),
                        DateTo: $('#DateTo').val(),
                        AdministrationId: $('#AdministrationId').val(),
                        Type: $('#Type').val(),
                        IdentitySecurityLevel: $('#IdentitySecurityLevel').val(),
                        TypeEntry: $('#TypeEntry').val(),
                        StatusId: $('#StatusId').val(),
                        IsActive: $("#IsActive").prop("checked"),
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
                    name: 'status',
                    data: 'status',
                    title: 'Статус',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'baseAddress',
                    data: 'baseAddress',
                    title: 'Базов адрес',
                    sortable: true,
                    searchable: true,
                    "render": function (data, type, row) {
                        if (row.deployed) {
                            return `<a href="${data}" target="_blank" data-tooltip="Базов адрес" >${data}</a>`
                        } else {
                            return data
                        }
                    }
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
                        return editButton(`/Register/Edit/${data}`) +`<a href="/Register/indexAdministration?registerId=${row.id}" data-tooltip="Администрации" class="ui tertiary icon button">
                                  <i class="tasks icon"></i>
                               </a>` +
                               `<a href="/Register/EditStatus?registerId=${row.id}" data-tooltip="Статус" class="ui tertiary icon button">
                                   <i class="forward icon"></i>
                               </a>`+
                               `<a href="/Register/IndexStatus?registerId=${row.id}" data-tooltip="История на статуси" class="ui tertiary icon button">
                                   <i class="history icon"></i>
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

async function showRegisterBaseAddress(id) {
    const view = await post_fetch_string_async(`/Register/GetBaseAddress/${id}`, {});
    $(view).modal({
        centered: true,
        closable: false,
    }).modal('show');
}

async function updateRegisterBaseAddress() {
    await post_fetch_form_async('', '#formUpdateRegisterBaseAddress'); 
    LoadRegisters();
}
