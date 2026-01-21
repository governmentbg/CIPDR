using DataTables.AspNet.Core;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Contracts;
using URegister.Core.Models.Service;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.RegistersCatalog;
using static URegister.Users.AppUserManager;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    [Display(Name = "Услуги")]
    public class ServiceController(
        IServiceService service,
        IRegisterService registerService,
        IFormConfigurationPersistenceService formConfigurationPersistenceService,
        INomenclatureClientService nomenclatureClient,
        RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
        AppUserManagerClient appUserManagerClient,
        ILogger<ServiceController> logger) : BaseController
    {
        [Display(Name = "Зареждане на списък с услуги")]
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
        [Display(Name = "Извличане на списък с услуги")]
        public async Task<IActionResult> GetServiceList(IDataTablesRequest request)
        {
            return await service.GetServiceList(request);
        }


        private async Task SetViewBag(int serviceTypeId)
        {
            ViewBag.ServiceTypeId_ddl = await service.GetServiceTypeDDL();
            ViewBag.StepId_ddl = await service.GetStepDDL();
            ViewBag.FormParentId_ddl = await formConfigurationPersistenceService.GetFormsDDL();
            var roles = await appUserManagerClient.GetRolesAsync(new Empty());
            ViewBag.Roles_ddl = roles.Roles
                                      .Where(r => !r.Name.Equals(UserRoles.GlobalAdmin))
                                      .Select(r => new SelectListItem
                                      {
                                          Value = r.RoleId.ToString(),
                                          Text = r.Label
                                      })
                                      .ToList();
            await nomenclatureClient.SetViewBagProcess(ViewData);
        }

        /// <summary>
        /// Добавяне на услуга
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне на нова услуга")]
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
        [Display(Name = "Зареждане на форма за редакция на услуга")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await service.GetService(id, true);
            await SetViewBag(model.ServiceTypeId);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на  тип услуга
        /// </summary>
        /// <param name="model">Модел на услуга</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Запис или редакция на услуга")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceVM model)
        {
            if (!ModelState.IsValid) {
                RemoveErrorForNotUsed();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    OperationResult result = await service.AppendUpdate(model);

                    if (!result.IsSuccess)
                    {
                        SetErrorMessage(result.ErrorMessage);
                        await SetViewBag(model.ServiceTypeId);
                        return View(nameof(Edit), model);
                    }
                    var registerId = await registerService.GetCurrentRegisterId();
                    await registerGrpcClient.SaveServiceAsync(new ServiceItem
                    {
                        RegisterId = registerId,
                        ServiceId = model.Id,
                        EformCode=model.EFormCode,
                        ServiceTypeId = model.ServiceTypeId,
                        IsActive = true,
                    });

                    SetSuccessMessage(model.IsInsert ? "Успешно добавена тип услуга" : "Успешна редакция на тип услуга");
                    return RedirectToAction(nameof(Index), new { id = model.Id });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Проблем при запис в {nameof(Edit)}!");
                    SetErrorMessage("Проблем при запис!");
                }
            }
            else 
            {
                SetErrorMessage("Невалидни данни!");
            }
            await SetViewBag(model.ServiceTypeId);
            return View(nameof(Edit), model);
        }


        /// <summary>
        /// Зареждане на стъпки за тип услуга
        /// </summary>
        /// <param name="model">Модел на услуга</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на стъпки за услуга")]
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
        [Display(Name = "Генериране на диаграма за стъпките на услуга")]
        public async Task<IActionResult> Flowchart(int serviceId)
        {
            string flowchart = "flowchart TD;";

            var steps = await service.GetServiceSteps(serviceId);

            List<string> listSteps = new List<string>();
            if (steps.Count > 0)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    if (i == 0 || i == steps.Count - 1)
                    {
                        listSteps.Add(steps[i].Id + "(" + steps[i].Title + ")");
                    }
                    else
                    {
                        listSteps.Add(steps[i].Id + "[" + steps[i].Title + "]");
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
        [Display(Name = "Добавяне на стъпка към услуга")]
        public async Task<IActionResult> AddStep(int index, string prefix, int serviceTypeId)
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

        /// <summary>
        /// Изтриване на услуга
        /// </summary>
        /// <param name="id">Идентификатор на услуга за изтриване</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Изтриване на услуга")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            OperationResult deleteResult = await service.Delete(id);

            if (deleteResult.IsSuccess)
            {
                var registerId = await registerService.GetCurrentRegisterId();
                var model = await service.GetService(id);
                await registerGrpcClient.SaveServiceAsync(new ServiceItem
                {
                    RegisterId = registerId,
                    ServiceId = id,
                    EformCode = model.EFormCode,
                    ServiceTypeId = model.ServiceTypeId,
                    IsActive = false,
                });
                SetSuccessMessage("Услугата е изтрита успешно");
            }
            else
            {
                SetErrorMessage(deleteResult.ErrorMessage);
            }

            return Json(null);
        }

        private void RemoveErrorForNotUsed()
        {
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                                     .Select(x => new { x.Key, x.Value.Errors })
                                     .ToList();
            foreach (var error in errors)
            {
                if (error.Key.EndsWith("].Name"))
                {
                    ModelState.Remove(error.Key);
                }
            }
        }
    }
}
