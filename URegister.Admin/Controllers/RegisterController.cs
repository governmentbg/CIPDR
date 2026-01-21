using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OpenDataClient;
using Org.BouncyCastle.Utilities;
using System.ComponentModel.DataAnnotations;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Models.OpenData;
using URegister.Core.Models.Register;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.IntegrationsCatalog;
using URegister.RegistersCatalog;
using static FastExpressionCompiler.ExpressionCompiler;
using static URegister.IntegrationsCatalog.IntegrationGrpc;

namespace URegister.Admin.Controllers
{
    /// <summary>
    /// Регистри
    /// </summary>
    /// <param name="nomenclatureClient"></param>
    /// <param name="registerClient"></param>
    [Display(Name = "Регистри")]
    public class RegisterController(
        INomenclatureClientService nomenclatureClient,
        IRegisterClientService registerClient,
        RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
        IntegrationGrpcClient integrationGrpcClient,
        IOpenDataClientService openDataClient,
        ILogger<RegisterController> logger
        ) : BaseController
    {
        /// <summary>
        /// Списък регистри
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Зареждане на списък с регистри")]
        public async Task<IActionResult> Index()
        {
            var filter = new RegisterFilterVM();
            await nomenclatureClient.SetViewBagRegister(ViewData);

            var administrationDdlItems = (await registerClient.GetAllAdministrations())
                .Administrations.Select(a => new SelectListItem(a.Name, a.Id)).ToList();
            administrationDdlItems.Insert(0, new SelectListItem
            {
                Text = "Изберете",
                Value = null
            });
            ViewBag.AdministrationId_ddl = administrationDdlItems;

            return View(filter);
        }

        /// <summary>
        /// Списък на регистри
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с регистри")]
        public async Task<IActionResult> GetRegisterList(IDataTablesRequest request, RegisterFilterVM filter)
        {
            return await registerClient.GetRegisterFullList(request, filter);
        }

        /// <summary>
        /// Списък регистри
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Зареждане на списък с администрации за регистър")]
        public async Task<IActionResult> IndexAdministration(int registerId)
        {
            var response = await registerGrpcClient.GetRegisterAsync(new GetRegisterRequest
            {
                RegisterId = registerId
            });

            var model = new RegisterVM
            {
                Id = registerId,
                Name = response.Data.Name,
                NameEDelivery = response.Data.NameEDelivery,
            };
            return View(model);
        }

        /// <summary>
        /// Списък на администрации към регистър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с администрации към регистър")]
        public async Task<IActionResult> GetAdministrationList(IDataTablesRequest request, AdministrationFilterVM filter)
        {
            return await registerClient.GetAdministrationList(request, filter);
        }


        /// <summary>
        /// Списък от оторозирани лица
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Зареждане на списък с упълномощени лица за администрация")]
        public IActionResult IndexPerson(Guid registerAdministrationId, int registerId)
        {
            var filter = new PersonFilterVM
            {
                RegisterAdministrationId = registerAdministrationId,
                RegisterId = registerId
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
        public async Task<IActionResult> GetPersonList(IDataTablesRequest request, PersonFilterVM filter)
        {
            return await registerClient.GetPersonList(request, filter);
        }

        /// <summary>
        /// Добавяне на регистър
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне на нов регистър")]
        public async Task<IActionResult> Add()
        {
            await nomenclatureClient.SetViewBagRegister(ViewData);
            var model = await registerClient.CreateRegister();
            SetRegisterFilesLabel(model);
            return View(nameof(Edit), model);
        }

        private void SetRegisterFilesLabel(RegisterVM model)
        {
            model.RegisterFiles.FilesLabel = "Прикачени файлове за регистър";
            model.AdministrationFiles.FilesLabel = "Прикачени файлове за администрация";
        }

        /// <summary>
        /// Редакция на регистър
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за редакция на регистър")]
        public async Task<IActionResult> Edit(int id)
        {
            await nomenclatureClient.SetViewBagRegister(ViewData);
            var model = await registerClient.GetRegister(id, Guid.Empty);
            if (model.Administration.Id == Guid.Empty)
            {
                return View("EditRegister", model);
            }
            SetRegisterFilesLabel(model);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на добавен регистър 
        /// </summary>
        /// <param name="model">Модел на регистър</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Запис или редакция на регистър или администрация")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RegisterVM model)
        {
            if (ModelState.IsValid)
            {              
                model.Manager.Phone = model.Manager.Phone.Trim();
                if (model.Manager.Phone.StartsWith('0'))
                {
                    model.Manager.Phone = "+359" + model.Manager.Phone.Remove(0, 1);
                }

                model.ContactPersons.ForEach(p =>
                { 
                p.Phone = p.Phone.Trim();
                if (p.Phone.StartsWith("0"))
                {
                    p.Phone = "+359" + p.Phone.Remove(0, 1);
                }
                });

                (var result, var errMsg) = await registerClient.AddRegister(model);
                if (result)
                {
                    SetSuccessMessage(model.IsEditAdministration ? 
                        "Успешно записана администрация" : 
                        "Успешно записан регистър");
                    return model.IsEditAdministration ? RedirectToAction("IndexAdministration", new {registerId = model.Id}) : RedirectToAction("Index");
                }
                else
                {
                    SetErrorMessage($"Проблем при запис!{Environment.NewLine}{errMsg}");
                }
            }
            await nomenclatureClient.SetViewBagRegister(ViewData);
            if (model.Administration.Id == Guid.Empty && model.Id > 0)
            {
                return View(nameof(EditRegister), model);
            }
            return View(nameof(Edit) , model);
        }

        private void RemoveErrorForNotUsed(string startWith)
        {
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                                     .Select(x => new { x.Key, x.Value.Errors })
                                     .ToList();
            foreach (var error in errors)
            {
                if (error.Key.StartsWith(startWith))
                {
                    ModelState.Remove(error.Key);
                }
            }
        }

        /// <summary>
        /// Запис на добавен регистър 
        /// </summary>
        /// <param name="model">Модел на регистър</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Запис на редактиран регистър")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRegister(RegisterVM model)
        {
            RemoveErrorForNotUsed("Manager.");
            RemoveErrorForNotUsed("Administration.");
            RemoveErrorForNotUsed("ContactPersons");
            if (ModelState.IsValid)
            {
                (var result, var errMsg) = await registerClient.EditRegister(model);
                if (result)
                {
                    SetSuccessMessage("Успешно записан регистър");
                    return RedirectToAction("Index");
                }
                else
                {
                    SetErrorMessage($"Проблем при запис!{Environment.NewLine}{errMsg}");
                }
            }
            await nomenclatureClient.SetViewBagRegister(ViewData);
            return View("EditRegister", model);
        }
        /// <summary>
        /// Форма за добавяне на администрация към регистър
        /// </summary>
        /// <param name="registerId">Идентификатор на регистъра</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне на администрация към регистър")]
        public async Task<IActionResult> AddAdministration(int registerId)
        {
            await nomenclatureClient.SetViewBagRegister(ViewData);
            var model = await registerClient.GetRegisterForAddAdministration(registerId);
            model.IsEditAdministration = true;
            SetRegisterFilesLabel(model);
            return View(nameof(Edit), model);
        }


        /// <summary>
        /// Форма за добавяне на администрация към регистър
        /// </summary>
        /// <param name="registerId">Идентификатор на регистъра</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за редакция на администрация към регистър")]
        public async Task<IActionResult> EditAdministration(int registerId, Guid registerAdministrationId)
        {
            await nomenclatureClient.SetViewBagRegister(ViewData);
            var model = await registerClient.GetRegister(registerId, registerAdministrationId);
            model.IsEditAdministration = true;
            SetRegisterFilesLabel(model);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Добавяне на администрация към регистър
        /// </summary>
        /// <param name="model">Модел на администрацията</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Добавяне на администрация към регистър")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdministration(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                (var result, var errMsg) = await registerClient.AddRegister(model);
                if (result)
                {
                    SetSuccessMessage("Успешно добавена администрация");
                    return RedirectToAction("IndexAdministration", new {registerId = model.Id});
                }
                else
                {
                    SetErrorMessage($"Проблем при запис!{Environment.NewLine}{errMsg}");
                }
            }
            await nomenclatureClient.SetViewBagRegister(ViewData);
            return View(nameof(Edit), model);
        }

        [HttpPost]
        [Display(Name = "Извличане на базов адрес за регистър")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetBaseAddress(int id)
        {
            var response = await registerGrpcClient.GetRegisterAsync(new GetRegisterRequest
            {
                RegisterId = id
            });
            var model = new RegisterBaseAddressVM { 
                Id = id,
                Code = response.Data.Code,
                Name = response.Data.Name,
                BaseAddress = response.Data.BaseAddress,
            };
            return PartialView("_BaseAddress", model);
        }

        /// <summary>
        /// Partial за оторозиране лице
        /// </summary>
        /// <param name="index"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        [Display(Name = "Добавяне на контактно лице към администрация")]
        public IActionResult AddContactPerson(int index, string prefix)
        {
            var model = new PersonVM
            {
                Index = index,
                Type = PersonTypeValue.AuthorizedPerson,
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = string.IsNullOrEmpty(prefix) ? $"ContactPersons[{index}]" : $"{prefix}.ContactPersons[{index}]";
            return PartialView("_Person", model);
        }

        /// <summary>
        /// Премахване на администрация от регистър
        /// </summary>
        /// <param name="id">Идентификатор администрацията</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Премахване на администрация от регистър")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRegisterAdministration(Guid id)
        {
            RemoveAdministrationFromRegisterRequest request =
                new RemoveAdministrationFromRegisterRequest
                {
                    RegisterAdministrationId = id.ToString()
                };

            ResultStatus resultStatus = registerGrpcClient.RemoveAdministrationFromRegister(request);

            if (resultStatus.Code == ResultCodes.Ok)
            {
                SetSuccessMessage("Услугата е изтрита успешно");
            }
            else
            {
                SetErrorMessage(resultStatus.Message);
            }

            return Json(null);
        }

        [HttpPost]
        [Display(Name = "Извличане на данни за компания по ЕИК")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetCompanyData(string uic)
        {
            var validationResult = PidValidateService.ValidateCompanyId(uic, (int)CidTypes.EIK);

            if (!validationResult)
            {
                return new JsonResult(new { success = false, message = $"'{uic}' e невалиден" });
            }

            GetCompanyInfoRequest request = new GetCompanyInfoRequest()
            {
                Cid = uic,
                CidType = (int)CidTypes.EIK,
                ContextInfo = new IntegrationServiceContextInfo()
                {
                    EmployeeAdministration = 
                        UserContext.AvailableAdministrations.FirstOrDefault(a => UserContext.AdministrationId.ToString() == a.Id)?.Name,
                    EmployeeNames = UserContext.FirstName + " " + UserContext.LastName,
                    EmployeePosition = string.Join(UserRoles.GlobalAdmin)
                }
            };

            GetCompanyInfoResponse response = await integrationGrpcClient.GetCompanyInfoAsync(request);

            if (response.ResultStatus.Code != ResultCodes.Ok)
            {
                logger.LogError($"Не може да се извлекат данни за компания в {nameof(GetCompanyData)}");
                return new JsonResult(new
                {
                    success = false,
                    //message = response.ResultStatus.Message,
                    message = "Проблем при извличане на данни за компания",
                });
            }

            return new JsonResult(new
            {
                success = true,
                companyName = response.Name,
                legalFormCode = response.LegalFormCode,
                legalFormName = response.LegalFormName,
                apartmentNumber = response.ApartmentNumber,
                buildingNumber = response.BuildingNumber,
                countryCode = response.CountryCode,
                countryName = response.CountryName,
                entranceName = response.EntranceName,
                floorNumber = response.FloorNumber,
                foreignAddress = response.ForeignAddress,
                postCode = response.PostCode,
                regionCode = response.RegionCode,
                regionName = response.RegionName,
                settlementCode = response.SettlementCode,
                settlementName = response.SettlementName,
                streetName = response.StreetName,
                streetNumber = response.StreetNumber
            });
        }
        /// <summary>
        /// Редакция на статус регистър
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за редакция на статус на регистър")]
        public async Task<IActionResult> EditStatus(int registerId)
        {
            await nomenclatureClient.SetViewBagRegister(ViewData);
            var register = await registerClient.GetRegister(registerId, Guid.Empty);
            var model = new RegisterStatusVM
            {
                RegisterId = register.Id,
                StatusId = register.StatusId,
            };
            return View("EditStatus", model);
        }
        /// <summary>
         /// Запис на статус регистър
         /// </summary>
         /// <returns></returns>
        [HttpPost]
        [Display(Name = "Запис на статус на регистър")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(RegisterStatusVM model)
        {
            if (!ModelState.IsValid) {
                await nomenclatureClient.SetViewBagRegister(ViewData);
                return View("EditStatus", model);
            }
            await registerClient.AddRegisterStatus(model);
            SetSuccessMessage("Успешно записан статус на регистър");
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Partial за файл към администрация
        /// </summary>
        /// <param name="index"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        [Display(Name = "Добавяне на файл към администрация")]
        public IActionResult AddRegisterFile(int index, string prefix)
        {
            var model = new RegisterFileVM
            {
                Index = index,
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = $"{prefix}[{index}]";
            return PartialView("_RegisterFile", model);
        }

        /// <summary>
        /// Уплоад на файл
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Уплоад на файл")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UploadFile(IFormFile file)
        {
            var metaFileId = await registerClient.UploadFile(file, Guid.Empty, 0);
            return Json(new { metaFileId });
        }

        /// <summary>
        /// Доунлоад на файл
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Доунлоад на файл")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            (var fileData, var contentType, var fileName) = await registerClient.DownloadFile(id);
            return File(fileData, contentType,Uri.EscapeDataString(fileName));
        }



        // <summary>
        /// списък с статуси на регистър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с статуси на регистър")]
        public async Task<IActionResult> GetRegisterStatusList(IDataTablesRequest request, AdministrationFilterVM filter)
        {
            return await registerClient.GetRegisterStatusList(request, filter.RegisterId);
        }

        // <summary>
        /// Списък регистри
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Зареждане на списък с статуси на регистър")]
        public async Task<IActionResult> IndexStatus(int registerId)
        {
            var response = await registerGrpcClient.GetRegisterAsync(new GetRegisterRequest
            {
                RegisterId = registerId
            });

            var model = new RegisterVM
            {
                Id = registerId,
                Name = response.Data.Name
            };
            return View(model);
        }

        [Display(Name = "Преглед на статус")]
        public async Task<IActionResult> PreviewStatus(Guid registerStatusId)
        {
            await nomenclatureClient.SetViewBagRegister(ViewData);
            var model = await registerClient.GetRegisterStatus(registerStatusId);
            return View("PreviewStatus", model);
        }
        /// <summary>
        /// Параметри на OpenData към администрация
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Параметри на OpenData към администрация към регистър")]
        public async Task<IActionResult> OpenDataAdministration(Guid administrationId, int registerId)
        {
            var response = await registerGrpcClient.GetOpenDataParamAsync(new OpenDataParamRequest
            {
                RegisterId = registerId,
                AdministrationId = administrationId.ToString(),
            });
            var model = new OpenDataAdministrationVM
            {
                ApiKey = response.Data.ApiKey,
                OrganizationId = response.Data.OrganisationId,
                AdministrationId = administrationId,
                AdministrationName = response.Data.AdministrationName,
                FrequencyAdministrationId = response.Data.FrequencyAdministrationId,
                FrequencyId = response.Data.FrequencyId,
                RegisterId = registerId
            };
            await ViewBagOpenDataAdministration();
            return View(model);
        }

        /// <summary>
        /// Запис параметри на OpenData към администрация
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Параметри на OpenData към администрация")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenDataAdministration(OpenDataAdministrationVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await registerGrpcClient.SaveOpenDataAdministrationAsync(new OpenDataAdministrationSaveRequest
                    {
                        AdministrationId = model.AdministrationId.ToString(),
                        FrequencyId = model.FrequencyAdministrationId,
                        OrganisationId = model.OrganizationId,
                    });
                    SetSuccessMessage("Успешeн запис");
                    return RedirectToAction("OpenDataAdministration", new {administrationId = model.AdministrationId, registerId = model.RegisterId});
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
            await nomenclatureClient.SetViewBagOpenDataAdministration(ViewData, true);
        }
    }
}
