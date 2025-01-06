using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using URegister.Admin.Models.Service;
using URegister.Common;
using URegister.Core.Models.Common;
using URegister.Infrastructure.Extensions;
using URegister.ObjectsCatalog;
using static URegister.ObjectsCatalog.ObjectsCatalogGrpc;

namespace URegister.Admin.Controllers
{
    public class ServiceController(
        ObjectsCatalogGrpcClient serviceGrpcClient
    ) : BaseController
    {
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
        public async Task<IActionResult> GetServiceTypesList(IDataTablesRequest request)
        {
            var protoRequest = request!.GetDataTablesRequestProto();
            var result = await serviceGrpcClient.GetServiceTypesAsync(protoRequest);
            return request.GetResponseServerPaging(result.ServiceTypes, result.CountAll);
        }
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
        public async Task<IActionResult> GetStepList(IDataTablesRequest request)
        {
            var protoRequest = request!.GetDataTablesRequestProto();
            var result = await serviceGrpcClient.GetStepListAsync(protoRequest);
            return request.GetResponseServerPaging(result.Steps, result.CountAll);
        }

        /// <summary>
        /// Добавяне на стъпка от тип услуга
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AddStep()
        {
            var model = new StepVM();
            return View(nameof(EditStep), model);
        }

        /// <summary>
        /// Редакция на стъпка от тип услуга
        /// </summary>
        /// <param name="id">Идентификатор на стъпката</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditStep(int id)
        {
            var response = await serviceGrpcClient.GetStepAsync(new GetStepMessage
            {
                StepId = id
            });
            var model = new StepVM {
                Id = response.Step.Id,
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStep(StepVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var request = new StepMessage {
                Id = model.Id,
                Name = model.Name,
                Type = model.Type,
                Method = model.Method,
                IsForPublicUse = model.IsForPublicUse,
                IsForOfficialUse = model.IsForOfficialUse,
            };
            var result = await serviceGrpcClient.AppendUpdateStepAsync(request);
            if (result?.Code == ResultCodes.Ok)
            {
                SetSuccessMessage(model.IsInsert ? "Успешно добавенa стъпка" : "Успешна редакция на стъпка");
                return RedirectToAction(nameof(IndexStep));
            }
            else
            {
                SetErrorMessage(result?.Message ?? "Проблем при запис!");
            }
            return View(model);
        }

        /// <summary>
        /// Добавяне на тип услуга
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var model = new ServiceTypeVM();
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Форма за редакция на тип услуга
        /// </summary>
        /// <param name="id">Идентификатор на типа услуга</param>
        /// <returns></returns>
        [HttpGet]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceTypeVM model)
        {
            if (!ModelState.IsValid)
            {
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
                SetSuccessMessage(model.IsInsert ? "Успешно добавенa стъпка" : "Успешна редакция на стъпка");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                SetErrorMessage(result?.Message ?? "Проблем при запис!");
            }
            return View(model);
        }
    }
}
