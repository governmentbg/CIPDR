
function showToast(classType, toastMessage) {
    let displayTime = 5000;
    $.toast({
        class: classType,
        message: toastMessage,
        displayTime: displayTime,
        compact: false
    });
}


$(function () {
    if ($('#ServerErrorMessage').val()) {
        showToast("error", $('#ServerErrorMessage').val());
        $('#ServerErrorMessage').val('');
    }
});

$(function () {
    if ($('#ServerWarningMessage').val()) {
        showToast("warning",$('#ServerWarningMessage').val());
        $('#ServerWarningMessage').val('');
    }
});

$(function () {
    if ($('#ServerSuccessMessage').val()) {
        showToast("success", $('#ServerSuccessMessage').val());
        $('#ServerSuccessMessage').val('');
    }
});


$(function () {
    if ($('#SwalServerErrorMessage').val()) {
        Swal.fire(
            "Грешка",
            $('#SwalServerErrorMessage').val(),
            'error',
        )
        $('#SwalServerErrorMessage').val('');
    }
});

$(function () {
    if ($('#SwalServerWarningMessage').val()) {
        Swal.fire(
            "Предупреждение",
            $('#SwalServerWarningMessage').val(),
            'warning',
        )
        $('#SwalServerWarningMessage').val('');
    }
});

$(function () {
    if ($('#SwalServerSuccessMessage').val()) {
        Swal.fire({
            title: 'Резултат',
            text: $('#SwalServerSuccessMessage').val(),
            confirmButtonText: "OK",
        });
        $('#SwalServerSuccessMessage').val('')
    }
});

