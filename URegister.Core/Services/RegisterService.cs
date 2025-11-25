using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Register;
using URegister.Core.Models.CurrentRegister;
using URegister.Infrastructure.Constants;
using URegister.NomenclaturesCatalog;
using URegister.RegistersCatalog;

namespace URegister.Core.Services
{
    public class RegisterService : BaseService, IRegisterService
    {
        private readonly INomenclatureClientService nomenclatureClientService;
        private readonly RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient;
        private readonly IRegisterClientService registerClient;
        private readonly ILogger<BaseService> logger;
        public RegisterService(
            IApplicationRepository repo,
            ILogger<BaseService> logger,
            INomenclatureClientService nomenclatureClientService,
            RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
            IRegisterClientService registerClient
        ) : base(repo, logger)
        {
            this.nomenclatureClientService = nomenclatureClientService;
            this.registerGrpcClient = registerGrpcClient;
            this.registerClient = registerClient;
            this.logger = logger;
        }
        public async Task<int> GetCurrentRegisterId()
        {
            return await Repo.AllReadonly<Register>()
                .Select(x => x.Id)               
                .TagWith(nameof(GetCurrentRegisterId))
                .SingleAsync();
        }

        public  int GetCurrentRegisterIdForAudit()
        {
            return (Repo.AllReadonly<Register>()
                .Select(x => (int?)x.Id)
                .TagWith(nameof(GetCurrentRegisterId))
                .FirstOrDefault()) ?? 0;
        }

        public async Task<RegisterVM> GetCurrentRegister()
        {
            var id = await GetCurrentRegisterId();
            var model = await registerClient.GetRegister(id, Guid.Empty);
            return new RegisterVM
            {
                Id = model.Id,
                Type = model.Type,
                LegalBasis = model.LegalBasis,
                TypeEntry = model.TypeEntry,
                Code = model.Code,
                Description = model.Description,
                IdentitySecurityLevel = model.IdentitySecurityLevel,
                Name = model.Name,
                HistoryNotPublic = model.HistoryNotPublic
            };
        }

        public async Task SaveRegister(RegisterVM model)
        {
            var register = new Core.Models.Register.RegisterVM();
            register.Id = model.Id;
            register.Type = model.Type;
            register.LegalBasis = model.LegalBasis;
            register.TypeEntry = model.TypeEntry;
            register.Code = model.Code;
            register.Description = model.Description;
            register.IdentitySecurityLevel = model.IdentitySecurityLevel;
            register.Name = model.Name;
            await registerClient.EditRegister(register);
        }

        public async Task<List<NomenclatureTypePublicResponse>> GetPersonTypes()
        {
            var nomTypes = new[] {
                InternalNomenclatureTypes.PersonType,
            };
            return await nomenclatureClientService.GetNomenclaturePublic(await GetCurrentRegisterId(), nomTypes);
        }
        

        public async Task<PersonVM> InitPerson(Guid administrationId, string personType)
        {
            var nomenclatureTypes = await GetPersonTypes();
            return new PersonVM
            {
                AdministrationId = administrationId,
                Type = personType,
                TypeName = nomenclatureClientService.GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.PersonType, personType)
            };
        }


        public async Task<RegisterVM> StartRegister(string registerCode)
        {
            var response = (await registerGrpcClient.GetRegisterAndMarkAsStartedAsync(new GetRegisterByCodeRequest { RegisterCode = registerCode }));
            if (response.Status.Code != ResultCodes.Ok)
            {
                logger.LogError(response.Status.Message+ registerCode);
            }
            var registerItem = response.Data;
            var register = new Register
            {
                Id = registerItem.Id,
                Code = registerItem.Code,
                Name = registerItem.Name,
                Description = registerItem.Description,
                LegalBasis = registerItem.LegalBasis,
                TypeEntry = registerItem.EntryType,
                Type = registerItem.Type,
                IdentitySecurityLevel = registerItem.IdentitySecurityLevel,
                CreatedOn = DateTime.UtcNow,
            };
            await Repo.AddAsync(register);
            await Repo.SaveChangesAsync();
            return await GetCurrentRegister();
        }

       
    }
}
