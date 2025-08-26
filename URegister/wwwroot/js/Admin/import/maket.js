$(function () {
      $('.upload-file-input').change(function () {
        var selectedFiles = $(this).prop("files");
        const parent = $(this).parent();
        if (selectedFiles.length > 0) {
            parent.find('.selected-file').text(selectedFiles[0].name);
            $("#button_submit").removeAttr("disabled");
        } else {
            parent.find('.selected-file').text('');
            $("#button_submit").attr('disabled', 'disabled')
        }
    });
});

function showMaketModal() {
    $('#uploadModal').modal('show');
}


