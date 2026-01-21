using Google.Protobuf.WellKnownTypes;
using IO.SignTools.Contracts;
using IO.SignTools.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Core.Models.Common;
using URegister.Core.Models.Previewer;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.IntegrationsCatalog;
using URegister.RegistersCatalog;
using URegister.Users;

namespace URegister.Core.Services
{
    public class CommonFileService : BaseService, ICommonFileService
    {
        private readonly IObjectStoreService objectStoreService;
        private readonly IUserContext userContext;
        private readonly IConfiguration configuration;
        private readonly IRegisterService registerService;
        private readonly RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient;
        private readonly IIOSignToolsService signToolsService;
        private readonly IntegrationGrpc.IntegrationGrpcClient integrationGrpcClient;
        private readonly AppUserManager.AppUserManagerClient appUserManager;
        public CommonFileService(IApplicationRepository repo,
            IObjectStoreService objectStoreService,
            IUserContext userContext,
            IConfiguration configuration,
            RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
            IRegisterService registerService,
            IIOSignToolsService signToolsService,
            IntegrationGrpc.IntegrationGrpcClient integrationGrpcClient,
            AppUserManager.AppUserManagerClient appUserManager,
            ILogger<CommonFileService> logger)
            : base(repo, logger)
        {
            this.objectStoreService = objectStoreService;
            this.userContext = userContext;
            this.configuration = configuration;
            this.registerGrpcClient = registerGrpcClient;
            this.registerService = registerService;
            this.signToolsService = signToolsService;
            this.integrationGrpcClient = integrationGrpcClient;
            this.appUserManager = appUserManager;
        }

        public async Task<FileInfoModel> GetFileInfo(string id)
        {
            var guid = Guid.Parse(id);
            return await Repo.AllReadonly<FileMetadata>()
                             .Where(x => x.Id == guid)
                             .Select(x => new FileInfoModel
                             {
                                 Id = x.Id.ToString(),
                                 ContentType = PreviewConstants.ContentType.PDF,
                                 IsSigned = !string.IsNullOrEmpty(x.Signature),
                                 Signature = x.Signature,
                                 Name = x.FileName,
                                 Size = 1,

                             })
                             .FirstAsync();
        }

        public async Task<(byte[], FileMetadata, string)> GetFileById(string id)
        {
            var guid = Guid.Parse(id);
            var metadata = await Repo.AllReadonly<FileMetadata>()
                              .Where(x => x.Id == guid)
                              .FirstAsync();
            var file = await objectStoreService.GetObject(metadata.FileId.ToString());
            return (file.data, metadata, file.contentType);
        }

        public async Task ChangeFile(string fileId, byte[] newPdf, Guid? roleId)
        {
            var guid = Guid.Parse(fileId);
            var fileMeta = await Repo.All<FileMetadata>()
                               .Where(x => x.Id == guid)
                               .FirstAsync();
            var blankSignatures = await Repo.All<BlankSignature>()
                               .Where(x => x.BlankTemplateId == fileMeta.BlanksTemplateId)
                               .OrderBy(x => x.OrderNum)
                               .ToListAsync();
            var blankSignature = blankSignatures.Where(x => x.OrderNum == (fileMeta.SignOrder + 1)).FirstOrDefault();
            fileMeta.IsActive = false;
            var newFileId = await objectStoreService.SaveObject(fileMeta.FileName, newPdf, "application/pdf", null);
            if (!string.IsNullOrEmpty(newFileId))
            {
                var newMetaFile = new FileMetadata
                {
                    FileName = fileMeta.FileName,
                    Description = fileMeta.Description,
                    FileSourceTypeId = fileMeta.FileSourceTypeId,
                    SourceId = fileMeta.SourceId,
                    ProcessId = fileMeta.ProcessId,
                    BlanksTemplateId = fileMeta.BlanksTemplateId,
                    IsStamped = fileMeta.IsStamped,
                    FileId = (newFileId ?? string.Empty).ToGuid() ?? Guid.Empty,
                    SignById = userContext.UserId,
                    SignByRoleId = roleId,
                    OutMessageId = fileMeta.OutMessageId,
                    SignOrder = blankSignature?.OrderNum ?? 0,
                    BlankSignatureId = blankSignature?.Id 
                };
                DraftToOfficial(newMetaFile);
                await Repo.AddAsync(newMetaFile);

            }
            await Repo.SaveChangesAsync();
        }

        public async Task<Guid> StampFile(Guid fileId)
        {
            var fileMeta = await Repo.All<FileMetadata>()
                               .Include(x => x.BlankSignature)
                               .Where(x => x.Id == fileId)
                               .FirstAsync();
            fileMeta.IsActive = false;
            (var filesAsBytes, var contentType) = await objectStoreService.GetObject(fileMeta.FileId.ToString());
            var process = await Repo.AllReadonly<Process>()
                                    .Include(x => x.ProcessSteps)
                                    .IgnoreQueryFilters()
                                    .Where(x => x.Id == fileMeta.ProcessId)
                                    .FirstAsync();
            var administration = await registerGrpcClient.GetAdministrationAsync(new GetAdministrationRequest { AdministrationId = userContext.AdministrationId.ToString() });
            var register = await registerService.GetCurrentRegister();
            var administrationName = !string.IsNullOrEmpty(administration.Data.NameEDelivery) ? administration.Data.NameEDelivery : administration.Data.Name;
            var registerName = !string.IsNullOrEmpty(register.NameEDelivery) ? register.NameEDelivery : register.Name;

            var service = await Repo.AllReadonly<Service>()
                                    .IgnoreQueryFilters()
                                    .Where(x => x.Id == process.ServiceId)
                                    .FirstAsync();
            var options = new IOStampOptions
            {
                DisplayText = administration.Data.Name,
                Reason = service.Title,
                Coordinates = new iText.Kernel.Geom.Rectangle(400, 800, 180, 28),
                PageNum = 1,
                PathToStamp = configuration.GetValue<string>("Signer:CertificateFile"),
                Password = configuration.GetValue<string>("Signer:CertificatePassword"),
                Font = "SignFonts/times.ttf"
            };


            var filemetadata = new FileMetadata
            {
                FileSourceTypeId = fileMeta.FileSourceTypeId,
                FileName = fileMeta.FileName,
                ProcessId = process.Id,
                SourceId = fileMeta.SourceId,
                BlanksTemplateId = fileMeta.BlanksTemplateId,
                Description = fileMeta.Description,
                IsStamped = true,
                OutMessageId = fileMeta.OutMessageId,
            };
            DraftToOfficial(filemetadata);
            filesAsBytes = signToolsService.StampIt(filesAsBytes, options);
            using MemoryStream ms = new MemoryStream(filesAsBytes);
            filesAsBytes = signToolsService.AddLTV(ms);

            filemetadata.FileId = Guid.Parse(await objectStoreService.SaveObject(filemetadata.FileName, filesAsBytes, contentType, null));
            await Repo.AddAsync(filemetadata);
            await Repo.SaveChangesAsync();
            return filemetadata.Id;
        }



       
        public async Task<OutMessage?> GetOutMessageByFileId(Guid id)
        {
            return await Repo.All<FileMetadata>()
                             .Where(x => x.Id == id)
                             .Select(x => x.OutMessage)
                             .FirstAsync();
        }

        public async Task<OutMessage> GetOutMessage(Guid id)
        {
            return await Repo.All<OutMessage>()
                               .Where(x => x.Id == id)
                               .FirstAsync();
        }

        public void DraftToOfficial(FileMetadata data)
        {
            data.FileName = data.FileName.Replace("Draft", string.Empty);
            switch (data.FileSourceTypeId)
            {
                case (int)FileSourceType.CertificateDraft:
                    data.FileSourceTypeId = (int)FileSourceType.Certificate;
                    break;
                case (int)FileSourceType.RefuseDraft:
                    data.FileSourceTypeId = (int)FileSourceType.Refuse;
                    break;
                case (int)FileSourceType.InstructionDraft:
                    data.FileSourceTypeId = (int)FileSourceType.Instruction;
                    break;
                default:
                    break;
            }

        }

        public async Task<List<FileMetadata>> GetFilesForSign(List<Guid> roles)
        {
            var blankSignatures = Repo.AllReadonly<BlankSignature>()
                                      .Where(x => x.RoleId != null && roles.Contains(x.RoleId.Value));
            return await Repo.AllReadonly<FileMetadata>()
                             .Where(x => x.BlanksTemplateId != null &&
                                         blankSignatures.Any(s => s.BlankTemplateId == x.BlanksTemplateId &&
                                                                  s.OrderNum == (x.SignOrder + 1)))
                             .ToListAsync();

        }


        public async Task<List<Guid>> UserRolesForSign()
        {
            var result = new List<Guid>();
            var roleSigningList = await Repo.AllReadonly<BlankSignature>()
                             .Where(x => x.RoleId != null)
                             .Select(x => x.RoleId)
                             .Distinct()
                             .ToListAsync();
            if (roleSigningList.Any())
            {
                var roles = await appUserManager.GetRolesAsync(new Empty());

                foreach (var roleSigning in roleSigningList)
                {
                    var role = roles.Roles.Where(x => x.RoleId == roleSigning.ToString()).First();
                    if (userContext.IsInRole(role.Name))
                    {
                        result.Add(role.RoleId.ToGuid() ?? Guid.Empty);
                    }
                }
            }
            return result;
        }

        public async Task<FileMetadata?> GetFileForSignByProcess(Guid processId)
        {
            var roles = await UserRolesForSign();
            if (!roles.Any())
               return null;

            var blankSignatures = Repo.AllReadonly<BlankSignature>()
                          .Where(x => x.RoleId != null && roles.Contains(x.RoleId.Value));
            return await Repo.AllReadonly<FileMetadata>()
                             .Where(x => x.ProcessId == processId &&
                                         x.BlanksTemplateId != null &&
                                         blankSignatures.Any(s => s.BlankTemplateId == x.BlanksTemplateId &&
                                                                  s.OrderNum == (x.SignOrder + 1)))
                             .FirstOrDefaultAsync();

        }
    }
}
