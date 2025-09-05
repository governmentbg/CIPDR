//$(function () {
//    openAccordionFilter();
//    $('form.ui.form').on('submit', function () {
//        //$('#page-loader').dimmer('show');
//        showLoader('body');
//    });
//});
$(function () {
    initializeComponents();
    openAccordionFilter();
    $(document).on('submit', 'form.ui.form', function (e) {
        e.preventDefault(); // Prevent default form submission
        showLoader('body');

        // Serialize form data
        var formData = $(this).serialize();

        // Send AJAX request
        $.ajax({
            url: $(this).attr('action'), // Use the form's action URL
            type: 'POST',
            data: formData,
            success: function (response) {
                // Replace or update the content with the response
                $('#statisticsContainer').html($(response).find('#statisticsContainer').html());
                hideLoader('body');
                openAccordionFilter();
                initializeComponents();
            },
            error: function (xhr, status, error) {
                console.error('√решка:', error);
                hideLoader('body');              
            }
        });
    });
});

function initializeComponents() {
    $('.dateonly-calendar').not(function () {
        return $(this).has('.ui.disabled').length > 0;
    }).calendar({
        type: 'date',
        monthFirst: false,
        formatter: {
            date: 'DD.MM.YYYY'
        },
        text: calendarTextConfig
    });
}