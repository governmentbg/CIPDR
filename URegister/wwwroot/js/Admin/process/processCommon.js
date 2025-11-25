function ProcessesColumns(addApplicant, hideHistoryButton) {
    let result = [];

    result.push({
        name: 'incomingNumber',
        data: 'incomingNumber',
        title: 'Входящ номер',
        sortable: true,
        searchable: true,
        type: 'string'
    });

    result.push({
        name: 'incomingDate',
        data: 'incomingDate',
        title: 'Входирано на',
        sortable: true,
        searchable: false,
        "render": function (data) {
            return JsonBGDateTime(data);
        }
    });

    result.push({
        name: 'oldIncomingDate',
        data: 'oldIncomingDate',
        title: 'Стара дата входиране',
        sortable: true,
        searchable: false,
        "render": function (data) {
            return JsonBGDateTime(data);
        }
    });

    result.push({
        name: 'oldIncomingNumber',
        data: 'oldIncomingNumber',
        title: 'Стар входящ номер',
        sortable: true,
        searchable: true,
        type: 'string'
    });

    result.push({
        name: 'registerNumber',
        data: 'registerNumber',
        title: 'Рег. номер',
        sortable: true,
        searchable: true,
        type: 'string'
    });

    result.push({
        name: 'serviceName',
        data: 'serviceName',
        title: 'Услуга',
        sortable: true,
        searchable: true
    });

    if (!$('#RegisterServiceHasJustOneStep').length || $('#RegisterServiceHasJustOneStep').val() !== 'True') {
        result.push({
            name: 'statusId',
            data: 'statusId',
            title: 'Статус',
            sortable: true,
            searchable: false,
            render: function (data, type, row) {
                return row.status;
            }
        });

        result.push({
            name: 'stepName',
            data: 'stepName',
            title: 'Изпълнена стъпка',
            sortable: true,
            searchable: true
        });
    }

    result.push({
        name: 'partida',
        data: 'partida',
        title: 'Партида',
        sortable: false,
        searchable: false
    });

    if (addApplicant) {
        result.push({
            name: 'applicant',
            data: 'applicant',
            title: 'Заявител',
            sortable: false,
            searchable: false
        });
    }

    result.push({
        name: 'actions',
        data: "id",
        title: "Действия",
        sortable: false,
        searchable: false,
        className: "text-left noExport",
        width: 150,
        "render": function (data, type, row) {
            let result = `<a href="/Admin/Process/PreView?processId=${data}&isReadonly=true" data-tooltip="Преглед" class="ui tertiary icon button">
                                <i class="info circle icon"></i>
                           </a>`;

            if (!hideHistoryButton) {
                result += `<a href="javascript:showHistoryModal('${data}')" data-tooltip="История" class="ui tertiary icon button">
                                <i class="history icon"></i>
                           </a>`;
            }
                
            if (row.hasNextStep) {
                result += `<a href="/Admin/Process/AddStep?processId=${data}" data-tooltip="Следваща стъпка ${row.nextStep}" class="ui tertiary icon button">
                                <i class="angle double right icon"></i>
                           </a>`
            }
            if (row.hasClose) {
                let deleteLink = `<a href="javascript:deleteProcess('${data}', '${row.incomingNumber}')"
                                                  type="button"
                                                  class="ui tertiary icon button"
                                                  data-tooltip="Прекратяване">
                                                  <i class="times right icon"></i>
                                              </a>`;
                result += deleteLink;
            }
            if (row.hasChange) {
                result += `<a href="/Admin/Process/AddChange?processId=${data}" data-tooltip="Промяна" class="ui tertiary icon button">
                                <i class="pen icon"></i>
                           </a>`;
            }
            if (row.hasInstruction) {
                result += `<a href="/Admin/Process/InstructionIndex?processId=${data}" data-tooltip="Указания" class="ui tertiary icon button">
                                <i class="hand point right outline icon"></i>
                           </a>`;
            }
            if (row.hasCertificate) {
                result += `<a href="/Admin/Process/GetCertificateFileSigned?processId=${data}" data-tooltip="Удостоверение" class="ui tertiary icon button">
                                <i class="file icon"></i>
                           </a>`;
            }
            if (row.hasDelivery) {
                result += `<a href="/Admin/Process/ProcessDeliveryIndex?processId=${data}" data-tooltip="Връчвания" class="ui tertiary icon button">
                                <i class="envelope open outline icon"></i>
                           </a>`;
            }

            if (row.hasDeAssignUser) {
                let deAssignUserLink = `<a href="javascript:deAssignUser('${data}', '${row.incomingNumber}')"
                                                  type="button"
                                                  class="ui tertiary icon button"
                                                  data-tooltip="Връщане за обработка">
                                                  <i class="reply icon"></i>
                                              </a>`;
                result += deAssignUserLink;
            }
            return result;
        }
    });

    return result;
}

function deleteProcess(id, incomingNumber) {
    $('#confirmActionText').text("Сигурни ли сте, че искате да прекратите заявена услуга с входящ номер " + incomingNumber);
    $('.confirm-action')
        .modal({
            centered: true,
            closable: false,
            onApprove: function () {
                let url = '/Admin/Process/Refuse';
                let data = {
                    id: id,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                    reasonForRejection: $('#reasonForRejection').val()
                }
                post_async(url, data)
                    .then((result) => {
                        if (result.success) {
                            window.location.reload();
                        }
                        else {
                            showToast('error', result.error);
                            console.error(result.error);
                        }
                    })
                    .catch((error) => {
                        console.error('Грешка при URL ' + actionUrl + " : " + error.statusText);
                    });
            }
        })
        .modal('show');
}
function deAssignUser(id, incomingNumber) {
    $('#confirmActionText').text("Сигурни ли сте, че искате да върнете за обработка заявена услуга с входящ номер " + incomingNumber);
    $('.confirm-action')
        .modal({
            centered: true,
            closable: false,
            onApprove: function () {
                let url = `/Admin/Process/DeAssignUser?processId=${id}`;
                post_async(url, {})
                    .then((result) => {
                        if (result.success) {
                            window.location.reload();
                        }
                        else {
                            showToast('error', result.error);
                            console.error(result.error);
                        }
                    })
                    .catch((error) => {
                        console.error('Грешка при URL ' + actionUrl + " : " + error.statusText);
                    });
            }
        })
        .modal('show');
}
function ProcessSmallDom() {
    return '<"ui padded grid row"' +
        '<"ui row"' +
        '<"ui left aligned eight wide column dt buttons"B>' + // Custom buttons in the center
        '<"ui left aligned three wide column dt length">' + // l Length change control on the left
        '<"ui right aligned four wide column dt search"f>' + // Search filter on the right
        '<"ui four wide right aligned column custom buttons dtBtnContainer">' + // button add
        '>' +
        'rt>';
}