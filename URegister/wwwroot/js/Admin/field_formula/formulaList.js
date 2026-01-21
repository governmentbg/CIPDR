$(function () {
    LoadServiceTypes();
});

function LoadServiceTypes() {    
    const tableId = '#field_formula';
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
                    name: 'targetField',
                    data: 'targetField',
                    title: 'Поле за резултат',
                    sortable: false,
                    searchable: true
                },
                {
                    name: 'formulaText',
                    data: 'formulaText',
                    title: 'Формула',
                    sortable: false,
                    searchable: true
                },
                //{
                //    name: 'label',
                //    data: 'label',
                //    title: 'Наименование при визуализиране',
                //    sortable: false,
                //    searchable: true
                //},
                {
                    name: 'actions',
                    data: "id",
                    title: "Действия",
                    sortable: false,
                    searchable: false,
                    className: "text-left noExport",
                    width: 240,
                    "render": function(data, type, row) {
                        return editButton(`/Admin/FieldFormula/Edit?id=${data}&formParentId=${row.formParentId}`) +         
                            `<a href="javascript:orderNumUp(${data})" data-tooltip="Нагоре" class="ui tertiary icon button">
                               <i class="level up icon"></i>
                            </a>`+
                            `<a href="javascript:orderNumDown(${data})" data-tooltip="Надолу" class="ui tertiary icon button">
                               <i class="level down icon"></i>
                            </a>`+
                            "<a href='javascript:actionWithConfirmation(\"/Admin/FieldFormula/DeleteTemplate\", " +
                            data + ", \"Сигурни ли сте, че искате да изтриете " +
                            row.formulaText +
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

async function orderNumUp(id) {
    await post_fetch_string_async(`/Admin/FieldFormula/PriorityUp/${id}`, {});
    refreshTable('#field_formula');
}

async function orderNumDown(id) {
    await post_fetch_string_async(`/Admin/FieldFormula/PriorityDown/${id}`, {});
    refreshTable('#field_formula');
}

function SetAddButtonPublicTemplate(href) {
    if (href) {
        var markup = `<div class="ui fluid container basic clearing">
                    <a href="${href}" class="ui primary button right floated">
                        <i class="icon plus"></i>
                        Добави
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
