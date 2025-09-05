$(function () {
    $('#mainForm').on('submit', function (event) {
                
        if ($('#userRoles').DataTable().page.info().recordsTotal > 0) {        
            return true;
        } else {            
            event.preventDefault();
            $('#errorMessage')
                .removeClass('hidden')
                .addClass('negative');
            return false;
        }
    });

    $('#errorMessage .close').on('click', function () {
        $('#errorMessage').addClass('hidden');
    });

    $('.menu .item').tab();

    loadUserRoles();

    $('#rolesModal').modal({
        closable: false
    });

    $('#cancelRolesButton').on('click', function () {
        $('#rolesModal').modal('hide');
    });

    $('#submitAddRoleButton').on('click', function () {

        let roleValue = $('#rolesDropdown').dropdown('get value');        
        let userId = $("#userRoles").data('userid');

        post_async('/Admin/Admin/UpdateUserRoles', {
            userId: userId,
            roleIds: roleValue,            
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
});
function openRolesModal() {
    $('#rolesModal').modal('show');
    $.ajax({
        url: '/Admin/Admin/GetAllRoles',
        type: 'GET',
        dataType: 'json',
        data: {
            administrationId: $('#AdministrationId').val(),
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (result) {
            populateRolesDropdown(result);
        },
        error: function (xhr, status, error) {
            console.error('Error fetching roles:', xhr.responseText || error);
        }
    });
}
function populateRolesDropdown(result) {
    let dropdown = $('#rolesDropdown');    
    dropdown.empty();    
    dropdown.dropdown('destroy').dropdown({
        placeholder: 'Избери роля'
    });    
    result.roles.forEach(function (role) {
        dropdown.append(`<option value="${role.roleId}">${role.label}</option>`);
    });    
    dropdown.dropdown('clear');
    dropdown.dropdown('refresh');
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
                    sortable: true,
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
                let url = "/Admin/Admin/UnassignRole";
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