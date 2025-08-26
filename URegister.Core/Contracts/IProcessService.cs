using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Core.Models.Process;
using URegister.Core.Services;
using URegister.Infrastructure.Model.EDelivery;
using URegister.Infrastructure.Model.RegisterForms;

namespace URegister.Core.Contracts
{
    public interface IProcessService: IBaseService
    {
        Task<(ProcessStepVM, Process)> AddStep(ProcessStepVM model, 
            string targetAdministrationUic = "", 
            Guid? eFormRegisteredServiceNumber = null);
        Task<(ProcessStepVM, Process)> GetFormViewModel(Guid processId, bool preview);
        Task<IActionResult> GetProcessList(IDataTablesRequest request, ProcessFilterVM filter);
        Task<ProcessStepVM> ToProcessStepVM(
            Guid processId,
            Guid? fromProcessId,
            int serviceId,
            int serviceStepId,
            int orderNum,
            string? oldIncomingNumber,
            DateTime? oldIncomingDate,
            FormViewModel formModel,
            bool preview);

        ///// <summary>
        ///// Връща всички въведени от потребителят данни за дадена форма
        ///// </summary>
        ///// <param name="processId">Идентификатор на заявена услуга</param>
        ///// <returns></returns>
        //public Task<JsonResult> GetFormData(Guid processId);
        Task<ProcessStepVM> GetFormViewModel(int serviceId, string? OldIncomingNumber, DateTime? OldIncomingDate, bool isOld);
        Task<Process> Refuse(Guid processId, string reasonForRejection);
        Task<ProcessStepVM> GetFormViewModelFrom(Guid fromProcessId, int serviceId);

        /// <summary>
        /// Записва файл в хранилището
        /// </summary>
        /// <param name="file">Файлът за запис</param>
        /// <param name="key">Ключ на файла при презапис</param>
        /// <param name="eformId">Идентификатор на е-форма, ако файлът е заявление подадено чрез е-форма</param>
        /// <param name="eformDateOfFill">Дата на попълване на е-формата, ако файлът е заявление подадено чрез е-форма</param>
        /// <returns>Ключа на записания файл</returns>
        public Task<SaveOperationResult> SaveUploadedFile(IFormFile file, Guid key, Guid? eformId = null, DateTime? eformDateOfFill = null);

        /// <summary>
        /// Премахва качен файл по ключ на файла
        /// </summary>
        /// <param name="key">Ключ на файла</param>
        /// <returns></returns>
        public Task<OperationResult> DeleteFile(string key);

        /// <summary>
        /// Списък на заявени услуги по статус за Dashboard
        /// </summary>
        /// <param name="request"></param>
        /// <param name="statusId">Идентификатор на статус</param>
        /// <returns></returns>
        public Task<IActionResult> GetProcessListDashboard(IDataTablesRequest request, int statusId);
        public Task<int> GetUserAssignedProcessCount();
        public Task<Process?> GetAssignableProcess();
        public Task AssignProcess(Guid processid);


        /// <summary>
        /// Връща заявена услуга за вписване по списък от полета от форма
        /// </summary>
        /// <param name="formFields">Списък от полета от форма</param>
        /// <returns></returns>
        Task<Process?> GetProcessForCertificate(List<FormField> formFields);

        Task<byte[]> GetCertificateFile(Guid id);
        Task<bool> IsCertificateStep(int serviceStepId);
        Task<Guid?> SaveCertificateFileDraft(Guid processId, byte[] filesAsBytes, int typeMessageId);
        Task<byte[]> GetCertificateFileSigned(Guid processId);
        Task<List<RegisterItem>> AddRegisterItems(Process process, List<FormField> formFields, Guid processStepId, int userTimeZoneOffsetInMinutes);
        Task SendMessageForProcess(Guid processId, byte[] filesAsBytes, int typeMessageId, string sourceId, string message);
        (string?, string?, string?) GetMPRIData(int roleId, List<FormField> formFields);

        /// <summary>
        /// Връща историята на заявена услуга
        /// </summary>
        /// <param name="processId">Идентификатор на процесс</param>
        /// <returns></returns>
        Task<IActionResult> GetProcessHistory(Guid processId);

        /// <summary>
        /// Връща заявена услуга по номер на заявена услуга от е-форма
        /// </summary>
        /// <param name="eFormRegisteredServiceNumber"></param>
        /// <returns></returns>
        public Task<ProcessVM> GetProcess(Guid eFormRegisteredServiceNumber);

        /// <summary>
        /// Връща заявена услуга по номер на стар запис
        /// </summary>
        /// <param name="oldIncomingNumber">Номер на стар запис</param>
        /// <returns></returns>
        public Task<ProcessVM> GetProcessByOldIncomingNumber(string oldIncomingNumber);
        Task<string?> GetProcessLabel(Guid processId);
        Task<IActionResult> GetInstructionList(IDataTablesRequest request, InstructionFilterVM filter);
        Task<(Process, Guid)> SaveInstruction(InstructionVM model);
        Task ImportEDeliveryFile(EDeliveryMessageVM model);
        Task<InstructionResponseVM> GetInstructionResponses(Guid instructionId);
        Task<BlanksTemplate?> GetBlankOnRegister(int formParentId);
        Task<Process?> GetProcessForCertificateOnRegister(Guid processId);
        Task<BlanksTemplate?> GetBlankInstruction(int formParentId);
        Task<BlanksTemplate?> GetBlankRefuse(int formParentId);
        Task<InstructionResponseVM> GetInstructionResponsesOnProcess(Guid processId);
        IActionResult GetInstructionResponseList(IDataTablesRequest request, Guid instructionId);
        Task<Guid> SaveAttachedFile(IFormFile file);
        Task SaveInstructionResponse(InstructionResponseItemVM model);
        Task<InstructionResponseItemVM> GetInstructionResponse(Guid id);
        Task<(string, string)> GetAttachedFileUrl(Guid id);
        void FillProcessInfoVM(IFormCollection form, ProcessInfoVM ProcessInfo);
        Task<DateTime> GetDeadlineDate(int deadlineId, Guid processId);
        Task<List<FileMetadata>> ImportApplicationEDeliveryFile(List<EDeliveryFileVM> files);
        Task ImportApplicationEDeliveryFileSetProcess(Guid processId, List<FileMetadata> files);
        Task SetInstructionActive(Guid id);
        Task DeAssignUser(Guid processId);
        Task<IActionResult> GetProcessDeliveryList(IDataTablesRequest request, ProcessDeliveryFilterVM filter);
    }
}