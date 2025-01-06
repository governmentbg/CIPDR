$(function () {
    LoadUsers();
    $('#groupModal').modal({
        closable: false
    });
        $('#openGroupModal').on('click', function () {
            $('#groupModal').modal('show');
    });

    $('#cancelButton').on('click', function () {
        $('#groupModal').modal('hide');
    });

    $('#submitButton').on('click', function () {
        const name = $('#nameInput').val();
        var urlGroup = $(this).data('url');
        if (name) {
            $.ajax({
                url: urlGroup,
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(name),
                success: function (response) {
                    showToast("success", response);
                    $('#groupModal').modal('hide'); 
                },
                error: function (data) {
                    showToast("error", data.responseText);
                }
            });
            $('#nameInput').val('');
        } else {
            alert('Моля попълнете полето за име.');
        }
    });

});

function LoadUsers() {
    const tableId = '#usersTable';
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else {
        let url = $(tableId).data('url');
        let dt = $(tableId).DataTable({
            filter: false,
            ajax: {
                "url": url,
                "type": "POST",
                "datatype": "json",
                data: { "__RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
                error: function (xhr, status, error) {
                    messageHelper.ShowErrorMessage(xhr.responseText);
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
                    name: 'username',
                    data: 'username',
                    title: 'Username',
                    sortable: true,
                    searchable: false,
                    render: function (data, type, row, metadata) {
                        var ahref = '<a href="/User/UserDetails?userId=' + row.id + '">' +
                            data + '</a>'
                               
                        if (row.enabled.toString().toLowerCase() === 'false') {
                            return "<i class='icon red attention' title='Неактивен'></i>" + ahref;
                        }
                        return ahref;
                    }
                },
                {
                    name: 'firstName',
                    data: 'firstName',
                    title: 'Име',
                    sortable: true,
                    searchable: false
                },
                {
                    name: 'lastName',
                    data: 'lastName',
                    title: 'Фамилия',
                    sortable: true,
                    searchable: false
                },
                {
                    name: 'email',
                    data: 'email',
                    title: 'Email',
                    sortable: true,
                    searchable: false,
                    render: function (data, type, row, metadata) {
                        if (row.emailVerified.toString().toLowerCase() === 'false') {
                            return "<i class='icon icon red attention' title='Email верификация'></i>" + data;
                        }
                        return data
                    }
                },
                {
                    name: 'attributes',
                    data: 'attributes',
                    title: 'Администрация',
                    sortable: true,
                    searchable: false,
                    render: function (data)
                    {
                        if (data != null) {
                            return data.administrationID
                        }
                        return "";
                    }
                },
                {
                    name: 'requiredActions',
                    data: 'requiredActions',
                    title: 'Желани действия',
                    sortable: true,
                    searchable: false,
                    render: function (data) {
                        if (data != null) {
                            return data
                        }
                        return "";
                    }
                },
                {
                    name: 'createdTimestamp',
                    data: 'createdTimestamp',
                    title: 'Дата на създаване',
                    sortable: true,
                    searchable: false,
                    render: function (data) {
                        var date = new Date(data);
                        return jsonBGdatetime(date);
                    }
                }
            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}