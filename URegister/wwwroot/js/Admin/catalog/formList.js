$(function () {
    LoadForms();
});

function LoadForms() {
    const tableId = '#forms';
    let url = $(tableId).data('url');

    get_async("/Admin/Catalog/GetForms")
        .then((result) => {
            if (result && result.data) { // Ensure the response has a `data` property
                // Store isGlobalAdmin from the response
                let isGlobalAdmin = result.isGlobalAdmin;
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
                        },
                        {
                            name: 'id',
                            data: 'id',
                            title: 'Действия',
                            className: "dt-center noExport",
                            width: '15%',
                            sortable: false,
                            render: function (data, type, row, meta) {
                                let editLink = `<a href='/Admin/Catalog/EditForm?formParentId=${row.parentId}' type='button' class='ui tertiary icon button' data-tooltip='Редактирай'><i class="edit icon"></i></a>`;
                                let configureLink = `<a href="/Admin/Designer/Index?formParentId=${row.parentId}" type='button' class='ui tertiary icon button' data-tooltip='Конфигуратор'><i class="table icon"></i></a>`;
                                let jsonImportLink = `<a href="/Admin/Designer/SubmitJson?formId=${data}" type='button' class='ui tertiary icon button' data-tooltip='Подай JSON данни'><i class="file import icon"></i></a>`;
                                let conditionsLink = `<a href="/Admin/Catalog/FormConditions?formParentId=${row.parentId}" type='button' class='ui tertiary icon button' data-tooltip='Условия към форма'><i class="project diagram icon"></i></a>`;
                                let calculationsLink = `<a href="/Admin/FieldFormula/Index?formParentId=${row.parentId}" type='button' class='ui tertiary icon button' data-tooltip='Изчисления'><i class="calculator icon"></i></a>`;
                                let deleteLink = "<a href='javascript:actionWithConfirmation(\"/Admin/Catalog/DeleteForm\", " +
                                    data + ", \"Сигурни ли сте, че искате да изтриете " +
                                    row.title.replace(/"/g, '\\"') +
                                    "?\", null)' type='button' class='ui tertiary icon button' data-tooltip='Изтрий'><i class='red trash alternate icon'></i></button>";

                                let icons = editLink + configureLink + jsonImportLink + conditionsLink + calculationsLink;

                                // Show deleteLink if form is waiting approval (for any user) or if form is approved and user is Global Admin
                                if (row.waitingApproval || (!row.waitingApproval && isGlobalAdmin)) {
                                    icons = icons + deleteLink;
                                }
                                return icons;
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