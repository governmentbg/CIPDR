$(function () {
    $('.ui.dropdown').dropdown();

    $('#GroupList').on('change', function () {
        let selectedValues = $(this).val();
        $('#GroupIds').val(selectedValues.join(','));
    });
})