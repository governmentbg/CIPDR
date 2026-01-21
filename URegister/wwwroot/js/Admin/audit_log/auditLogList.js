$(function () {
    LoadAuditLogRecords();
});

function LoadAuditLogRecords() {
    const tableId = '#auditLogRecords';
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else {
        openAccordionFilter()
        let url = $(tableId).data('url');
        let dt = $(tableId).DataTable({
            'order': [[0, 'desc']],
            ajax: {
                "url": url,
                "type": "POST",
                "datatype": "json",
                data: function (d) {
                    d.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val();
                    d.filter = {
                        dateFrom: $('#DateFrom').val(),
                        dateTo: $('#DateTo').val(),
                        action: $('#Action').val(),
                        ipAddress: $('#IpAddress').val(),
                        userName: $('#UserName').val()
                    }
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: false,
            columns: [
                {
                    name: 'createdDate',
                    data: 'createdDate',
                    title: 'Извършено на',
                    sortable: true,
                    searchable: false,
                    "render": function (data) {
                        return JsonBGDateTimeWithSeconds(data);
                    }
                },
                {
                    name: 'userFullName',
                    data: 'userFullName',
                    title: 'Потребител',
                    sortable: true,
                    searchable: false,
                    type: 'string'
                },               
                //{
                //    name: 'assemblyName',
                //    data: 'assemblyName',
                //    title: 'Модул',
                //    sortable: false,
                //    searchable: false,
                //    type: 'string'
                //},
                {
                    name: 'controller',
                    data: 'controller',
                    title: 'Модул',
                    sortable: false,
                    searchable: false
                },
                {
                    name: 'action',
                    data: 'action',
                    title: 'Действие',
                    sortable: false,
                    searchable: false,                  
                },
                //{
                //    name: 'actionType',
                //    data: 'actionType',
                //    title: 'Тип на действие',
                //    sortable: false,
                //    searchable: false
                //},
                {
                    name: 'ipAddressStr',
                    data: 'ipAddressStr',
                    title: 'IP адрес',
                    sortable: false,
                    searchable: false
                },               
                {
                    name: 'actions',
                    data: "id",
                    title: "Действия",
                    sortable: false,
                    searchable: false,
                    className: "dt-center noExport",                  
                    "render": function (data, type, row) {
                        const parameters = row.parameters || ""; // fallback to empty string
                        const jsonParameters = parameters.replaceAll('"', '');
                        let result = `<a onclick="showAuditRecordDetails('${jsonParameters}', '${row.id}')" data-tooltip="Детайли" class="ui tertiary icon button">
                                <i class="eye icon"></i>
                           </a>`;                                           
                        return result;
                    }
                }
            ]
        });      
    }
}

function showAuditRecordDetails(parameters, auditId) {
    $('#previewValuesBtn').attr('data-audit-id', auditId);
    $('.coupled.modal')
        .modal({
            allowMultiple: true
        });
    // open second modal on first modal buttons
    $('.second.modal')
        .modal('attach events', '.first.modal .button');
    // show first immediately
    $('.first.modal')
        .modal('show');

    if (parameters && parameters.trim() !== "[]") {
        $('#parameters').text(parameters);  // Directly display the JSON string
    } else {
        $('#parameters').text("Няма параметри");  // If no parameters, display a message
    }

}

$('#previewValuesBtn').on('click', async function () {
    const auditId = $(this).attr('data-audit-id');
    if (!auditId) return;
  
    try {
        const result = await get_async('/Admin/AuditLog/GetAuditEntityValues', { auditId: auditId });
        const $description = $('.second.modal .description');
        $description.empty();

        if (!result?.length) {
            $description.append('<p>Няма стойности</p>');
            return;
        }

        const escapeHtml = unsafe => unsafe.replace(/&/g, "&").replace(/</g, "<").replace(/>/g, ">").replace(/"/g, '"').replace(/'/g, "'");
        const decodeUnicode = str => str.replace(/\\u([\dA-F]{4})/gi, (_, code) => String.fromCharCode(parseInt(code, 16)));
        const renderValues = (values, header, divider = false) => {
            if (!values) return `<div class="ui header">${header}</div><p>Няма стойности</p>${divider ? '<div class="ui divider"></div>' : ''}`;
            const formatted = escapeHtml(decodeUnicode(values));
            return `<div class="ui header">${header}</div><pre style="white-space: pre-wrap; font-family: monospace; overflow-x: auto;">${formatted}</pre>${divider ? '<div class="ui divider"></div>' : ''}`;
        };      

        const accordionItems = result.map(item => `
        <div class="title">
            <i class="dropdown icon"></i>
            Идентификатор на обект: ${escapeHtml(item.primaryKey || 'Няма идентификатор')}
        </div>
        <div class="content">
            ${renderValues(item.oldValues, 'Стари стойности')}
            ${renderValues(item.newValues, 'Нови стойности', true)}
        </div>
        `).join('');

        const accordionHtml = `<div class="ui basic styled fluid accordion">${accordionItems}</div>`;
        $description.append(accordionHtml);

        // Initialize accordion
        $('.ui.accordion').accordion();
       
    } catch (e) {
        console.error('Error:', e);
        $description.html('<div class="ui header">Грешка</div><p>Неуспешно зареждане на данните</p>');
    }
});