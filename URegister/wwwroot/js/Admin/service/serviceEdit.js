$(function () {
    $('.service-type-id').on('change', async function () {
        const form = '#formService';
        const view = await post_fetch_form_async('/Admin/Service/GetServiceSteps', form);
        $(form).find('.step-list').html(view);
        InitFormStep();
    });
    InitFormStep();

    $("#main-container").sortable({
        containment: "parent",
        scroll: false,
        cursor: "move",
        update: function () {
            updateOrder();
        }
    });

});

function InitFormStep() {
    $('.ui.dropdown').dropdown();
    $('.ui.accordion').accordion();
    initDynamicForms(function () {
             InitFormStep();
        },
        function(){
            return {
                serviceTypeId: $('.service-type-id').val()
            };
        }
    )
}

function updateOrder() {
    $("#main-container .draggable").each(function (index) {
        let indexOrder = index + 1;
        let inputOrder = $(this).find('.order-input')
        inputOrder.val(indexOrder);
    });
}
