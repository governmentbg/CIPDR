async function submitAutoSave() {
    try {
        const data = JSON.parse(localStorage.getItem('iscipr_autosave'))
        const formData = new FormData();
        Object.entries(data).forEach(([key, value]) => {
            formData.append(key, value);
        });

        const responce = await fetch('/admin/process/loadautosave', {
            method: "POST",
            body: formData,
        })
        const html = await responce.text();
        $('.content-autosave').html(html);
    } catch (e) {
        console.error(e);
    }
}
$(function () {
    submitAutoSave();
});