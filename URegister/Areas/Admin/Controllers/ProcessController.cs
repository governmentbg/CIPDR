using DataTables.AspNet.Core;
using IO.HtmlToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using URegister.Core.Contracts;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Core.Models.Common;
using URegister.Core.Models.Process;
using URegister.Core.Models.Service;
using URegister.Core.Services;
using URegister.Extensions;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.NomenclaturesCatalog;


namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Display(Name = "Заявени услуги")]
    public class ProcessController(
        IFormConfigurationPersistenceService formConfigurationPersistenceService,
        IServiceService service,
        IFormFieldsLayoutService formFieldsLayoutService,
        IFormValidationService formValidationService,
        NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
        IRegisterService registerService,
        IProcessService processService,
        IProcessTemplateService processTemplateService,
        INomenclatureClientService nomenclatureClient,
        IUserContext userContext,
        ILogger<ProcessController> logger,
        IDeadlineService deadlineService
        ) : BaseController
    {

        private async Task SetViewBag(int serviceId)
        {
            ViewBag.DeadlineId_ddl = await deadlineService.GetDeadlineDDL(serviceId);
            await nomenclatureClient.SetViewBagProcess(ViewData);
        }
        [Authorize(Roles = $"{UserRoles.Manager},{UserRoles.Editor},{UserRoles.Registrator}")]
        [Display(Name = "Зареждане на страница Заявени услуги")]
        public async Task<IActionResult> Index()
        {
            ViewBag.ServiceId_ddl = await service.GetServiceDDL([]);
            ViewBag.StepId_ddl = await service.GetStepDDL();
            await SetViewBag(0);
            var request = new NomenclaturePublicRequest
            {
                RegisterId = 0,
            };
            request.NomenclatureTypes.Add(NomenclatureTypes.PidType);
            var response = await nomenclatureGrpcClient.GetNomenclaturePublicAsync(request);
            var pidTypes = response.NomenclatureTypes.First().CodeableConcepts;
            ViewBag.PidType_ddl = pidTypes.Select(x => new SelectListItem
            {
                Value = x.Code,
                Text = x.Value
            }).ToList();
            var model = new ProcessFilterVM();
            return View(nameof(Index), model);
        }

        [Display(Name = "Зареждане на страница Връщане на заявена услуга за обработка")]
        public async Task<IActionResult> IndexDeAssign()
        {
            ViewBag.ServiceId_ddl = await service.GetServiceDDL([]);
            ViewBag.StepId_ddl = await service.GetStepDDL();
            await SetViewBag(0);
            var request = new NomenclaturePublicRequest
            {
                RegisterId = 0,
            };
            request.NomenclatureTypes.Add(NomenclatureTypes.PidType);
            var response = await nomenclatureGrpcClient.GetNomenclaturePublicAsync(request);
            var pidTypes = response.NomenclatureTypes.First().CodeableConcepts;
            ViewBag.PidType_ddl = pidTypes.Select(x => new SelectListItem
            {
                Value = x.Code,
                Text = x.Value
            }).ToList();
            var model = new ProcessFilterVM
            {
                ForDeAssignUser = true
            };
            return View(nameof(Index), model);
        }
        

        [Authorize(Roles = $"{UserRoles.Manager},{UserRoles.Editor}")]
        [Display(Name = "Зареждане на списък с указания за заявена услуга")]
        public async Task<IActionResult> InstructionIndex(Guid processId)
        {
            var process = await processService.GetByIdAsync<Core.Data.Models.Process.Process>(processId);
            var model = new InstructionFilterVM
            {
                ProcessId = processId,
                ProcessLabel = await processService.GetProcessLabel(processId) ?? string.Empty,
                CanAdd = process.StatusId != (int)ProcessStatus.Refused &&
                     process.StatusId != (int)ProcessStatus.Registered

            };
            return View(model);
        }

        [Authorize(Roles = $"{UserRoles.Manager},{UserRoles.Editor}")]
        [Display(Name = "Зареждане на списък с изпълнени указания")]
        public async Task<IActionResult> InstructionResponseIndex(Guid instructionId)
        {
            var instruction = await processService.GetByIdAsync<Instruction>(instructionId);
            var model = new InstructionVM
            {
                Id = instruction.Id,
                CanAdd = instruction.ClosedOn == null

            };
            return View(model);
        }


        /// <summary>
        /// Списък на заявени услуги
        /// </summary>
        /// <param name="request">Заявка</param>
        /// <param name="filter">Филтър</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък със заявени услуги")]
        public async Task<IActionResult> GetProcessList(IDataTablesRequest request, ProcessFilterVM filter)
        {
            return await processService.GetProcessList(request, filter);
        }
        /// <summary>
        /// Списък на заявени услуги
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък със заявени услуги, присвоени на текущия потребител")]
        public async Task<IActionResult> GetProcessListAssigned(IDataTablesRequest request)
        {
            var filter = new ProcessFilterVM
            {
                AssignedToUserId = userContext.UserId,
            };
            return await processService.GetProcessList(request, filter);
        }


        /// <summary>
        /// Списък на заявени услуги
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с указания за заявена услуга")]
        public async Task<IActionResult> GetInstructionList(IDataTablesRequest request, InstructionFilterVM filter)
        {
            return await processService.GetInstructionList(request, filter);
        }



        /// <summary>
        /// Списък на заявени услуги по статус за Dashboard
        /// </summary>
        /// <param name="request"></param>
        /// <param name="statusId">Идентификатор на статус</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък със заявени услуги по статус за табло")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetProcessListDashboard(IDataTablesRequest request, int statusId)
        {
            return await processService.GetProcessListDashboard(request, statusId);
        }

        /// <summary>
        /// Форма за добавяне на процес
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне на заявена услуга")]
        public async Task<IActionResult> Add()
        {
            ViewBag.ServiceId_ddl = await service.GetServiceDDL([(int)ServiceTypes.Register, (int)ServiceTypes.Document]);
            var model = new ProcessVM();
            return View(model);
        }

        /// <summary>
        /// Добавяне на процес
        /// </summary>
        /// <param name="model">Модел на процес</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Добавяне на заявена услуга")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProcessVM model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("AddStepInit", "Process", new { serviceId = model.ServiceId });
            }
            ViewBag.ServiceId_ddl = await service.GetServiceDDL([(int)ServiceTypes.Register]);
            return View(model);
        }
        /// <summary>
        /// Форма за добавяне на процес
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне на стара заявена услуга")]
        [Authorize(Roles = $"{UserRoles.Admin}")]
        public async Task<IActionResult> AddOld()
        {
            ViewBag.ServiceId_ddl = await service.GetServiceDDL([(int)ServiceTypes.Register]);
            var model = new ProcessVM();
            return View(model);
        }

        /// <summary>
        /// Добавяне на процес
        /// </summary>
        /// <param name="model">Модел на процес</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Добавяне на стара заявена услуга")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{UserRoles.Admin}")]
        public async Task<IActionResult> AddOld(ProcessVM model)
        {
            if (ModelState.IsValid)
            {
                await SetViewBag(model.ServiceId!.Value);
                var stepModel = await processService.GetFormViewModel(model.ServiceId!.Value, model.OldIncomingNumber, model.OldIncomingDate, true);
                return View("AddStep", stepModel);
            }
            ViewBag.ServiceId_ddl = await service.GetServiceDDL([(int)ServiceTypes.Register]);
            return View(model);
        }


        /// <summary>
        /// Форма за добавяне на процес
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне на услуга за промяна/заличаване")]
        public async Task<IActionResult> AddChange(Guid processId)
        {
            var model = new ProcessVM
            {
                FromProcessId = processId,
            };
            ViewBag.ServiceId_ddl = await service.GetServiceDDL([(int)ServiceTypes.Change, (int)ServiceTypes.Deletion, (int)ServiceTypes.AskForCorrectionError]);
            return View(model);
        }

        /// <summary>
        /// Добавяне на процес
        /// </summary>
        /// <param name="model">Модел на процес</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Добавяне на услуга за промяна/заличаване")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChange(ProcessVM model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("AddChangeStepInit", "Process", new { fromProcessId = model.FromProcessId, serviceId = model.ServiceId });
            }
            ViewBag.ServiceId_ddl = await service.GetServiceDDL([(int)ServiceTypes.Register]);
            return View(model);
        }

        /// <summary>
        /// Форма за добавяне на процес
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне на стъпка към заявена услуга")]
        public async Task<IActionResult> AddChangeStepInit(Guid fromProcessId, int serviceId)
        {
            await SetViewBag(serviceId);
            var model = await processService.GetFormViewModelFrom(fromProcessId, serviceId);
            return View(nameof(AddStep), model);
        }

        [Display(Name = "Зареждане на форма за добавяне на стъпка към заявена услуга")]
        public async Task<IActionResult> AddStepInit(int serviceId)
        {
            await SetViewBag(serviceId);
            var model = await processService.GetFormViewModel(serviceId, null, null, false);
            return View("AddStep", model);
        }

        [Display(Name = "Зареждане на форма за добавяне на стъпка към заявена услуга")]
        public async Task<IActionResult> AddStepInitOld(int serviceId, string? oldIncomingNumber, DateTime? oldIncomingDate)
        {
            await SetViewBag(serviceId);
            var model = await processService.GetFormViewModel(serviceId, oldIncomingNumber, oldIncomingDate, true);
            return View("AddStep", model);
        }

        [Display(Name = "Зареждане на форма за добавяне на стъпка към заявена услуга")]
        public async Task<IActionResult> AddStep(Guid processId)
        {
            (var model, _) = await processService.GetFormViewModel(processId, false);
            await SetViewBag(model.ServiceId);
            model.DontUploadFilesToStorage = false;
            if (await processService.IsCertificateStep(model.ServiceStepId))
            {
                return RedirectToAction("Certificate", new { processId });
            }
            return View(model);
        }

        [Display(Name = "Зареждане на форма за създаване на удостоверение")]
        public async Task<IActionResult> Certificate(Guid processId)
        {
            (var fileId, var message) = await MakeCertificateDraft(processId);
            if (fileId == null)
            {
                SetErrorMessage($"Не намирам вписване за {message}");
                return RedirectToAction("Index");
            }
            var modelVm = new CertificateVM
            {
                ProcessId = processId,
                FileId = fileId ?? Guid.Empty
            };
            return View("Certificate", modelVm);
        }

        private async Task<(Guid?, string)> MakeCertificateDraft(Guid processId)
        {
            (var modelCertificate, var processCertificate) = await processService.GetFormViewModel(processId, true);
            var process = await processService.GetProcessForCertificate(modelCertificate.FormFields);
            if (process == null)
            {
                (var pidType, var pid, var name) = processService.GetMPRIData(PersonRole.Partida, modelCertificate.FormFields);
                return (null, pid);
            }
            var registerItemsCertificate = await processService.AddRegisterItems(processCertificate, modelCertificate.FormFields, Guid.Empty, modelCertificate.UserTimeZoneOffsetInMinutes);
            var html = await processTemplateService.GetProcessCertificateHtml(process, processCertificate, modelCertificate.ServiceId, registerItemsCertificate, process.RegisterItems);
            var bytes = await (this.HttpContext.RequestServices.GetService<IIOHtmlToPdfService>() ?? throw new ArgumentNullException("pdfService")).ConvertHtmlToPdf(html, ControllerExtentions.GetPrintPDFOptions());
            return (await processService.SaveCertificateFileDraft(processId, bytes, (int)EDeliveryMessageType.OutCertificate), string.Empty);
        }

        private async Task<(Guid?, string)> MakeCertificateDraftOnRegister(Guid processId, BlanksTemplate blanksTemplate)
        {
            var process = await processService.GetProcessForCertificateOnRegister(processId);
            if (process?.StatusId != (int)ProcessStatus.Registered)
            {
                return (null, string.Empty);
            }
            var html = await processTemplateService.GetProcessCertificateOnRegisterHtml(process, process.RegisterItems, blanksTemplate);
            var bytes = await (this.HttpContext.RequestServices.GetService<IIOHtmlToPdfService>() ?? throw new ArgumentNullException("pdfService")).ConvertHtmlToPdf(html, ControllerExtentions.GetPrintPDFOptions());
            return (await processService.SaveCertificateFileDraft(processId, bytes, (int)EDeliveryMessageType.OutCertificate), string.Empty);
        }

        private async Task<Guid?> MakeDraftBlankForNotRegistered(Guid processId, BlanksTemplate blanksTemplate, int typeMessageId)
        {
            (var model, var process) = await processService.GetFormViewModel(processId, true);
            var registerItems = await processService.AddRegisterItems(process, model.FormFields, Guid.Empty, model.UserTimeZoneOffsetInMinutes);
            var html = await processTemplateService.GetProcessCertificateOnRegisterHtml(process, registerItems, blanksTemplate);
            var bytes = await (this.HttpContext.RequestServices.GetService<IIOHtmlToPdfService>() ?? throw new ArgumentNullException("pdfService")).ConvertHtmlToPdf(html, ControllerExtentions.GetPrintPDFOptions());
            return await processService.SaveCertificateFileDraft(processId, bytes, typeMessageId);
        }

        [HttpPost]
        [Display(Name = "Подписване и запис на удостоверение")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignCertificate(CertificateVM model)
        {
            (var modelStep, var process) = await processService.GetFormViewModel(model.ProcessId, false);
            await processService.AddStep(modelStep);
            (var fileId, var message) = await MakeCertificateDraft(model.ProcessId);
            if (fileId == null)
            {
                SetErrorMessage($"Не намирам вписване за {message}");
            }
            else
            {
                var serviceModel = await service.GetService(process.ServiceId);
                byte[] filesAsBytes = await processService.GetCertificateFile(fileId ?? Guid.Empty);
                await processService.SendMessageForProcess(
                    model.ProcessId, 
                    filesAsBytes, 
                    (int)EDeliveryMessageType.OutCertificate, 
                    model.ProcessId.ToString(),
                    $"По повод заявление с вх. № {process.IncomingNumber} от {process.IncomingDate:dd.MM.yyyy} г., " +
                    $"Ви уведомяваме, че е издаден документ: {serviceModel.Name} № {process.RegisterNumber}"
                );
                SetSuccessMessage("Успешен запис");
            }
            return View("Index");
        }

        [Display(Name = "Извличане на файл на удостоверение")]
        public async Task<FileResult> GetCertificateFile(Guid fileId)
        {
            var bytes = await processService.GetCertificateFile(fileId);
            return File(bytes, "application/pdf", "certificate.pdf");
        }

        [Display(Name = "Извличане на подписан файл на удостоверение")]
        public async Task<FileResult> GetCertificateFileSigned(Guid processId)
        {
            var bytes = await processService.GetCertificateFileSigned(processId);
            return File(bytes, "application/pdf", "certificate.pdf");
        }


        /// <summary>
        /// Преглед с на данни по заявена услуга
        /// </summary>
        /// <param name="processId">Идентификатор на заявената услуга</param>
        /// <param name="isReadonly">Дали е само за преглед</param>
        /// <param name="backTo">Към коя страница да ни връща бутонът "Назад"</param>
        /// <returns></returns>
        [Display(Name = "Преглед на данни по заявена услуга")]
        public async Task<IActionResult> Preview(Guid processId, bool isReadonly = false, string backTo = "Index")
        {
            (var model, _) = await processService.GetFormViewModel(processId, true);
            await SetViewBag(model.ServiceId);
            model.DontUploadFilesToStorage = false;
            ViewBag.BackTo = backTo;
            if (isReadonly)
            {
                return View("PreViewReadonly", model);
            }
            else
            {
                return View("PreView", model);
            }
        }

        /// <summary>
        /// Потвърждаване на формата с полета
        /// </summary>
        [HttpPost]
        [Display(Name = "Добавяне на стъпка към заявена услуга")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStep(IFormCollection form)
        {
            bool isOldDataImport = !string.IsNullOrWhiteSpace(form["ProcessInfo.OldIncomingNumber"]);
            var serviceId = 0;
            ProcessStepVM model = null;
            try
            {
                int formParentId = int.Parse(form[nameof(FormViewModel.FormParentId)]);
                FormViewModel viewModel = await formConfigurationPersistenceService.GetFormViewModel(formParentId);
                var processId = form[nameof(ProcessStepVM.ProcessId)].ToString().ToGuid() ?? Guid.Empty;
                Guid? fromProcessId = form[nameof(ProcessStepVM.FromProcessId)].ToString().ToGuid();
                var serviceStepId = int.Parse(form[nameof(ProcessStepVM.ServiceStepId)]);
                serviceId = int.Parse(form[nameof(ProcessStepVM.ServiceId)]);
                var orderNum = int.Parse(form[nameof(ProcessStepVM.OrderNum)]);
                var oldIncomingNumber = form["ProcessInfo.OldIncomingNumber"].ToString();
                DateTime? oldIncomingDate = null;
                var oldIncomingDateStr = form["ProcessInfo.OldIncomingDate"].ToString();
                if (!string.IsNullOrEmpty(oldIncomingDateStr))
                {
                    oldIncomingDate = DateTime.ParseExact(oldIncomingDateStr, "dd.MM.yyyy", null);
                }

                formFieldsLayoutService.DistributePostedFieldValuesToViewModel(form, viewModel);

                model = await processService.ToProcessStepVM(processId, fromProcessId, serviceId, serviceStepId, orderNum, oldIncomingNumber, oldIncomingDate, viewModel, false);
                processService.FillProcessInfoVM(form, model.ProcessInfo);
                model.DontUploadFilesToStorage = false;

                if (isOldDataImport && !User.IsInRole(UserRoles.Admin))
                {
                    SetErrorMessage("Нямате права за въвеждане на стари данни");
                    return View(model);
                }

                bool isViewModelValidationSuccess = await formValidationService.ValidateViewModel(
                    viewModel,
                    nomenclatureGrpcClient,
                    await registerService.GetCurrentRegisterId(),
                    model.IncomingDate,
                    isOldDataImport);

                if (isViewModelValidationSuccess)
                {
                    (var savedModel,var process) = await processService.AddStep(model);
                    var serviceModel = await service.GetRegisterService();
                    if (serviceModel.Id == process.ServiceId && process.StatusId == (int)ProcessStatus.Registered)
                    {
                        var blankOnRegister = await processService.GetBlankOnRegister(formParentId);
                        if (blankOnRegister != null)
                        {
                            (var fileId, var message) = await MakeCertificateDraftOnRegister(savedModel.ProcessId, blankOnRegister);
                            if (fileId != null)
                            {
                                byte[] filesAsBytes = await processService.GetCertificateFile(fileId ?? Guid.Empty);
                                await processService.SendMessageForProcess(
                                    savedModel.ProcessId,
                                    filesAsBytes,
                                    (int)EDeliveryMessageType.OutCertificate,
                                    savedModel.ProcessId.ToString(),
                                    $"По повод заявление с вх. № {process.IncomingNumber} от {process.IncomingDate:dd.MM.yyyy} г., " +
                                    $"Ви уведомяваме, че е издаден документ № {process.RegisterNumber}"
                                );
                            }
                        }
                        else
                        {
                            await processService.SendMessageForProcess(
                                    savedModel.ProcessId,
                                    new byte[0],
                                    (int)EDeliveryMessageType.RegisterApplication,
                                    savedModel.ProcessId.ToString(),
                                    $"По повод заявление с вх. № {process.IncomingNumber} от {process.IncomingDate:dd.MM.yyyy} г., " +
                                    $"Ви уведомяваме, че по заявлението е постановено вписване с регистров № {process.RegisterNumber}"
                                );
                        }
                    }
                    SetSuccessMessage("Успешен запис");
                    return RedirectToAction("Index");
                }
                await SetViewBag(serviceId);
                var errors = await formValidationService.GetValidatedFormFieldsErrors(viewModel);
                var errMessage = "Невалидни данни! Моля проверете полетата с индикация за грешка."; /*+ string.Join(Environment.NewLine, errors.Values);*/
                SetErrorMessage(errMessage);
                return View(model);
            }
            catch (Exception ex)
            {
                await SetViewBag(serviceId);
                logger.LogError(ex, ex.InnerException?.Message + $"Грешка в {nameof(AddStep)}");
                SetErrorMessage("Проблем при запис на формата");
                return View(model);
            }
        }

        [HttpPost]
        [Display(Name = "Прекратяване на заявена услуга")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refuse(Guid id, string? reasonForRejection = null)
        {
            if (string.IsNullOrWhiteSpace(reasonForRejection))
            {
                return Json(new { success = false, error = "Въведете причина за прекратяване на заявената услуга." });
            }

            reasonForRejection = reasonForRejection.Trim();

            if (reasonForRejection.Length > 1000)
            {
                return Json(new { success = false, error = "Въведете причина с дължина под 1000 символа." });
            }

            try
            {
                var process = await processService.Refuse(id, reasonForRejection);
                byte[] filesAsBytes = new byte[0];
                var blankRefuse = await processService.GetBlankRefuse(process.Form.ParentId ?? 0);
                if (blankRefuse != null)
                {
                    var fileId = await MakeDraftBlankForNotRegistered(process.Id, blankRefuse, (int)EDeliveryMessageType.OutRefuse);
                    if (fileId != null)
                    {
                        filesAsBytes = await processService.GetCertificateFile(fileId ?? Guid.Empty);
                    }
                }
                await processService.SendMessageForProcess(
                    process.Id, 
                    filesAsBytes, 
                    (int)EDeliveryMessageType.OutRefuse, 
                    process.Id.ToString(),
                    $"По повод заявление с вх. № {process.IncomingNumber} от {process.IncomingDate:dd.MM.yyyy} г., "+
                    $"Ви уведомяваме, че по заявлението е постановен отказ за вписване на заявените обстоятелства, със следните мотиви: {reasonForRejection}");

                SetSuccessMessage("Успешно прекратяване на заявената услуга.");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                SetErrorMessage("Проблем при прекратяване на заявената услуга.");
                return Json(new { success = false, error = "Проблем при прекратяване на заявената услуга." });
            }
        }

        [HttpPost]
        [Display(Name = "Качване на файл към форма")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(IFormFile file, int formParentId, string fieldName, string key, bool dontUploadFilesToStorage = false)
        {
            try
            {
                if (file == null || file.Length <= 0)
                {
                    return Json(new { success = false, error = MessageConstant.Values.FileIsEmpty });
                }

                FormViewModel viewModel =
                    await formConfigurationPersistenceService.GetFormViewModel(formParentId, !dontUploadFilesToStorage);

                fieldName = Regex.Replace(fieldName, @"#\d+", string.Empty);

                FormField fieldMetadata = FindFieldMetadata(viewModel.FormFields, fieldName);

                if (fieldMetadata == null)
                {
                    return Json(new { success = false, error = MessageConstant.Values.FileUploadFailed });
                }

                bool validationResult = await formValidationService.ValidateFile(fieldMetadata, file);

                if (!validationResult)
                {
                    return Json(new { success = false, error = fieldMetadata.ValidationError });
                }

                //TODO : да се възстанови
                if (dontUploadFilesToStorage)
                {
                    return Json(new { success = true, fileKey = Guid.NewGuid() });
                }

                //TODO : файл може да е заменен или изтрит само от потребителя въвел го, или такъв в по-високи права?
                SaveOperationResult result = await processService.SaveUploadedFile(file,
                    string.IsNullOrEmpty(key) ? Guid.Empty : Guid.Parse(key));

                if (result.IsSuccess)
                {
                    return Json(new { success = true, fileKey = (Guid)result.AddedObjectId });
                }

                return Json(new { success = false, error = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Проблем при качване на файл в {UploadFile}, за форма с parentId {formParentId}");
                return Json(new { success = false, error = MessageConstant.Values.FileUploadFailed });
            }
        }

        [HttpPost]
        [Display(Name = "Изтриване на файл от форма")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(string key, bool dontUploadFilesToStorage = false)
        {
            //TODO : да се провери кой може да трие файла?
            if (dontUploadFilesToStorage)
            {
                return Json(new { success = true });
            }

            try
            {
                OperationResult result = await processService.DeleteFile(key);
                if (result.IsSuccess)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, error = result.ErrorMessage });

            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Проблем при изтриване на файл в {DeleteFile}, за файл с ключ {key}");
                return Json(new { success = false, error = MessageConstant.Values.DeleteFailed });
            }
        }

        /// <summary>
        /// Търси поле на форма по име до едно ниво навътре
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private FormField FindFieldMetadata(IEnumerable<FormField> fields, string fieldName)
        {
            foreach (var field in fields)
            {
                if (field.Name == fieldName)
                {
                    return field;
                }

                foreach (var innerField in field.Fields)
                {
                    if (innerField.Name == fieldName)
                    {
                        return innerField;
                    }
                }
            }

            return null;
        }

   
        /// <summary>
        /// Страница с табличен преглед на регистрациите по услуга
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = $"{UserRoles.Manager},{UserRoles.Editor},{UserRoles.Registrator}")]
        [HttpGet]
        [Display(Name = "Зареждане на табличен преглед на регистрациите")]
        public async Task<IActionResult> TableView(int serviceId)
        {
            var currentRegisterServices = await service.GetServiceDDL(new List<int>());
            string? serviceTitle = currentRegisterServices.SingleOrDefault(c => c.Value == serviceId.ToString())?.Text;

            var fieldNameDdl = await formConfigurationPersistenceService.GetColumnsForTableView(serviceId);
            fieldNameDdl.Insert(0, new SelectListItem { Value = String.Empty, Text = String.Empty });
            ViewBag.FieldName_Ddl = fieldNameDdl;

            CustomTableViewViewModel model = new CustomTableViewViewModel
            {
                ServiceId = serviceId,
                Title = serviceTitle
            };

            return View(model);
        }

        /// <summary>
        /// Страница с табличен преглед на потребителски справки
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на табличен преглед на потребителски справки")]
        public async Task<IActionResult> CustomTableView(int customViewId, string customViewName)
        {
            var fieldNameDdl = await formConfigurationPersistenceService.GetColumnsForTableView(0, customViewId);
            fieldNameDdl.Insert(0, new SelectListItem { Value = String.Empty, Text = String.Empty });
            ViewBag.FieldName_Ddl = fieldNameDdl;

            CustomTableViewViewModel model = new CustomTableViewViewModel
            {
                CustomViewId = customViewId,
                Title = customViewName
            };

            return View(model);
        }

        /// <summary>
        /// Списък от регистрациите
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPost]
        [Display(Name = "Извличане на списък с регистрациите за услуга или справка")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetRegisteredEntitiesList(int serviceId, int customViewId, IDataTablesRequest request, CustomTableViewViewModel filter)
        {
            if (serviceId == 0 && customViewId == 0)
            {
                return new JsonResult(new { });
            }

            IActionResult result = await formConfigurationPersistenceService.GetTableDataForService(serviceId, customViewId, request, filter);
            return result;
        }

        [Display(Name = "Извличане на информация за форма по услуга")]
        public async Task<JsonResult> GetFormInfo(int serviceId)
        {
            var aService = await service.GetService(serviceId);
            var aForm = await service.GetForm(aService.FormParentId);
            var formModel = await formConfigurationPersistenceService.GetFormViewModel(aService.FormParentId);
            var fields = formModel.FormFields.Select(x => new BlanksTemplateParamVM
            {
                Label = x.Label,
                Name = x.Name,
            })
            .ToList();
            return Json(new { formName = aForm.Title, fields });
        }

        [Display(Name = "Зареждане на форма за добавяне на указание към заявена услуга")]
        public async Task<IActionResult> AddInstruction(Guid processId)
        {
            var process = await processService.GetByIdAsync<Process>(processId);
            var model = new InstructionVM
            {
                ProcessId = processId,
                ResultDeliveryMethod = process.PreferredResultDeliveryMethod
            };
            return View("InstructionEdit", model);
        }

        [Display(Name = "Запис на указание към заявена услуга")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InstructionEdit(InstructionVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    (var process, var sourceId) = await processService.SaveInstruction(model);
                    byte[] filesAsBytes = new byte[0];
                    var blankInstruction = await processService.GetBlankInstruction(process.Form.ParentId ?? 0);
                    if (blankInstruction != null)
                    {
                        var fileId = await MakeDraftBlankForNotRegistered(process.Id, blankInstruction, (int)EDeliveryMessageType.OutInstruction);
                        if (fileId != null)
                        {
                            filesAsBytes = await processService.GetCertificateFile(fileId ?? Guid.Empty);
                        }

                    }
                    if (model.ResultDeliveryMethod == ChannelType.EDelivery)
                    {
                        await processService.SendMessageForProcess(
                            process.Id,
                            filesAsBytes,
                            (int)EDeliveryMessageType.OutInstruction,
                            sourceId.ToString(),
                            $"По повод заявление с вх. № {process.IncomingNumber} от {process.IncomingDate:dd.MM.yyyy} г., " +
                            $"Ви уведомяваме, че по заявлението са дадени указания, както следва: {model.Content}");
                    }
                    await processService.SetInstructionActive(sourceId);
                    SetSuccessMessage("Успешен запис на указание.");
                    return RedirectToAction("InstructionIndex", new { processId = model.ProcessId });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message, ex);
                    SetErrorMessage("Проблем при  запис на указание.");
                }
            }
            else
            {
                SetErrorMessage("Невалидни данни.");
            }
            return View("InstructionEdit", model);
        }

        [Display(Name = "Преглед на отговорите към указание")]
        public async Task<IActionResult> InstructionResponse(Guid instructionId)
        {
            var model = await processService.GetInstructionResponses(instructionId);
            return View("InstructionResponses", model);
        }

        [Display(Name = "Взимане на списък с указания")]
        public IActionResult GetInstructionResponseList(IDataTablesRequest request, InstructionVM filter)
        {
            return processService.GetInstructionResponseList(request, filter.Id);
        }

        [Display(Name = "Зареждане на форма за добавяне на указание към заявена услуга")]
        public IActionResult AddInstructionResponse(Guid instructionId)
        {
            var model = new InstructionResponseItemVM
            {
                InstructionId = instructionId,
            };
            return View("InstructionResponseEdit", model);
        }

        [HttpGet]
        [Display(Name = "Зареждане на форма за редакция на указание към заявена услуга")]
        public async Task<IActionResult> InstructionResponseEdit(Guid id)
        {
            var model = await processService.GetInstructionResponse(id);
            return View("InstructionResponseEdit", model);
        }

        /// <summary>
        /// Partial за файл
        /// </summary>
        /// <param name="index"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        [Display(Name = "Добавяне на файл")]
        public IActionResult AddAttachedFile(int index, string prefix)
        {
            var model = new FileVM
            {
                Index = index,
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = $"{prefix}[{index}]";
            return PartialView("_AttachedFile", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Запис изпълнение на указание към заявена услуга")]
        public async Task<IActionResult> InstructionResponseEdit(InstructionResponseItemVM model)
        {
            try
            {
                await processService.SaveInstructionResponse(model);
                SetSuccessMessage("Успешен запис на изпълнение на указание.");
                return RedirectToAction("InstructionResponseIndex", new { instructionId = model.InstructionId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                SetErrorMessage("Проблем при запис на изпълнение на указание.");
            }
            return View("InstructionResponseEdit", model);
        }

        /// <summary>
        /// Уплоад на файл
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Уплоад на файл")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UploadAttachedFile(IFormFile file)
        {
            var metaFileId = await processService.SaveAttachedFile(file);
            return Json(new { metaFileId });
        }

        /// <summary>
        /// Уплоад на файл
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Урл за сваляне на файл")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetAttachedFileUrl(Guid id)
        {
            (var fileUrl, var fileName) = await processService.GetAttachedFileUrl(id);
            return Json(new { fileUrl, fileName });
        }
        /// <summary>
        /// Изчисляване на срок на услуга
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Изчисляване на срок на услуга")]
        public async Task<JsonResult> GetDeadlineDate(int deadlineId, Guid processId)
        {
            var dateSrok = await processService.GetDeadlineDate(deadlineId, processId);
            return Json(new { dateSrok });
        }

        [Authorize(Roles = $"{UserRoles.Admin}")]
        [HttpPost]
        [Display(Name = "Връщане на заявена услуга за обработка")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeAssignUser(Guid processId)
        {
            try
            {
                await processService.DeAssignUser(processId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Връщане на заявена услуга за обработка {processId}");
                return Json(new { success = false, error = $"Проблем при Връщане на заявена услуга за обработка" });
            }
        }

        [Authorize(Roles = $"{UserRoles.Manager},{UserRoles.Editor}")]
        [Display(Name = "Зареждане на списък с връчвания на указания/откази/удостоверения за заявена услуга")]
        public async Task<IActionResult> ProcessDeliveryIndex(Guid processId)
        {
            var process = await processService.GetByIdAsync<Core.Data.Models.Process.Process>(processId);
            var model = new ProcessDeliveryFilterVM
            {
                ProcessId = processId,
                ProcessLabel = await processService.GetProcessLabel(processId) ?? string.Empty,
            };
            return View(model);
        }

        /// <summary>
        /// списък с връчвания за заявена услуга
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с връчвания за заявена услуга")]
        public async Task<IActionResult> GetProcessDeliveryList(IDataTablesRequest request, ProcessDeliveryFilterVM filter)
        {
            return await processService.GetProcessDeliveryList(request, filter);
        }
    }
}
