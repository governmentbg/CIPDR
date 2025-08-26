$(function () {
    LoadImportList();
});

async function LoadImportList() {
    let dataSet = []
    const tableId = '#import';
    const url = $(tableId).data('url');
    const data = await post_fetch_json_async(url, {
        serviceId: $('#ServiceId').val(),
        fileId: $('#FileId').val(),
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    });
    const fields = data[0];
    for (var i = 1; i < data.length; i++) {
        let row = {}; 
        const dataRow = data[i];
        fields.forEach(function (item) {
            const dataItem = dataRow.find((f) => f.key === item.key);
            row[item.key] = dataItem.value;
            if (dataItem.error) {
                row[item.key] += ` <span style="color:red">${dataItem.error}</span>`
                $('button.ui.primary.button').hide();
            }
        });
        dataSet.push(row);
    }
    const columns = fields.map((x) => ({
        name: x.key,
        data: x.key,
        title: x.value,
        sortable: true,
        searchable: true,
        type: 'string'
    }));

    let dt = $(tableId).DataTable({
        'order': [[0, 'asc']],
        data: dataSet,
        processing: false,
        serverSide: false,
        "paging": false,
        "lengthMenu": [[-1], ["Покажи всички"]], data: dataSet,
        columns: columns
        });
}
