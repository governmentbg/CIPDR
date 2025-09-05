$(function () {
    $('#SourceType').change();
});

function blankSourceTypeChange(control)
{
    const sourceType = $(control).val();
    if (sourceType == 1) {
        $('.container-service-id').show();
    } else {
        $('.container-service-id').hide();
    }
}
