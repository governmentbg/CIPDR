using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Contracts;
using URegister.Core.Models.Deadline;
using URegister.Infrastructure.Constants;

namespace URegister.Areas.Admin.Controllers
{
    /// <summary>
    /// Срокове на заявени услуги
    /// </summary>
    /// <param name="deadlineService"></param>
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    [Display(Name = "Срокове на заявени услуги")]
    public class DeadlineController(
        IDeadlineService deadlineService,
        INomenclatureClientService nomenclatureClient,
        IServiceService serviceService
        ) : BaseController
    {
        /// <summary>
        /// Зареждане на списък със срокове
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Зареждане на списък със срокове")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// списък със срокове
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък със срокове")]
        public async Task<IActionResult> GetDeadlineList(IDataTablesRequest request)
        {
            return await deadlineService.GetDeadlineList(request);
        }

        private async Task SetViewBag()
        {
            await nomenclatureClient.SetViewBagDeadline(ViewData);
            ViewBag.ServiceId_ddl = await serviceService.GetServiceDDL([]);
        }


        [Display(Name = "Зареждане на форма за добавяне на срок")]
        public async Task<IActionResult> Add()
        {
            await SetViewBag();
            var model = new DeadlineVM();
            return View(nameof(Edit), model);
        }

        [Display(Name = "Зареждане на форма за добавяне на срок")]
        public async Task<IActionResult> Edit(int id)
        {
            await SetViewBag();
            var model = await deadlineService.GetDeadline(id);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Запис на форма за добавяне на срок")]
        public async Task<IActionResult> Edit(DeadlineVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await deadlineService.SaveDeadline(model);
                    SetSuccessMessage(model.Id > 0 ? "Успешно добавена бланка" : "Успешна редакция на бланка");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    SetErrorMessage("Проблем при запис!");
                }
            }
            else
            {
                SetErrorMessage("Невалидни данни!");

            }
            await SetViewBag();
            return View(nameof(Edit), model);
        }
    }
}
