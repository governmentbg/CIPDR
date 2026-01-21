$(function () {
    LoadServiceTypes();
});

function LoadServiceTypes() {
    debugger
    const tableId = '#field_template';
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else {
        let url = $(tableId).data('url');
        let dt = $(tableId).DataTable({
            order: [],
            ajax: {
                "url": url,
                "type": "POST",
                "datatype": "json",
                data: function (d) {
                    d.filter = {}
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: false,
            columns: [
                {
                    name: 'fieldName',
                    data: 'fieldName',
                    title: 'Поле',
                    sortable: false,
                    searchable: true
                },
                {
                    name: 'label',
                    data: 'label',
                    title: 'Наименование при визуализиране',
                    sortable: false,
                    searchable: true
                },
                {
                    name: 'actions',
                    data: "id",
                    title: "Действия",
                    sortable: false,
                    searchable: false,
                    className: "text-left noExport",
                    width: 240,
                    "render": function(data, type, row) {
                        return editButton(`/Admin/PublicFieldTemplate/Edit/${data}`) +
                            `<a href="/Admin/PublicFieldTemplate/EditContent/${data}" data-tooltip="Бланка" class="ui tertiary icon button">
                               <i class="file alternate outline icon"></i>
                            </a>`+
                            `<a href="javascript:orderNumUp(${data})" data-tooltip="Нагоре" class="ui tertiary icon button">
                               <i class="level up icon"></i>
                            </a>`+
                            `<a href="javascript:orderNumDown(${data})" data-tooltip="Надолу" class="ui tertiary icon button">
                               <i class="level down icon"></i>
                            </a>`+
                            "<a href='javascript:actionWithConfirmation(\"/Admin/PublicFieldTemplate/DeleteTemplate\", " +
                            data + ", \"Сигурни ли сте, че искате да изтриете " +
                            row.label +
                            "?\", null)' type='button' class='ui tertiary icon button' data-tooltip='Изтрий'><i class='red trash alternate icon'></i></button>";;
                    }
                }
            ]
        });

        dt.ready(function () {
            SetAddButtonPublicTemplate($(tableId).data('add-url'));
        });
    }
}

async function orderNumUp(id){
   await post_fetch_string_async(`/Admin/PublicFieldTemplate/OrderNumUp/${id}`, {});
   refreshTable('#field_template');
}

async function orderNumDown(id){
   await post_fetch_string_async(`/Admin/PublicFieldTemplate/OrderNumDown/${id}`, {});
   refreshTable('#field_template');
}

function SetAddButtonPublicTemplate(href) {
    if (href) {
        var markup = `<div class="ui fluid container basic clearing">
                    <a href="${href}" class="ui primary button right floated">
                        <i class="icon plus"></i>
                        Добави
                    </a>
                    <a href="/Admin/Register/OpenDataRegister" class="ui primary button right floated">
                        <i class="icon archive"></i>
                        OpenData
                    </a>
                  </div>`;

        $('.custom.buttons.dtBtnContainer').html(markup);
        $('.no-add-button').hide()
    }
    else {
        $('.no-add-button').show()
        $('.custom.buttons.dtBtnContainer').hide();
    }
}
