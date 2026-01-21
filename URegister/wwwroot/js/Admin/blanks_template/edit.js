function blankSourceTypeChange(control)
{
    const sourceType = $(control).val();
    if (sourceType == 1) {
        $('.container-service-id').show();
    } else {
        $('.container-service-id').hide();
    }
    if (sourceType == 2) {
        $('.container-register-number').show();
    } else {
        $('.container-register-number').hide();
    }
}

function SignByOperatorClick(control) {
    const parent = $(control).parents('.item-template:first');
    if ($(control).is(':checked')) {
        parent.find(".role-item").hide();
    } else {
        parent.find(".role-item").show();
    }
}

$(function () {
    $('#SourceType').change();
    InitFormSignature();

    $("#main-container").sortable({
        containment: "parent",
        scroll: false,
        cursor: "move",
        update: function () {
            updateOrder();
        }
    });

});

function InitFormSignature() {
    $('.ui.dropdown').dropdown();
    $('.ui.accordion').accordion();
    initDynamicForms(function () {
        InitFormSignature();
    });
}

function updateOrder() {
    $("#main-container .draggable").each(function (index) {
        let indexOrder = index + 1;
        let inputOrder = $(this).find('.order-input')
        inputOrder.val(indexOrder);
    });
}
