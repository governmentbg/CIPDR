using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Data.Models.Common;
using URegister.Core.Models.Service;
using URegister.Core.Services;
using URegister.Infrastructure.Model.RegisterForms;

namespace URegister.Core.Contracts
{
    public interface IPublicFieldTemplateService
    {
        Task AppendUpdate(PublicFieldTemplateVM model);
        Task AppendUpdateContent(PublicFieldTemplateVM model);
        Task<OperationResult> DeleteTemplate(int id);
        Task<PublicFieldTemplateVM> GetTemplate(int id);
        Task<IActionResult> GetTemplateList(IDataTablesRequest request);
        Task<List<BlanksTemplateParamVM>> GetTemplateParam(FormViewModel formModel, string prefix);
        Task<List<PublicFieldTemplate>> GetTemplates();
        Task OrderNumChange(int id, bool up);
    }
}
