async function DeadlineChange(control)
{
    const processId = $('#ProcessId').val();
    const deadlineId = $(control).val();
    const response = await get_fetch_json_async('/Admin/Process/GetDeadlineDate', { deadlineId, processId })
    const deadlineDate = moment(response.dateSrok).format('DD.MM.YYYY');
    console.log(deadlineDate)
    $('[id$=DeadlineDate]')
        .closest('.dateonly-calendar')
        .calendar('set date', deadlineDate);
}
