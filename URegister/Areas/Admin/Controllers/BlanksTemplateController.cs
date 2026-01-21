using DataTables.AspNet.Core;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.ComponentModel.DataAnnotations;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Data.Models.Common;
using URegister.Core.Models.Service;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.ObjectsCatalog;
using URegister.Users;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    [Display(Name = "Бланки")]
    public class BlanksTemplateController(
        IServiceService serviceService,
        IFormConfigurationPersistenceService formConfigurationPersistenceService,
        AppUserManager.AppUserManagerClient appUserManager,
        INomenclatureClientService nomenclatureService
        ) : BaseController
    {
        [Display(Name = "Зареждане на страница Бланки")]
        public IActionResult Index()
        {
            return View();
        }

        [Display(Name = "Задаване на данни за падащи списъци за бланки")]
        public async Task SetViewBag()
        {
            ViewBag.ServiceId_ddl = await serviceService.GetServiceDDL([(int)ServiceTypes.Document]);
            ViewBag.FormParentId_ddl = await formConfigurationPersistenceService.GetFormsDDL();
            await SetViewBagRoles();
            await nomenclatureService.SetViewBagBlankTemplate(ViewData);
        }

        [Display(Name = "Задаване на данни за роли")]
        public async Task SetViewBagRoles()
        {
            var roles = await appUserManager.GetRolesAsync(new Empty());
            var roleId_ddl = roles.Roles.Select(x => new SelectListItem
            {
                Value = x.RoleId,
                Text = x.Name,
            }).ToList();
            roleId_ddl.Insert(0, new SelectListItem("Изберете", ""));
            ViewBag.RoleId_ddl = roleId_ddl;
        }

        [Display(Name = "Зареждане на форма за добавяне на нова бланка")]
        public async Task<IActionResult> Add()
        {
            await SetViewBag();
            var model = new BlanksTemplateVM();
            if (!GlobalConsts.ShowBlankCode)
            {
                model.Code = "CODE";
            }
            return View(nameof(Edit), model);
        }
        [Display(Name = "Зареждане на форма за редакция на бланка")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await serviceService.GetBlankTemplate(id);
            await SetViewBag();
            return View(model);
        }
        [Display(Name = "Зареждане на форма за редакция на съдържание на бланка")]
        public async Task<IActionResult> EditContent(int id)
        {
            var model = await serviceService.GetBlankTemplateContent(id);
            await SetViewBag();
            return View(model);
        }
        /// <summary>
        /// Запис на  тип услуга
        /// </summary>
        /// <param name="model">Модел на услуга</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Запис или редакция на бланка")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BlanksTemplateVM model)
        {
            if (model.SourceType != (int)BlankSourceType.Certicicate)
            {
                ModelState.Remove("ServiceId");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await serviceService.AppendUpdate(model);
                    SetSuccessMessage(model.Id > 0 ? "Успешно добавена бланка" : "Успешна редакция на бланка");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    SetErrorMessage("Проблем при запис!");
                }
            } else
            {
                SetErrorMessage("Невалидни данни!");
                
            }
                await SetViewBag();
            return View(model);
        }
        /// <summary>
        /// Запис на  тип услуга
        /// </summary>
        /// <param name="model">Модел на услуга</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Запис на съдържание на бланка")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditContent(BlanksTemplateContentVM model)
        {
            try
            {
                await serviceService.AppendUpdateContent(model);
                SetSuccessMessage("Успешна записана бланка");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                SetErrorMessage("Проблем при запис!");
            }
            await SetViewBag();
            return View(model);
        }

        [Display(Name = "Извличане на полета за формуляр на бланка")]
        public async Task<JsonResult> GetFormFields(int sourceType, int serviceId, int formParentId, bool numberOnRegister)
        {
            var formModel = await formConfigurationPersistenceService.GetFormViewModel(formParentId);
            var paramList = await serviceService.GetTemplateParam(formModel, string.Empty);
            paramList.Insert(0, new BlanksTemplateParamVM
            {
                Name = "Process",
                Label = "Заявена услуга",
                Templates = serviceService.GetTemplateProcessParam(string.Empty)
            });
            if (sourceType == (int)BlankSourceType.Certicicate)
            {
                var certificateService = await serviceService.GetService(serviceId, true);
                var formModelCertificate = await formConfigurationPersistenceService.GetFormViewModel(certificateService.FormParentId);
                var paramListCertificate = await serviceService.GetTemplateParam(formModelCertificate, "certificate.");
                paramListCertificate.Insert(0, new BlanksTemplateParamVM
                {
                    Name = "ProcessCertificate",
                    Label = "Заявена услуга",
                    Templates = serviceService.GetTemplateProcessParam("certificate.")
                });
                paramList.Insert(0, new BlanksTemplateParamVM
                {
                    Name = "Certificate",
                    Label = "Удостоверение",
                    Templates = paramListCertificate
                });
            }
            if (sourceType == (int)BlankSourceType.Instruction)
            {
                paramList.Insert(0, new BlanksTemplateParamVM
                {
                    Name = "InstructionText",
                    Label = "Текст на указание",
                });
            }
            if (sourceType == (int)BlankSourceType.CertificateOnRegister && numberOnRegister)
            {
                paramList.Insert(0, new BlanksTemplateParamVM
                {
                    Name = "Process_RegisterCertificateNumber",
                    Label = "Номер удостоверение при вписване",
                });
            }
            return Json(paramList);
        }

        /// <summary>
        /// Списък на  услуги
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с бланки")]
        public async Task<IActionResult> GetBlankTemplateList(IDataTablesRequest request)
        {
            return await serviceService.GetBlankTemplateList(request);
        }

        /// <summary>
        /// Изтриване на банка
        /// </summary>
        /// <param name="id">Идентификатор на номенклатура</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Изтриване на бланка")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var result = await serviceService.DeleteTemplate(id);
            if (result.IsSuccess)
            {
                SetSuccessMessage("Бланката е изтрита успешно");
            }
            else
            {
                SetErrorMessage(result.ErrorMessage);
            }

            return Json(null);
        }
        /// <summary>
        /// Partial за подпис 
        /// </summary>
        /// <param name="index">индекс в списък</param>
        /// <param name="prefix">html prefix</param>
        /// <returns></returns>
        [Display(Name = "Добавяне на подпис")]
        public async Task<IActionResult> AddBlankSignature(int index, string prefix, int serviceTypeId)
        {
            var model = new BlankSignatureVM
            {
                Index = index,
                OrderNum = index + 1,
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = string.IsNullOrEmpty(prefix) ? $"BlankSignatures[{index}]" : $"{prefix}.BlankSignatures[{index}]";
            await SetViewBagRoles();
            return PartialView("_Signature", model);
        }
    }
}
