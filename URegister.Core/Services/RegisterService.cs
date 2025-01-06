using Core.Services;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Register;
using URegister.Core.Models.CurrentRegister;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.RegistersCatalog;

namespace URegister.Core.Services
{
    public class RegisterService: BaseService, IRegisterService
    {
        private readonly INomenclatureClientService nomenclatureClientService;
        private readonly RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient;
        public RegisterService(
            IApplicationRepository repo, 
            ILogger<BaseService> logger,
            INomenclatureClientService nomenclatureClientService,
            RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient
        ) : base(repo, logger)
        {
            this.nomenclatureClientService = nomenclatureClientService;
            this.registerGrpcClient = registerGrpcClient;
        }
        public async Task<int> GetCurrentRegisterId()
        {
            return await Repo.AllReadonly<Register>()
                .Select(x => x.Id)
                .TagWith(nameof(GetCurrentRegisterId))
                .SingleAsync();
        }

        public async Task<RegisterVM> GetCurrentRegister()
        {
            return await Repo.AllReadonly<Register>()
                             .Select(x => new RegisterVM
                             {
                                 Id = x.Id,
                                 Type = x.Type,
                                 LegalBasis = x.LegalBasis,
                                 TypeEntry = x.TypeEntry,
                                 Code = x.Code,
                                 Description = x.Description,
                                 IdentitySecurityLevel = x.IdentitySecurityLevel,
                                 Name = x.Name,
                             })
                             .TagWith(nameof(GetCurrentRegister))
                             .SingleAsync();
        }

        public async Task SaveRegister(RegisterVM model)
        {
            var register = await Repo.All<Register>().SingleAsync();
            register.Type = model.Type;
            register.LegalBasis = model.LegalBasis;
            register.TypeEntry = model.TypeEntry;
            register.Code = model.Code;
            register.Description = model.Description;
            register.IdentitySecurityLevel = model.IdentitySecurityLevel;
            register.Name = model.Name;
            await Repo.SaveChangesAsync();
        }
        public async Task<IActionResult> GetPersonList(IDataTablesRequest request, Guid administrationId)
        {
            var nomTypes = new[] {
                InternalNomenclatureTypes.PersonType,
            };
            var nomenclatureTypes = await nomenclatureClientService.GetNomenclaturePublic(await GetCurrentRegisterId(), nomTypes);
            var query = Repo.AllReadonly<AdministrationPerson>()
                          .Where(x => x.AdministrationId == administrationId)
                          .Select(x => new PersonListItem
                          {
                              Id = x.Id,
                              FirstName = x.FirstName,
                              MiddleName = x.MiddleName,
                              LastName = x.LastName,
                              Position = x.Position,
                              Type = x.Type,
                              Email = x.Email,
                              Phone = x.Phone,
                          })
                          .TagWith(nameof(GetPersonList));
            var countAll = 0;
            (query, countAll) = request.GetResponseData(query);
            var data = await query.ToListAsync();
            data.ForEach(x => x.Type = nomenclatureClientService.GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.PersonType, x.Type));
            return request.GetResponseJson(query, countAll);
        }

        public async Task<IActionResult> GetAdministrationList(IDataTablesRequest request)
        {
            var nomTypes = new string[] {
                InternalNomenclatureTypes.RegisterType,
                InternalNomenclatureTypes.RegisterEntryType,
                InternalNomenclatureTypes.RegisterIdentitySecurityLevel 
            };
            var registerId = await GetCurrentRegisterId();
            //var nomenclatureTypes = await nomenclatureClientService.GetNomenclaturePublic(await GetCurrentRegisterId(), nomTypes);
            var query = Repo.AllReadonly<Administration>()
                             .Where(x => x.RegisterId == registerId)
                             .Select(x => new AdministrationListItem
                             {
                                 Id = x.Id.ToString(),
                                 Uic = x.Uic,
                                 Name = x.Name,
                                 LegalBasis = x.LegalBasis,
                             })
                             .TagWith(nameof(GetAdministrationList));
            return request.GetResponse(query);
        }

        public async Task<AdministrationVM> GetAdministration(Guid administrationId)
        {
            return await Repo.AllReadonly<Administration>()
                             .Where(x => x.Id == administrationId)
                             .Select(x => new AdministrationVM
                             {
                                 Id = x.Id,
                                 Uic = x.Uic,
                                 Name = x.Name,
                                 LegalBasis = x.LegalBasis,
                                 Manager = x.People.Where(x => x.Type == PersonTypeValue.Manager)
                                            .Select(p => new PersonVM
                                            {
                                                Id = p.Id,
                                                Type = p.Type,
                                                FirstName = p.FirstName,
                                                MiddleName = p.MiddleName,
                                                LastName = p.LastName,
                                                Phone = p.Phone,
                                                Email = p.Email,
                                                Position = p.Position,
                                            })
                                            .FirstOrDefault()!
                             })
                             .TagWith(nameof(GetAdministration))
                             .FirstAsync();
        }

        public async Task SaveAdministration(AdministrationVM model)
        {
            var administration = await Repo.All<Administration>()
                                           .Include(x => x.People)   
                                           .Where(x => x.Id == model.Id)
                                           .FirstAsync();
            administration.Uic = model.Uic;
            administration.Name = model.Name;
            administration.LegalBasis = model.LegalBasis;
            var manager = administration.People
                .First(x => x.Type == PersonTypeValue.Manager);
            manager.FirstName = model.Manager.FirstName;
            manager.MiddleName = model.Manager.MiddleName;
            manager.LastName = model.Manager.LastName;
            manager.Phone = model.Manager.Phone;
            manager.Email = model.Manager.Email;
            manager.Position = model.Manager.Position;
            await Repo.SaveChangesAsync();
        }

        public async Task<List<SelectListItem>> GetRegisterNotStartedDdl()
        {
            return (await registerGrpcClient.GetRegisterNotStartedListAsync(new Google.Protobuf.WellKnownTypes.Empty()))
                          .Data.Select(x => new SelectListItem
                          {
                              Text = x.Label,
                              Value = x.Id.ToString()
                          })
                          .ToList();
        }

        public async Task StartRegister(int registerId)
        {
            var registerItem = (await registerGrpcClient.GetRegisterAndMarkAsStartedAsync(new GetRegisterRequest { RegisterId = registerId })).Data;
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
            register.Administrations = registerItem.Administrations.Select(x => new Administration
            {
                Id = Guid.Parse(x.Id),
                Name = x.Name,
                LegalBasis = x.LegalBasis,
                Uic = x.Uic,
                CreatedOn = DateTime.UtcNow,
                People = x.Persons.Select(p => new AdministrationPerson
                {
                    Id = p.Id,
                    Type = p.Type,
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    Position = p.Position,
                    Phone = p.Phone,
                    Email = p.Email,
                    CreatedOn = DateTime.UtcNow
                }).ToList()
            }).ToList();
            await Repo.AddAsync(register);
            await Repo.SaveChangesAsync();
        }
    }
}
