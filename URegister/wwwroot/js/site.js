// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(function () {
    $('.select2-drop-down').select2({
        theme: 'bootstrap-5'
    });
});

function ShowModalDialog(title, html, wide, big) {
    let modalDialog = $('.master-modal').find('.modal-dialog');
    modalDialog.removeClass('wide');
    modalDialog.removeClass('mw-100');
    modalDialog.find('.modal-content')
    modalDialog.removeClass('w-75');
    if (wide) {
        modalDialog.addClass('wide');
    }
    modalDialog.find('.modal-title').text(title);
    modalDialog.find('.modal-form').html(html);
    if (big) {
        modalDialog.addClass('mw-100');
        modalDialog.addClass('w-75');
    }
    modalDialog.find('.select2').select2({
        dropdownAutoWidth: true,
        theme: 'bootstrap-5',
        dropdownParent: ".master-modal",
    });

    // 
    $('.master-modal').modal({ backdrop: 'static', keyboard: false })
    $('.master-modal').modal("show");
}

function MakeBigModalDialog(container) {
    var _width = $(window).width() * 90 / 100;
    var _height = $(window).height() * 80 / 100;
    $(container)
        .dialog({
            modal: true, width: _width, height: _height
        });
}

function HideModal() {
    $('.master-modal').modal('hide');
}

function LogOut()
{
    $('#formLogOut').submit();
}


function GoHomeIndex() {
    window.location.href = "/Home/Index"
}

function editButton(url) {
    return `<a href="${url}" data-tooltip="Редакция" class="ui tertiary icon button">
               <i class="edit icon"></i>
            </a>`;
}