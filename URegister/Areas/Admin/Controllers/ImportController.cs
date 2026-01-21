using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Contracts;
using URegister.Core.Models.Process;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.NomenclaturesCatalog;

namespace URegister.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Display(Name = "Импорт на услуги")]
    [Authorize(Roles = $"{UserRoles.Admin}")]
    public class ImportController(
       IImportService importService,
       IFormConfigurationPersistenceService formConfigurationPersistenceService,
       IServiceService service,
       IFormFieldsLayoutService formFieldsLayoutService,
       IFormValidationService formValidationService,
       NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
       IRegisterService registerService,
       IProcessService processService,
       ILogger<ImportController> logger,
       IFieldFormulaCalculationService fieldFormulaCalculationService
    ) : BaseController
    {

        [HttpGet]
        [Display(Name = "Зареждане на форма за импортиране на файл")]
        public async Task<IActionResult> ImportFile()
        {
            ViewBag.ServiceId_ddl = await service.GetServiceDDL([(int)ServiceTypes.Register]);
            var model = new ImportFileVM();
            return View(nameof(ImportFile), model);
        }

        [HttpPost]
        [Display(Name = "Импортиране на файл")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFile(ICollection<IFormFile> files, ImportFileVM model)
        {
            ViewBag.ServiceId_ddl = await service.GetServiceDDL([(int)ServiceTypes.Register]);
            if (model.ServiceId <= 0)
            {
                ModelState.AddModelError("ServiceId", "Изберете услуга");
            }
            if (!files.Any())
            {
                ModelState.AddModelError("ServiceId", "Прикачете файл");
            }
            if (!ModelState.IsValid)
            {
                return View(nameof(ImportFile), model);
            }

            model.FileId = await importService.SaveImportFile(files.First());
            if (string.IsNullOrEmpty(model.FileId))
            {
                SetErrorMessage("Проблем при запис на файл");
                return View(nameof(ImportFile), model);
            }
            return RedirectToAction("ImportFileSave", "Import", new { area = "Admin", fileId = model.FileId, serviceId = model.ServiceId });
        }

        [HttpGet]
        [Display(Name = "Зареждане на форма за запазване на импортиран файл")]
        public async Task<IActionResult> ImportFileSave(int serviceId, string? fileId)
        {
            var aService = await service.GetService(serviceId, true);
            var aForm = await service.GetForm(aService.FormParentId);
            var model = new ImportFileVM
            {
                FileId = fileId,
                ServiceId = serviceId,
                ServiceName = aService?.Name,
                FormName = aForm?.Title
            };
            return View(model);
        }

        [HttpPost]
        [Display(Name = "Извличане на списък с данни от импортиран файл")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetImportDataList([FromBody] ImportFileVM model)
        {
            var importItems = new List<List<ImportItemVM>>();
            var data = await importService.GetImportData(model.FileId);
            var aService = await service.GetService(model.ServiceId ?? 0, true);
            var aForm = await service.GetForm(aService.FormParentId);
            var formFields = await formConfigurationPersistenceService.GetFormFieldNamesInFlatListByParentId(aService.FormParentId);
            var fields = formFields.Select(x => new ImportItemVM
            {
                Key = x.Key,
                Value = x.Value,
            }).ToList();
            importItems.Add(fields);
            foreach (var rowData in data)
            {
                var formData = new Dictionary<string, StringValues>();
                foreach (var kv in rowData)
                {
                    formData[kv.Key] = new StringValues(kv.Value);
                }
                IFormCollection formImport = new FormCollection(formData);
                FormViewModel viewModel = await formConfigurationPersistenceService.GetFormViewModel(aService.FormParentId);
                var processId = Guid.Empty;
                formFieldsLayoutService.DistributePostedFieldValuesToViewModel(formImport, viewModel);
                await formConfigurationPersistenceService.ApplyConditionTreeOnFormModel(viewModel);
                bool isViewModelValidationSuccess = await formValidationService.ValidateViewModel(
                    viewModel,
                    nomenclatureGrpcClient,
                    await registerService.GetCurrentRegisterId());
                var errors = await formValidationService.GetValidatedFormFieldsErrors(viewModel);

                OperationResult calculationsResult = await fieldFormulaCalculationService.CalculateFormulas(viewModel);

                if (!calculationsResult.IsSuccess)
                {
                    logger.LogError($"Изчисленията при импорт в {nameof(GetImportDataList)} не минаха успешно. {calculationsResult.ErrorMessage}");
                    //TODO : да се върне в резултата?
                }

                var row = new List<ImportItemVM>();
                foreach (var field in fields)
                {
                    var item = new ImportItemVM
                    {
                        Key = field.Key
                    };
                    if (rowData.ContainsKey(field.Key))
                    {
                        item.Value = rowData[field.Key];
                    }
                    if (!isViewModelValidationSuccess)
                    {
                        if (errors.ContainsKey(field.Key))
                        {
                            item.Error = errors[field.Key];
                        }
                    }
                    row.Add(item);
                }
                importItems.Add(row);
            }
            return Json(importItems);
        }

        [HttpPost]
        [Display(Name = "Запазване на импортирани данни от файл")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFileSave(ImportFileVM model)
        {
            ViewBag.ServiceId_ddl = await service.GetServiceDDL([(int)ServiceTypes.Register]);

            if (!ModelState.IsValid)
            {
                return View(nameof(ImportFile), model);
            }

            var data = await importService.GetImportData(model.FileId);
            try
            {
                foreach (var rowData in data)
                {
                    var formData = new Dictionary<string, StringValues>();
                    foreach (var kv in rowData)
                    {
                        formData[kv.Key] = new StringValues(kv.Value);
                    }
                    IFormCollection formImport = new FormCollection(formData);
                    var serviceVM = await service.GetService(model.ServiceId ?? 0, true);
                    int formParentId = serviceVM.FormParentId;
                    FormViewModel viewModel = await formConfigurationPersistenceService.GetFormViewModel(formParentId);
                    var processId = Guid.Empty;
                    Guid? fromProcessId = null;
                    var serviceStep = serviceVM.Steps.Where(x => x.StatusId == (int)ProcessStatus.Registered).First();
                    var serviceId = model.ServiceId;
                    await formConfigurationPersistenceService.ApplyConditionTreeOnFormModel(viewModel);
                    formFieldsLayoutService.DistributePostedFieldValuesToViewModel(formImport, viewModel);
                    bool isViewModelValidationSuccess = await formValidationService.ValidateViewModel(
                        viewModel,
                        nomenclatureGrpcClient,
                        await registerService.GetCurrentRegisterId());

                    OperationResult calculationsResult = await fieldFormulaCalculationService.CalculateFormulas(viewModel);

                    if (!calculationsResult.IsSuccess)
                    {
                        logger.LogError($"Изчисленията при импорт в {nameof(ImportFileSave)} не минаха успешно. {calculationsResult.ErrorMessage}");
                        //TODO : да се върне в резултата?
                    }

                    var processStep = await processService.ToProcessStepVM(processId, fromProcessId, serviceId ?? 0, serviceStep.Id, serviceStep.OrderNum, null, null, viewModel, false);
                    await processService.AddStep(processStep);

                }
                SetSuccessMessage("Успешен импорт");
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка в {nameof(ImportFile)}");
                SetErrorMessage("Проблем при импорт");
                return View(nameof(ImportFile), model);
            }
        }
        [HttpGet]
        [Display(Name = "Зареждане на форма за запазване на импортиран файл")]
        public async Task<IActionResult> Maket()
        {
            var model = await importService.GetMaketFile();
            return View(model);
        }

        [HttpPost]
        [Display(Name = "Уплоад на шаблон за импортиране")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadMaketFile(ICollection<IFormFile> files)
        {
            await importService.SaveImportMaketFile(files.First());
            return RedirectToAction("Maket");
        }

    }
}
