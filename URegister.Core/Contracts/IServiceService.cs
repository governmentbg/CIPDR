using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Data.Models.Common;
using URegister.Core.Models.Process;
using URegister.Core.Models.Service;
using URegister.Core.Services;
using URegister.Infrastructure.Model.RegisterForms;

namespace URegister.Core.Contracts
{
    public interface IServiceService
    {
        Task<OperationResult> AppendUpdate(ServiceVM model);
        Task<ServiceVM> GetService(int id, bool ignoreSoftDeletedSteps = false);

        Task<ServiceVM> GetRegisterService();

        Task<List<SelectListItem>> GetServiceDDL(List<int> serviceTypes);
        Task<IActionResult> GetServiceList(IDataTablesRequest request);
        Task<ServiceStep> GetServiceStep(int id);
        Task<List<SelectListItem>> GetServiceStepDDL(int serviceTypeId);
        Task<List<SelectListItem>> GetServiceTypeDDL();

        Task<List<ServiceStep>> GetServiceSteps(int serviceId);

        Task<List<SelectListItem>> GetStepDDL();
        Task<IActionResult> GetBlankTemplateList(IDataTablesRequest request);
        Task<OperationResult> Delete(int id);
        Task AppendUpdate(BlanksTemplateVM model);
        Task<BlanksTemplateVM> GetBlankTemplate(int id);
        Task<Form> GetForm(int formParentId);
        Task<BlanksTemplateContentVM> GetBlankTemplateContent(int id);
        Task AppendUpdateContent(BlanksTemplateContentVM model);
        Task<OperationResult> DeleteTemplate(int id);
        Task<List<BlanksTemplateParamVM>> GetTemplateParam(FormViewModel formModel, string prefix);
        List<BlanksTemplateParamVM> GetTemplateProcessParam(string prefix);
    }
}
