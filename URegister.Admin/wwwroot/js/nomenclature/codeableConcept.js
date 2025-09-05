$(function () {
    LoadCodeableConcept();
});

function LoadCodeableConcept() {
    const tableId = '#codeableConcepts';
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else {
        let url = $(tableId).data('url');
        let dt = $(tableId).DataTable({
            'order': [[0, 'asc']],
            buttons: [
                'io_pageLength',
                'io_colvis',
                'io_excel',
                'io_pdf',
                'io_csv',
                'io_print',
                {
                    text: '<i class="file code outline icon"></i>',
                    titleAttr: 'Експорт в json формат',
                    className: 'basic',
                    action: function (e, dt, node, config) {
                        const data = dt.rows({ search: 'applied' }).data().toArray();
                        const data1 = dt.rows({ search: 'applied' }).data();
                        
                        $.ajax({
                            url: '/Nomenclature/GetCodeableConceptListExport',
                            type: 'POST',
                            data: {
                                filterType: $('#Type').val(),
                                filterName: $('#Name').val(),
                                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                            },
                            dataType: 'json',
                            success: function (response) {
                                const blob = new Blob(
                                    [JSON.stringify(response, null, 2)],
                                    { type: 'application/json' }
                                );
                                const url = URL.createObjectURL(blob);
                                const a = document.createElement('a');
                                a.href = url;
                                a.download = 'codeableConcepts.json';
                                document.body.appendChild(a);
                                a.click();
                                document.body.removeChild(a);
                                URL.revokeObjectURL(url);
                            },
                            error: function (xhr, status, error) {
                                messageHelper.ShowErrorMessage('Проблем при четене ' + xhr.responseText);
                            }
                        });
                       
                    }
                }
            ],
            ajax: {
                "url": url,
                "type": "POST",
                "datatype": "json",
                data: function (d) {
                    d.filter = {
                        type: $('#Type').val(),
                    }
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            columns: [
                {
                    name: 'code',
                    data: 'code',
                    title: 'Тип',
                    sortable: true,
                    searchable: true,
                    type: 'string'
                },
                {
                    name: 'value',
                    data: 'value',
                    title: 'Стойност',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'valueEn',
                    data: 'valueEn',
                    title: 'Стойност EN',
                    sortable: true,
                    searchable: true
                },
                {
                    name: 'dateFrom',
                    data: 'dateFrom',
                    title: 'Валидно от',
                    sortable: false,
                    searchable: false,
                    "render": function (data) {
                        return JsonBGdateTS(data);
                    }
                },
                {
                    name: 'dateTo',
                    data: 'dateTo',
                    title: 'Валидно до',
                    sortable: false,
                    searchable: false,
                    "render": function (data) {
                        return JsonBGdateTS(data);
                    }
                },
                {
                    name: 'status',
                    data: 'status',
                    title: 'Статус',
                    sortable: false,
                    searchable: false
                },
                {
                    name: 'actions',
                    data: "id",
                    title: "Действия",
                    sortable: false,
                    searchable: false,
                    className: "text-left noExport",
                    width: 120,
                    "render": function (data, type, row) {
                        if (row.statusId == "2") {
                            return editButton(`/Nomenclature/Edit/${data}`);
                        }
                        if (row.statusId == "1") {
                            const name = `${row.code} : ${row.value}`
                            return `<a href="javascript:confirmCodeableConcept(${row.id}, '${row.code}', '${name}')" data-tooltip="Потвърди" class="ui tertiary icon button">
                                     <i class="check icon"></i>
                             </a>` + `<a href="javascript:refuseCodeableConcept(${row.id}, '${row.code}', '${name}')" data-tooltip="Откажи" class="ui tertiary icon button">
                                     <i class="times icon"></i>
                             </a>`;
                        }
                        if (row.statusId == "3") {
                            const name = `${row.code} : ${row.value}`
                            return `<a href="javascript:confirmCodeableConcept(${row.id}, '${row.code}', '${name}')" data-tooltip="Потвърди" class="ui tertiary icon button">
                                     <i class="check double icon"></i>
                             </a>`;
                        }
                        return ``;
                    }
                }
            ]
        });

        dt.ready(function () {
            SetAddButton($(tableId).data('add-url'));
        });
    }
}

async function confirmCodeableConcept(id, code, name) {
    const result = await confirmDialog('Потвърдете номенклатурна стойност', name, 'Потвърди');
    if (result) {
        const param = $('#codeableConcepts').DataTable().ajax.params();
        const data = {
            type: param.filter.type,
            id,
            code,
            statusId: 2
        };
        await post_fetch_string_async('/Nomenclature/UpdateCodeableConceptStatus', data);
        LoadCodeableConcept();
    }
}

async function refuseCodeableConcept(id, code, name) {
    const result = await confirmDialog('Отказ номенклатурна стойност', name, 'Потвърди отказ');
    if (result) {
        const param = $('#codeableConcepts').DataTable().ajax.params();
        const data = {
            type: param.filter.type,
            id,
            statusId: 3,
            code
        };
        await post_fetch_string_async('/Nomenclature/UpdateCodeableConceptStatus', data);
        LoadCodeableConcept();
    }
}