using DataTables.AspNet.Core;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Models.Register;
using URegister.Infrastructure.Extensions;
using URegister.RegistersCatalog;

namespace URegister.Core.Services
{
    public class RegisterClientService(RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient): IRegisterClientService
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
            foreach (var person in persons) {
                administration.Persons.Add(new PersonItem
                {
                    FirstName = person.FirstName,
                    MiddleName = person.MiddleName,
                    LastName = person.LastName,
                    Email = person.Email,
                    Phone = person.Phone,
                    Position = person.Position,
                    Type = person.Type,
                });
            }
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
                });
            return request.GetResponseServerPaging(result.Data, result.CountAll);
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

        public async Task<IActionResult> GetPersonList(IDataTablesRequest request, PersonFilterVM filter)
        {
            var protoRequest = request!.GetDataTablesRequestProto();
            var result = await registerGrpcClient.GetPersonListAsync(
                new PersonListRequest
                {
                    DataTableRequest = protoRequest,
                    AdministrationId = filter.AdministrationId.ToString(),
                });
            return request.GetResponseServerPaging(result.Data, result.CountAll);
        }

        private RegisterVM RegisterItemToVM(RegisterItem registerItem)
        {
            return new RegisterVM
            {
                Id = registerItem.Id,
                Type = registerItem.Type,
                Code = registerItem.Code,
                LegalBasis = registerItem.LegalBasis,
                Name = registerItem.Name,
                Description = registerItem.Description,
                TypeEntry = registerItem.EntryType,
                IdentitySecurityLevel = registerItem.IdentitySecurityLevel,
            };
        }
        public async Task<RegisterVM> GetRegisterForAddAdministration(int registerId)
        {
            var registerResponse = await registerGrpcClient.GetRegisterForAddAdministrationAsync(
                new GetRegisterRequest
                {
                    RegisterId = registerId
                }
            );
            return RegisterItemToVM(registerResponse.Data);
        }

        public async Task<RegisterVM> CreateRegister()
        {
            var registerResponse = await registerGrpcClient.CreateRegisterAsync(
                new Empty()
            );
            return RegisterItemToVM(registerResponse.Data);
        }

    }
}
