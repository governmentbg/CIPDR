$(function () {
    LoadFieldTypes();
});

function LoadFieldTypes() {
    const tableId = '#custom-views';
    let url = $(tableId).data('url');

    get_async("/Admin/Catalog/GetCustomViews")
        .then((result) => {
            if (result && result.data) { // Ensure the response has a `data` property
                // Initialize DataTable with data from the server
               let dt = new DataTable(tableId, {
                    data: result.data,  // Directly use the data returned from the server
                    serverSide: false,
                    columns: [
                        {
                            name: 'customViewTitle',
                            data: 'customViewTitle',
                            title: 'Име',
                            orderable: true,
                            searchable: true,
                            width: '80%',
                        },                        
                        {
                            name: 'id',
                            data: 'id',
                            title: 'Действия',
                            className: "dt-center noExport",
                            width: '20%',
                            exportData: false,
                            sortable: false,
                            render: function (data, type, row, meta) {
                                let editLink = `<a href='/Admin/Catalog/UpsertCustomView?id=${data}' type='button' class='ui tertiary icon button' data-tooltip='Редактирай'><i class="edit icon"></i></a>`;
                                let executeLink = `<a href='/Admin/Process/CustomTableView?customViewId=${data}&customViewName=${row.customViewTitle}' type='button' class='ui tertiary icon button' data-tooltip='Изпълни'><i class="play icon"></i></a>`;                                                                
                                let deleteLink = "<a href='javascript:actionWithConfirmation(\"/Admin/Catalog/DeleteCustomView\", " +
                                    data + ", \"Сигурни ли сте, че искате да изтриете " +
                                    row.customViewTitle +
                                    "?\", null)' type='button' class='ui tertiary icon button' data-tooltip='Изтрий'><i class='red trash alternate icon'></i></button>";

                                return editLink + executeLink + deleteLink;
                            }
                        }
                    ],

                });

                dt.ready(function () {
                    SetAddButton($(tableId).data('add-url'));
                });

            } else {
                showToast('error', 'Проблем при извличане на данните.');
                console.error("Invalid response or no data found");
            }
        })
        .catch((error) => {
            showToast('error', 'Проблем при визуализиране на данните.');
            console.error("Error fetching data:", error);
        });
}