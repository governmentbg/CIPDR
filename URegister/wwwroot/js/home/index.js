$(function () {
    loadAdministrationRegistries();
    $('.ui.multiple.progress').progress({
        text: {
            percent: '{bar} {value}',
            bars: ['Подадени', 'Обработка', 'Вписани', 'Отказани', 'Издадено удостоверение']
        },
        showActivity: false
    });

    $('#usersDashboardInfo').progress({
        text: {
            percent: '{bar} {value}',
            bars: ['Активни', 'Неактивни']
        },
        showActivity: false
    });

    $('#formsDashboardInfo').progress({
        text: {
            percent: '{bar} {value}',
            bars: ['Одобрени', 'Чакащи одобрение']
        },
        showActivity: false
    });

    $(".basic.card").on('click', function () {
        $(".card").removeClass('black');
        $(".card").addClass('basic');
        $(this).removeClass("basic");
        $(this).addClass("black");
        $('.ui.multiple.progress').removeClass('active');
        $(this).find('.ui.multiple.progress').addClass('active');
    })

    $(".basic.card").hover(
        function () {
            $(this).removeClass("basic");
        },
        function () {
            if ($(this).hasClass("black") == false) {
                $(this).addClass("basic");

            }
        }
    );

    $('#toggleRegisters').on('click', function () {
        const container = $('#registersContainer');
        const icon = $(this).find('i');

        if (container.is(':visible')) {
            container.transition('slide up');
            icon.removeClass('caret up').addClass('caret down');
        } else {
            container.transition('slide down');
            icon.removeClass('caret down').addClass('caret up');
        }
    });
});

function loadProcesses(status, element) {
    $(".active-black").removeClass("active-black");
    $(element).addClass('active-black');
    $('#usersDashboard_wrapper').hide();
    $('#formsDashboard_wrapper').hide();
    const tableId = '#processesDashboard';
    if ($.fn.dataTable.isDataTable(tableId)) {
        $(tableId).DataTable().destroy();
        $(tableId).empty();
    }
    $(tableId).show();
    let url = $(tableId).data('url');

    var table = $(tableId).DataTable({
        stateSave: false,
        order: [[0, 'asc']],
        buttons: [],
        ajax: {
            "url": url,
            "type": "POST",
            "datatype": "json",
            data: function (d) {
                d.statusId = status,
                d.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val();
            },
            error: function (error) {
                messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
            }
        },
        columns: [
            {
                name: 'incomingNumber',
                data: 'incomingNumber',
                title: 'Входящ номер',
                sortable: true,
                searchable: true,
                type: 'string'
            },
            {
                name: 'incomingDate',
                data: 'incomingDate',
                title: 'Входирано на',
                sortable: false,
                searchable: false,
                render: function (data) {
                    return JsonBGdate(data);
                }
            },
            {
                name: 'registerNumber',
                data: 'registerNumber',
                title: 'Рег. номер',
                sortable: true,
                searchable: true,
                type: 'string'
            },
            {
                name: 'fromName',
                data: 'fromName',
                title: 'Насочено към',
                sortable: true,
                searchable: true
            },
            {
                name: 'serviceName',
                data: 'serviceName',
                title: 'Услуга',
                sortable: true,
                searchable: true
            },
            {
                name: 'statusId',
                data: 'statusId',
                title: 'Статус',
                sortable: true,
                searchable: false,
                render: function (data, type, row) {
                    return row.status;
                }
            },
            {
                name: 'stepName',
                data: 'stepName',
                title: 'Изпълнена стъпка',
                sortable: true,
                searchable: true
            },
            {
                name: 'partida',
                data: 'partida',
                title: 'Партида',
                sortable: false,
                searchable: false
            },
            {
                name: 'applicant',
                data: 'applicant',
                title: 'Заявител',
                sortable: false,
                searchable: false
            },
        ]
    });

    $(tableId + ' tbody').on('click', 'tr', function () {
        var rowData = table.row(this).data();

        if (rowData) {
            var redirectUrl = `/Admin/Process/PreView?processId=${rowData.id}&isReadonly=true`
            window.location.href = redirectUrl;
        }
    });
}

function loadUsers(activeUsers, element) {
    $(".active-black").removeClass("active-black");
    $(element).addClass('active-black');
    $('#processesDashboard_wrapper').hide();
    $('#formsDashboard_wrapper').hide();
    const tableId = '#usersDashboard';
    if ($.fn.dataTable.isDataTable(tableId)) {
        $(tableId).DataTable().destroy();
        $(tableId).empty();
    }
    $(tableId).show();

    let url = $(tableId).data('url');
    let table = $(tableId).DataTable({
        stateSave: false,
        order: [[0, 'asc']],
        buttons: [],
        ajax: {
            "url": url,
            "type": "POST",
            "datatype": "json",
            data: function (d) {
                d.activeUsers = activeUsers
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
            }
        ]
    });

    $(tableId + ' tbody').on('click', 'tr', function () {
        var rowData = table.row(this).data();

        if (rowData) {
            var redirectUrl = `/Admin/Admin/UserDetails?userId=${rowData.id}`
            window.location.href = redirectUrl;
        }
    });
}

function loadForms(approvalStatus, element) {
    $(".active-black").removeClass("active-black");
    $(element).addClass('active-black');
    $('#processesDashboard_wrapper').hide();
    $('#usersDashboard_wrapper').hide();
    const tableId = '#formsDashboard';
    if ($.fn.dataTable.isDataTable(tableId)) {
        $(tableId).DataTable().destroy();
        $(tableId).empty();
    }
    $(tableId).show();

    let url = $(tableId).data('url');
    let table = $(tableId).DataTable({
        stateSave: false,
        order: [[0, 'asc']],
        buttons: [],
        ajax: {
            "url": url,
            "type": "POST",
            "datatype": "json",
            data: function (d) {
                d.approvalStatus = approvalStatus
                d.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val();               
                return d;
            },          
            error: function (xhr, status, error) {
                messageHelper.ShowErrorMessage(xhr.responseText);
            }
        },
        columns: [           
                {
                    name: 'title',
                    data: 'title',
                    title: 'Име',
                    orderable: true,
                    searchable: true,
                    width: '20%',
                },
                {
                    name: 'purpose',
                    data: 'purpose',
                    title: 'Предназначение на формата',
                    orderable: true,
                    searchable: true,
                    width: '60%',
                },
                {
                    name: 'waitingApproval',
                    data: 'waitingApproval',
                    title: 'Чака одобрение',
                    orderable: true,
                    searchable: false,
                    width: '5%',
                    className: 'center aligned',
                    render: function (data) {
                        return data ? 'Да' : 'Не';
                    }
                }
        ]
    });

    $(tableId + ' tbody').on('click', 'tr', function () {
        var rowData = table.row(this).data();

        if (rowData) {
            var redirectUrl = `/Admin/Designer/Index?formParentId=${rowData.parentId}`
            window.location.href = redirectUrl;
        }
    });    
}

function loadAdministrationRegistries() {
    const tableId = '#registersTable'; 
    if ($.fn.dataTable.isDataTable(tableId)) {
        $(tableId).DataTable().destroy();
        $(tableId).empty();
    }
    $(tableId).show();

    let url = $(tableId).data('url');
    let registerBaseUrl = $(tableId).data('baseurl');
    let table = $(tableId).DataTable({
        stateSave: false,
        order: [[0, 'asc']],
        ordering: false,
        buttons: [],
        paging: false,
        searching: false,
        dom: 't',
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
                name: 'code',
                data: 'code',
                title: 'Код',
            },
            {
                name: 'name',
                data: 'name',
                title: 'Име',
            },
            {
                name: 'id',
                data: 'id',
                title: 'Адрес',
                render: function (data, type, row, metadata) {
                    let baseUrlTemplate = registerBaseUrl;
                    let url = baseUrlTemplate.replace('{0}', row.code);
                    return `<a href="${url}" target="_blank" style="cursor: pointer;">${url}</a>`;
                }
            }
        ],
        columnDefs: [
            { targets: 0, width: '5%' },
            { targets: 1, width: '50%' },
            { targets: 2, width: '45%' }
        ],
        initComplete: function (settings, json) {
            if (json && json.data && json.data.length == 0) {
                $(tableId).hide();
                $(tableId).DataTable().destroy();
                $(tableId).empty();
                $('#toggleRegisters').hide();
            } else {
                $(tableId).show();
                $('#toggleRegisters').show();

            }
        }
    });
}

$(function () {
    const data = localStorage.getItem('iscipr_autosave')
    debugger
    if (data) {
        $('.container-loadautosave').show()
    }
});