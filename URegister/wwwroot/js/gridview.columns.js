/*
GridView.Columns Pluggin

columns: {
            rowClick: '(string)[row callback function name]',
            rowClickArgs: (array)['columns names to pass'],
            rowUrl:'(string)[postback url on row clicked]',
            rowCss: function (row) {
               return (returns string) on condition ;
            },
            items: [
                {
                    titleColumn: '(string) [column name for title]',
                    title: '(string) [title]',
                    name: '(string) [name of a data column in row]',
                    cssClass: '(string) [column css class]',
                    render: function (el, row) {
                        //el - current value of the data column
                        //row - all row data
                        return (returns string) on condition ;
                    }
                }
                ]
           }
*/



function gridViewGenerateHtmlForColumns(state, data) {
    let result = data.reduce(
        (accumulator, row) => {
            let rowUrl = '#';
            if (typeof state.columns.rowUrl === 'function') {
                rowUrl = state.columns.rowUrl(row);
            }
            let rowClick = '';
            if (state.columns.rowClick) {
                rowClick = `onclick="${state.columns.rowClick}(`;
                if (state.columns.rowClickArgs.length > 0) {
                    for (var i = 0; i < state.columns.rowClickArgs.length; i++) {
                        rowClick += `'${row[state.columns.rowClickArgs[i]]}',`;
                    }
                }
                rowClick += ');return false;"';
            }
            let rowCss = '';
            if (typeof state.columns.rowCss === 'function') {
                rowCss = state.columns.rowCss(row);
            }
            let html = '<li class="list__item">'
                + `<a href="${rowUrl}" ${rowClick} class="gridview-card ${rowCss}">`
                + '<div class="gridview-card__body">'
                + '<div class="row">';

            html += state.columns.items.reduce(
                (accumulatorColumn, column) => {
                    const cssClass = column.cssClass || 'col-md';
                    let htmlColumn = `<div class="${cssClass}">`;
                    if (column.titleColumn) {
                        htmlColumn += `<span class="list__label">${row[column.titleColumn]}</span>`;
                    } else {
                        htmlColumn += `<span class="list__label">${column.title}</span>`;
                    }
                    if (column.render) {
                        htmlColumn += `<div class="list__output">${column.render(row[column.name], row)}</div>`;
                    } else {
                        let rowValue = row[column.name];
                        if (rowValue == null) {
                            rowValue = '';
                        }
                        htmlColumn += `<div class="list__output text-wrap">${rowValue}</div>`;
                    }
                    htmlColumn += '</div>'
                    return accumulatorColumn + htmlColumn;
                }, '');

            html += '</div></div></a></li>'
            return accumulator + html;
        }, '');
    return result;

}
