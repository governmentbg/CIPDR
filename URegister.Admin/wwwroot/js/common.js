
// Async get request
async function get_async(url, data) {
    return new Promise((resolve, reject) => {
        $.ajax({
            type: 'GET',
            async: true,
            cache: false,
            contentType: "application/json;charset=utf-8",
            dataType: 'json',
            url: url,
            data: data,
            success: function (result) {
                resolve(result);
            },
            error: function (err) {
                reject(err);
            },
        });
    });
}

// Async post request
async function post_async(url, data) {
    return new Promise((resolve, reject) => {
        $.ajax({
            type: 'POST',
            async: true,
            cache: false,
            dataType: 'json',
            url: url,
            data: data,
            success: function (result) {
                resolve(result);
            },
            error: function (err) {
                reject(err);
            },
        });
    });
}

// Async get request за dataType: 'string'
async function get_string_async(url, data) {
    return new Promise((resolve, reject) => {
        $.ajax({
            type: 'GET',
            async: true,
            cache: false,
            contentType: "application/json;charset=utf-8",
            dataType: 'text',
            url: url,
            data: data,
            success: function (result) {
                resolve(result);
            },
            error: function (err) {
                reject(err);
            },
        });
    });
}

async function get_drop_down_async(url, data, dropdown, selected) {
    const items = await get_fetch_json_async(url, data)
    fill_drop_down(items, dropdown, selected)
}

async function get_fetch_json_async(url, data) {
    const response = await fetch(url + "?" + new URLSearchParams(data));
    return await ResolveResponseJson(response);
}

async function get_fetch_string_async(url, data) {
    const response = await fetch(url + "?" + new URLSearchParams(data));
    return ResolveResponseString(response)
}

async function post_fetch_string_async(url, data) {
    const response = await fetch(url,
        {
            method: "POST",
            body: JSON.stringify(data),
            headers: {
                'Cache-Control': 'no-cache',
                'Content-Type': 'application/json',
                "X-CSRF-TOKEN": getRequestVerificationToken()
            }
        });
    return ResolveResponseString(response)
}

async function post_fetch_json_async(url, data) {
    const response = await fetch(url,
        {
            method: "POST",
            body: JSON.stringify(data),
            headers: {
                'Cache-Control': 'no-cache',
                'Content-Type': 'application/json',
                "X-CSRF-TOKEN": getRequestVerificationToken()
            }
        });
    return await ResolveResponseJson(response);
}

function showErrorModal(text = "Изтекла е потребителската сесия.") {
    const modalHtml = `
                        <div class="ui small modal center aligned" id="sessionExpiredModal">
                          <div class="center aligned header">Внимание</div>
                          <div class="center aligned content">
                            <i class="exclamation triangle icon" style="color: red; font-size: 2rem;"></i>
                            <p>${text}</p>
                          </div>
                          <div class="actions">
                            <div class="ui primary button ok-button">OK</div>
                          </div>
                        </div>
                      `;

    // Append the modal to the body
    $('body').append(modalHtml);

    // Initialize and show the modal
    $('#sessionExpiredModal')
        .modal({
            onHidden: function () {
                // Clean up after closing
                $('#sessionExpiredModal').remove();
            },
        })
        .modal('show');
}

async function ResolveResponseString(response) {
    if (response.redirected) {
        showErrorModal();
        $('#sessionExpiredModal .ok-button').on('click', function () {
            $('#sessionExpiredModal').modal('hide');
            window.location.href = window.location.href
            return null;
        });
    }
    let text = await response.text();
    return text;
}

async function ResolveResponseJson(response) {
    let text = "Възникна непредвидена грешка";
    const contentType = response.headers.get('content-type');
    if (response.status == 200 && contentType.startsWith('application/json;')) {
        try {
            return await response.json();
        }
        catch (e) {
            messageHelper.ShowErrorMessage(text);
        }
    } else {
        if (response.redirected) {
            showErrorModal();
            $('#sessionExpiredModal .ok-button').on('click', function () {
                $('#sessionExpiredModal').modal('hide');
                window.location.href = window.location.href
            });
        }
        else {
            showErrorModal(await response.text());
            $('#sessionExpiredModal .ok-button').on('click', function () {
                $('#sessionExpiredModal').modal('hide');
            });
        }
    }
}

function deleteItemWithConfirmation(deleteUrl, callback = null) {
    $('.confirm-delete')
        .modal({
            centered: true,
            closable: false,
            onApprove: function () {
                let url = deleteUrl;
                get_async(url)
                    .then((result) => {
                        if (callback !== null) {
                            callback();
                        }
                        else if (result.redirectUrl) {
                            window.location.href = result.redirectUrl;
                        }
                        else {
                            window.location.reload();
                        }
                    })
                    .catch((error) => {
                        console.error('Грешка при изтриване от URL ' + deleteUrl + " : " + error.statusText);
                    });
            }
        })
        .modal('show');
};

var messageHelper = (function () {
    function ShowErrorMessage(message) {
        showToast("error", message);
    }

    function ShowSuccessMessage(message) {
        showToast("success", message);
    }

    function ShowWarning(message) {
        showToast("warning", message);
    }

    return {
        ShowErrorMessage: ShowErrorMessage,
        ShowSuccessMessage: ShowSuccessMessage,
        ShowWarning: ShowWarning
    };
})();

function fomanticConfirm(text, callBackApprove) {
    // Create the modal dynamically
    const modalHtml = `
        <div class="ui small modal center aligned" id="fomantic-confirm-modal">
            <div class="center aligned header">Потвърди</div>
            <div class="center aligned content">
                <p>${text}</p>
            </div>
            <div class="actions">
                <button class="ui red cancel button">Отказ</button>
                <button class="ui green approve button">Потвърди</button>
            </div>
        </div>
    `;

    // Append the modal to the body
    $('body').append(modalHtml);

    // Initialize the modal
    const $modal = $('#fomantic-confirm-modal');

    $modal.modal({
        onApprove: function () {
            callBackApprove();
        },
        onHidden: function () {
            $modal.remove();
        },
    }).modal('show');
}

async function PerformAddItem(addItem, addData) {
    const containerId = addItem.data("container-id");
    const container = containerId ?
        $(`#${containerId}`) :
        $(addItem).parents('.dynamic-form-container:first');
    const index = container.data('index');
    const prefix = container.data('prefix');
    const beforebtn = addItem.data('beforebtn');
    let data = {
        index,
        prefix
    };
    if (addData) {
        data = { ...data, ...addData };
    }
    const html = await get_string_async(addItem.data('url'), data);
    container.data('index', index + 1);
    if (beforebtn) {
        $(html).hide().insertBefore(addItem.parent()).slideDown();
    } else {
        $(html).hide().appendTo(container).slideDown();
    }

    let form = container.parents('form:first');
    form.removeData("validator")    // Added by jQuery Validation
        .removeData("unobtrusiveValidation");   // Added by jQuery Unobtrusive Validation
    $.validator.unobtrusive.parse(form);
}
function initDynamicForms(addCallback) {

    $('button.add-item').each(function (i, btn) {
        if ($(btn).data("is-set-click") !== undefined)
            return;
        $(btn).click(async function () {
            const addItem = $(this);
            await PerformAddItem(addItem);
            if (addCallback) {
                addCallback();
            }
            return false;
        });
        $(btn).data("is-set-click", true);
    });

    async function PerformRemoveItem(removeLink) {
        if (removeLink.data('alert')) {
            fomanticConfirm(removeLink.data('alert'), function () { removeLink.parents('.item-template:first').hide('normal').remove() });
        } else {
            removeLink.parents('.item-template:first').hide('normal').remove();
        }
    }

    $('button.remove-item').each(function (i, btn) {
        if ($(btn).data("is-set-click") !== undefined) {
            return;
        }
        $(btn).click(async function () {
            const removeLink = $(this);
            await PerformRemoveItem(removeLink);
        });
        $(btn).data("is-set-click", true);
    });
}


function getValueLongNullable(el) {
    return $(el).val() || null;
}

function getValueDateTimeNullable(el) {
    moment.locale('bg');
    return moment($(el).val(), 'DD.MM.YYYY').toDate() || null;
}

function getFormData($form) {
    var unindexed_array = $form.serializeArray();
    var indexed_array = {};

    $.map(unindexed_array, function (n, i) {
        indexed_array[n['name']] = n['value'];
    });

    return indexed_array;
}


function fill_drop_down(items, dropdown, selected) {
    const ddl = items.reduce(
        (accumulator, currentValue) => {
            const isSelected = (currentValue.value == selected ? "selected" : "");
            return accumulator + (currentValue.value == null ?
                `<option value ${isSelected}>${currentValue.text}</option>` :
                `<option value="${currentValue.value}" ${isSelected}>${currentValue.text}</option>`);
        }
        , '');
    dropdown.html(ddl);
}


async function downloadFile(url, request) {
    let fileName = 'report.xlsx';
    $('#ajaxLoader').hide();
    try {
        const res = await fetch(
            url,
            {
                method: "POST",
                body: JSON.stringify(request),
                headers: {
                    'Cache-Control': 'no-cache',
                    'Content-Type': 'application/json',
                    "X-CSRF-TOKEN": getRequestVerificationToken()
                }
            });
        const header = res.headers.get('Content-Disposition');
        const parts = header.split(';');
        fileName = parts[1].split('=')[1];
        const blob = await res.blob();
        var url = window.URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a); // append the element to the dom
        a.click();
        a.remove(); // afterwards, remove the element  
    } catch (e) {
        console.error(e);
    }
    $("body").css("cursor", "default");
}

function replaceAll(str, find, replace) {
    return str.replace(new RegExp(find, 'g'), replace);
}

async function post_fetch_async(url, request) {
    try {
        const res = await fetch(
            url,
            {
                method: "POST",
                body: JSON.stringify(request),
                headers: {
                    'Cache-Control': 'no-cache',
                    'Content-Type': 'application/json',
                    "X-CSRF-TOKEN": getRequestVerificationToken()
                }
            });
        return await res.json();
    } catch (e) {
        console.error(e);
    }
}

async function post_fetch_form_async(url, formSelector) {
    try {
        const form = $(formSelector)
        if (url == '') {
            url = form.prop('action')
        }
        const data = new FormData(form[0]);
        const responce = await fetch(url, {
            method: 'post',
            body: data,
        })
        return await responce.text();
    } catch (e) {
        console.error(e);
    }
}
function getRequestVerificationToken() {
    return document.getElementsByName("__RequestVerificationToken")[0].value;
}

function htmlEncode(input) {
    return input
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function JsonBGdate(value) {
    if (!value) {
        return '';
    }
    try {
        let date = Date.parse(value);
        return new Intl.DateTimeFormat('bg-BG').format(date);
    }
    catch (e) {
        console.log(value);
        return '';
    }
}
function JsonBGdateTS(value) {
    if (!value) {
        return '';
    }
    try {
        return new Intl.DateTimeFormat('bg-BG').format(value.seconds * 1000);
    }
    catch (e) {
        console.log(value);
        return '';
    }
}

function JsonBGdateTSWithTime(value) {
    if (!value) {
        return '';
    }
    try {
        return new Intl.DateTimeFormat('bg-BG', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
            hour12: false
        }).format(new Date(value.seconds * 1000));
    }
    catch (e) {
        console.log(value);
        return '';
    }
}

function JsonBGDateTime(value) {
    if (!value) {
        return '';
    }
    try {
        let date = Date.parse(value);
        return new Intl.DateTimeFormat('bg-BG', {
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit"
        }).format(date);
    }
    catch (e) {
        console.log(value);
        return '';
    }
}

function JsonBGDateTimeWithSeconds(value) {
    if (!value) {
        return '';
    }
    try {
        let date = Date.parse(value);
        return new Intl.DateTimeFormat('bg-BG', {
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit"
        }).format(date);
    }
    catch (e) {
        console.log(value);
        return '';
    }
}

function logValidationError(form) {
    var formerrorList = $(form).data("validator").errorList;
    $.each(formerrorList, function (key, value) {
        console.log(formerrorList[key].element.id);
    });
}

async function ResolveIsOkResponse(response) {
    let result = await ResolveResponseJson(response)
    if (result.state == "OK") {
        return true;
    } else {
        messageHelper.ShowErrorMessage(result.message)
    }
}
function StartButtonAction(btn) {
    $(btn).prop('disabled', true);
}
function EndButtonAction(btn) {
    $(btn).prop('disabled', false);
}

function refreshTable(dataTableID) {
    $(dataTableID).DataTable().ajax.reload(null, true);
    return true;
}

async function confirmDialog(title, text, action) {
    const dialog = `<div class="ui small modal confirm-dialog">
        <div class="header">${title}</div>
        <div class="content">
            <p>${text}</p>
        </div>
        <div class="actions">
            <button class="ui positive right labeled icon button">${action}<i class="check icon"></i></button>
            <button class="ui negative button">Затвори</button>
        </div>
    </div>`;
    return new Promise((resolve, reject) => {
        $(dialog).modal({
            centered: true,
            closable: false,
            onApprove: function () {
                resolve(true);
            },
            onDeny: function () {
                resolve(false);
            }
        })
            .modal('show');
    });
}

// Async post request
async function upload_file_async(url, data) {
    return new Promise((resolve, reject) => {
        $.ajax({
            type: 'POST',
            async: true,
            cache: false,
            dataType: 'json',
            url: url,
            data: data,
            processData: false,
            contentType: false,
            success: function (result) {
                resolve(result);
            },
            error: function (err) {
                reject(err);
            },
        });
    });
}

function setDataTablesFilter(container, storageName) {
    return;
    const elements = $(container).find('input, select, textarea');
    let obj = {};
    elements.each(function () {
        const val = $(this).val()
        obj[this.name] = val;
    })
    localStorage.setItem(storageName, JSON.stringify(obj))
}

function getDataTablesFilter(container, storageName) {
    return;
    const clear_filter = $(container).find('#ClearFilter').val();
    if (clear_filter && clear_filter.toString().toLowerCase() == "true") {
        return;
    }
    const obj = JSON.parse(localStorage.getItem(storageName))
    for (let key in obj) {
        $(`#${key}`).val(obj[key]);
    }
    openAccordionFilter()
}

function openAccordionFilter() {
    $('.accordion').accordion("open", 0);
}

async function uploadRegisterFile(url, file) {
    showLoader('body');
    var data = new FormData()
    data.append('file', file)
    const response = await fetch(url,
        {
            method: "POST",
            body: data,
            headers: {
                "X-CSRF-TOKEN": getRequestVerificationToken()
            }
        });
    hideLoader('body');
    return await ResolveResponseJson(response);
}
