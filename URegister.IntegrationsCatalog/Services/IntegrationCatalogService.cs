using Grpc.Core;
using IO.RegixClient;
using IO.RegixClient.ServiceModels.RA;
using Regix;
using System.ComponentModel;
using URegister.Common;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Helper;
using URegister.Infrastucture.Extensions;
using URegister.IntegrationsCatalog.Contracts;
using System.Xml.Serialization;
using System.Xml;
using URegister.IntegrationsCatalog.Helpers;
using URegister.IntegrationsCatalog.Models;
using URegister.NomenclaturesCatalog;

namespace URegister.IntegrationsCatalog.Services;

public class IntegrationCatalogService : IntegrationGrpc.IntegrationGrpcBase
{
    private const string RegixEikResponseCompanyAddressFieldNumber = "00050";
    private const string RegixEikResponseCompanyNameFieldNumber = "00020";

    private readonly IRegixClient _regixClient;
    private readonly NomenclatureGrpc.NomenclatureGrpcClient _nomenclatureGrpcClient;
    private readonly ILogger<IntegrationCatalogService> _logger;
    private readonly IEDeliveryService edeliveryService;

    public IntegrationCatalogService(
        IRegixClient regixClient,
        NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
        IEDeliveryService edeliveryService,
        ILogger<IntegrationCatalogService> logger)
    {
        _regixClient = regixClient;
        _nomenclatureGrpcClient = nomenclatureGrpcClient;
        this.edeliveryService = edeliveryService;
        _logger = logger;
    }

    public override async Task<GetPersonInfoResponse> GetPersonInfo(GetPersonInfoRequest request, ServerCallContext context)
    {
        try
        {
            var callContext = GetCallContext(request.ContextInfo);

            PersonDataResponseType response = await _regixClient.GetPersonAsync(request.Pid, callContext);
            return new GetPersonInfoResponse
            {
                FirstName = response.PersonNames.FirstName,
                MiddleName = response.PersonNames.SurName,
                LastName = response.PersonNames.FamilyName,
                ResultStatus = new ResultStatus
                {
                    Code = ResultCodes.Ok
                }
            };
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"Проблем при извличане данни за лице с ЕГН {request.Pid}, в {nameof(GetPersonInfo)}");
            return new GetPersonInfoResponse
            {
                ResultStatus = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError
                }
            };
        }
    }

    private CallContext GetCallContext(IntegrationServiceContextInfo requestContextInfo)
    {
        return new CallContext()
        {
            AdministrationName = "МЕУ",
            AdministrationOId = "2.16.100.1.1.208230.1.4",
            EmployeeIdentifier = "1",
            EmployeeNames = requestContextInfo.EmployeeNames,
            EmployeePosition = requestContextInfo.EmployeePosition,
            LawReason = "За целите на ИСЦИПР",
            Remark = requestContextInfo.EmployeeAdministration,
            ServiceType = "За административна услуга",
            ServiceURI = "2.16.100.1.1.117.1.2"
        };
    }

    public static TEnum GetEnumValueByDescription<TEnum>(string description) where TEnum : Enum
    {
        var field = typeof(TEnum).GetFields()
            .FirstOrDefault(f => f.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .Cast<DescriptionAttribute>()
            .Any(attr => attr.Description == description));

        return field != null ? (TEnum)field.GetValue(null) : default;
    }

    public override async Task<GetCompanyInfoResponse> GetCompanyInfo(GetCompanyInfoRequest request, ServerCallContext context)
    {
        try
        {
            var callContext = GetCallContext(request.ContextInfo);

            if (request.CidType == (int)CidTypes.EIK)
            {
                ActualStateResponseV3 response =
                    await _regixClient.TR_GetActualStateV3Async(request.Cid, callContext);

                System.Xml.XmlNode[] companyAddress =
                    (response.Deed.Subdeeds.Subdeed
                        .SelectMany(s => s.Records
                            .Where(r => r.MainField.MainFieldIdent == RegixEikResponseCompanyAddressFieldNumber))
                        .Single()
                        .RecordData as System.Xml.XmlNode[]);

                System.Xml.XmlNode[] companyName =
                    (response.Deed.Subdeeds.Subdeed
                        .SelectMany(s => s.Records
                            .Where(r => r.MainField.MainFieldIdent == RegixEikResponseCompanyNameFieldNumber))
                        .Single()
                        .RecordData as System.Xml.XmlNode[]);

                RegixDataModels.Address address;
                var serializer = new XmlSerializer(typeof(RegixDataModels.Address));
                using (var reader = new XmlNodeReader(companyAddress.Single().FirstChild))
                {
                    address = (RegixDataModels.Address)serializer.Deserialize(reader);
                }

                return new GetCompanyInfoResponse
                {
                    Name = /*response.Deed.CompanyName*/companyName[0].InnerText,
                    LegalFormCode = (int)GetEnumValueByDescription<LegalFormsEIK>(response.Deed.LegalForm.ToString()),
                    ResultStatus = new ResultStatus
                    {
                        Code = ResultCodes.Ok
                    },
                    LegalFormName = response.Deed.LegalForm.ToString(),
                    ApartmentNumber = address.Apartment,
                    BuildingNumber = address.Block,
                    CountryCode = RegixDataHelper.CountryCodeMapEIK[address.CountryCode],
                    CountryName = address.Country,
                    EntranceName = address.Entrance,
                    FloorNumber = address.Floor,
                    ForeignAddress = address.ForeignPlace,
                    PostCode = address.PostCode,
                    RegionCode = address.AreaEkatte,
                    RegionName = address.Area,
                    SettlementCode = address.SettlementEKATTE,
                    SettlementName = address.Settlement,
                    StreetName = address.Street,
                    StreetNumber = address.StreetNumber 
                };
            }
            else
            {
                var response =
                    await _regixClient.Bulstat_GetStateOfPlay(request.Cid, callContext);

                var address = response.Subject.Addresses
                    .Single(a => a.AddressType.Code == RegixDataHelper.ManagementAddressTypeCodeBulstat.ToString());

                var result = new GetCompanyInfoResponse
                {
                    Name = response.Subject.LegalEntitySubject.CyrillicFullName,
                    LegalFormCode = int.Parse(response.Subject.LegalEntitySubject.LegalForm.Code),
                    ResultStatus = new ResultStatus { Code = ResultCodes.Ok },
                    LegalFormName = ((LegalFormsBULSTAT)int.Parse(response.Subject.LegalEntitySubject.LegalForm.Code))
                        .GetDescription(),
                    ApartmentNumber = address.Apartment,
                    BuildingNumber = address.Building,
                    CountryCode = RegixDataHelper.CountryCodeDictionaryBulstat[int.Parse(address.Country.Code)],
                    CountryName = "[Заредена стойност]", //взима се със заявка по-надолу
                    EntranceName = address.Entrance,
                    FloorNumber = address.Floor,
                    ForeignAddress = address.ForeignLocation,
                    PostCode = address.PostalCode,
                    RegionCode = address.Region?.Code,
                    RegionName = address.Region != null ? "[Заредена стойност]" : null, //взима се със заявка по-надолу
                    SettlementCode = address.Location.Code,
                    SettlementName = "[Заредена стойност]", //взима се със заявка по-надолу
                    StreetName = address.Street,
                    StreetNumber = address.StreetNumber
                };

                NomenclaturePublicRequest getNomenclaturesRequest = new NomenclaturePublicRequest
                {
                    RegisterId = 0,
                    NomenclatureTypes = { NomenclatureTypes.Ekatte, NomenclatureTypes.EkCountry }
                };

                try
                {
                    NomenclaturePublicResponse nomenclaturesResult =
                        await _nomenclatureGrpcClient.GetNomenclaturePublicAsync(getNomenclaturesRequest);

                    if (nomenclaturesResult.ResultStatus.Code != ResultCodes.Ok)
                    {
                        _logger.LogError(
                            $"Не може да зареди номенклатури в {nameof(GetCompanyInfo)}");
                    }
                    else
                    {
                        result.SettlementName = nomenclaturesResult.NomenclatureTypes.First(n => n.Type == NomenclatureTypes.Ekatte).CodeableConcepts
                            .Single(c => c.Code == result.SettlementCode).Value;

                        result.CountryName = nomenclaturesResult.NomenclatureTypes.First(n => n.Type == NomenclatureTypes.EkCountry).CodeableConcepts
                            .Single(c => c.Code == result.CountryCode).Value;
                    }

                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Грешка при {nameof(_nomenclatureGrpcClient.GetNomenclaturePublicAsync)} {e.InnerException?.Message}");
                }

                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Проблем при извличане данни за компания с {((CidTypes)request.CidType).GetDescription()} {request.Cid}, в {nameof(GetCompanyInfo)}");
            return new GetCompanyInfoResponse
            {
                ResultStatus = new ResultStatus
                {
                    Code = ResultCodes.InternalServerError
                }
            };
        }
    }

    public override async Task<SendMessageResponse> SendMessage(OutboxMessage request, ServerCallContext context)
    {
        var reply = new SendMessageResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            await edeliveryService.SendMessage(request);
        } 
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<IntegrationFilesResponse> GetIntegrationFilesUrl(IntegrationFileRequest request, ServerCallContext context)
    {

        var reply = new IntegrationFilesResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Files.AddRange(await edeliveryService.GetIntegrationFilesUrl(request));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    /// <summary>
    /// Връща списък със записи в лог на електронни връчвания
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<EDeliveryMessagesListReply> GetEDeliveryLogRecordsList(DatatableRequest request, ServerCallContext context)
    {
        var reply = new EDeliveryMessagesListReply()
        {
            Status = CommonGrpcHelper.CreateStatusOK(),
        };
        try
        {
            (var data, var countAll) = await edeliveryService.GetEDeliveryLogRecordsList(request);
            reply.EdeliveryMessages.AddRange(data);
            reply.CountAll = countAll;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }
}
