
function showToast(classType, toastMessage) {
    let displayTime = 5000;
    $.toast({
        class: classType === 'warning' ? 'inverted yellow' : classType,
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

