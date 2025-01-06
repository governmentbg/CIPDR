using Amazon.Runtime.Internal;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using System;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.NomenclaturesCatalog;
using URegister.RegistersCatalog.Contracts;
using URegister.RegistersCatalog.Data;
using URegister.RegistersCatalog.Data.Models;

namespace URegister.RegistersCatalog.Services
{
    /// <summary>
    /// Работа с базата на регистър каталог
    /// </summary>
    /// <param name="repo">репозитори за работа с регистри</param>
    /// <param name="nomenclatureGrpcClient">grpc клиент за номенклатури</param>
    public class RegisterInfoService(
       IRegistersCatalogRepository repo,
       NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient
    ) : IRegisterInfoService
    {

        /// <summary>
        /// Списък регистри за checklist
        /// </summary>
        /// <returns></returns>
        public async Task<List<Common.ListItem>> GetRegisterList()
        {
            return await repo.AllReadonly<Register>()
                             .Select(x => new Common.ListItem
                             {
                                 Id = x.Id,
                                 Label = x.Name,
                             })
                             .ToListAsync();
        }

        /// <summary>
        /// Списък  не стартирали регистри за checklist
        /// </summary>
        /// <returns></returns>
        public async Task<List<Common.ListItem>> GetRegisterNotStartedList()
        {
            return await repo.AllReadonly<Register>()
                             .Where(x => x.StartedOn == null)
                             .Select(x => new Common.ListItem
                             {
                                 Id = x.Id,
                                 Label = x.Name,
                             })
                             .ToListAsync();
        }


        /// <summary>
        /// Страницирани данни за datatables с регистри
        /// </summary>
        /// <param name="request">datatables филтър</param>
        /// <returns>Данни за datatables с регистри</returns>
        public async Task<(List<RegisterListItem>, int)> GetRegisterFullList(RegisterListRequest request)
        {
            var nomenclatureRequest = new NomenclaturePublicRequest();
            nomenclatureRequest.NomenclatureTypes.Add(InternalNomenclatureTypes.RegisterType);
            nomenclatureRequest.NomenclatureTypes.Add(InternalNomenclatureTypes.RegisterEntryType);
            nomenclatureRequest.NomenclatureTypes.Add(InternalNomenclatureTypes.RegisterIdentitySecurityLevel);
            var nomenclatureTypes = (await nomenclatureGrpcClient.GetNomenclaturePublicAsync(nomenclatureRequest))
                                    .NomenclatureTypes
                                    .ToList();

            var query = repo.AllReadonly<Register>()
                            .Select(x => new RegisterListItem
                            {
                                Id = x.Id,
                                Code = x.Code,
                                Name = x.Name,
                                Description = x.Description,
                                LegalBasis = x.LegalBasis,
                                Type = x.Type,
                                EntryType = x.TypeEntry,
                                IdentitySecurityLevel = x.IdentitySecurityLevel,
                            });
            if (!string.IsNullOrEmpty(request.Code))
            {
                query = query.Where(x => EF.Functions.ILike(x.Code, request.Code.ToPaternSearch()));
            }
            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(x => EF.Functions.ILike(x.Name, request.Name.ToPaternSearch()));
            }
            if (!string.IsNullOrEmpty(request.Description))
            {
                query = query.Where(x => EF.Functions.ILike(x.Description, request.Description.ToPaternSearch()));
            }
            var countAll = 0;
            (query, countAll) = await request.DataTableRequest.GetFilteredData(query);
            var data = await query.ToListAsync();
            data.ForEach(x =>
            {
                x.Type = GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.RegisterType, x.Type);
                x.EntryType = GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.RegisterEntryType, x.EntryType);
                x.IdentitySecurityLevel = GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.RegisterIdentitySecurityLevel, x.IdentitySecurityLevel);
            });

            return (data, countAll);
        }

        private async Task<string> GetNewRegNumber()
        {
            var regNumber = await repo.AllReadonly<Register>()
                                          .MaxAsync(x => (string?)x.Code);
            var num = 0;
            if (!string.IsNullOrEmpty(regNumber))
            {
                regNumber = regNumber.Replace(RegisterConstants.CodePrefix, string.Empty);
                int.TryParse(regNumber, out num);
            }
            num++;
            return $"{RegisterConstants.CodePrefix}{num:00000}";
        }

        /// <summary>
        /// Добавяне на регистър
        /// </summary>
        /// <param name="request">данни за регистър</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task AddRegister(RegisterItem request)
        {
            var administrationItem = request.Administrations.First();
            Register? register = new();
            var administration = await repo.All<Administration>()
                                           .Where(x => x.Uic == administrationItem.Uic)
                                           .FirstOrDefaultAsync();
            if (request.Id > 0)
            {
                var administrationId = administration?.Id;
                register = await repo.All<Register>()
                                     .Include(x => x.RegisterAdministrations.Where(a => a.AdministrationId == administrationId))
                                     .Where(x => x.Id == request.Id)
                                     .FirstOrDefaultAsync();
                if (register == null)
                {
                    throw new ArgumentException($"Не намирам запис за регистър с Id {request.Id}");
                }
                if (register.RegisterAdministrations.Any())
                {
                    throw new ArgumentException($"Администрация с ЕИК/БУЛСТАТ {administrationItem.Uic} вече е добавена");
                }
            }
            else
            {
                register.Code = await GetNewRegNumber();
                await repo.AddAsync(register);
            }
            register.Name = request.Name;
            register.Description = request.Description;
            register.LegalBasis = request.LegalBasis;
            register.Type = request.Type;
            register.TypeEntry = request.EntryType;
            register.IdentitySecurityLevel = request.IdentitySecurityLevel;

            if (administration == null)
            {
                administration = new Administration
                {
                    Uic = administrationItem.Uic,
                    Name = register.Name,
                };
                await repo.AddAsync(administration);
            }
            var registerAdministration = new RegisterAdministration
            {
                LegalBasis = register.LegalBasis,
                AdministrationId = administration.Id
            };
            foreach (var personItem in administrationItem.Persons)
            {
                administration.People.Add(new AdministrationPerson
                {
                    FirstName = personItem.FirstName,
                    MiddleName = personItem.MiddleName,
                    LastName = personItem.LastName,
                    Email = personItem.Email,
                    Phone = personItem.Phone,
                    Position = personItem.Position,
                    Type = personItem.Type,
                    AdministrationId = administration.Id,
                    Register = register,
                });
            }
            register.RegisterAdministrations.Add(registerAdministration);
            await repo.SaveChangesAsync();
        }

        /// <summary>
        /// Списък администрации
        /// </summary>
        /// <param name="request">идентификатор на регистър</param>
        /// <returns>администрации</returns>
        public async Task<(List<AdministrationListItem>, int)> GetAdministrationList(AdministrationListRequest request)
        {
            var query = repo.AllReadonly<RegisterAdministration>()
                            .Where(x => x.RegisterId == request.RegisterId)
                            .Select(x => new AdministrationListItem
                            {
                                Id = x.Id.ToString(),
                                Uic = x.Administration.Uic,
                                Name = x.Administration.Name,
                                LegalBasis = x.LegalBasis,
                            });
            var countAll = 0;
            (query, countAll) = await request.DataTableRequest.GetFilteredData(query);
            var data = await query.ToListAsync();

            return (data, countAll);
        }

        /// <summary>
        /// Извличане стойност на номенклатура
        /// </summary>
        /// <param name="nomenclatureTypes">списък от номенклатуре каталог</param>
        /// <param name="nomType">тип</param>
        /// <param name="code">код</param>
        /// <returns>Стойност</returns>
        private string GetNomenclatureValue(List<NomenclatureTypePublicResponse> nomenclatureTypes, string nomType, string code)
        {
            var nomenclatureType = nomenclatureTypes.Where(x => x.Type == nomType).FirstOrDefault();
            return nomenclatureType?.CodeableConcepts.Where(x => x.Code == code)
                                                     .Select(x => x.Value)
                                                     .FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// Списък лица към администрация
        /// </summary>
        /// <param name="request">идентификатор на администрация</param>
        /// <returns></returns>
        public async Task<(List<PersonListItem>, int)> GetPersonList(PersonListRequest request)
        {
            var administrationId = Guid.Parse(request.AdministrationId);
            var administration = await repo.AllReadonly<RegisterAdministration>()
                                           .Where(x => x.Id == administrationId)
                                           .FirstAsync();
            var nomenclatureRequest = new NomenclaturePublicRequest
            {
                RegisterId = administration.RegisterId,
            };
            nomenclatureRequest.NomenclatureTypes.Add(InternalNomenclatureTypes.PersonType);
            var nomenclatureTypes = (await nomenclatureGrpcClient.GetNomenclaturePublicAsync(nomenclatureRequest))
                                    .NomenclatureTypes
                                    .ToList();

            var query = repo.AllReadonly<AdministrationPerson>()
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
                            });
            var countAll = 0;
            (query, countAll) = await request.DataTableRequest.GetFilteredData(query);
            var data = await query.ToListAsync();
            data.ForEach(x => x.Type = GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.PersonType, x.Type));
            return (data, countAll);
        }

        /// <summary>
        /// Регистри за администрация
        /// </summary>
        /// <param name="registerId">идентификатор</param>
        /// <returns>регистър</returns>
        public async Task<RegisterItem> GetRegisterForAddAdministration(int registerId)
        {
            var register = await repo.AllReadonly<Register>()
                               .Where(x => x.Id == registerId)
                               .FirstAsync();
            return new RegisterItem
            {
                Id = register.Id,
                Type = register.Type,
                Code = register.Code,
                LegalBasis = register.LegalBasis,
                Name = register.Name,
                Description = register.Description,
                EntryType = register.TypeEntry,
                IdentitySecurityLevel = register.IdentitySecurityLevel,
            };
        }

        public async Task<RegisterItem> CreateRegister()
        {
            return new RegisterItem
            {
                Code = await GetNewRegNumber()
            };
        }

        public async Task<RegisterItem> GetRegister(int registerId)
        {
            var register = await repo.AllReadonly<Register>()
                             .Include(x => x.RegisterAdministrations)
                             .ThenInclude(x => x.Administration)
                             .ThenInclude(x => x.People)
                             .Where(x => x.Id == registerId)
                             .FirstAsync();
            var result = new RegisterItem
            {
                Id = register.Id,
                Type = register.Type,
                Code = register.Code,
                LegalBasis = register.LegalBasis,
                Name = register.Name,
                Description = register.Description,
                EntryType = register.TypeEntry,
                IdentitySecurityLevel = register.IdentitySecurityLevel,
            };
            foreach (var administration in register.RegisterAdministrations)
            {
                var administrationItem = new AdministrationItem
                {
                    Id = administration.Id.ToString(),
                    Uic = administration.Administration.Uic,
                    Name = administration.Administration.Name,
                    LegalBasis = administration.LegalBasis,
                };
                administrationItem.Persons.AddRange(
                    administration.Administration.People.Select(x => new PersonItem
                    {
                        Id = x.Id,
                        Type = x.Type,
                        FirstName = x.FirstName,
                        MiddleName = x.MiddleName,
                        LastName = x.LastName,
                        Position = x.Position,
                        Phone = x.Phone,
                        Email = x.Email,
                    }));
                result.Administrations.Add(administrationItem);
            }
            return result;
        }

        public async Task SetRegisterAsStarted(int registerId)
        {
            var register = await repo.All<Register>()
                                     .Where(x => x.Id == registerId &&
                                                 x.StartedOn == null)
                                     .FirstAsync();
            register.StartedOn = DateTime.UtcNow;
            await repo.SaveChangesAsync();
        }

        public async Task<string> AddMasterPersonRecordsIndex(MasterPersonRecordsIndexAddMessage request)
        {
            var result = string.Empty;
            var mpri = await repo.All<MasterPersonRecordsIndex>()
                                 .Include(x => x.RegisterPersonRecords) 
                                 .Where(x => x.Pid == request.Pid &&
                                             x.PidType == request.PidType)
                                 .FirstOrDefaultAsync();

            if (mpri == null)
            {
                mpri = new MasterPersonRecordsIndex
                {
                    Id = Guid.NewGuid(), 
                    Pid = request.Pid,
                    PidType = request.PidType,
                    Name = request.Name,
                };
                await repo.AddAsync(mpri);
            }
            if (!mpri.RegisterPersonRecords.Any(x => x.RegisterId == request.RegisterId))
            {
                mpri.RegisterPersonRecords.Add(
                    new RegisterPersonRecord
                    {
                        RegisterId = request.RegisterId,
                        MasterPersonRecordId = mpri.Id
                    }
                );
            }
            await repo.SaveChangesAsync();
            result = mpri.Id.ToString();
            return result;
        }
    }
}