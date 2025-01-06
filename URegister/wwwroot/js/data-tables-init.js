(function () {
    $.fn.dataTable.ext.buttons.io_excel = {
        extend: 'excel',
        text: '<i class="file excel outline icon"></i>',
        titleAttr: 'Excel',
        className: 'basic',
        exportOptions: {
            "columns": "thead th:not(.noExport)"
        }
    };

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
            '<"ui left aligned five wide column dt buttons"B>' + // Custom buttons in the center
            '<"ui left aligned three wide column dt length">' + // l Length change control on the left
            '<"ui left aligned three wide column dt no-add-button">' + // l Length change control on the left
            '<"ui right aligned five wide column dt search"f>' + // Search filter on the right
            '<"ui three wide right aligned column custom buttons">' + // button add
            '>' +
            'rt>' +
            '<"ui padded grid row"' +
            '<"ui row"' +
            '<"ui seven wide column dt-info"i>' + // Info on the left
            '<"ui nine wide column right aligned dt-pagination"p>' + // Pagination on the right
            '>' +
            '>',
        buttons: ['io_pageLength', 'io_colvis', 'io_excel', 'io_pdf', 'io_print'],
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

        $('.ui.three.wide.right.aligned.column.custom.buttons').html(markup);
        $('.no-add-button').hide()
    }
    else
    {
        $('.no-add-button').show()
        $('.ui.three.wide.right.aligned.column.custom.buttons').hide();
    }
}


