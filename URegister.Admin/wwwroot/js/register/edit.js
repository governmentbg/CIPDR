$(function () {
    $('.load-company-info').on("click", async function () {
        showLoader('body');
        const uic = $(".uic").val();
        const actionUrl = `/Register/GetCompanyData?uic=${uic}`;
        const response = await post_fetch_json_async(actionUrl, {});
        console.log(response.companyName)
        $('.company-name').val(response.companyName);
        hideLoader('body');
    });
});
