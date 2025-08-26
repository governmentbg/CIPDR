function loadPdf(url, container) {
    var { pdfjsLib } = globalThis;

    // The workerSrc property shall be specified.
    pdfjsLib.GlobalWorkerOptions.workerSrc = '/library/pdf.js/pdf.worker.mjs';

    // Asynchronous download of PDF
    var loadingTask = pdfjsLib.getDocument(url);
    loadingTask.promise.then(function (pdf) {
        let numPages = pdf.numPages;

        for (let pageNumber = 1; pageNumber <= numPages; pageNumber++) {
            pdf.getPage(pageNumber).then(function (page) {

                let scale = 2;
                let viewport = page.getViewport({ scale: scale });

                // Prepare canvas using PDF page dimensions
                let canvas = getPdfPageContainer(page.pageNumber);
                container.appendChild(canvas);
                let context = canvas.getContext('2d');
                canvas.height = viewport.height;
                canvas.width = viewport.width;

                // Render PDF page into canvas context
                let renderContext = {
                    canvasContext: context,
                    viewport: viewport
                };

                let renderTask = page.render(renderContext);
                renderTask.promise.then(function () {
                    // Page rendered
                });
            });
        }


    }, function (reason) {
        // PDF loading error
        console.error(reason);
    });

    function getPdfPageContainer(pageNumber) {
        let sigSelector;

        try {
            sigSelector = signatureSelector;
        } catch (e) {
            sigSelector = undefined;
        }

        let canvas = document.createElement('canvas');
        canvas.classList.add('embeded-pdf-page');
        canvas.setAttribute('data-page', pageNumber);
        if (sigSelector !== undefined) {
            canvas.addEventListener('mousedown', sigSelector.mouseDownHandler);
            canvas.addEventListener('mousemove', sigSelector.mouseMoveHandler);
            canvas.addEventListener('mouseup', sigSelector.mouseUpHandler);
        }

        return canvas;
    }
}

function LoadPdfContainer() {
    document.addEventListener('DOMContentLoaded', function () {
        const canvas = document.getElementById('pdf-container');
        const fileUrl = canvas.dataset.fileurl;
        loadPdf(fileUrl, canvas);
    }, false);
}
LoadPdfContainer();
