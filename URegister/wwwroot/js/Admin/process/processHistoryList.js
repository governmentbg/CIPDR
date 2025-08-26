async function showHistoryModal(id) {
    const modalHtml = `<div class="ui fullscreen modal center aligned" id="historyModal">
                            <i class="close icon"></i>
                            <div class="header">Преглед</div>
                            <div class="content">
                               <table id="processes_history" class="ui celled striped very compact table" style="width:100%; padding:0px;"
                                      data-url="/Admin/Process/GetProcessList"
                               </table>
                            </div>
                        </div>`;

    // Append the modal to the body
    $('body').append(modalHtml);
    LoadProcessProcessHistory(id);
    // Initialize and show the modal
    $('#historyModal')
        .modal({
            onHidden: function () {
                // Clean up after closing
                $('#historyModal').remove();
            },
        })
        .modal('show');
}
var c = ``;

function LoadProcessProcessHistory(id) {
    const tableId = '#processes_history';
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
                    d.filter = {
                        fromProcessId: id
                    }
                },
                error: function (error) {
                    messageHelper.ShowErrorMessage('Проблем при четене ' + error.responseText);
                }
            },
            pageLength: -1,
            dom: '<"ui padded grid row"' +
                '<"ui row"' +
                '<"ui left aligned eight wide column dt buttons"B>' + // Custom buttons in the center
                '<"ui left aligned three wide column dt length">' + // l Length change control on the left
                '<"ui right aligned four wide column dt search"f>' + // Search filter on the right
                '<"ui four wide right aligned column custom buttons dtBtnContainer">' + // button add
                '>' +
                'rt>',
            buttons: ['io_colvis', 'io_excel', 'io_pdf', 'io_csv', 'io_print'],
            filter: false,
            columns: ProcessesColumns()
        });
    }
}
