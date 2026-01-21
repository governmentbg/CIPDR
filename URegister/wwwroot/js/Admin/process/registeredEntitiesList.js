$(function () {    
    LoadItems();    
});
function LoadItems() {
    const tableId = '#registeredEntities';
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else
    {
        openAccordionFilter();

        let columns = [];

        columns.push({
            "data": "ModifiedOn",
            "title": "Дата на последна промяна",
            "sortable": false,
            "searchable": false,
            "width": "5%",
            "render": function (data) {
                return JsonBGdate(data);
            }
        });
        
        $("#FieldName option").each(function () {
            let value = $(this).val();
            let text = $(this).text();

            if (value) {
                columns.push({
                    "data": value,
                    "title": text,
                    "sortable": false,
                    "searchable": false,
                    "render": function (data) {
                        if (data?.startsWith("https://objectstore")) {
                            return `<a href="${data}" type='button' class='ui tertiary icon button' data-tooltip='Свали файла'><i class="file download icon"></i></a>`;
                        }
                        else if (data?.startsWith("http") || data?.startsWith("ftp")) {
                            return `<a href="${data}" type='button' class='ui tertiary icon button'>${data}</a>`;
                        }
                        return data.replace(/\n/g, '<br>');
                    }
                });
            }
        });

        columns.push({
            "data": "ProcessId",
            "title": "Действия",
            "sortable": false,
            "searchable": false,
            "width": "5%",
            "render": function (data, type, row) {
                if (type === "display") {
                    let backTo = $('#CustomViewId').val() > 0 ? "CustomTableView" : "TableView";
                    return `<a href="/Admin/Process/Preview?processId=${data}&isReadonly=true&backTo=${backTo}" type='button' class='ui tertiary icon button' data-tooltip='Виж заявлението'><i class="eye outline icon"></i></a>`;
                }
                return data;
            }
        });

        let url = $(tableId).data('url');
        let dt = $(tableId).DataTable({
            'order': [[0, 'asc']],
            "processing": true,
            "serverSide": true,
            "ajax": {
                "url": url,
                "type": "POST",
                "datatype": "json",
                "data": function (d) {
                    d.serviceId = $('#ServiceId').val();
                    d.customViewId = $('#CustomViewId').val();
                    d.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val();
                    d.filter = {
                        fieldName: $('#FieldName').val(),
                        searchPattern: $('#SearchPattern').val(),                    
                        serviceTitle: $('#ServiceTitle').val(),
                        incomingNumber: $('#IncomingNumber').val(),
                        registerNumber: $('#RegisterNumber').val(),
                        incomingDateFrom: $('#IncomingDateFrom').val(),
                        incomingDateTo: $('#IncomingDateTo').val(),
                        submitterId: $('#SubmitterId').val(),
                        mprId: $('#MprId').val()
                    };
                },
                "dataSrc": function (json) {
                    return json.data || [];
                },
                "error": function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: false,
            "columns": columns,        
        });

        dt.ready(function () {
            SetAddButtonsProcess($(tableId).data('add-url'));
        });
    }
}

function SetAddButtonsProcess(href) {
    $('.no-add-button').hide()
}