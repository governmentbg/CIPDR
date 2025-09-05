$(function () {
    $('#FormParentId').on("change", function () { getFormFields() });
});

function addTepmlateParam(name, label) {
    let activeCKE = window.ckEditorInstance;
    const viewFragment = activeCKE.data.processor.toView(`{{${name}:${label}}}`);
    const modelFragment = activeCKE.data.toModel(viewFragment);
    activeCKE.model.insertContent(modelFragment, activeCKE.model.document.selection);
}

async function getFormFields() {
    const formParentId = $('#FormParentId').val();
    const templateParams = await post_fetch_json_async(`/Admin/BlanksTemplate/GetFormFields?formParentId=${formParentId}`, {});
    let templateParamsHtml = '';
    templateParams.forEach((templateParam) => {
        templateParamsHtml += `<div><a href="javascript:addTepmlateParam('${templateParam.name}','${templateParam.label}')" data-tooltip="${templateParam.label}" class="ui tertiary icon button"><i class="plus icon">${templateParam.name}</i></a></div>`;
    })
    $('#divFields').html(templateParamsHtml)
}

