function gridViewTranslate(lang) {
    switch (lang) {
        case 'en':
            return {
                emptyText: 'No elements found.',
                exportInExcel: 'Export as Excel',
                buttonAdd: 'Append',
                pagerAreaText: 'Paging',
                pagerShowAll: 'Show all',
                pagerSizeTitle: 'Show',
                pagerFilterByTitle: 'Filter by',
                pagerAllItems: 'All',
                pagerOnThePage: 'on page',
                pagerPageFirst: 'First',
                pagerPagePrior: 'Prior',
                pagerPageNext: 'Next',
                pagerPageLast: 'Last'
            }
        default:
            return {}
    }
}