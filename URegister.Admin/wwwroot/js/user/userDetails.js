$(function () {
    $('.menu .item').tab({
        onVisible: function (tabPath) {
            if (tabPath === 'userRoles') {
                loadUserRoles();
            }
            else if (tabPath === 'userAdministrations') {
                loadUserAdministrations();
            }
        }
    });

    $('#rolesModal').modal({
        closable: false
    });

    $('#cancelRolesButton').on('click', function () {
        $('#rolesModal').modal('hide');
    });

    $('#administrationModal').modal({
        closable: false
    });

    $('#cancelAdministrationsButton').on('click', function () {
        $('#administrationModal').modal('hide');
    });

    $('#submitAddRoleButton').on('click', function () {

        let roleValue = $('#rolesDropdown').dropdown('get value');
        let regValue = $('#registriesDropdown').dropdown('get value');
        let userId = $("#userRoles").data('userid');

        post_async('/User/UpdateUserRoles', {
            userId: userId,
            roleIds: roleValue,
            registerCode: regValue,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        })
            .then((result) => {
                loadUserRoles();
                showToast("success", result.message);
                $('#rolesModal').modal('hide');
            })
            .catch((result) => {
                loadUserRoles();
                showToast("error", result.responseJSON?.message);
            });
    });

    $('#submitAddAdministrationsButton').on('click', function () {
        let administrationValue = $('#administrationsDropdown').dropdown('get value');
        let userId = $('#Id').val();

        post_async('/User/AddUserAdministrations', {
            userId: userId,
            administrationIds: administrationValue,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        })
            .then((result) => {
                loadUserAdministrations();
                showToast("success", result.message);
                $('#administrationModal').modal('hide');
            })
            .catch((result) => {
                loadUserAdministrations();
                showToast("error", result.responseJSON?.message);
            });
    });
});

function openRolesModal() {
    $('#rolesModal').modal('show');
    $.ajax({
        url: '/User/GetAllRoles',
        type: 'GET',
        dataType: 'json',
        data: {
            administrationId: $('#AdministrationId').val(),
            userId: $('#Id').val(),
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (result) {
            populateRolesDropdown(result);
        },
        error: function (xhr, status, error) {
            showToast("error", xhr.responseText)
            console.error('Error fetching roles:', xhr.responseText || error);
        }
    });
}

function openAdministrationsModal() {
    $('#administrationModal').modal('show');
    $.ajax({
        url: '/User/GetAdministrationsForAssign',
        type: 'GET',
        dataType: 'json',
        data: {
            userId: $('#Id').val(),
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (result) {
            populateAdministrationsDropdown(result);
        },
        error: function (xhr, status, error) {
            console.error('Error fetching administrations:', xhr.responseText || error);
        }
    });
}

function populateRolesDropdown(result) {
    let dropdown = $('#rolesDropdown');
    let registriesDropDown = $('#registriesDropdown');

    dropdown.empty();
    registriesDropDown.empty();
    dropdown.dropdown('destroy').dropdown({
        placeholder: 'Избери роля'
    });
    registriesDropDown.dropdown('destroy').dropdown({
        placeholder: 'Избери регистър'
    });

    result.roles.forEach(function (role) {
        dropdown.append(`<option value="${role.roleId}">${role.label}</option>`);
    });
    result.registries.forEach(function (reg) {
        registriesDropDown.append(`<option value="${reg.code}">${reg.name} (${reg.code})</option>`);
    });
    dropdown.dropdown('clear');
    registriesDropDown.dropdown('clear');
    dropdown.dropdown('refresh');
    registriesDropDown.dropdown('refresh');
}

function loadUserRoles() {
    const tableId = '#userRoles';
    let userID = $(tableId).data('userid');
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else {
        let url = $(tableId).data('url');
        let dtRoles = $(tableId).DataTable({
            filter: false,
            ajax: {
                "url": url,
                "type": "POST",
                "datatype": "json",
                data: function (d) {
                    d.userId = userID;
                    d.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val()
                },
                error: function (xhr, status, error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + xhr.responseText);
                }
            },
            columns: [
                {
                    name: 'label',
                    data: 'label',
                    title: 'Име на роля',
                    sortable: true,
                    searchable: false
                },
                {
                    name: 'registerCode',
                    data: 'registerCode',
                    title: 'Код на регистър',
                    sortable: true,
                    searchable: false
                },
                {
                    name: 'registerName',
                    data: 'registerName',
                    title: 'Име на регистър',
                    sortable: true,
                    searchable: false
                },
                {
                    title: 'Действия',
                    sortable: false,
                    searchable: false,
                    className: "dt-center",
                    render: function (data, type, row, meta) {
                        return `<a data-roleId=${row.roleId} data-code="${row.registerCode}" data-role-name="&quot;${row.label}&quot;" onclick="confirmUnassignRole(event)" 
                                                  type="button" 
                                                  class="ui tertiary icon button" 
                                                  data-tooltip="Прекратяване">
                                                  <i class="red trash alternate icon"></i>
                                              </a>`;
                    }
                }
            ]
        });

        dtRoles.ready(function () {
            SetAddButtonWithTitle("openRolesModal", "Добави роля", "openRolesModal();", '#userRoles_wrapper');
        });
    }
}

function loadUserAdministrations() {
    const tableId = '#userAdministrations';
    let userID = $(tableId).data('userid');
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else {
        let url = $(tableId).data('url');
        let dtAdministrations = $(tableId).DataTable({
            ajax: {
                "url": url,
                "type": "POST",
                "datatype": "json",
                data: function (d) {
                    d.userId = userID;
                    d.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val()
                },
                dataSrc: function (json) {
                    return json.data;
                },
                error: function (xhr, status, error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + xhr.responseText);
                }
            },
            columns: [
                {
                    name: 'name',
                    data: 'name',
                    title: 'Име',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'uic',
                    data: 'uic',
                    title: 'Булстат',
                    sortable: true,
                    searchable: true
                },
                {
                    title: 'Действия',
                    sortable: false,
                    searchable: false,
                    className: "dt-center",
                    render: function (data, type, row, meta) {
                        return `<a data-administrationId=${row.id} data-administration-name="${row.name}" onclick="confirmRemoveFromAdministration(event)" 
                                                  type="button" 
                                                  class="ui tertiary icon button" 
                                                  data-tooltip="Прекратяване">
                                                  <i class="red trash alternate icon"></i>
                                              </a>`;
                    }
                }
            ]
        });

        dtAdministrations.ready(function () {
            SetAddButtonWithTitle("openAdministrationsModal", "Добави администрация", "openAdministrationsModal();", '#userAdministrations_wrapper');
        });
    }
}

function populateAdministrationsDropdown(result) {
    let dropdown = $('#administrationsDropdown');

    dropdown.empty();
    dropdown.dropdown('destroy').dropdown({
        placeholder: 'Избери администрация'
    });

    result.forEach(function (administration) {
        if (administration.uic == '000000000') {//Всички администрации
            return;
        }
        dropdown.append(`<option value="${administration.id}">${administration.name}(${administration.uic})</option>`);
    });

    dropdown.dropdown('clear');
    dropdown.dropdown('refresh');
}

function confirmUnassignRole(event) {
    let roleName = $(event.currentTarget).data('role-name');
    $('#confirmActionText').text(`Сигурни ли сте, че искате да премахнете ролята ${roleName}?`);
    let userID = $("#userRoles").data('userid');
    let roleID = $(event.currentTarget).data('roleid');
    let code = $(event.currentTarget).data('code');

    $('.confirm-action')
        .modal({
            centered: true,
            closable: false,
            onApprove: function () {
                let url = "/User/UnassignRole";
                post_async(url, {
                    userId: userID,
                    roleId: roleID,
                    registerCode: code,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                )
                    .then((result) => {
                        if (result.message) {
                            loadUserRoles()
                            showToast("success", result.message);
                        }
                    })
                    .catch((error) => {
                        console.error('Грешка: ' + error);
                    });
            }
        })
        .modal('show');
};

function confirmRemoveFromAdministration(event) {
    let administrationName = $(event.currentTarget).data('administration-name');
    $('#confirmActionText').text(`Сигурни ли сте, че искате да премахнете администрация ${administrationName}?`);
    let userID = $('#Id').val();
    let administrationID = $(event.currentTarget).data('administrationid');

    $('.confirm-action')
        .modal({
            centered: true,
            closable: false,
            onApprove: function () {
                let url = "/User/RemoveAdministration";
                post_async(url, {
                    userId: userID,
                    administrationId: administrationID,
                    administrationName: administrationName,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                )
                    .then((result) => {
                        if (result.message) {
                            loadUserAdministrations()
                            showToast("success", result.message);
                        }
                    })
                    .catch((error) => {
                        console.error('Грешка: ' + error);
                    });
            }
        })
        .modal('show');
};