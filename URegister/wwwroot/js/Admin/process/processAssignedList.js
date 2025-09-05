$(function () {
    LoadProcessesAssigned();
});

function LoadProcessesAssigned() {
    const tableId = '#processes_assigned';
    if ($.fn.dataTable.isDataTable(tableId)) {
        refreshTable(tableId);
    }
    else {
        let url = $(tableId).data('url');
        let dt = $(tableId).DataTable({
            'order': [[0, 'asc']],
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
            dom: '< "ui padded grid row"' +
                '<"ui row"' +
                '<"ui left aligned eight wide column dt buttons">' + // Custom buttons in the center
                '<"ui left aligned three wide column dt length">' + // l Length change control on the left
                '<"ui right aligned four wide column dt search">' + // Search filter on the right
                '<"ui four wide right aligned column custom buttons dtBtnContainer">' + // button add
                '>' +
                '>',
            filter: false,
            columns: ProcessesColumns($(tableId).data('applicant'))
        });
    }
}