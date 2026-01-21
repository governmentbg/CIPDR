$(function () {
    LoadEDeliveryLogRecords();
});

function LoadEDeliveryLogRecords() {
    const tableId = '#eDeliveryLogRecords';
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else {
        let url = $(tableId).data('url');
        let dt = $(tableId).DataTable({
            'order': [[0, 'desc']],
            ajax: {
                "url": url,
                "type": "POST",
                "datatype": "json",
                data: function (d) {
                    d.__RequestVerificationToken = $('input[name="__RequestVerificationToken"]').val();                  
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: false,
            columns: [
                {
                    name: 'modifiedOn',
                    data: 'modifiedOn',
                    title: 'Дата на последна промяна',
                    sortable: true,
                    searchable: false,
                    "render": function (data) {
                        return JsonBGdateTSWithTime(data);
                    }
                },
                {
                    name: 'registerId',
                    data: 'registerId',
                    title: 'Идентификатор на регистър',
                    sortable: false,
                    searchable: false,
                    "render": function (data, type, row) {
                        // Display blank if registerId is 0 or unset
                        return (row.hasRegisterId && data !== 0) ? data : '';
                    }
                },
                {
                    name: 'registerName',
                    data: 'registerName',
                    title: 'Наименование на регистър',
                    sortable: false,
                    searchable: false                   
                },        
                {
                    name: 'publicServiceName',
                    data: 'publicServiceName',
                    title: 'Име на услуга',
                    sortable: false,
                    searchable: false
                },
                {
                    name: 'publicServiceIdentifier',
                    data: 'publicServiceIdentifier',
                    title: 'Идентификатор на услуга',
                    sortable: false,
                    searchable: false,                  
                },
                {
                    name: 'messageId',
                    data: 'messageId',
                    title: 'Идентификатор на съобщение',
                    sortable: false,
                    searchable: false,
                },
                {
                    name: 'errorMessage',
                    data: 'errorMessage',
                    title: 'Грешка',
                    sortable: false,
                    searchable: false
                }                             
            ]
        });      
    }
}