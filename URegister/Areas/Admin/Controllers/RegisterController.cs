using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OpenDataClient;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Contracts;
using URegister.Core.Models.CurrentRegister;
using URegister.Core.Models.OpenData;
using URegister.Infrastructure.Constants;
using URegister.RegistersCatalog;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Manager}, {UserRoles.Editor}")]
    [Display(Name = "Регистър")]
    public class RegisterController(
        INomenclatureClientService nomenclatureClient,
        IRegisterService registerService,
        IRegisterClientService registerClient,
        RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
        IUserContext userContext,
        IOpenDataClientService openDataClient,
        ILogger<RegisterController> logger
    ) : BaseController
    {
        /// <summary>
        /// Списък регистри
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Зареждане на списък с регистри")]
        public IActionResult IndexAdministration()
        {
            return View();
        }

        /// <summary>
        /// Списък на администрации към регистърa
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с администрации към регистър")]
        public async Task<IActionResult> GetAdministrationList(IDataTablesRequest request)
        {
            // return await registerService.GetAdministrationList(request);
            var filter = new Core.Models.Register.AdministrationFilterVM
            {
                RegisterId = await registerService.GetCurrentRegisterId()
            };
            return await registerClient.GetAdministrationList(request, filter);
        }


        /// <summary>
        /// Страница за редакция на регистър
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за редакция на регистър")]
        public async Task<IActionResult> Edit()
        {
            await nomenclatureClient.SetViewBagRegister(ViewData);
            var model = await registerService.GetCurrentRegister();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на регистър
        /// </summary>
        /// <param name="model">Модел на регистъра</param>
        /// <returns></returns>
        //[HttpPost]
        //[Display(Name = "Редактиране на регистър")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(RegisterVM model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            await registerService.SaveRegister(model);
        //            SetSuccessMessage("Успешно редакция");
        //            return RedirectToAction("Index", "Home", new { area = string.Empty });
        //        }
        //        catch (Exception ex)
        //        {
        //            {
        //                logger.LogError(ex, "Проблем при запис на данни за регистър");
        //                SetErrorMessage($"Проблем при запис!{Environment.NewLine}{ex.Message}");
        //            }
        //        }
        //    }
        //    await nomenclatureClient.SetViewBagRegister(ViewData);
        //    return View(nameof(Edit), model);
        //}

        /// <summary>
        /// Списък от оторозирани лица
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Зареждане на списък с упълномощени лица")]
        public IActionResult IndexPerson(Guid administrationId)
        {
            var filter = new PersonFilterVM
            {
                AdministrationId = administrationId,
            };
            return View(filter);
        }

        /// <summary>
        /// Списък на оторозирани лица към администрация
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с упълномощени лица към администрация")]
        public async Task<IActionResult> GetPersonList(IDataTablesRequest request, Core.Models.Register.PersonFilterVM filter)
        {
            return await registerClient.GetPersonList(request, filter);
            //  return await registerService.GetPersonList(request, filter.AdministrationId);
        }

        /// <summary>
        /// Параметри на OpenData към администрация
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = $"{UserRoles.Admin}")]
        [Display(Name = "Параметри на OpenData към администрация към регистър")]
        public async Task<IActionResult> OpenDataAdministration(Guid administrationId)
        {
            var registerId = await registerService.GetCurrentRegisterId();
            var response = await registerGrpcClient.GetOpenDataParamAsync(new OpenDataParamRequest
            {
                AdministrationId = administrationId.ToString(),
                RegisterId = registerId
            });
            var model = new OpenDataAdministrationVM
            {
                ApiKey = response.Data.ApiKey,
                OrganizationId = response.Data.OrganisationId,
                AdministrationId = administrationId,
                AdministrationName = response.Data.AdministrationName,
                FrequencyAdministrationId = response.Data.FrequencyAdministrationId,
                FrequencyId = response.Data.FrequencyId,
                RegisterId = registerId,
            };
            await ViewBagOpenDataAdministration();
            return View(model);
        }

        /// <summary>
        /// Запис параметри на OpenData към администрация
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = $"{UserRoles.Admin}")]
        [Display(Name = "Параметри на OpenData към администрация към регистър")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenDataAdministration(OpenDataAdministrationVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var registerId = await registerService.GetCurrentRegisterId();
                    await registerGrpcClient.SaveOpenDataRegisterAdministrationAsync(new OpenDataRegisterAdministrationSaveRequest
                    {
                        RegisterId = registerId,
                        AdministrationId = model.AdministrationId.ToString(),
                        FrequencyId = model.FrequencyId
                    });
                    SetSuccessMessage("Успешeн запис");
                    return RedirectToAction("OpenDataAdministration", new {asministrationId = model.AdministrationId, redirect = true});
                }
                catch (Exception ex)
                {
                    {
                        logger.LogError(ex, "Проблем при запис на данни за OpenData за администрация");
                        SetErrorMessage($"Проблем при запис!{Environment.NewLine}{ex.Message}");
                    }
                }
            }
            await ViewBagOpenDataAdministration();
            return View(model);
        }
        /// <summary>
        /// Стартиране на OpenData
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = $"{UserRoles.Admin}")]
        [Display(Name = "Стартиране на OpenData")]
        public async Task<IActionResult> OpenDataRegister()
        {
            var registerId = await registerService.GetCurrentRegisterId();
            var response = await registerGrpcClient.GetOpenDataParamAsync(new OpenDataParamRequest
            {
                AdministrationId = userContext.AdministrationId.ToString(),
                RegisterId = registerId
            });
            await ViewBagOpenDataRegister();
            var model = new OpenDataRegisterVM
            {
                CategoryId = response.Data.CategoryId,
            };
            return View(model);
        }

        private async Task ViewBagOpenDataRegister()
        {
            var ddl = (await openDataClient.ListDataCategoriesAsync()).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToList();
            ddl.Insert(0,
                new SelectListItem
                {
                    Value = "0",
                    Text = "Не се изпращат данни към OpenData",
                });
            ViewBag.CategoryId_ddl = ddl;
        }

        private async Task ViewBagOpenDataAdministration()
        {
            var ddl = (await openDataClient.GetUserOrganisationsAsync()).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToList();
            ddl.Insert(0,
                new SelectListItem
                {
                    Value = "0",
                    Text = "Не е избрана организация съответстваща на администрацията",
                });
            ViewBag.OrganizationId_ddl = ddl;
            await nomenclatureClient.SetViewBagOpenDataAdministration(ViewData, false);
        }

        /// <summary>
        /// Запис параметри на OpenData към администрация
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = $"{UserRoles.Admin}")]
        [Display(Name = "Запис на Стартиране на OpenData")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenDataRegister(OpenDataRegisterVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var registerId = await registerService.GetCurrentRegisterId();
                    await registerGrpcClient.SaveOpenDataRegisterAsync(new OpenDataRegisterSaveRequest
                    {
                        RegisterId = registerId,
                        CategoryId = model.CategoryId,
                    });
                    SetSuccessMessage("Успешeн запис");
                    return RedirectToAction("Index", "PublicFieldTemplate");
                }
                catch (Exception ex)
                {
                    {
                        logger.LogError(ex, "Проблем при запис на данни за OpenData за регистър");
                        SetErrorMessage($"Проблем при запис!{Environment.NewLine}{ex.Message}");
                    }
                }
            }
            await ViewBagOpenDataRegister();
            return View(model);
        }
       
    }
}
