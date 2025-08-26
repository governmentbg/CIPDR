using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Contracts;
using URegister.Core.Models;

namespace URegister.Admin.Controllers
{
    public class CalendarController(
        ICalendarService calendarService,
        INomenclatureClientService nomenclatureClientService,
        ILogger<CalendarController> logger
    ) : BaseController
    {
        [Display(Name = "Начална страница на Календар")]
        public IActionResult Index()
        {
            CalendarFilterVM model = new()
            {
                DateFrom = DateTime.Now,
                DateTo = new DateTime(DateTime.Now.Year, 12, 31)
            };

            return View(model);
        }

        /// <summary>
        /// Списък на заявени услуги
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък календар")]
        public async Task<IActionResult> GetCalendarList(IDataTablesRequest request, CalendarFilterVM filter)
        {
            return await calendarService.GetCalendarList(request, filter);
        }

        /// <summary>
        /// Добавяне на неработен ден
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Добавяне на неработен ден")]
        public async Task<IActionResult> Add()
        {
            await nomenclatureClientService.SetViewBagCalendar(ViewData);
            CalendarVM model = new()
            {
                CurrentDate = DateTime.Today,
            };
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Добавяне на неработен ден
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Редакция на работен/неработен ден")]
        public async Task<IActionResult> Edit(int id)
        {
            await nomenclatureClientService.SetViewBagCalendar(ViewData);
            var  model = await calendarService.GetCalendarVM(id);
            return View(model);
        }

        /// <summary>
        /// Запис на работен/неработен ден
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Запис на работен/неработен ден")]
        public async Task<IActionResult> Edit(CalendarVM model)
        {
            await nomenclatureClientService.SetViewBagCalendar(ViewData);
            try
            {
                if (ModelState.IsValid)
                {
                    await calendarService.SaveCalendar(model);
                    SetSuccessMessage("Успешен запис");
                    return RedirectToAction("Index");
                } 
                else
                {
                    SetErrorMessage($"Невалидни данни!");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                SetErrorMessage($"Проблем при запис!");
            }
            return View(model);
        }

    }
}
