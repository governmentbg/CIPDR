using DataTables.AspNet.Core;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using iText.Layout.Element;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Text.RegularExpressions;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Models.Register;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.NomenclaturesCatalog;
using URegister.RegistersCatalog;
using URegister.Users;
using static FastExpressionCompiler.ExpressionCompiler;
using static URegister.Users.AppUserManager;

namespace URegister.Core.Services
{
    public class RegisterClientService(
        RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
        NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
        AppUserManagerClient appUserManagerClient
    ) : IRegisterClientService
    {
        public async Task<(bool, string)> AddRegister(RegisterVM register)
        {
            var request = new RegisterItem
            {
                Id = register.Id,
                Code = register.Code,
                Name = register.Name,
                Description = register.Description,
                LegalBasis = register.LegalBasis,
                Type = register.Type,
                EntryType = register.TypeEntry,
                IdentitySecurityLevel = register.IdentitySecurityLevel,
            };
            var administration = new AdministrationItem
            {
                Id = register.Administration.Id.ToString(),
                Uic = register.Administration.Uic,
                Name = register.Administration.Name,
            };
            request.Administrations.Add(administration);
            var persons = register.ContactPersons.ToList();
            persons.Add(register.Manager);
            foreach (var person in persons)
            {
                administration.Persons.Add(new PersonItem
                {
                    Id = person.Id,
                    FirstName = person.FirstName,
                    MiddleName = person.MiddleName,
                    LastName = person.LastName,
                    Email = person.Email,
                    Phone = person.Phone,
                    Position = person.Position,
                    Type = person.Type,
                });
            }
            request.RegisterFiles.AddRange(RegisterFilesToItem(register));
            request.RegisterFiles.AddRange(AdministrationFilesToItem(register));
            var result = await registerGrpcClient.AddRegisterAsync(request);
            return (result.Code == ResultCodes.Ok, result.Message);
        }

        private List<RegisterFileItem> RegisterFilesToItem(RegisterVM register)
        {
            var result = new List<RegisterFileItem>();
            foreach (var registerFile in register.RegisterFiles.Files)
            {
                var registerFileItem = new RegisterFileItem
                {
                    MetaFileId = registerFile.MetaFileId.ToString(),
                    SourceId = register.Id.ToString(),
                    SourceType = (int)RegisterFileSourceType.Register,
                    Description = registerFile.Description,
                    NomenclatureType = registerFile.NomenclatureType,
                    CodeableConceptCode = registerFile.CodeableConceptCode,
                    FileName = registerFile.FileName,
                };
                result.Add(registerFileItem);
            }
            return result;
        }
        private List<RegisterFileItem> AdministrationFilesToItem(RegisterVM register)
        {
            var result = new List<RegisterFileItem>();
            foreach (var registerFile in register.AdministrationFiles.Files)
            {
                var registerFileItem = new RegisterFileItem
                {
                    MetaFileId = registerFile.MetaFileId.ToString(),
                    SourceId = register.Administration.Id.ToString(),
                    SourceType = (int)RegisterFileSourceType.Administration,
                    Description = registerFile.Description,
                    NomenclatureType = registerFile.NomenclatureType,
                    CodeableConceptCode = registerFile.CodeableConceptCode,
                    FileName = registerFile.FileName,
                };
                result.Add(registerFileItem);
            }
            return result;
        }

        public async Task<(bool, string)> EditRegister(RegisterVM register)
        {
            var request = new RegisterItem
            {
                Id = register.Id,
                Code = register.Code,
                Name = register.Name,
                Description = register.Description,
                LegalBasis = register.LegalBasis,
                Type = register.Type,
                EntryType = register.TypeEntry,
                IdentitySecurityLevel = register.IdentitySecurityLevel,
            };
            request.RegisterFiles.AddRange(RegisterFilesToItem(register));
            var result = await registerGrpcClient.AddRegisterAsync(request);
            return (result.Code == ResultCodes.Ok, result.Message);
        }

        public async Task<IActionResult> GetRegisterFullList(IDataTablesRequest request, RegisterFilterVM filter)
        {
            var protoRequest = request!.GetDataTablesRequestProto();
            var result = await registerGrpcClient.GetRegisterFullListAsync(
                new RegisterListRequest
                {
                    DataTableRequest = protoRequest,
                    Code = filter.Code,
                    Name = filter.Name,
                    Description = filter.Description,
                    DateFrom = filter.DateFrom.HasValue ? filter.DateFrom.Value.ToUniversalTime().ToTimestamp() : null,
                    DateTo = filter.DateTo.HasValue ? filter.DateTo.Value.ToUniversalTime().ToTimestamp() : null,
                    Type = filter.Type,
                    TypeEntry = filter.TypeEntry,
                    IdentitySecurityLevel = filter.IdentitySecurityLevel,
                    StatusId = filter.StatusId,
                    AdministrationId = filter.AdministrationId.HasValue ? filter.AdministrationId.ToString() : String.Empty,
                    IsActive = filter.IsActive
                });
            return request.GetResponseServerPaging(result.Data, result.CountAll);
        }

        public async Task<int> GetRegisterCount()
        {
            var result = await registerGrpcClient.GetRegisterFullListAsync(
                new RegisterListRequest
                {
                    DataTableRequest = new Common.DatatableRequest
                    {
                        // You can send a request with 0 length if you only care about the count
                        Start = 0,
                        Length = 1 // or 0 if your backend supports it
                    }
                });

            return result.CountAll;
        }


        public async Task<List<string>> FormatUserRoles(UserListData user)
        {
            var userRoles = new List<string>();
            var slitRoles = user.RoleName.Split(", ");
            foreach (var role in slitRoles)
            {
                Match match = Regex.Match(role, @"\(([^)]+)\)");
                if (match.Success)
                {
                    string extractedValue = match.Groups[1].Value;
                    var register = await registerGrpcClient.GetRegisterByRegisterCodeAsync(new GetRegisterByCodeRequest { RegisterCode = extractedValue });
                    if (register.Status.Code == ResultCodes.Ok)
                    {
                        if (extractedValue == "R00000")
                        {
                            userRoles.Add(role.Replace("(" + extractedValue + ")", string.Empty));
                        }
                        else
                        {
                            userRoles.Add(role.Replace(extractedValue, register.Data == null ? string.Empty : register.Data.Name));
                        }

                    }
                }
            }
            return userRoles;
        }

        /// <summary>
        /// Връща списък от всички регистри
        /// </summary>
        /// <returns></returns>
        public async Task<RegisterFullListResponse> GetRegisterFullList()
        {
            var protoRequest = new DatatableRequest { Length = int.MaxValue };
            var result = await registerGrpcClient.GetRegisterFullListAsync(
                new RegisterListRequest
                {
                    DataTableRequest = protoRequest,
                    IsActive = true
                });
            return result;
        }

        public async Task<List<RegisterVM>> GetAllRegisterInAdministration(string administrationId)
        {
            AppAdministration request = new AppAdministration
            {
                Id = administrationId,
            };
            GetRegistriesResponse response = await registerGrpcClient.GetAdministrationRegistriesAsync(request);
            List<RegisterVM> result = new List<RegisterVM>();
            foreach (var register in response.Data)
            {
                result.Add(new RegisterVM()
                {
                    Id = register.Id,
                    Name = register.Name,
                    Code = register.Code
                });
            }
            return result;
        }

        public async Task<IActionResult> GetAdministrationList(IDataTablesRequest request, AdministrationFilterVM filter)
        {
            var protoRequest = request!.GetDataTablesRequestProto();
            var result = await registerGrpcClient.GetAdministrationListAsync(
                new AdministrationListRequest
                {
                    DataTableRequest = protoRequest,
                    RegisterId = filter.RegisterId,
                });
            return request.GetResponseServerPaging(result.Data, result.CountAll);
        }

        public async Task<AppAdministrations> GetAllAdministrations()
        {
            return await registerGrpcClient.GetAllAdministrationsAsync(new Empty());
        }

        public async Task<GetAdministrationResponse> GetAdministrationById(string administrationId)
        {
            return await registerGrpcClient.GetAdministrationAsync(new GetAdministrationRequest
            {
                AdministrationId = administrationId
            });
        }
        public async Task<GetAdministrationResponse> GetAdminAdministration()
        {
            return await registerGrpcClient.GetAdminAdministrationAsync(new Empty());
        }

        public async Task<IActionResult> GetPersonList(IDataTablesRequest request, PersonFilterVM filter)
        {
            var protoRequest = request!.GetDataTablesRequestProto();
            var result = await registerGrpcClient.GetPersonListAsync(
                new PersonListRequest
                {
                    DataTableRequest = protoRequest,
                    RegisterAdministrationId = filter.RegisterAdministrationId.ToString(),
                    RegisterId = filter.RegisterId
                });
            return request.GetResponseServerPaging(result.Data, result.CountAll);
        }

        private List<RegisterFileVM> RegisterFilesToVmList(RegisterItem registerItem, int sourceType)
        {
            return registerItem.RegisterFiles.Where(x => x.SourceType == sourceType).Select(x => new RegisterFileVM
            {
                MetaFileId = x.MetaFileId.ToGuid(),
                Description = x.Description,
                FileName = x.FileName,
                NomenclatureType = x.NomenclatureType,
                CodeableConceptCode = x.CodeableConceptCode,
            })
            .ToList();
        }

        private RegisterVM RegisterItemToVM(RegisterItem registerItem, Guid? registerAdministrationId)
        {
            var result = new RegisterVM
            {
                Id = registerItem.Id,
                Type = registerItem.Type,
                Code = registerItem.Code,
                LegalBasis = registerItem.LegalBasis,
                Name = registerItem.Name,
                Description = registerItem.Description,
                TypeEntry = registerItem.EntryType,
                IdentitySecurityLevel = registerItem.IdentitySecurityLevel,
                StatusId = registerItem.StatusId,
            };
            result.RegisterFiles.Files.AddRange(RegisterFilesToVmList(registerItem, (int)RegisterFileSourceType.Register));
            result.AdministrationFiles.Files.AddRange(RegisterFilesToVmList(registerItem, (int)RegisterFileSourceType.Administration));
            if (registerAdministrationId != null)
            {
                AdministrationItem? administrationItem = null;
                if (registerAdministrationId == Guid.Empty)
                {
                    if (registerItem.Administrations.Count == 1)
                    {
                        administrationItem = registerItem.Administrations.First();
                    }
                }
                else
                {
                    administrationItem = registerItem.Administrations.FirstOrDefault(x => x.Id == registerAdministrationId.ToString());
                }
                if (administrationItem != null)
                {
                    result.ContactPersons.Clear();
                    result.Administration.Id = Guid.Parse(administrationItem.Id);
                    result.Administration.Name = administrationItem.Name;
                    result.Administration.Uic = administrationItem.Uic;
                    foreach (var personItem in administrationItem.Persons)
                    {
                        var person = new PersonVM
                        {
                            Id = personItem.Id,
                            Type = personItem.Type,
                            Email = personItem.Email,
                            Phone = personItem.Phone,
                            FirstName = personItem.FirstName,
                            LastName = personItem.LastName,
                            MiddleName = personItem.MiddleName,
                            Position = personItem.Position,
                        };
                        if (person.Type == PersonTypeValue.Manager)
                        {
                            result.Manager = person;
                        }
                        else
                        {
                            person.Index = result.ContactPersons.Count;
                            result.ContactPersons.Add(person);
                        }
                    }
                }
            }
            return result;
        }
        public async Task<RegisterVM> GetRegisterForAddAdministration(int registerId)
        {
            var registerResponse = await registerGrpcClient.GetRegisterForAddAdministrationAsync(
                new GetRegisterRequest
                {
                    RegisterId = registerId
                }
            );
            return RegisterItemToVM(registerResponse.Data, null);
        }

        public async Task<RegisterVM> CreateRegister()
        {
            var registerResponse = await registerGrpcClient.CreateRegisterAsync(
                new Empty()
            );
            return RegisterItemToVM(registerResponse.Data, null);
        }
        public async Task<RegisterVM> GetRegister(int registerId, Guid registerAdministrationId)
        {
            var registerResponse = await registerGrpcClient.GetRegisterAsync(
                new GetRegisterRequest
                {
                    RegisterId = registerId
                }
            );
            return RegisterItemToVM(registerResponse.Data, registerAdministrationId);
        }

        public async Task AddRegisterStatus(RegisterStatusVM model)
        {
            var request = new RegisterStatusItem
            {
                Id = model.Id.ToString(),
                IsActive = model.StatusId != (int)RegisterStatusType.Deleted,
                RegisterId = model.RegisterId,
                StatusId = model.StatusId,
                Remark = model.Remark
            };
            request.RegisterFiles.AddRange(model.RegisterFiles.Files.Select(r =>  new RegisterFileItem
            {
                MetaFileId = r.MetaFileId.ToString(),
                SourceId = model.Id.ToString(),
                SourceType = (int)RegisterFileSourceType.RegisterStatus,
                Description = r.Description,
                NomenclatureType = r.NomenclatureType,
                CodeableConceptCode = r.CodeableConceptCode,
                FileName = r.FileName,
            }));
             await registerGrpcClient.AddRegisterStatusAsync(request);
        }

       
        public async Task<Guid?> UploadFile(IFormFile file, Guid sourceId, int sourceType)
        {
            using MemoryStream ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var uploadFile = registerGrpcClient.UploadFile();
            var request = new FileContent
            {
                FileName = file.FileName,
                FileSize = file.Length,
                ContentType = file.ContentType,
                FileInfo = new FileMessage
                {
                    SourceId = sourceId.ToString(),
                    SourceTypeId = sourceType
                }
            };
            byte[] buffer = new byte[2048];
            ms.Position = 0;
            while ((request.ReadedByte = await ms.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                request.Buffer = ByteString.CopyFrom(buffer, 0, request.ReadedByte);
                await uploadFile.RequestStream.WriteAsync(request);
            }
            await uploadFile.RequestStream.CompleteAsync();
            var response = await uploadFile;
            return response?.MetaFileId.ToGuid();
        }

        public async Task<(byte[],string, string)> DownloadFile(Guid id)
        {
            using MemoryStream ms = new MemoryStream();
            var fileName = string.Empty;
            var contentType = string.Empty;
            var downloadFile = registerGrpcClient.DownloadFile(new FileDownLoadMessage { Id = id.ToString()});
            await foreach (var response in downloadFile.ResponseStream.ReadAllAsync())
            {
                await ms.WriteAsync(response.Buffer.ToByteArray(), 0, response.ReadedByte);
                fileName = response.FileName;
                contentType = response.ContentType;
            }
            return (ms.ToArray(), contentType, fileName);
        }

        public async Task<IActionResult> GetRegisterStatusList(IDataTablesRequest request, int registerId)
        {
            var requestNom = new NomenclaturePublicRequest
            {
                RegisterId = registerId,
            };
            requestNom.NomenclatureTypes.Add(InternalNomenclatureTypes.RegisterStatus);
            var resultNom = await nomenclatureGrpcClient.GetNomenclaturePublicAsync(requestNom);
            var codeableConcepts = resultNom.NomenclatureTypes.First().CodeableConcepts;

            var result = await registerGrpcClient.GetRegisterStatusListAsync(
                new RegisterStatusRequest { 
                    DataTableRequest = request!.GetDataTablesRequestProto(),
                    RegisterId = registerId
            });
            var data = new List<RegisterStatusListItemVM>();
            var usersGuids = result.Data.Select(item => item.ModifiedBy.ToString()).Distinct().ToList();
            var userNameDict = new Dictionary<string, string>();
            if (usersGuids.Any())
            {
                var requestUsers = new UserGuidsRequest
                {
                    UserGuids = { usersGuids }
                };
                var resultUsers = await appUserManagerClient.GetUserNamesByGuidsAsync(requestUsers);
                userNameDict = resultUsers.UserNamesByGuid.ToDictionary(
                    u => u.Guid,
                    u => string.Join(" ", new[] { u.FirstName, u.MiddleName, u.LastName }.Where(s => !string.IsNullOrEmpty(s)).Select(s => s.Trim())).Trim()
                );
            }
            foreach (var item in result.Data)
            {
                data.Add(new RegisterStatusListItemVM
                {
                    Id = item.Id.ToGuid() ?? Guid.Empty,
                    Remark = item.Remark,
                    ModifiedOn = item.ModifiedOn.ToDateTime().ConvertUtcToBGTime(),
                    ModifiedBy = userNameDict.TryGetValue(item.ModifiedBy.ToString(), out var userFullName) ? userFullName : string.Empty,
                    Status = codeableConcepts.Where(x => x.Code == item.StatusId.ToString()).Select(x => x.Value).FirstOrDefault(),
                });
            }
            return request.GetResponseServerPaging(data, result.CountAll);
        }

        public async Task<RegisterStatusVM> GetRegisterStatus(Guid registerStatusId) {
            var response = await registerGrpcClient.GetRegisterStatusAsync(
             new GetRegisterStatusRequest
             {
                 RegisterStatusId = registerStatusId.ToString()
             });
            var result = new RegisterStatusVM
            {
                Id = response.Data.Id.ToGuid() ?? Guid.Empty,
                RegisterId = response.Data.RegisterId,
                StatusId = response.Data.StatusId,
                Remark = response.Data.Remark,
            };
            result.RegisterFiles.Files = response.Data.RegisterFiles.Select(x => new RegisterFileVM
            {
                FileName = x.FileName,
                CodeableConceptCode = x.CodeableConceptCode,
                MetaFileId = x.MetaFileId.ToGuid() ?? Guid.Empty,
                Description = x.Description,
                NomenclatureType = x.NomenclatureType
            }).ToList();
            return result;

        }
    }
}
