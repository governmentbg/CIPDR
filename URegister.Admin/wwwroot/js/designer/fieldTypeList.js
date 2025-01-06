$(function () {
    LoadFieldTypes();
});

function LoadFieldTypes() {
    const tableId = '#field-types';
    let url = $(tableId).data('url');

    get_async("/Designer/GetFieldTypes")
        .then((result) => {
            if (result && result.data) { // Ensure the response has a `data` property
                // Initialize DataTable with data from the server
                let dt = new DataTable(tableId, {
                    data: result.data,  // Directly use the data returned from the server
                    serverSide: false,
                    order: [[2, 'asc']], // Default sorting by `isComplex` column (index 2)
                    stateSave: false, // Disable saving of table state
                    columns: [
                        {
                            name: 'label',
                            data: 'label',
                            title: 'Име',
                            orderable: true,
                            searchable: true                            
                        },
                        {
                            name: 'type',
                            data: 'type',
                            title: 'Тип',
                            orderable: true,
                            searchable: true,

                        },
                        {
                            name: 'isComplex',
                            data: 'isComplex',
                            title: 'Сложен тип',
                            orderable: true,
                            searchable: false,
                            className: "dt-center", 
                            render: function (data, type, row, metadata) {
                                if (data === true) {
                                    return '<i class="check icon"></i>'
                                }
                                return '';
                            }
                        },
                        {
                            name: 'templateName',
                            data: 'templateName',
                            title: 'Шаблон за визуализация',
                            orderable: true,
                            searchable: true
                        },
                        {
                            name: 'id',
                            data: 'id',
                            title: 'Действия',
                            className: "dt-center noExport",
                            sortable: false,
                            render: function (data, type, row, meta) {                                                                
                                let configureLink = `<a href="/Designer/ConfigureFields?preSelectedType=${row.type}" type='button' class='ui tertiary icon button' data-tooltip='Конфигуратор'><i class="table icon"></i></a>`;
                                //let deleteLink = "<a href='javascript:actionWithConfirmation(\"/Admin/Catalog/DeleteForm\", " +
                                //    data + ", \"Сигурни ли сте, че искате да изтриете " +
                                //    row.title +
                                //    "?\", null)' type='button' class='ui tertiary icon button' data-tooltip='Изтрий'><i class='red trash alternate icon'></i></button>";

                                return configureLink;
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