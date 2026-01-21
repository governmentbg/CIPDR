$(function () {
    const urlParams = new URLSearchParams(window.location.search);
    const param = urlParams.get('clearautosave');
    if (param) {
        console.log("clear auto save")
        localStorage.setItem('iscipr_autosave', "")
    }
});