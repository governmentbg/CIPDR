var GridViewInstances = [];

class GridView {

    gridViewMessages = {
        emptyText: 'Няма намерени елементи.',
        exportInExcel: 'Експорт в Ескел',
        buttonAdd: 'Добави',
        pagerAreaText: 'Странициране на документи',
        pagerShowAll: 'Виж всички',
        pagerSizeTitle: 'Покажи по',
        pagerFilterByTitle:'Филтрирай по',
        pagerAllItems: 'Всички',
        pagerOnThePage: 'на страница',
        pagerPageFirst:'Първа',
        pagerPagePrior: 'Предходна',
        pagerPageNext:'Следваща',
        pagerPageLast:'Последна'
    }

    constructor(options) {
        this.state = {};
        this.state.container = options.container;
        $(this.state.container).addClass('gridview-container');
        if (options.class) {
            $(this.state.container).addClass(options.class);
        }
        this.state.url = options.url;
        this.state.size = options.size ?? 10;
        this.state.xlsExport = options.xlsExport ?? false;
        this.state.pager = options.pager ?? true;
        this.state.sizeSelector = options.sizeSelector ?? true;
        this.state.method = options.method ?? 'POST';
        this.state.data = options.data;
        this.state.template = options.template;
        this.state.columns = options.columns;
        this.state.full_template = options.full_template;
        this.state.view_top_pages = options.view_top_pages ?? true;
        this.state.view_all_url = options.view_all_url;
        this.state.loader_text = options.loader_text || '';
        this.state.empty_text = options.empty_text || this.gridViewMessages.emptyText;
        this.state.grid_title = options.grid_title || '';
        this.state.grid_title_class = options.grid_title_class || '';
        this.state.addUrl = options.addUrl;
        this.state.loader = options.loader;
        if ((options.lang || 'bg') != 'bg') {
            this.gridViewMessages = gridViewTranslate(options.lang);
        }
        GridViewInstances.push(this);
        if (options.autoload != false) {
            $(this.state.container).html(this.state.loader_text);
            this.loadData(1);
        }
    }

    loadData(pageNo) {
        this.state.page = pageNo;
        var gridRequest = new Object();
        if (this.state.data)
            gridRequest.data = JSON.stringify(this.state.data());
        gridRequest.page = this.state.page;
        gridRequest.size = this.state.size;
        fetch(this.state.url,
            {
                method: this.state.method,
                body: JSON.stringify(gridRequest),
                headers: {
                    'Cache-Control': 'no-cache',
                    'Content-Type': 'application/json'
                }
            }
        )
            .then((response) => {
                return response.json();
            })
            .then((data) => {
                this.showData(data);
            }
            ).catch((error) => {
                console.log(error);
            });
    }

    exportData(exportFormat) {
        var gridRequest = new Object();
        if (this.state.data)
            gridRequest.data = JSON.stringify(this.state.data());
        gridRequest.exportFormat = exportFormat;
        let fileName = 'report.xlsx';
        fetch(this.state.url,
            {
                method: this.state.method,
                body: JSON.stringify(gridRequest),
                mode: 'no-cors',
                headers: {
                    'Cache-Control': 'no-cache',
                    'Content-Type': 'application/json'
                }
            }
        )
            .then(res => res.blob())
            .then((blob) => {

                var url = window.URL.createObjectURL(blob);
                var a = document.createElement('a');
                a.href = url;
                a.download = fileName;
                document.body.appendChild(a); // append the element to the dom
                a.click();
                a.remove(); // afterwards, remove the element  
            })
            .catch((error) => {
                console.log(error);
            });
    }

    showData(result) {
        if (!$(this.state.container).data('loaded')) {
            $(this.state.container).data('loaded', 'loaded');
        }
        if (result.data && result.data.length == 0 && this.state.empty_text) {
            $(this.state.container).html(this.state.empty_text);
            return;
        }
        var pages = [];
        if (result.total_pages > 1) {
            for (var i = 1; i <= result.total_pages; i++) {
                pages.push(i);
            }
        }
        result.container = this.state.container;

        let gridHtml = this.templateStart(result);
        if (typeof gridViewGenerateHtmlForColumns === 'function') {
            gridHtml += gridViewGenerateHtmlForColumns(this.state, result.data);
        }
        if (typeof gridViewGenerateHtmlForHandlebars === 'function') {
            gridHtml += gridViewGenerateHtmlForHandlebars(this.state, result.data);
        }
        gridHtml += this.templateEnd(result);

        $(this.state.container).html(gridHtml);

    }
    templateCurrentRows(data) {
        if (!this.state.view_top_pages && !this.state.view_all_url) {
            return '';
        }
        let from = data.size * (data.page - 1) + 1;
        let to = from + data.size - 1;
        if (to > data.total_rows) {
            to = data.total_rows
        }
        let result =
            '<div class="col-auto">' +
            '<div class="pagination">' +
            `<nav aria-label="${this.gridViewMessages.pagerAreaText}">`;
        if (this.state.view_top_pages) {
            result += `<li class="page-location">${from}-${to} от ${data.total_rows}</li>`;
        }
        result += '</ul>' +
            '</nav></div></div>';
        if (this.state.view_all_url) {
            result += `<div class="col-auto"><a href="${this.state.view_all_url}" class="">${this.gridViewMessages.pagerShowAll}</a></div>`;
        }
        return result;
    }
    setSize(newSize) {
        this.state.size = newSize;
        this.loadData(1);
    }

    templateSizeSelector(selectedSize) {
        var sizes = [];
        sizes.push(10);
        sizes.push(20);
        sizes.push(50);

        var result = '<form action="" class="u-form col-auto ms-auto row">';
        result += `<div class="u-form-cell col-auto"><label>${this.gridViewMessages.pagerSizeTitle} </label>`;
        result += `<select class="form-select" aria-label="${this.gridViewMessages.pagerFilterByTitle}" onchange="gridViewSetSize(\'' + this.state.container + '\',$(this).val());return false;">`
        for (var i = 0; i < sizes.length; i++) {
            var selected = '';
            if (selectedSize == sizes[i]) {
                selected = ' selected="selected"';
            }
            var allText = sizes[i].toString();
            if (sizes[i] > 1000) {
                allText = this.gridViewMessages.pagerAllItems;
            }
            result += '<option ' + selected + ' value="' + sizes[i] + '">' + allText + '</option>';
        }
        result += `</select><small>${this.gridViewMessages.pagerOnThePage}</small></div></form>`;
        return result;
    }

    templateExportXls() {
        var result = '<form action="" class="u-form col-auto ms-auto row">';
        result += `<div class="u-form-cell col-auto"><label onclick="gridViewExportData(\'' + this.state.container + '\',\'xls\');return false;">${this.gridViewMessages.exportInExcel}</label>`;
        result += '</div></form>';
        return result;
    }

    templateStart(data) {
        let startTemplate = '<section class="section">' +
            '<div class="section-header" >' +
            '<div class="row">' +
            `<div class="col-auto"><h2 class="section-title ${this.state.grid_title_class}">${this.state.grid_title}</h2>`;
        if (this.state.addUrl && this.state.addUrl.length > 0) {
            startTemplate += `<a href="${this.state.addUrl}" class="btn ms-5">${this.gridViewMessages.buttonAdd}</a>`;
        }
        startTemplate += '</div>';
        if (this.state.sizeSelector === true) {
            startTemplate += this.templateSizeSelector(data.size);
        }
        if (this.state.xlsExport === true) {
            startTemplate += this.templateExportXls();
        }
        startTemplate += this.templateCurrentRows(data);

        startTemplate += '</div>' +
            '</div>' +
            '<div class="section-body">' +
            '<ul class="list">';

        return startTemplate;
    }
    generatePageNumbers(page, totalPages) {
        let from = page - 5;
        if (from < 1) {
            from = 1;
        }
        let to = page + 5;
        if (to > totalPages) {
            to = totalPages;
        }
        let pageNumbers = [];
        for (var pageNo = from; pageNo <= to; pageNo++) {
            pageNumbers.push(pageNo);
        }
        return pageNumbers;
    }
    templateEnd(data) {
        let templateEnd = '</ul></div></section >';
        if (this.state.pager === true) {
            templateEnd += this.templatePager(data);
        }
        return templateEnd;
    }
    templatePager(data) {
        let result = '<div class="page-pagination">' +
            `<nav aria-label="${this.gridViewMessages.pagerAreaText}">` +
            '<ul class="pagination">';
        if (data.page > 1) {
            result += '<li class="page-item page-first">';
            result += `<a class="page-link" title="${this.gridViewMessages.pagerPageFirst}" href="#" onclick="gridViewLoadData('${data.container}',${1});return false;"><span class="visually-hidden">first</span></a>`;
            result += '</li>';
        } else {
            result += '<li class="page-item page-first page-inactive">';
            result += `<a class="page-link" title="${this.gridViewMessages.pagerPageFirst}"><span class="visually-hidden">first</span></a>`;
            result += '</li>';
        }

        if (data.page > 1) {
            result += '<li class="page-item page-prev">';
            result += `<a class="page-link" title="${this.gridViewMessages.pagerPagePrior}" href="#" onclick="gridViewLoadData('${data.container}',${data.page - 1});return false;"><span class="visually-hidden">first</span></a>`;
            result += '</li>';
        } else {
            result += '<li class="page-item page-prev page-inactive">';
            result += `<a class="page-link" title="${this.gridViewMessages.pagerPagePrior}"><span class="visually-hidden">first</span></a>`;
            result += '</li>';
        }

        let pageNumbers = this.generatePageNumbers(data.page, data.total_pages);
        for (var i = 0; i < pageNumbers.length; i++) {
            if (pageNumbers[i] == data.page) {
                result += '<li class="page-item active">';
                result += `<a class="page-link" href="#" onclick="return false;" >${pageNumbers[i]}</a>`;
            } else {
                result += '<li class="page-item">';
                result += `<a class="page-link" href="#" onclick="gridViewLoadData('${data.container}',${pageNumbers[i]});return false;">${pageNumbers[i]}</a>`;
            }
            result += '</li>';
        }

        if (data.page < data.total_pages) {
            result += '<li class="page-item page-next">';
            result += `<a class="page-link" title="${this.gridViewMessages.pagerPageNext}" href="#" onclick="gridViewLoadData('${data.container}',${data.page + 1});return false;"><span class="visually-hidden">next</span></a>`;
            result += '</li>';
        } else {
            result += '<li class="page-item page-next page-inactive">';
            result += `<a class="page-link" title="${this.gridViewMessages.pagerPageNext}"><span class="visually-hidden">next</span></a>`;
            result += '</li>';
        }
        if (data.page < data.total_pages) {
            result += '<li class="page-item page-last">';
            result += `<a class="page-link" title="${this.gridViewMessages.pagerPageLast}" href="#" onclick="gridViewLoadData('${data.container}',${data.total_pages});return false;"><span class="visually-hidden">last</span></a>`;
            result += '</li>';
        } else {
            result += '<li class="page-item page-last page-inactive">';
            result += `<a class="page-link" title="${this.gridViewMessages.pagerPageLast}" ><span class="visually-hidden">last</span></a>`;
            result += '</li>';
        }

        result += '</ul></nav></div>';
        return result;
    }

}

function gridViewFindByContainer(container) {
    for (var i = 0; i < GridViewInstances.length; i++) {
        if (GridViewInstances[i].state.container == container) {
            return GridViewInstances[i];
            break;
        }
    }
}

function gridViewSetSize(container, size) {
    let gridView = gridViewFindByContainer(container);
    gridView.setSize(size);
}

function gridViewExportData(container, exportFormat) {

    try {
        let gridView = gridViewFindByContainer(container);
        gridView.exportData(exportFormat);
    } catch (e) {
        console.log(e);
    }
}

function gridViewLoadData(container, page) {

    try {
        let gridView = gridViewFindByContainer(container);
        gridView.loadData(page || 1);
    } catch (e) {
        console.log(e);
    }
}

function gridViewDifferLoad(container) {
    if ($(container).data('loaded')) {
        return;
    }
    let gridView = gridViewFindByContainer(container);
    gridView.loadData(1);
}
