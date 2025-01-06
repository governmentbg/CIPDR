using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using URegister.Core.Contracts;
using URegister.Core.Models.Process;
using URegister.Core.Services;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.NomenclaturesCatalog;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProcessController(
        IFormConfigurationPersistenceService formConfigurationPersistenceService,
        IServiceService service,
        IFormFieldsLayoutService formFieldsLayoutService,
        IFormValidationService formValidationService,
        NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
        IRegisterService registerService,
        IProcessService processService,
        ILogger<ProcessController> logger
     
        ) : BaseController
    {
        public async Task<IActionResult> Index()
        {
            ViewBag.ServiceId_ddl = await service.GetServiceDDL();
            ViewBag.StepId_ddl = await service.GetStepDDL();
            return View();
        }
        /// <summary>
        /// Списък на заявени услуги
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetProcessList(IDataTablesRequest request)
        {
            return processService.GetProcessList(request);
        }

        /// <summary>
        /// Форма за добавяне на процес
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            ViewBag.ServiceId_ddl = await service.GetServiceDDL();
            var model = new ProcessVM();
            return View(model);
        }

        /// <summary>
        /// Добавяне на процес
        /// </summary>
        /// <param name="model">Модел на процес</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProcessVM model)
        {
            if (ModelState.IsValid) {
                try
                {
                    await processService.AddProcess(model);
                    SetSuccessMessage("Успешен запис");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message, ex);
                    SetErrorMessage("Проблем при запис");
                }
            }
            ViewBag.ServiceId_ddl = await service.GetServiceDDL();
            return View(model);
        }
        public async Task<IActionResult> AddStep(Guid processId)
        {
            var model = await processService.GetFormViewModel(processId);
            return View(model);
        }
        /// <summary>
        /// Потвърждаване на формата с полета
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DisplayName("Генериране на изглед")]
        public async Task<IActionResult> AddStep(IFormCollection form)
        {
            ProcessStepVM model = null;
            try
            {
                int formParentId = int.Parse(form[nameof(FormViewModel.FormParentId)]);
                FormViewModel viewModel = await formConfigurationPersistenceService.GetFormViewModel(formParentId);
                var processId = Guid.Parse(form[nameof(ProcessStepVM.ProcessId)]);
                var serviceStepId = int.Parse(form[nameof(ProcessStepVM.ServiceStepId)]);
                var orderNum = int.Parse(form[nameof(ProcessStepVM.OrderNum)]);
                formFieldsLayoutService.DistributePostedFieldValuesToViewModel(form, viewModel);
                bool isViewModelValidationSuccess = await formValidationService.ValidateViewModel(
                    viewModel,
                    nomenclatureGrpcClient,
                    await registerService.GetCurrentRegisterId());
                model = processService.ToProcessStepVM(processId, serviceStepId, orderNum, viewModel);
                if (isViewModelValidationSuccess)
                {
                    await processService.AddStep(model);
                    SetSuccessMessage("Успешен запис");
                    return RedirectToAction("Index");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка в {nameof(AddStep)}");
                SetErrorMessage("Проблем при зареждане на формата");
                return View(model);
            }
        }

    }
}
