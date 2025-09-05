using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Models.Common;

namespace URegister.Core.Contracts
{
    public interface IImportService
    {
        Task<List<Dictionary<string, string>>> GetImportData(string? fileId);
        Task<FileVM> GetMaketFile();
        Task<string?> SaveImportFile(IFormFile file);
        Task<string?> SaveImportMaketFile(IFormFile file);
    }
}
