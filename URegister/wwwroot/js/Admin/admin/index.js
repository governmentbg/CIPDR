$(function () {
    LoadUsers();
});

function LoadUsers() {
    const tableId = '#usersTable';

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
                d.registerCOde = $('#RegisterCode').val();
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
                    return `<a href="/Admin/Admin/UserDetails?userId=${row.id}" type='button' class='ui tertiary icon button' data-tooltip='Профил'><i class="users cog icon"></i></a>`;
                }
            }
        ]
    });

    dt.ready(function () {
        SetAddButton($(tableId).data('add-url'));
    });
}