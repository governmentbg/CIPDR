$(function () {
    LoadRoles();

    $('#rolesModal .cancel').on('click', function () {
        $('#rolesModal').modal('hide');
    });

    $('#confirmDeleteRolesModal .cancel').on('click', function () {
        $('#confirmDeleteRolesModal').modal('hide');
    });

    $('.confirm-delete').on('click', function () {
        var roleName = $('#deleteRoleName').html();
        var roleId = $('#deleteRoleId').val();
        var deleteUrl = $(this).data('delete-url');
        $.ajax({
            url: deleteUrl,
            method: 'POST',
            data: {
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                roleName: roleName,
                roleId: roleId
            },
            success: function (response) {
                showToast("success", response);
                $('#confirmDeleteRolesModal').modal('hide');
                LoadRoles();
            },
            error: function (xhr) {
                let message = xhr.responseText || 'Възникна грешка.';
                showToast("error", message);
            }
        })
    })

    $('#addUpdateRoleForm').on('submit', function (e) {
        e.preventDefault();

        var roleName = $('#roleName').val();
        var roleId = $('#roleId').val();
        var addUpdateRoleUrl = $('#rolesTable').data('update-url');
        if (roleId == '') {
            addUpdateRoleUrl = $('#rolesTable').data('add-url');
        }
        
        $.ajax({
            url: addUpdateRoleUrl, 
            method: 'POST',
            data: {
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                roleName: roleName,
                roleId: roleId
            },
            success: function (response) {
                showToast("success", response);
                $('#rolesModal').modal('hide');
                LoadRoles();
            },
            error: function (xhr) {
                let message = xhr.responseText || 'Възникна грешка.';
                showToast("error", message);
            }
        });
    });
});

function LoadRoles() {
    const tableId = '#rolesTable';
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
                d.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val();
                return d;
            },
            error: function (xhr, status, error) {
                messageHelper.ShowErrorMessage(xhr.responseText);
            }
        },
        columns: [
            {
                name: 'label',
                data: 'label',
                title: 'Роля',
                sortable: true,
                searchable: true
            },
            {
                name: '',
                data: '',
                title: 'Действия',
                sortable: false,
                searchable: false,
                width: '15%',
                class:'center aligned',
                render: function (data, type, row, metadata) {
                    if (row.name == row.roleId)
                    {
                        return `<a href="javascript:void(0)" type='button' class='ui tertiary icon button' onclick="editRole('${row.roleId}', '${row.label}')" data-tooltip='Редакция на име'><i class="cog icon"></i></a>
                                <a href="javascript:void(0)" type='button' class='ui tertiary icon button' onclick="deleteRole('${row.roleId}', '${row.label}')" data-tooltip='Изтриване на роля'><i class="red trash alternate icon"></i></a>`
                    }
                    return "";
                }
            }
        ],
        createdRow: function (row, data, dataIndex) {
            $(row).css('height', '55px');
        }
    });

    dt.ready(function () {
        SetAddButtonWithTitle("openRolesModal", "Създай роля", "openRolesModal();", '#rolesTable_wrapper');
    });
}

function openRolesModal() {
    $('#roleName').val('');
    $('#roleId').val('');
    $('.confirm').html('<i class="icon plus"></i> Създай');
    $('.updateCreateHeader').html('Създаване на роля');
    $('#rolesModal').modal('show');
}

function editRole(roleId, roleLabel) {
    $('#rolesModal #roleName').val(roleLabel);
    $('#rolesModal #roleId').val(roleId);
    $('.confirm').html('<i class="icon pencil"></i> Редактирай');
    $('.updateCreateHeader').html('Редактирай роля');
    $('#rolesModal').modal('show');
}

function deleteRole(roleId, roleLabel) {
    $('#confirmDeleteRolesModal #deleteRoleName').html(roleLabel);
    $('#confirmDeleteRolesModal #deleteRoleId').val(roleId);
    $('#confirmDeleteRolesModal').modal('show');
}