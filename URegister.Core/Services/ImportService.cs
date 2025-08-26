using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdParty.BouncyCastle.Utilities.IO.Pem;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Identity;
using URegister.Core.Models.Common;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Extensions;

namespace URegister.Core.Services
{
    public class ImportService : BaseService, IImportService
    {
        private readonly IObjectStoreService objectStoreService;
        public ImportService(
            IApplicationRepository repo,
            IObjectStoreService objectStoreService,
            ILogger<BaseService> logger
        ) : base(repo, logger)
        {
            this.objectStoreService = objectStoreService;
        }
        private Dictionary<string, string> GetKeys(ExcelWorksheet sheet)
        {
            var result = new Dictionary<string, string>();
            var colCount = sheet.Dimension.Columns;
            var lenght = 26;
            for (int i = 0; i < colCount; i++)
            {
                var colCode = string.Empty;
                var prefix = i / lenght;
                if (prefix > 0)
                {
                    colCode += (char)(((int)'A') + prefix - 1);
                }
                colCode += (char)(((int)'A') + i - prefix * lenght);
                var val = sheet.Cells[$"{colCode}1"].Value?.ToString();
                result.Add(val ?? string.Empty, $"{colCode}");
            }
            return result;
        }

        public async Task<List<Dictionary<string, string>>> GetImportData(string? fileId)
        {
            var data = new List<Dictionary<string, string>>();
            (var bytes, _) = await objectStoreService.GetObject(fileId);
            using (var ms = new MemoryStream(bytes))
            {
                using (var package = new ExcelPackage(ms))
                {
                    var sheet = package.Workbook.Worksheets[0];
                    var keys = GetKeys(sheet);
                    var rowCount = sheet.Dimension.Rows;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var rowData = new Dictionary<string, string>();
                        var hasValue = false;
                        foreach (var kv in keys)
                        {
                            var val = sheet.Cells[$"{kv.Value}{row}"].Value?.ToString() ?? string.Empty;
                            rowData.Add(kv.Key, val);
                            if (!string.IsNullOrEmpty(val))
                            {
                                hasValue = true;
                            }
                        }
                        if (hasValue)
                        {
                            data.Add(rowData);
                        }
                    }
                }
            }
            return data;
        }
        public async Task<string?> SaveImportFile(IFormFile file)
        {
            try
            {
                byte[] filesAsBytes = [];
                using MemoryStream ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;
                filesAsBytes = ms.ToArray();
                var fileId = await objectStoreService.SaveObject(file.FileName, filesAsBytes, file.ContentType, null);
                var metaFile = new FileMetadata
                {
                    FileName = file.FileName,
                    FileSourceTypeId = (int)FileSourceType.Import,
                    FileId = fileId.ToGuid() ?? Guid.Empty,
                };
                await Repo.AddAsync(metaFile);
                await Repo.SaveChangesAsync();
                return fileId;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем при качване на файл {file.FileName}");
                return null;
            }
        }

        public async Task<string?> SaveImportMaketFile(IFormFile file)
        {
            try
            {
                var sourceId = "IMPORT_MAKET";
                byte[] filesAsBytes = [];
                using MemoryStream ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;
                filesAsBytes = ms.ToArray();
                var fileId = await objectStoreService.SaveObject(file.FileName, filesAsBytes, file.ContentType, null);
                var metaFile = await Repo.All<FileMetadata>()
                                         .Where(x => x.FileSourceTypeId == (int)FileSourceType.ImportMaket &&
                                                     x.SourceId == sourceId)
                                         .FirstOrDefaultAsync();
                if (metaFile == null)
                {
                    metaFile = new FileMetadata
                    {
                        FileSourceTypeId = (int)FileSourceType.ImportMaket,
                        SourceId = sourceId
                    };
                    await Repo.AddAsync(metaFile);
                }

                metaFile.FileName = file.FileName;
                metaFile.FileId = fileId.ToGuid() ?? Guid.Empty;
                await Repo.SaveChangesAsync();
                return fileId;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем при качване на файл {file.FileName}");
                return null;
            }
        }
        public async Task<FileVM> GetMaketFile()
        {
            var sourceId = "IMPORT_MAKET";
            byte[] filesAsBytes = [];
            return await Repo.All<FileMetadata>()
                                     .Where(x => x.FileSourceTypeId == (int)FileSourceType.ImportMaket &&
                                                 x.SourceId == sourceId)
                                     .Select(x => new FileVM
                                     {
                                         Description = x.Description,
                                         FileName = x.FileName,
                                         MetaFileId = x.Id,
                                     })
                                     .FirstOrDefaultAsync() ?? 
                                     new FileVM();
        }

    }
}