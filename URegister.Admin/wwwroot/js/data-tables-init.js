(function () {
    $.fn.dataTable.ext.buttons.io_excel = {
        extend: 'excel',
        text: '<i class="file excel outline icon"></i>',
        titleAttr: 'Excel',
        className: 'basic',
        exportOptions: {
            "columns": "thead th:not(.noExport)",
            orthogonal: 'excel'
        }
    };

    $.fn.dataTable.ext.buttons.io_csv = {
        extend: 'csvHtml5',
        text: '<i class="file csv outline icon"></i>',
        titleAttr: 'CSV',
        className: 'basic',
        exportOptions: {
            columns: function (idx, data, node) {
                let isNoExport = $(node).hasClass("noExport");
                let columnTitle = $(node).text().trim().toLowerCase();
                let isActionsColumn = columnTitle === "действия";
                return !isNoExport && !isActionsColumn;
            }
        }
    };

    ///Trim на филтъра
    $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
        for (var i = 0; i < data.length; i++) {
            data[i] = $.trim(data[i]);
        }
        return true;
    });

    $.fn.dataTable.ext.buttons.io_pdf = {
        extend: 'collection',
        text: '<i class="file pdf outline icon"></i>',
        titleAttr: 'Pdf',
        className: 'buttons-pdf basic',
        autoClose: true,
        buttons: [
            {
                extend: 'pdf',
                text: 'Портретно',
                exportOptions: {
                    "columns": "thead th:not(.noExport)"
                },
                orientation: 'portrait'
            },
            {
                extend: 'pdf',
                text: 'Пейзажно',
                exportOptions: {
                    "columns": "thead th:not(.noExport)"
                },
                orientation: 'landscape'
            },
        ]
    };

    $.fn.dataTable.ext.buttons.io_print = {
        extend: 'print',
        text: '<i class="print icon"></i>',
        titleAttr: 'Печат',
        className: 'basic',
        exportOptions: {
            "columns": "thead th:not(.noExport)"
        }
    };

    $.fn.dataTable.ext.buttons.io_colvis = {
        extend: 'colvis',
        text: '<i class="eye icon"></i>',
        titleAttr: 'Видими Колони',
        className: 'basic'
    };

    $.fn.dataTable.ext.buttons.io_pageLength = {
        extend: 'pageLength',
        className: 'basic'
    };

    $.extend(true, $.fn.dataTable.defaults, {
        "initComplete": function (settings, json) {
            initDataTablesSearch(settings);
            dataTablesActionsButtonsTooltipPosition();
        },
        "lengthMenu": [
            [10, 25, 50, 100, 1000],
            ['10 реда', '25 реда', '50 реда', '100 реда', '1000 реда']
        ],
        "bAutoWidth": false,
        "language": {
            "url": "/js/dataTables.bgBG.json"
        },
        // dom: '<"ui grid"<"row"<"eight wide column"B><"eight wide column right aligned"l>>>rt<"ui grid"<"row extra-top"<"seven wide column"i><"nine wide column right aligned"p>>>',
        dom: '<"ui padded grid row"' +
            '<"ui row"' +
            '<"ui left aligned six wide column dt buttons"B>' + // Custom buttons in the center
            '<"ui left aligned three wide column dt length">' + // l Length change control on the left
            '<"ui right aligned seven wide column dt search"' + // l Length change control on the left
            '<"ui buttons right floated"'+
            '<"ui form"f>' + // Search filter on the right
            '<"custom buttons dtBtnContainer">' + // button add
            '>' +
            '>' +
            '>' +
            'rt>' +
            '<"ui padded grid row"' +
            '<"ui row"' +
            '<"ui seven wide column dt-info"i>' + // Info on the left
            '<"ui nine wide column right aligned dt-pagination"p>' + // Pagination on the right
            '>' +
            '>',
        buttons: ['io_pageLength', 'io_colvis', 'io_excel', 'io_pdf', 'io_csv', 'io_print'],
        filter: true,
        "searching": true,
        "info": true,
        "bLengthChange": true,
        "serverSide": true,
        "processing": true,
        "paging": true,
        "pageLength": 10,
        "stateSave": true,
        "stateDuration": -1
    });

    function initDataTablesSearch(dtSettings) {
        // Search form events
        var initSearchForm = $('.search-form');
        //var initTable = $('.dataTable');
        var initWrapper = $(dtSettings.nTableWrapper);
        var initTable = $(dtSettings.nTable);

        if (initSearchForm.length > 0 && initTable.length > 0) {
            initSearchForm.on('submit', function () {
                var t = initTable.DataTable();
                t.state.clear();
            });
        }

        var secondCount = 0;
        var keysPressed = -1;
        var searchQuery = '';
        var timer = '';

        //var $searchInput = $('div.dataTables_filter input');
        var $searchInput = initWrapper.find('div.dataTables_filter input');
        $searchInput.unbind();
        $searchInput.bind('keyup', function (e) {
            if (this.value.length > 2 || this.value === '') {
                secondCount = 0;
            }
            keysPressed = this.value.length;
            searchQuery = this.value;
        });
        $searchInput.bind('keydown', function (e) {
            if (!timer) {
                timer = setInterval(function () {
                    if (secondCount >= 1 && (keysPressed > 2 || keysPressed === 0)) {
                        keysPressed = -1;
                        SearchDataTable(searchQuery, initTable);
                        clearInterval(timer);
                        timer = '';
                    } else {
                        secondCount += 1;
                    }
                }, 1000);
            }
        });
    }
})();

function SetAddButton(href) {
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
    else
    {
        $('.no-add-button').show()
        $('.custom.buttons.dtBtnContainer').hide();
    }
}

function SetAddButtonWithTitle(id, title, onclick, wrapper) {
    if (title) {
        var markup = `<div class="ui fluid container basic clearing">
            <button class="ui button green" onclick="${onclick}" id="${id}">${title}</button>
        </div>`;
        $(wrapper).find($('.custom.buttons.dtBtnContainer')).html(markup);
        $('.no-add-button').hide()
    }
    else {
        $('.no-add-button').show()
        $('.custom.buttons.dtBtnContainer').hide();
    }
}

function dataTablesActionsButtonsTooltipPosition() {
    //Tooltip on the left for all datatables for buttons under "Действия" column
    $(document).ready(function () {
        var table = $('table.dataTable'); // Selects any initialized DataTable

        
            // Find the index of the "Действия" column
            var actionsColumnIndex = table.find('th').filter(function () {
                return $(this).text().trim() === "Действия";
            }).index();

            if (actionsColumnIndex === -1) return; // Exit if "Действия" column is not found

            // Apply logic whenever the table is drawn or updated
            table.on('draw.dt', function () {
                table.find('tbody tr').each(function () {
                    $(this).find('td').eq(actionsColumnIndex)
                        .find('.ui.tertiary.icon.button')
                        .attr('data-position', 'left center');
                });
            });

            // Apply immediately after initialization
            table.trigger('draw.dt');
        
    });
}