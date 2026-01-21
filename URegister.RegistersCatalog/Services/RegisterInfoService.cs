using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Security.AccessControl;
using URegister.Common;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Data.Common;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Helper;
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
       NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
       IObjectStoreService objectStoreService,
       IHttpRequester httpRequester,
       IConfiguration configuration,
       IAuditInfo auditInfo,
       IHttpClientFactory clientFactory,
       ILogger<RegisterInfoService> logger) : IRegisterInfoService
    {

        private Expression<Func<Register, RegisterItem>> RegisterToItem()
        {
            return r => new RegisterItem
            {
                Id = r.Id,
                Type = r.Type,
                Code = r.Code,
                LegalBasis = r.LegalBasis,
                Name = r.Name,
                Description = r.Description,
                EntryType = r.TypeEntry,
                IdentitySecurityLevel = r.IdentitySecurityLevel,
                BaseAddress = r.BaseAddress,
                Deployed = r.Deployed,
                StatusId = r.StatusId,
                HistoryNotPublic = r.HistoryNotPublic ?? false
            };
        }

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
        /// Списък с всички активни администрации.
        /// </summary>
        /// <returns></returns>

        public async Task<List<Administration>> GetAdministrations()
        {
            return await repo.AllReadonly<Administration>().OrderBy(a => a.Name).ThenBy(a => a.Uic).ToListAsync();
        }

        public async Task<Administration> GetAdministrationById(Guid administrationId)
        {
            return await repo.AllReadonly<Administration>().FirstOrDefaultAsync(x => x.Id == administrationId);
        }
        public async Task<Administration> GetAdminAdministration()
        {
            return await repo.AllReadonly<Administration>().FirstOrDefaultAsync(x => x.Uic == "000000000");
        }

        public async Task<ICollection<RegisterItem>> GetAdministrationRegistries(Guid administrationId)
        {
            var globalAdminAdministration =
                await repo.AllReadonly<Administration>().FirstOrDefaultAsync(a => a.Id == administrationId) ?? new Administration();

            Expression<Func<RegisterAdministration, bool>> predicate;

            if (globalAdminAdministration.Uic == "000000000")
            {
                predicate = x => true;
            }
            else
            {
                predicate = x => x.AdministrationId == administrationId;
            }

            List<RegisterItem> administrationRegistries = await repo.AllReadonly<RegisterAdministration>()
                 .TagWith(nameof(GetAdministrationRegistries))
                 .Where(predicate)
                 .Include(x => x.Register)
                 .Select(x => new RegisterItem
                 {
                     Id = x.Register.Id,
                     Code = x.Register.Code,
                     Name = x.Register.Name
                 })
                 .GroupBy(x => x.Id)
                 .Select(g => g.First())
                 .ToListAsync();

            return administrationRegistries;
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
            nomenclatureRequest.NomenclatureTypes.Add(InternalNomenclatureTypes.RegisterStatus);
            var nomenclatureTypes = (await nomenclatureGrpcClient.GetNomenclaturePublicAsync(nomenclatureRequest))
                                    .NomenclatureTypes
                                    .ToList();

            var query = repo.AllReadonly<Register>()
                .TagWith(nameof(GetRegisterFullList))
                .IgnoreQueryFilters();


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
            if (!string.IsNullOrEmpty(request.Type))
            {
                query = query.Where(x => x.Type == request.Type);
            }
            if (!string.IsNullOrEmpty(request.TypeEntry))
            {
                query = query.Where(x => x.TypeEntry == request.TypeEntry);
            }
            if (request.StatusId > 0)
            {
                query = query.Where(x => x.StatusId == request.StatusId);
            }
            if (!string.IsNullOrEmpty(request.AdministrationId))
            {
                Guid firlterAdministationIdGuid = new Guid(request.AdministrationId);
                query = query.Where(x => x.RegisterAdministrations.Any(a => a.IsActive && a.AdministrationId == firlterAdministationIdGuid));
            }
            if (!string.IsNullOrEmpty(request.IdentitySecurityLevel))
            {
                query = query.Where(x => x.IdentitySecurityLevel == request.IdentitySecurityLevel);
            }
            if (request.DateFrom != null)
            {
                DateTime dateFrom = request.DateFrom.ToDateTime();//it's UTC
                query = query.Where(x => x.CreatedOn >= dateFrom);
            }
            if (request.DateTo != null)
            {
                DateTime dateTo = request.DateTo.ToDateTime().AddDays(1);//it's UTC
                query = query.Where(x => x.CreatedOn <= dateTo);
            }
            if (request.DeployedOnly)
            {
                query = query.Where(x => x.Deployed);
            }

            var queryRegisterListItem = query.Select(x => new RegisterListItem
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                LegalBasis = x.LegalBasis,
                Type = x.Type,
                EntryType = x.TypeEntry,
                IdentitySecurityLevel = x.IdentitySecurityLevel,
                BaseAddress = x.BaseAddress,
                Status = x.StatusId.ToString(),
                Deployed = x.Deployed,
                SoleAdministrationId = x.RegisterAdministrations.Count(r => r.IsActive) != 1 ?
                    null : x.RegisterAdministrations.Single(r => r.IsActive).AdministrationId.ToString(),
                SoleAdministrationName = x.RegisterAdministrations.Count(r => r.IsActive) != 1 ?
                null : x.RegisterAdministrations.Single(r => r.IsActive).Administration.Name
            });

            var countAll = 0;
            (queryRegisterListItem, countAll) = await request.DataTableRequest.GetFilteredData(queryRegisterListItem);
            var data = await queryRegisterListItem.ToListAsync();
            data.ForEach(x =>
            {
                x.Type = GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.RegisterType, x.Type);
                x.EntryType = GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.RegisterEntryType, x.EntryType);
                x.IdentitySecurityLevel = GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.RegisterIdentitySecurityLevel, x.IdentitySecurityLevel);
                x.Status = GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.RegisterStatus, x.Status);
            });

            return (data, countAll);
        }

        private async Task<string> GetNewRegNumber()
        {
            var regNumber = await repo.AllReadonly<Register>()
                .IgnoreQueryFilters()
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

        public async Task SaveRegisterInfo(RegisterItem request)
        {
            var register = await repo.All<Register>()
                                  .Where(x => x.Id == request.Id)
                                  .FirstAsync();
            register.Name = request.Name;
            register.Description = request.Description;
            register.LegalBasis = request.LegalBasis;
            register.Type = request.Type;
            register.TypeEntry = request.EntryType;
            register.IdentitySecurityLevel = request.IdentitySecurityLevel;
            SetBaseAddress(register);
            await SaveRegisterFiles(
                register.Id,
                (int)RegisterFileSourceType.Register,
                register.Id.ToString(),
                request.RegisterFiles.ToList()
            );
            register.ModifiedOn = DateTime.UtcNow;
            register.ModifiedByUserId = auditInfo?.UserId ?? Guid.Empty;
            register.HistoryNotPublic = request.HistoryNotPublic;
            await repo.SaveChangesAsync();
        }

        private async Task SaveRegisterFiles(int registerId, int sourceType, string sourceId, List<RegisterFileItem> registerFiles)
        {
            foreach (var file in registerFiles.Where(x => x.SourceType == sourceType).ToList())
            {
                var savedFile = await repo.All<RegisterFileMetadata>()
                                          .Where(x => x.Id == file.MetaFileId.ToGuid())
                                          .FirstAsync();
                savedFile.IsActive = true;
                savedFile.FileSourceTypeId = sourceType;
                savedFile.SourceId = sourceId;
                savedFile.RegisterId = registerId;
                savedFile.Description = file.Description;
                savedFile.NomenclatureType = file.NomenclatureType;
                savedFile.CodeableConceptCode = file.CodeableConceptCode;
                savedFile.ModifiedOn = DateTime.UtcNow;
                savedFile.ModifiedByUserId = auditInfo?.UserId ?? Guid.Empty;
            }
            await DeleteRegisterFiles(sourceType, sourceId, registerFiles);
        }
        private async Task DeleteRegisterFiles(int sourceType, string sourceId, List<RegisterFileItem> registerFiles)
        {
            var savedFiles = await repo.All<RegisterFileMetadata>()
                                      .Where(x => x.FileSourceTypeId == sourceType &&
                                                  x.SourceId == sourceId)
                                      .ToListAsync();
            foreach (var file in savedFiles)
            {
                if (!registerFiles.Any(x => x.MetaFileId == file.Id.ToString()))
                {
                    file.IsActive = false;
                    file.DeletedOn = DateTime.UtcNow;
                    file.ModifiedOn = DateTime.UtcNow;
                    file.ModifiedByUserId = auditInfo?.UserId ?? Guid.Empty;
                }
            }
        }

        /// <summary>
        /// Добавяне на регистър
        /// </summary>
        /// <param name="request">данни за регистър</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task AddRegister(RegisterItem request)
        {
            if (!request.Administrations.Any())
            {
                await SaveRegisterInfo(request);
                return;
            }

            var administrationItem = request.Administrations.First();
            Register? register = new();
            var administration = await repo.All<Administration>()
                                           .Include(x => x.People)
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
                if (register.RegisterAdministrations.Any() && administrationItem.Id == Guid.Empty.ToString())
                {
                    throw new ArgumentException($"Администрация с ЕИК/БУЛСТАТ {administrationItem.Uic} вече е добавена");
                }
                //if (!register.RegisterAdministrations.Any() && administrationItem.Id != Guid.Empty.ToString())
                //{
                //    throw new ArgumentException($"Не намирам администрация с ЕИК/БУЛСТАТ {administrationItem.Uic}");
                //}
            }
            else
            {
                register.Code = await GetNewRegNumber();
                await repo.AddAsync(register);
            }
            SetBaseAddress(register);
            register.Name = request.Name;
            register.Description = request.Description;
            register.LegalBasis = request.LegalBasis;
            register.Type = request.Type;
            register.TypeEntry = request.EntryType;
            register.IdentitySecurityLevel = request.IdentitySecurityLevel;
            register.ModifiedOn = DateTime.UtcNow;
            register.ModifiedByUserId = auditInfo?.UserId ?? Guid.Empty;
            register.HistoryNotPublic = request.HistoryNotPublic;

            if (administration == null)
            {
                administration = new Administration
                {
                    Uic = administrationItem.Uic,
                    Name = administrationItem.Name,
                };
                await repo.AddAsync(administration);
            }
            else
            {
                administration.Name = administrationItem.Name;
            }
            var registerAdministration = register.RegisterAdministrations.FirstOrDefault();

            if (registerAdministration == null)
            {
                registerAdministration = new RegisterAdministration
                {
                    LegalBasis = register.LegalBasis,
                    AdministrationId = administration.Id,
                    Register = register
                };
                await repo.AddAsync(registerAdministration);
            }
            registerAdministration.ModifiedOn = DateTime.UtcNow;
            registerAdministration.ModifiedByUserId = auditInfo?.UserId ?? Guid.Empty;

            foreach (var person in administration.People)
            {
                if (administrationItem.Persons.Any(x => x.Id == person.Id))
                {
                    person.IsActive = false;
                    person.ModifiedOn = DateTime.UtcNow;
                    person.ModifiedByUserId = auditInfo?.UserId ?? Guid.Empty;

                }

            }
            foreach (var personItem in administrationItem.Persons)
            {
                var person = administration.People.Where(x => x.Id == personItem.Id).FirstOrDefault();
                if (person == null)
                {
                    person = new AdministrationPerson
                    {
                        Register = register
                    };
                    await repo.AddAsync(person);
                }
                person.FirstName = personItem.FirstName;
                person.MiddleName = personItem.MiddleName;
                person.LastName = personItem.LastName;
                person.Email = personItem.Email;
                person.Phone = personItem.Phone;
                person.Position = personItem.Position;
                person.Type = personItem.Type;
                person.AdministrationId = administration.Id;
                person.IsActive = true;
                person.ModifiedOn = DateTime.UtcNow;
                person.ModifiedByUserId = auditInfo?.UserId ?? Guid.Empty;
            }
            await SaveRegisterFiles(
                register.Id,
                (int)RegisterFileSourceType.Register,
                register.Id.ToString(),
                request.RegisterFiles.ToList()
            );
            await SaveRegisterFiles(
                register.Id,
                (int)RegisterFileSourceType.Administration,
                registerAdministration.Id.ToString(),
                request.RegisterFiles.ToList()
            );
            await repo.SaveChangesAsync();
        }

        private void SetBaseAddress(Register register)
        {
            if (string.IsNullOrEmpty(register.BaseAddress))
            {
                register.BaseAddress = configuration.GetValue<string>("Infrastructure:RegisterBaseUrl");
                register.BaseAddress = register.BaseAddress?.Replace("RegisterCode", register.Code);
            }
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
                                AdministrationId = x.AdministrationId.ToString()
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
            var registerAdministrationId = Guid.Parse(request.RegisterAdministrationId);
            var registerAdministration = await repo.AllReadonly<RegisterAdministration>()
                                           .Where(x => x.Id == registerAdministrationId)
                                           .FirstAsync();
            var nomenclatureRequest = new NomenclaturePublicRequest
            {
                RegisterId = registerAdministration.RegisterId,
            };
            nomenclatureRequest.NomenclatureTypes.Add(InternalNomenclatureTypes.PersonType);
            var nomenclatureTypes = (await nomenclatureGrpcClient.GetNomenclaturePublicAsync(nomenclatureRequest))
                                    .NomenclatureTypes
                                    .ToList();

            var query = repo.AllReadonly<AdministrationPerson>()
                            .Where(x => x.AdministrationId == registerAdministration.AdministrationId &&
                                        x.RegisterId == registerAdministration.RegisterId)
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
            return await repo.AllReadonly<Register>()
                        .Where(x => x.Id == registerId)
                        .Select(RegisterToItem())
                        .FirstAsync();
        }

        public async Task<RegisterItem> CreateRegister()
        {
            return new RegisterItem
            {
                Code = await GetNewRegNumber()
            };
        }
        private async Task<List<RegisterFileItem>> GetRegisterFiles(int soutceType, string sourceId)
        {
            return await repo.AllReadonly<RegisterFileMetadata>()
                             .Where(x => x.FileSourceTypeId == soutceType &&
                                         x.SourceId == sourceId)
                             .Select(x => new RegisterFileItem
                             {
                                 SourceType = x.FileSourceTypeId,
                                 SourceId = x.SourceId,
                                 MetaFileId = x.Id.ToString(),
                                 Description = x.Description,
                                 FileName = x.FileName,
                                 NomenclatureType = x.NomenclatureType,
                                 CodeableConceptCode = x.CodeableConceptCode,
                             })
                             .ToListAsync();
        }

        public async Task<RegisterItem> GetRegister(int registerId)
        {
            var register = await repo.AllReadonly<Register>()
                             .IgnoreQueryFilters()
                             .Include(x => x.RegisterAdministrations)
                             .ThenInclude(x => x.Administration)
                             .ThenInclude(x => x.People.Where(p => p.RegisterId == registerId))
                             .Where(x => x.Id == registerId)
                             .FirstAsync();
            var result = RegisterToItem().Compile().Invoke(register);
            result.RegisterFiles.AddRange(await GetRegisterFiles((int)RegisterFileSourceType.Register, registerId.ToString()));
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
                result.RegisterFiles.AddRange(await GetRegisterFiles((int)RegisterFileSourceType.Administration, administration.Id.ToString()));
            }
            return result;
        }
        public async Task<RegisterItem?> GetRegisterByCode(GetRegisterByCodeRequest request)
        {
            return await repo.AllReadonly<Register>().TagWith(nameof(GetRegisterByCode))
                    .Select(RegisterToItem())
                    .FirstOrDefaultAsync(r => r.Code == request.RegisterCode);
        }

        public async Task<string> AddMasterPersonRecordIndex(MasterPersonRecordIndexAddMessage request)
        {
            //NOTE : решено е като временно решение да се позволяват празни PID
            //if (string.IsNullOrWhiteSpace(request.Pid))
            //{
            //    throw new ArgumentException("Празен идентификатор в заявката");
            //}

            var result = string.Empty;
            var mpri = await repo.All<MasterPersonRecordsIndex>()
                                 .IgnoreQueryFilters()
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
            else
            {
                if (string.IsNullOrWhiteSpace(request.Pid))
                {
                    //При празен PID не записваме име
                    mpri.Name = String.Empty;
                }
                else
                {
                    mpri.Name = request.Name;
                }

                if (!mpri.IsActive)
                {
                    mpri.IsActive = true;
                }
            }

            if (!mpri.RegisterPersonRecords.Any(x => x.RegisterId == request.RegisterId && x.RoleId == request.RoleId))
            {
                mpri.RegisterPersonRecords.Add(
                    new RegisterPersonRecord
                    {
                        RegisterId = request.RegisterId,
                        MasterPersonRecordId = mpri.Id,
                        RoleId = request.RoleId
                    }
                );
            }

            try
            {
                await repo.SaveChangesAsync();
            }
            catch (Exception)
            {
                logger.LogError($"Проблем при запис в {nameof(AddMasterPersonRecordIndex)}. Pid от заявка: {request.Pid}, тип от заявка: {request.PidType}");
                throw;
            }

            result = mpri.Id.ToString();
            return result;
        }

        public async Task<List<MPRILisItemMessage>> GetMasterPersonRecordIndex(GetMasterPersonRecordIndexMessage request)
        {
            Expression<Func<MasterPersonRecordsIndex, bool>> pidTypeExp = x => true;
            if (!string.IsNullOrEmpty(request.PidType))
            {
                pidTypeExp = x => x.PidType == request.PidType;
            }

            return await repo.All<MasterPersonRecordsIndex>()
                                 .Include(x => x.RegisterPersonRecords)
                                 .Where(x => x.Pid == request.Pid)
                                 .Where(pidTypeExp)
                                 .Select(x => new MPRILisItemMessage
                                 {
                                     Id = x.Id.ToString(),
                                     Name = x.Name,
                                     PidType = x.PidType,
                                     Pid = x.Pid
                                 })
                                 .ToListAsync();
        }

        public async Task<List<MPRILisItemMessage>> GetMasterPersonRecordIndexList(List<Guid> ids)
        {
            return await repo.All<MasterPersonRecordsIndex>()
                                .Include(x => x.RegisterPersonRecords)
                                .Where(x => ids.Contains(x.Id))
                                .Select(x => new MPRILisItemMessage
                                {
                                    Id = x.Id.ToString(),
                                    Name = x.Name,
                                    PidType = x.PidType,
                                    Pid = x.Pid
                                })
                                .ToListAsync();
        }

        /// <summary>
        /// Премахване на администрация от регистър
        /// </summary>
        /// <param name="registerAdministrationId">Идентификатор на регистърната администрация</param>
        /// <returns></returns>
        public async Task<ResultStatus> RemoveAdministrationFromRegister(Guid registerAdministrationId)
        {
            try
            {
                await repo.DeleteAsync<RegisterAdministration>(registerAdministrationId);
                await repo.SaveChangesAsync();
                return CommonGrpcHelper.CreateStatusOK();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Проблем при премахване на администрация към регистър с идентификатор {registerAdministrationId} в {nameof(RemoveAdministrationFromRegister)}");
                return new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = "Проблем при премахване на администрация към регистър"
                };
            }
        }

        public async Task<IList<AppAdministration>> GetAdministrationsByIds(RepeatedField<string> ids, int registerId)
        {
            IList<Guid> parsedIds = new List<Guid>();

            foreach (var item in ids)
            {
                if (Guid.TryParse(item, out Guid id))
                {
                    parsedIds.Add(id);
                }
            }

            if (parsedIds.Count == 0)
            {
                throw new ArgumentException("Невалидни идентификатори на администрации");
            }

            var administrations = await repo.AllReadonly<Administration>()
                .Where(x => parsedIds.Contains(x.Id) &&
                            x.RegisterAdministrations.Any(ra => ra.RegisterId == registerId))
                .Select(x => new
                {
                    x.Id,
                    x.Name
                })
                .ToListAsync();

            return administrations.Select(x => new AppAdministration()
            {
                Id = x.Id.ToString(),
                Name = x.Name
            })
            .ToList();
        }

        public async Task AddRegisterStatus(RegisterStatusItem request)
        {
            await repo.AddAsync(new RegisterStatus
            {
                Id = Guid.Parse(request.Id),
                RegisterId = request.RegisterId,
                StatusId = request.StatusId,
                IsActive = request.IsActive,
                Remark = request.Remark,
                ModifiedOn = DateTime.UtcNow,
                ModifiedByUserId = auditInfo?.UserId ?? Guid.Empty,
            });
            var register = await repo.All<Register>()
                                     .Where(x => x.Id == request.RegisterId)
                                     .IgnoreQueryFilters()
                                     .FirstAsync();
            register.StatusId = request.StatusId;
            register.IsActive = request.IsActive;
            register.ModifiedOn = DateTime.UtcNow;
            register.ModifiedByUserId = auditInfo?.UserId ?? Guid.Empty;
            await SaveRegisterFiles(
                     register.Id,
                     (int)RegisterFileSourceType.RegisterStatus,
                     request.Id,
                     request.RegisterFiles.ToList()
                 );

            await repo.SaveChangesAsync();
            if (request.StatusId == (int)RegisterStatusType.Register && !register.Deployed)
            {
                await CreateRegister(register);
                await repo.SaveChangesAsync();
            }
        }

        public async Task<Guid> UploadFile(byte[] filesAsBytes, string fileName, string contentType, int sourceTypeId, Guid sourceId)
        {
            var fileId = await objectStoreService.SaveObject(fileName, filesAsBytes, contentType, null);
            var fileMetadata = new RegisterFileMetadata
            {
                FileId = Guid.Parse(fileId),
                FileName = fileName,
                FileSourceTypeId = sourceTypeId,
                SourceId = sourceId.ToString(),
                IsActive = true,
                ModifiedOn = DateTime.UtcNow,
                ModifiedByUserId = auditInfo?.UserId ?? Guid.Empty,

            };
            await repo.AddAsync(fileMetadata);
            await repo.SaveChangesAsync();
            return fileMetadata.Id;
        }

        private string GetStampitFieldValue(string field, string page)
        {
            var fieldName = $@"name=""{field}""";
            var valueName = @"value=""";
            var pos = page.IndexOf(fieldName);
            if (pos < 0)
            {
                return string.Empty;
            }
            pos += fieldName.Length;
            pos = page.IndexOf(valueName, pos);
            pos += valueName.Length;
            var posEnd = page.IndexOf(@"""", pos);
            return page.Substring(pos, posEnd - pos);
        }

        private async Task InitStampit(Register register)
        {
            if (!string.IsNullOrEmpty(register.AppId))
            {
                return;
            }
            var registerCode = register.Code;
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != EnvironmentNames.Production)
            {
                registerCode = $"Test{registerCode}";
            }

            var client = clientFactory.CreateClient("stampit");
            var request = new HttpRequestMessage(HttpMethod.Post, $"manage/apps/create");
            List<KeyValuePair<string, string>> reqParams = new ();
            reqParams.Add(new("name", registerCode));
            reqParams.Add(new("description", register.Name));
            reqParams.Add(new("redirect", $"{register.BaseAddress?.ToLower()}/signin-stampit"));
            reqParams.Add(new("redirect_to_id", "1"));
            reqParams.Add(new("_roots[]", "eidas"));
            var content = new FormUrlEncodedContent(reqParams);
            request.Content = content;
            
            var bearerSource = client.DefaultRequestHeaders?.Authorization?.ToString();
            var bearer = (bearerSource?.Length ?? 0) > 50 ? $"{bearerSource?.Length} {bearerSource?.Substring(0, 50)}" : bearerSource;
            logger.LogError($"Bearer: {bearer}");

            var response = await client.SendAsync(request);
            if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400)
            {
                var redirectUri = response.Headers.Location;
                if (redirectUri != null)
                {
                    // Reissue the request to the redirected URL with the Authorization header
                    var redirectedResponse = await client.GetAsync($"{redirectUri}");
                    var page = await redirectedResponse.Content.ReadAsStringAsync();
                    if (redirectedResponse.IsSuccessStatusCode)
                    {
                        register.AppId = GetStampitFieldValue("public", page);
                        register.AppSecret = GetStampitFieldValue("private", page);
                        await repo.SaveChangesAsync();
                    } 
                    if (string.IsNullOrEmpty(register.AppId) || string.IsNullOrEmpty(register.AppSecret))
                    {
                        logger.LogError($"{redirectUri} {redirectedResponse.StatusCode} Bearer: {bearer}");
                    }
                } else
                {
                    logger.LogError($"{redirectUri}  Bearer: {bearer}");
                }
            } else
            {
                logger.LogError($"{response.StatusCode} Bearer: {bearer}");
            }
        }

        private async Task CreateRegister(Register register)
        {
            await InitStampit(register);

            var baseAddress = configuration.GetValue<string>("Infrastructure:BaseUrl") ?? string.Empty;
            var pass = Guid.NewGuid().ToString("d");
            register.DateDeploy = DateTime.UtcNow;
            var result = await httpRequester.PostAsync(string.Empty, baseAddress, new { 
                name = register.Code, 
                pass,
                appId = register.AppId,
                appSecret = register.AppSecret
            });
            if (result.IsSuccessStatusCode)
            {
                logger.LogError($"CreateRegister {baseAddress} {result.StatusCode} {result.Content}");
            }
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

        public async Task<List<AdministrationUicItem>> GetAdministrationUicList()
        {
            var result = new List<AdministrationUicItem>();
            var administrations = await repo.All<Administration>()
                                            .Where(x => x.IsActive)
                                            .IgnoreQueryFilters()
                                            .Include(x => x.RegisterAdministrations)
                                            .ToListAsync();

            foreach (var administration in administrations)
            {
                var adminictrationItem = new AdministrationUicItem
                {
                    AdministrationId = administration.Id.ToString(),
                    Name = administration.Name,
                    Uic = administration.Uic,
                };
                IList<int> registerIds = administration.RegisterAdministrations.Where(x => x.IsActive).Select(x => x.RegisterId).ToList();
                adminictrationItem.RegisterIds.AddRange(registerIds);
                result.Add(adminictrationItem);
            }
            return result;
        }

        public async Task<List<ServiceItem>> GetServiceList()
        {
            var result = new List<ServiceItem>();
            result.AddRange(await repo.AllReadonly<Data.Models.RegisterService>()
                                      .Where(x => !string.IsNullOrEmpty(x.EFormCode))
                                      .Select(x => new ServiceItem
                                      {
                                          RegisterId = x.RegisterId,
                                          EformCode = x.EFormCode,
                                          ServiceId = x.ServiceId,
                                          IsActive = x.IsActive,
                                          RegisterCode = x.Register.Code
                                      })
                                      .ToListAsync()
                );
            return result;
        }

        public async Task SaveService(ServiceItem request)
        {
            var service = await repo.All<Data.Models.RegisterService>()
                                    .Where(x => x.RegisterId == request.RegisterId &&
                                                x.ServiceId == request.ServiceId)
                                    .FirstOrDefaultAsync();
            if (service == null)
            {
                service = new Data.Models.RegisterService
                {
                    RegisterId = request.RegisterId,
                    ServiceId = request.ServiceId
                };
                await repo.AddAsync(service);
            }
            service.IsActive = request.IsActive;
            service.EFormCode = request.EformCode;
            service.ModifiedOn = DateTime.UtcNow;
            service.ModifiedByUserId = auditInfo?.UserId ?? Guid.Empty;
            await repo.SaveChangesAsync();
        }

        public async Task<(RegisterFileMetadata, byte[], string)> DownloadFile(Guid id)
        {
            var metaFile = await repo.AllReadonly<RegisterFileMetadata>()
                                     .Where(x => x.Id == id)
                                     .FirstAsync();
            (var data, var contentType) = await objectStoreService.GetObject(metaFile.FileId.ToString());
            return (metaFile, data, contentType);
        }
        public async Task<List<RegisterStatusItem>> GetRegisterStatusList(int registerId)
        {
            return await repo.AllReadonly<RegisterStatus>()
                                      .Where(x => x.RegisterId == registerId)
                                      .Select(x => new RegisterStatusItem
                                      {
                                          Id = x.Id.ToString(),
                                          RegisterId = x.RegisterId,
                                          IsActive = x.IsActive,
                                          Remark = x.Remark,
                                          StatusId = x.StatusId,
                                          ModifiedBy = x.ModifiedByUserId.ToString(),
                                          ModifiedOn = x.ModifiedOn.ToUniversalTime().ToTimestamp()
                                      })
                                      .ToListAsync();
        }

        public async Task<AppAdministration> GetAdministrationNameByUic(StringValue uic)
        {
            Administration foundAdministration = await repo.AllReadonly<Administration>()
                .FirstOrDefaultAsync(x => x.Uic == uic.Value);

            if (foundAdministration == null)
            {
                return null;
            }

            return new AppAdministration
            {
                Id = foundAdministration.Id.ToString(),
                Name = foundAdministration.Name,
                Uic = foundAdministration.Uic
            };
        }

        public async Task SaveCalendarDay(CalendarDayItem request)
        {
            var calendarDay = await repo.All<CalendarDay>()
                                        .IgnoreQueryFilters()
                                        .Where(x => x.Id == request.Id)
                                        .FirstOrDefaultAsync();
            if (calendarDay == null)
            {
                calendarDay = new CalendarDay();
                await repo.AddAsync(calendarDay);
            }
            calendarDay.CurrentDate = request.CurrentDate.ToDateTime().ConvertUtcToBGTime().Date;
            calendarDay.KindId = request.KindId;
            calendarDay.Description = request.Description;
            calendarDay.IsActive = true;
            await repo.SaveChangesAsync();
        }

        public async Task<CalendarDayItem> GetCalendarDay(int id)
        {
            return await repo.All<CalendarDay>()
                             .IgnoreQueryFilters()
                             .Where(x => x.Id == id)
                             .Select(x => new CalendarDayItem
                             {
                                 Id = x.Id,
                                 CurrentDate = x.CurrentDate.ToUniversalTime().ToTimestamp(),
                                 KindId = x.KindId,
                                 Description = x.Description,
                             })
                             .FirstAsync();

        }

        /// <summary>
        /// Списък с календар
        /// </summary>
        /// <param name="request">идентификатор на регистър</param>
        /// <returns>администрации</returns>
        public async Task<(List<CalendarDayItem>, int)> GetCalendarDayList(CalendarDayListRequest request)
        {
            DateTime dateFrom = request.FromDate.ToDateTime().ConvertUtcToBGTime().Date;
            DateTime dateTo = request.ToDate.ToDateTime().ConvertUtcToBGTime();

            var query = repo.AllReadonly<CalendarDay>()
                            .Where(x => dateFrom <= x.CurrentDate &&
                                        x.CurrentDate <= dateTo);

            var countAll = 0;
            (query, countAll) = await request.DataTableRequest.GetFilteredData(query);
            var data = await query.ToListAsync();
            var nomenclatureRequest = new NomenclaturePublicRequest();
            nomenclatureRequest.NomenclatureTypes.Add(InternalNomenclatureTypes.CalendarDayKind);
            var nomenclatureTypes = (await nomenclatureGrpcClient.GetNomenclaturePublicAsync(nomenclatureRequest))
                                    .NomenclatureTypes
                                    .ToList();
            var result = data.Select(x => new CalendarDayItem
            {
                Kind = GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.CalendarDayKind, x.KindId.ToString()),
                Id = x.Id,
                CurrentDate = x.CurrentDate.ToUniversalTime().ToTimestamp(),
                KindId = x.KindId,
                Description = x.Description
            })
            .ToList();
            return (result, countAll);
        }

        public async Task<DateTime> CalcWorkDays(DateTime dateFrom, int days)
        {
            var calendarDays = await repo.AllReadonly<CalendarDay>()
                                          .Where(x => dateFrom <= x.CurrentDate)
                                          .ToListAsync();
            for (int i = 0; i < days; i++)
            {
                bool isAddedDay = false;
                while (!isAddedDay)
                {
                    dateFrom = dateFrom.AddDays(1);
                    var calendarDay = calendarDays.Where(x => x.CurrentDate == dateFrom).FirstOrDefault();
                    if (calendarDay?.KindId == CalendarDayKind.WorkDay)
                        break;
                    if (dateFrom.DayOfWeek != DayOfWeek.Saturday && dateFrom.DayOfWeek != DayOfWeek.Sunday)
                        break;
                }
            }
            return dateFrom;
        }

        public async Task<RegisterStatusItem> GetRegisterStatus(Guid id)
        {
            var registerStatus = await repo.AllReadonly<RegisterStatus>()
                                           .Where(x => x.Id == id)
                                           .FirstAsync();
            var result = new RegisterStatusItem
            {
                Id = registerStatus.Id.ToString(),
                StatusId = registerStatus.StatusId,
                Remark = registerStatus.Remark,
                IsActive = registerStatus.IsActive,
                ModifiedOn = registerStatus.ModifiedOn.ToUniversalTime().ToTimestamp(),
                ModifiedBy = registerStatus.ModifiedByUserId.ToString(),
                RegisterId = registerStatus.RegisterId,
            };
            result.RegisterFiles.AddRange(await repo.AllReadonly<RegisterFileMetadata>()
                                                    .Where(x => x.SourceId == id.ToString() &&
                                                                x.FileSourceTypeId == (int)RegisterFileSourceType.RegisterStatus)
                                                    .Select(x => new RegisterFileItem
                                                    {
                                                        SourceId = x.SourceId,
                                                        CodeableConceptCode = x.CodeableConceptCode,
                                                        FileName = x.FileName,
                                                        Description = x.Description,
                                                        MetaFileId = x.Id.ToString(),
                                                        NomenclatureType = x.NomenclatureType,
                                                        SourceType = x.FileSourceTypeId,
                                                    })
                                                    .ToListAsync());
            return result;
        }

        public async Task<OpenDataParam> GetOpenDataParam(OpenDataParamRequest request)
        {
            var register = await repo.AllReadonly<Register>()
                                     .Where(x => x.Id == request.RegisterId)
                                     .FirstAsync();
            var administration = await repo.AllReadonly<Administration>()
                                                 .Where(x => x.Id == request.AdministrationId.ToGuid())
                                                 .FirstAsync();
            var registerAdministration = await repo.AllReadonly<RegisterAdministration>()
                                                 .Where(x => x.RegisterId == request.RegisterId &&
                                                             x.AdministrationId == request.AdministrationId.ToGuid())
                                                 .FirstAsync();

            return new OpenDataParam
            {
                ApiKey = administration.OpenDataApiKey,
                OrganisationId = administration.OpenDataOrgId,
                Tags = register.OpenDataTags,
                CategoryId = register.OpenDataCategoryId,
                DataSetId = registerAdministration.OpenDataDataSetId,
                ResourceMetaId = registerAdministration.ResourceMetaId,
                FrequencyId = registerAdministration.FrequencyId,
                FrequencyAdministrationId = administration.FrequencyId,
                AdministrationName = administration.Name
            };
        }
        public async Task SaveOpenDataRegister(OpenDataRegisterSaveRequest request)
        {
            var register = await repo.All<Register>()
                                     .Where(x => x.Id == request.RegisterId)
                                     .FirstAsync();
            register.OpenDataTags = request.Tags;
            register.OpenDataCategoryId = request.CategoryId;
            await repo.SaveChangesAsync();
        }

        public async Task SaveOpenDataAdministration(OpenDataAdministrationSaveRequest request)
        {
            var administration = await repo.All<Administration>()
                                     .Where(x => x.Id == request.AdministrationId.ToGuid())
                                     .FirstAsync();
            administration.OpenDataApiKey = request.ApiKey;
            administration.OpenDataOrgId = request.OrganisationId;
            administration.FrequencyId = request.FrequencyId;
            await repo.SaveChangesAsync();
        }

        public async Task SaveOpenDataRegisterAdministration(OpenDataRegisterAdministrationSaveRequest request)
        {
            var registerAdministration = await repo.All<RegisterAdministration>()
                                     .Where(x => x.AdministrationId == request.AdministrationId.ToGuid() && 
                                                 x.RegisterId == request.RegisterId )
                                     .FirstAsync();
            registerAdministration.FrequencyId = request.FrequencyId;
            await repo.SaveChangesAsync();
        }

        public async Task SaveOpenDataRegisterAdministrationMeta(OpenDataRegisterAdministrationMetaSaveRequest request)
        {
            var registerAdministration = await repo.All<RegisterAdministration>()
                                     .Where(x => x.AdministrationId == request.AdministrationId.ToGuid() &&
                                                 x.RegisterId == request.RegisterId)
                                     .FirstAsync();
            registerAdministration.OpenDataDataSetId = request.DataSetId;
            registerAdministration.ResourceMetaId = request.ResourceMetaId;
            await repo.SaveChangesAsync();
        }
    }
}