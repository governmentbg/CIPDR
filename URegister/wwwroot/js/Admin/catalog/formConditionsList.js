$(function () {
    LoadFormConditions();
});

function LoadFormConditions() {
    const tableId = '#formConditions';
    let url = $(tableId).data('url');
    let formParentId = $(tableId).data('form-parent-id');
    let requestUrl = formParentId ? `${url}?formParentId=${formParentId}` : url;
    //"/Admin/Catalog/GetFormConditions"
    get_async(requestUrl)
        .then((result) => {
            if (result && result.data) { // Ensure the response has a `data` property               
                // Initialize DataTable with data from the server
               let dt = new DataTable(tableId, {
                    data: result.data,  // Directly use the data returned from the server
                    serverSide: false,
                    columns: [
                        {
                            name: 'triggeringFieldName',
                            data: 'triggeringFieldName',
                            title: 'При промяна в поле:',
                            orderable: true,
                            searchable: true,
                            /*width: '20%',*/
                        },
                        {
                            name: 'triggeringNomenclatureValue',
                            data: 'triggeringNomenclatureValue',
                            title: 'И избрана стойност:',
                            orderable: true,
                            searchable: true,
                            /*width: '60%',*/
                        },
                        {
                            name: 'fieldsToHide',
                            data: 'fieldsToHide',
                            title: 'Скрий следните полета:',
                            orderable: true,
                            searchable: false,                          
                        },                        
                        {
                            name: 'id',
                            data: 'id',
                            title: 'Действия',
                            className: "dt-center noExport",
                            width: '15%',
                            sortable: false,
                            render: function (data, type, row, meta) {
                                let editLink = `<a href='/Admin/Catalog/EditFormCondition?formParentId=${row.formParentId}&conditionId=${row.id}' type='button' class='ui tertiary icon button' data-tooltip='Редактирай'><i class="edit icon"></i></a>`;                               
                                let deleteLink = "<a href='javascript:actionWithConfirmation(\"/Admin/Catalog/DeleteFormCondition\", " +
                                    data + ", \"Сигурни ли сте, че искате да изтриете условието " +                                  
                                    "?\", null)' type='button' class='ui tertiary icon button' data-tooltip='Изтрий'><i class='red trash alternate icon'></i></button>";
                                
                                return editLink + deleteLink;
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