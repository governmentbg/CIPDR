$(function () {
    LoadFieldTypes();
});

function LoadFieldTypes() {
    const tableId = '#field-types';
    let url = $(tableId).data('url');

    get_async("/Admin/Catalog/GetForms")
        .then((result) => {
            if (result && result.data) { // Ensure the response has a `data` property
                // Initialize DataTable with data from the server
               let dt = new DataTable(tableId, {
                    data: result.data,  // Directly use the data returned from the server
                    serverSide: false,
                    columns: [
                        {
                            name: 'title',
                            data: 'title',
                            title: 'Име',
                            orderable: true,
                            searchable: true                            
                        },
                        {
                            name: 'purpose',
                            data: 'purpose',
                            title: 'Предназначение на формата',
                            orderable: true,
                            searchable: true,
                        },
                        {
                            name: 'id',
                            data: 'id',
                            title: 'Действия',
                            className: "dt-center noExport",
                            sortable: false,
                            render: function (data, type, row, meta) {
                                let editLink = `<a href='/Admin/Catalog/EditForm?formParentId=${row.parentId}' type='button' class='ui tertiary icon button' data-tooltip='Редактирай'><i class="edit icon"></i></a>`;
                                let configureLink = `<a href="/Admin/Designer/Index?formParentId=${row.parentId}" type='button' class='ui tertiary icon button' data-tooltip='Конфигуратор'><i class="table icon"></i></a>`;
                                let deleteLink = "<a href='javascript:actionWithConfirmation(\"/Admin/Catalog/DeleteForm\", " +
                                    data + ", \"Сигурни ли сте, че искате да изтриете " +
                                    row.title +
                                    "?\", null)' type='button' class='ui tertiary icon button' data-tooltip='Изтрий'><i class='red trash alternate icon'></i></button>";

                                return editLink + configureLink + deleteLink;
                            }
                        }
                    ]
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