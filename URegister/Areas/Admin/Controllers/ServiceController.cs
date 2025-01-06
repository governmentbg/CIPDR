using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using URegister.Core.Contracts;
using URegister.Core.Models.Service;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ServiceController(
        IServiceService service,
        IFormConfigurationPersistenceService formConfigurationPersistenceService
        ) : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
        
        /// <summary>
        /// Списък на  услуги
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetServiceList(IDataTablesRequest request)
        {
            return await service.GetServiceList(request);
        }


        private async Task SetViewBag(int serviceTypeId)
        {
            ViewBag.ServiceTypeId_ddl = await service.GetServiceTypeDDL();
            ViewBag.StepId_ddl = await service.GetServiceStepDDL(serviceTypeId);
            ViewBag.FormParentId_ddl = await formConfigurationPersistenceService.GetFormsDDL();
        }

        /// <summary>
        /// Добавяне на услуга
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            await SetViewBag(0);
            var model = new ServiceVM();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Добавяне на услуга
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await service.GetService(id);
            await SetViewBag(model.ServiceTypeId);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на  тип услуга
        /// </summary>
        /// <param name="model">Модел на услуга</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await service.AppendUpdate(model);
                    SetSuccessMessage(model.IsInsert ? "Успешно добавена стъпка" : "Успешна редакция на стъпка");
                    return RedirectToAction(nameof(Edit), new { id = model.Id });
                }
                catch (Exception ex)
                {
                    SetErrorMessage("Проблем при запис!");
                }
            }
            await SetViewBag(model.ServiceTypeId);
            return View(model);
        }


        /// <summary>
        /// Зареждане на стъпки за тип услуга
        /// </summary>
        /// <param name="model">Модел на услуга</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetServiceSteps(ServiceVM model)
        {
            ModelState.Clear();
            await SetViewBag(model.ServiceTypeId);
            var steps = (List<SelectListItem>)ViewBag.StepId_ddl;
            model.Steps = model.Steps
                .Where(x => steps.Any(s => s.Value == x.StepId.ToString()))
                .ToList();
            model.Steps = model.Steps
                .OrderBy(x => x.OrderNum)
                .ThenBy(x => x.StepId)
                .ToList();
            return PartialView("_StepList", model);
        }

        /// <summary>
        /// Зареждане на диаграма
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public async Task<IActionResult> Flowchart(int serviceId)
        {
            string flowchart = "flowchart TD;";

            var steps = await service.GetServiceSteps(serviceId);

            List<string> listSteps = new List<string>();
            if (steps.Count > 0)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    if (i == 0)
                    {
                        listSteps.Add(steps[i].Id + "(" + steps[i].Title + ")");
                    }
                    else if (i != steps.Count - 1)
                    {
                        listSteps.Add(steps[i].Id + "[" + steps[i].Title + "];" + steps[i].Id);
                    }
                    if (i == steps.Count - 1)
                    {
                        listSteps.Add(steps[i].Id + "(" + steps[i].Title + ");");
                    }
                }
                flowchart += string.Join(" --> ", listSteps);
            }

            ViewBag.Flow = flowchart;
            return View();
        }

        /// <summary>
        /// Partial за стъпка 
        /// </summary>
        /// <param name="index">индекс в списък</param>
        /// <param name="prefix">html prefix</param>
        /// <returns></returns>
        public async Task<IActionResult> AddStep(int index, string prefix,  int serviceTypeId)
        {
            var model = new ServiceStepVM
            {
                Index = index,
                OrderNum = index + 1,
            };
            await SetViewBag(serviceTypeId);
            ViewData.TemplateInfo.HtmlFieldPrefix = string.IsNullOrEmpty(prefix) ? $"Steps[{index}]" : $"{prefix}.Steps[{index}]";
            return PartialView("_Step", model);
        }
    }
}
