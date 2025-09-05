$(function () {
    let administrationId = $('#administrationDropdown').dropdown('get value');
    let administrationName = $('#administrationDropdown').dropdown('get text');
    $('#administrationDropdown').dropdown({
        onChange: function (value) {
            administrationId = $('#administrationDropdown').dropdown('get value');
            administrationName = $('#administrationDropdown').dropdown('get text');
            LoadUsers(administrationId, administrationName);
        }
    });

    LoadUsers(administrationId, administrationName);
});

function openConfirmationModal() {
    let administrationId = $('#administrationDropdown').dropdown('get value');
    $('.ui.tiny.modal.confirm-modal')
        .modal({
            closable: false,
        }).modal('show');
    var url = $('#usersTable').data('add-url') + "?" + administrationId;
    $('.ui.primary.button.confirm').attr('href', url);

    $('.ui.tiny.modal.confirm-modal .cancel').on('click', function () {
        $('.ui.tiny.modal.confirm-modal').modal('hide');
    });
}

function openAdminRoleConfirmationModal(userId, url, isAssign) {
    let atagValue = '<i class="icon plus"></i>Добави роля'
    let confirmMsg = "Сигурни ли сте, че искате да добавите роля 'Администратор МЕУ' на потребител?";
    if (!isAssign) {
        atagValue = '<i class="icon minus"></i>Отписване от роля'
        confirmMsg = "Сигурни ли сте, че искате да премахнете роля 'Администратор МЕУ' на потребител?";
    }
    $('.admin-role-confirm.confirm').html(atagValue);
    $('#admin-role-msg').text(confirmMsg);

    $('.ui.tiny.modal.confirm-admin-role-modal')
        .modal({
            closable: false,
        }).modal('show');

    $('.ui.tiny.modal.confirm-admin-role-modal .confirm').off('click').on('click', function () {
        assignUnassignAdminRole(userId, url)
        $('.ui.tiny.modal.confirm-admin-role-modal').modal('hide');
    });

    $('.ui.tiny.modal.confirm-admin-role-modal .cancel').on('click', function () {
        $('.ui.tiny.modal.confirm-admin-role-modal').modal('hide');
    });
}

function assignUnassignAdminRole(userId, url) {
    $.ajax({
        url: url,
        type: "POST",
        data: {
            userId: userId,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            showToast("success", response.message);
            $('#usersTable').DataTable().ajax.reload(null, false);
        },
        error: function (xhr, status, error) {
            console.error("Error:", error);
        }
    });
}

function LoadUsers(administrationId, administrationName) {
    const tableId = '#usersTable';
    if (administrationName === "Всички администрации") {
        administrationId = null;
    }
    if ($.fn.dataTable.isDataTable(tableId)) {
        $(tableId).DataTable().destroy();
    }

    let url = $(tableId).data('url');
    let dt = $(tableId).DataTable({
        ajax: {
            "url": url,
            "type": "POST",
            "datatype": "json",
            data: function (d) {
                d.administrationId = administrationId;
                d.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val();
                return d;
            },
            error: function (xhr, status, error) {
                messageHelper.ShowErrorMessage(xhr.responseText);
            }
        },
        columns: [
            {
                name: 'firstName',
                data: 'firstName',
                title: 'Име',
                sortable: true,
                searchable: true,
                render: function (data, type, row, metadata) {
                    if (row.enabled.toString().toLowerCase() === 'false') {
                        return `<div data-tooltip='Неактивен'><i class='icon red attention'></i>${data}</div>`
                    }
                    return data;
                }
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
                name: 'roleName',
                data: 'roleName',
                title: 'Роля',
                sortable: false,
                searchable: true,
                render: function (data, type, row, metadata) {
                    return data.replace(/\(R00000\)/g, "");
                }
            },
            {
                name: 'email',
                data: 'email',
                title: 'Email',
                sortable: true,
                searchable: true
            },
            {
                name: 'enabled',
                data: 'enabled',
                title: 'Активен',
                width: '5%',
                className: 'center aligned',
                sortable: true,
                searchable: false,
                render: function (data) {
                    return data ? 'Да' : 'Не';
                }
            },
            {
                name: '',
                data: '',
                title: 'Действия',
                sortable: false,
                searchable: false,
                width: '5%',
                render: function (data, type, row, metadata) {
                    var btns = `<a href="/User/UserDetails?userId=${row.id}" type='button' class='ui tertiary icon button' data-tooltip='Профил'><i class="users cog icon"></i></a>`
                    if (row.roleName.includes("Администратор МЕУ")) {
                        btns += `<a href="javascript:void(0);" type="button" class="ui tertiary icon button"
                                   onclick="openAdminRoleConfirmationModal('${row.id}', '/User/UnassignGlobalAdminRole', ${false})" 
                                   data-tooltip='Отпиши от роля "Администратор МЕУ"'>
                                   <i class="user times icon"></i>
                                </a>`;
                    } else {
                        btns += `<a href="javascript:void(0);" type="button" class="ui tertiary icon button"
                                   onclick="openAdminRoleConfirmationModal('${row.id}', '/User/AssignGlobalAdminRole', ${true})" 
                                   data-tooltip='Добави роля "Администратор МЕУ"'>
                                   <i class="user ninja icon"></i>
                                </a>`;

                    }
                    return btns;
                }
            }
        ]
    });

    dt.ready(function () {
        if (administrationName === "Всички администрации") {
            SetAddButtonWithTitle("openConfirmationModal", "Добави администратор", "openConfirmationModal();", '#usersTable_wrapper');
        } else {
            SetAddButton($(tableId).data('add-url') + "?administrationId=" + $('#administrationDropdown').dropdown('get value'));
        }
    });
}