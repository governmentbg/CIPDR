using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Data.Models.Common;
using URegister.Core.Models.Process;
using URegister.Infrastructure.Model.RegisterForms;

namespace URegister.Core.Contracts
{
    public interface IProcessService
    {
        Task AddProcess(ProcessVM model);
        Task AddStep(ProcessStepVM model);
        Task<ProcessStepVM> GetFormViewModel(Guid processId);
        IActionResult GetProcessList(IDataTablesRequest request);
        ProcessStepVM ToProcessStepVM(Guid processId, int serviceStepId, int orderNum, FormViewModel formModel);
    }
}
