$(function () {
    LoadProcesses();
});

function LoadProcesses() {
    const tableId = '#processes';
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else {
        openAccordionFilter()
        let url = $(tableId).data('url');
        let dt = $(tableId).DataTable({
            'order': [[1, 'desc']],
            ajax: {
                "url": url,
                "type": "POST",
                "datatype": "json",
                data: function (d) {
                    d.filter = {
                        incomingDateFrom: $('#IncomingDateFrom').val(),
                        incomingDateTo: $('#IncomingDateTo').val(),
                        incomingNumber: $('#IncomingNumber').val(),
                        registerNumber: $('#RegisterNumber').val(),
                        fromRegisterNumber: $('#FromRegisterNumber').val(),
                        serviceId: $('#ServiceId').val(),
                        stepId: $('#StepId').val(),
                        statusId: $('#StatusId').val(),
                        forDeAssignUser: $('#ForDeAssignUser').val(),
                        personIdentifier: {
                            pidType: $('#PersonIdentifier_PidType').val(),
                            pid: $('#PersonIdentifier_Pid').val(),
                        },
                        personIdentifierApplicant: {
                            pidType: $('#PersonIdentifierApplicant_PidType').val(),
                            pid: $('#PersonIdentifierApplicant_Pid').val(),
                        },
                    }
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            filter: false,
            columns: ProcessesColumns($(tableId).data('applicant'))
        });

        dt.ready(function () {
            SetAddButtonsProcess($(tableId).data('add-url'));
        });
    }
}

function SetAddButtonsProcess(href) {
    var markup = href ? `<a href="${href}" class="ui primary button right floated">
                        <i class="icon plus"></i>
                        Добави
                    </a>` : '';
    var markupold = $('#canAddOldRecords').val() && $('#canAddOldRecords').val() != 'False' ? `<a href="/Admin/Process/AddOld" class="ui primary button right floated" style="margin-left: 0.2rem;">
                        <i class="icon plus"></i>
                        Добави стари
                    </a>` : '';

    var buttonsContainer = `<div class="ui buttons right floated"> ${markup} ${markupold} </div>`

    $('.custom.buttons.dtBtnContainer').html(buttonsContainer);
    $('.no-add-button').hide()

}
