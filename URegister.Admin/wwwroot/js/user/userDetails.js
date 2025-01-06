$(function () {
    $('.menu .item').tab();
    loadUserRoles();
    loadUserGroups()
    
    $('#rolesModal').modal({
        closable: false
    });
    $('#groupsModal').modal({
        closable: false
    });
    $('#cancelRolesButton').on('click', function () {
        $('#rolesModal').modal('hide');
    });

    $('#cancelGroupButton').on('click', function () {
        $('#groupsModal').modal('hide');
    });

    $('#submitAddRoleButton').on('click', function () {
        let selectedRoles = [];
        $('#rolesMultiselect').dropdown('get values').forEach(function (roleId) {
            let roleText = $('#rolesMultiselect option[value="' + roleId + '"]').text();
            selectedRoles.push({ id: roleId, name: roleText });
        });

        let userId = $("#userRoles").data('userid');
        $.ajax({
            url: '/User/UpdateUserRoles',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ userId: userId, roles: selectedRoles }),
            success: function (response) {
                loadUserRoles();
                showToast("success", response.message);
                $('#rolesModal').modal('hide');
            },
            error: function (xhr, status, error) {
                showToast("error", xhr.responseText);
            }
        });

        $('#rolesModal').modal('hide');
        loadUserRoles();
    });

    $('#submitAddGroupButton').on('click', function () {
        let selectedGroups = [];
        $('#groupsMultiselect').dropdown('get values').forEach(function (groupId) {
        
            selectedGroups.push(groupId);
        });

        let userId = $("#userGroups").data('userid');
        $.ajax({
            url: '/User/UpdateUserGroups',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ userId: userId, groups: selectedGroups }),
            success: function (response) {
                loadUserGroups();
                showToast("success", response.message);
                $('#groupsModal').modal('hide');
            },
            error: function (xhr, status, error) {
                showToast("error", xhr.responseText);
            }
        });

        $('#groupsModal').modal('hide');
        loadUserGroups();
    });

});

function openRolesModal() {
    $('#rolesModal').modal('show');
    let userID = $("#userRoles").data('userid');
    $.ajax({
        url: '/User/GetUserAvailableRoles',
        type: 'GET',
        dataType: 'json',
        data: { userId: userID },
        success: function (roles) {
            populateMultiselect(roles);
        },
        error: function (xhr, status, error) {
            console.error('Error fetching roles:', xhr.responseText || error);
        }
    });
}

function openGroupsModal() {
    $('#groupsModal').modal('show');
    let userID = $("#userGroups").data('userid');
    $.ajax({
        url: '/User/GetUserAvailableGroups',
        type: 'GET',
        dataType: 'json',
        data: { userId: userID },
        success: function (groups) {
            populateGroupsMultiselect(groups);
        },
        error: function (xhr, status, error) {
            console.error('Error fetching roles:', xhr.responseText || error);
        }
    });
}

function populateMultiselect(roles) {
    let multiselect = $('#rolesMultiselect');
    multiselect.empty();

    roles.forEach(function (role) {
        multiselect.append(`<option value="${role.id}">${role.name}</option>`);
    });
    multiselect.dropdown('clear');
    multiselect.dropdown('refresh');
}

function populateGroupsMultiselect(groups) {
    let multiselect = $('#groupsMultiselect');
    multiselect.empty();

    groups.forEach(function (group) {
        multiselect.append(`<option value="${group.id}">${group.name}</option>`);
    });
    multiselect.dropdown('clear');
    multiselect.dropdown('refresh');
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
                },
                error: function (xhr, status, error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + xhr.responseText);
                }
            },
            columns: [
                {
                    name: 'id',
                    data: 'id',
                    title: '#',
                    sortable: false,
                    searchable: false
                },
                {
                    name: 'name',
                    data: 'name',
                    title: 'Name',
                    sortable: true,
                    searchable: false
                },
                {
                    name: 'description',
                    data: 'description',
                    title: 'Описание',
                    sortable: true,
                    searchable: false
                },
                {
                    name: 'composite',
                    data: 'composite',
                    title: 'Композитна',
                    sortable: true,
                    searchable: false
                },
                {
                    name: 'clientRole',
                    data: 'clientRole',
                    title: 'Client Role',
                    sortable: true,
                    searchable: false
                },
                {
                    name: 'containerId',
                    data: 'containerId',
                    title: 'Container Id',
                    sortable: true,
                    searchable: false
                },
                {
                    name: '',
                    data: '',
                    title: 'Отписване',
                    sortable: true,
                    searchable: false,
                    class:"center aligned",
                    render: function (data, type, row, meta) {
                        return `<a data-id=${row.id} data-name="${row.name}" onclick="return confirmUnassign(event);" data-tooltip="Отпиши" class="ui tertiary button unassignRole">
                                  <i class="times circle red outline tasks icon"></i>
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

function loadUserGroups() {
    const tableId = '#userGroups';
    let userID = $(tableId).data('userid');
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else {
        let url = $(tableId).data('url');
        let dtGroups = $(tableId).DataTable({
            filter: false,
            ajax: {
                "url": url,
                "type": "POST",
                "datatype": "json",
                data: function (d) {
                    d.userId = userID;
                },
                error: function (xhr, status, error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            columns: [
                {
                    name: 'id',
                    data: 'id',
                    title: '#',
                    sortable: false,
                    searchable: false
                },
                {
                    name: 'name',
                    data: 'name',
                    title: 'Name',
                    sortable: true,
                    searchable: false
                },
                {
                    name: '',
                    data: '',
                    title: 'Отписване',
                    sortable: true,
                    searchable: false,
                    class: "center aligned",
                    render: function (data, type, row, meta) {
                        return `<a data-id=${row.id} onclick="return confirmUnassignGroup(event);" data-tooltip="Отпиши" class="ui tertiary button">
                                  <i class="times circle red outline tasks icon"></i>
                              </a>`;
                    }
                }
            ]
        });

        dtGroups.ready(function () {
            SetAddButtonWithTitle("openGroupsModal", "Добави група", "openGroupsModal();", '#userGroups_wrapper');
        });
    }
}

function confirmUnassign(event) {
    event.preventDefault();
    if (!confirm("Are you sure?")) {       
        return false;
    } else {
        let userID = $("#userRoles").data('userid');
        let roleID = $(event.currentTarget).data('id');
        let roleName = $(event.currentTarget).data('name');
        let selectedRoles = [];
        selectedRoles.push({ id: roleID, name: roleName });
        $.ajax({
            url: '/User/Unassign',
            type: 'DELETE',
            contentType: 'application/json',
            data: JSON.stringify({ userId: userID, roles: selectedRoles }),
            success: function (response) {
                loadUserRoles()
                showToast("success", response.message);
                return true;
            },
            error: function (xhr, status, error) {
                showToast("error", error);
                return false;
            }
        });
    }
}

function confirmUnassignGroup(event) {
    event.preventDefault();
    if (!confirm("Are you sure?")) {
        return false;
    } else {
        let userID = $("#userGroups").data('userid');
        let groupId = $(event.currentTarget).data('id');
        $.ajax({
            url: '/User/UnassignGroups',
            type: 'DELETE',
            contentType: 'application/json',
            data: JSON.stringify({ userId: userID, groupId: groupId }),
            success: function (response) {
                loadUserGroups()
                showToast("success", response.message);
                return true;
            },
            error: function (xhr, status, error) {
                showToast("error", error);
                return false;
            }
        });
    }
}

function beforeSubmit() {
    $('#GroupIds').val($('#GroupList').val().join(','));
}