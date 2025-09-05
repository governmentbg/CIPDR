using DataTables.AspNet.Core;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Drawing2D;
using URegister.Admin.Models.Service;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Models.Common;
using URegister.Core.Services;
using URegister.Infrastructure.Extensions;
using URegister.ObjectsCatalog;
using URegister.Users;
using static URegister.ObjectsCatalog.ObjectsCatalogGrpc;

namespace URegister.Admin.Controllers
{
    [Display(Name = "Услуги")]
    public class ServiceController(
        ObjectsCatalogGrpcClient serviceGrpcClient,
        AppUserManager.AppUserManagerClient appUserManagerClient,
        INomenclatureClientService nomenclatureClientService
    ) : BaseController
    {
        [Display(Name = "Зареждане на списък с типове услуги")]
        public IActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Списък на типове услуги
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с типове услуги")]
        public async Task<IActionResult> GetServiceTypesList(IDataTablesRequest request)
        {
            var protoRequest = request!.GetDataTablesRequestProto();
            var result = await serviceGrpcClient.GetServiceTypesAsync(protoRequest);
            return request.GetResponseServerPaging(result.ServiceTypes, result.CountAll);
        }

        [Display(Name = "Зареждане на списък със стъпки")]
        public IActionResult IndexStep()
        {
            return View();
        }

        /// <summary>
        /// Списък на стъпки
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък със стъпки")]
        public async Task<IActionResult> GetStepList(IDataTablesRequest request)
        {
            var protoRequest = request!.GetDataTablesRequestProto();
            var result = await serviceGrpcClient.GetStepListAsync(protoRequest);
            var roles = await GetToles();
            foreach (var step in result.Steps)
            {
                step.RoleName = roles.Where(x => x.Value == step.RoleId)
                                     .Select(x => x.Text)
                                     .FirstOrDefault();
            }
            return request.GetResponseServerPaging(result.Steps, result.CountAll);
        }
        private async Task<List<SelectListItem>> GetToles()
        {
            var roles = await appUserManagerClient.GetRolesAsync(new Empty());
            return roles.Roles.Select(x => new SelectListItem
            {
                Value = x.RoleId.ToString(),
                Text = x.Label
            }).ToList();
        }
        private async Task SetViewBagStep()
        {
            var ddl = new List<SelectListItem>();
            nomenclatureClientService.AddChoice(ddl);
            ddl.AddRange(await GetToles());
            ViewBag.RoleId_ddl = ddl;
        }

        /// <summary>
        /// Добавяне на стъпка от тип услуга
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне на стъпка")]
        public async Task<IActionResult> AddStep()
        {
            await SetViewBagStep();
            var model = new StepVM();
            return View(nameof(EditStep), model);
        }

        /// <summary>
        /// Редакция на стъпка от тип услуга
        /// </summary>
        /// <param name="id">Идентификатор на стъпката</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за редакция на стъпка")]
        public async Task<IActionResult> EditStep(int id)
        {
            await SetViewBagStep();
            var response = await serviceGrpcClient.GetStepAsync(new GetStepMessage
            {
                StepId = id
            });
            var model = new StepVM {
                Id = response.Step.Id,
                RoleId = response.Step.RoleId == null ? null : Guid.Parse(response.Step.RoleId),
                Name = response.Step.Name,
                Type = response.Step.Type,
                Method = response.Step.Method,
                IsForPublicUse = response.Step.IsForPublicUse,
                IsForOfficialUse = response.Step.IsForOfficialUse,
            };
            return View(nameof(EditStep), model);
        }

        /// <summary>
        /// Запис на стъпка от тип услуга
        /// </summary>
        /// <param name="model">Модел на стъпката</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Запис или редакция на стъпка")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStep(StepVM model)
        {
            if (!ModelState.IsValid)
            {
                await SetViewBagStep();
                return View(model);
            }
            var request = new StepMessage {
                Id = model.Id,
                RoleId = model.RoleId?.ToString() ?? string.Empty,
                Name = model.Name ,
                Type = model.Type,
                Method = model.Method,
                IsForPublicUse = model.IsForPublicUse,
                IsForOfficialUse = model.IsForOfficialUse,
            };
            var result = await serviceGrpcClient.AppendUpdateStepAsync(request);
            if (result?.Code == ResultCodes.Ok)
            {
                SetSuccessMessage(model.IsInsert ? "Успешно добавена стъпка" : "Успешна редакция на стъпка");
                return RedirectToAction(nameof(IndexStep));
            }
            else
            {
                SetErrorMessage(result?.Message ?? "Проблем при запис!");
            }
            await SetViewBagStep();
            return View(model);
        }

        /// <summary>
        /// Добавяне на тип услуга
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне на тип услуга")]
        public async Task<IActionResult> Add()
        {
            var model = new ServiceTypeVM();
            var response = await serviceGrpcClient.GetStepListAsync(new Common.DatatableRequest { Length = -1 });
            model.Steps = response.Steps.Select(x => new ChecklistItemViewModel
            {
                Id = x.Id,
                Label = x.Name,
            })
            .ToList();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Форма за редакция на тип услуга
        /// </summary>
        /// <param name="id">Идентификатор на типа услуга</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за редакция на тип услуга")]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await serviceGrpcClient.GetServiceTypeAsync(new GetServiceTypeRequest
            {
                ServiceId = id
            });
            var model = new ServiceTypeVM
            {
                Id = response.ServiceType.Id,
                Name = response.ServiceType.Name,
                Steps = response.ServiceType.Steps.Select(x => new ChecklistItemViewModel{
                    Id = x.Id,
                    Label = x.Label,
                    Value = x.Value,
                }).ToList()
            };
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Редакция на тип услуга
        /// </summary>
        /// <param name="model">Модел на типа услуга</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Запис или редакция на тип услуга")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceTypeVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ServiceTypeNameExistsReply response = await serviceGrpcClient.CheckServiceTypeNameExistsAsync(new ServiceTypeNameExistsRequest { Name = model.Name.Trim() });
            if (response.Status.Code != ResultCodes.Ok)
            {
                SetErrorMessage("Проблем при проверка на име на улуга.");
                return View(model);
            }

            if (response.IsExists && model.IsInsert)
            {
                SetErrorMessage("Съществува услуга с това име.");
                return View(model);
            }

            var request = new ServiceTypeMessage
            {
                Id = model.Id,
                Name = model.Name,
            };
            request.StepIds.AddRange(model.Steps
                                          .Where(x => x.Value)
                                          .Select(x => x.Id)
                                          .ToList());
            var result = await serviceGrpcClient.AppendUpdateAsync(request);
            if (result?.Code == ResultCodes.Ok)
            {
                SetSuccessMessage(model.IsInsert ? "Успешно добавена тип услуга" : "Успешна редакция на тип услуга");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                SetErrorMessage(result?.Message ?? "Проблем при запис!");
            }
            return View(model);
        }

        /// <summary>
        /// Изтриване на тип услуга
        /// </summary>
        /// <param name="id">Идентификатор на тип услуга за изтриване</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Изтриване на тип услуга")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            GetServiceTypeRequest request = new GetServiceTypeRequest()
            {
                ServiceId = id
            };

            ResultStatus result = await serviceGrpcClient.DeleteServiceTypeAsync(request);

            if (result.Code == ResultCodes.Ok)
            {
                SetSuccessMessage("Типа услуга е изтрит успешно");
            }
            else
            {
                SetErrorMessage(result.Message);
            }

            return Json(null);
        }

        /// <summary>
        /// Изтриване на стъпка
        /// </summary>
        /// <param name="id">Идентификатор на стъпка за изтриване</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Изтриване на стъпка")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStep(int id)
        {
            GetStepMessage request = new GetStepMessage()
            {
                StepId = id
            };

            ResultStatus result = await serviceGrpcClient.DeleteStepAsync(request);

            if (result.Code == ResultCodes.Ok)
            {
                SetSuccessMessage("Стъпката е изтрита успешно");
            }
            else
            {
                SetErrorMessage(result.Message);
            }

            return Json(null);
        }
    }
}
