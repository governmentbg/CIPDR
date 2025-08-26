var fields = [];

$(function () {
    $('#ServiceId').on("change", function () {
        const serviceId = $('#ServiceId').val()
        getFormInfo(serviceId)
    });
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

async function getFormInfo(serviceId) {
    const response = await post_fetch_json_async(`/Admin/Process/getFormInfo?serviceId=${serviceId}`, { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() });
    field = response.fields;
    $('#FormName').text(response.formName)
}


