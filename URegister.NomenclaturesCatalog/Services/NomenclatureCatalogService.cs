using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Npgsql;
using Npgsql.PostgresTypes;
using URegister.Common;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Helper;
using URegister.NomenclaturesCatalog.Contracts;

namespace URegister.NomenclaturesCatalog;

/// <summary>
/// Управление на номенклатури 
/// </summary>
public class NomenclatureCatalogService(
    ILogger<NomenclatureCatalogService> logger,
    INomenclatureInfoService nomenclatureInfoService
    ) : NomenclatureGrpc.NomenclatureGrpcBase
{
    /// <summary>
    /// Четене на номенклатурен тип
    /// </summary>
    /// <param name="request">заявка</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<NomenclatureTypeResponse> GetNomenclatureType(GetNomenclatureTypeRequest request, ServerCallContext context)
    {
        var reply = new NomenclatureTypeResponse();
        try
        {
            reply = await nomenclatureInfoService.GetNomenclatureType(request.Id);
            reply.ResultStatus = CommonGrpcHelper. CreateStatusOK();
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "NomenclatureService/GetNomenclatureType");
            reply!.ResultStatus = CommonGrpcHelper.CreateStatusBadRequest(ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/GetNomenclatureType");
            reply!.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }

        return reply;
    }
    /// <summary>
    /// Извличнае списък номенклатурни типове
    /// </summary>
    /// <param name="request">заявка с филтър</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<NomenclatureTypeListResponse> GetNomenclatureTypeList(NomenclatureTypeListRequest request, ServerCallContext context)
    {
        NomenclatureTypeListResponse reply;
        try
        {
            reply = await nomenclatureInfoService.GetNomenclatureTypeList(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/GetNomenclatureTypeList");
            reply = new NomenclatureTypeListResponse();
            reply.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }

        return reply;
    }


    /// <summary>
    /// Извличнае списък номенклатурни типове за регистър
    /// </summary>
    /// <param name="request">заявка с филтър</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<NomenclatureTypeRegisterListResponse> GetNomenclatureTypeRegisterList(NomenclatureTypeRegisterListRequest request, ServerCallContext context)
    {
        NomenclatureTypeRegisterListResponse reply;
        try
        {
            reply = await nomenclatureInfoService.GetNomenclatureTypeRegisterList(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/GetNomenclatureTypeRegisterList");
            reply = new NomenclatureTypeRegisterListResponse();
            reply.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }

        return reply;
    }


    /// <summary>
    /// Добвавяне на номенклатурен тип
    /// </summary>
    /// <param name="request">заявка с данни на номенклатурен тип</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<ResultStatus> AddNomenclatureType(NomenclatureTypeRequest request, ServerCallContext context)
    {
        var reply = CommonGrpcHelper.CreateStatusOK();

        try
        {
            await nomenclatureInfoService.AddNomenclatureType(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/AddNomenclatureType");
            reply = CommonGrpcHelper.CreateStatusUniqueIndexError(ex);
        }

        return reply;
    }


    /// <summary>
    /// Инициализиран номенклатурен тип за добавяне
    /// </summary>
    /// <param name="request">заявка</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<NomenclatureTypeResponse> CreateNewNomenclatureType(Empty request, ServerCallContext context)
    {
        NomenclatureTypeResponse reply;
        try
        {
            reply = await nomenclatureInfoService.CreateNewNomenclatureType();
            reply!.ResultStatus = CommonGrpcHelper.CreateStatusOK();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/CreateNewNomenclatureType");
            reply = new NomenclatureTypeResponse
            {
                ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex)
            };
        }
        return reply;
    }

    /// <summary>
    /// Редакция на номенклатурен тип
    /// </summary>
    /// <param name="request">заявка</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<ResultStatus> EditNomenclatureType(NomenclatureTypeRequest request, ServerCallContext context)
    {
        var reply = CommonGrpcHelper.CreateStatusOK();

        try
        {
            var result = await nomenclatureInfoService.EditNomenclatureType(request);
            reply = result ? CommonGrpcHelper.CreateStatusOK() : CommonGrpcHelper.CreateStatusBadRequest($"Няма запис с Type: {request.Type}");

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/EditNomenclatureType");
            reply = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }

        return reply;
    }

    /// <summary>
    /// Нова номенкалтурна стойност
    /// </summary>
    /// <param name="request">заявка</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<CodeableConceptResponse> CreateNewCodeableConcept(CreateNewCodeableConceptRequest request, ServerCallContext context)
    {
        CodeableConceptResponse reply = new();
        try
        {
            reply = await nomenclatureInfoService.CreateNewCodeableConcept(request.Type);
            reply!.ResultStatus = CommonGrpcHelper.CreateStatusOK();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/CreateNewCodeableConcept");
            reply.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    /// <summary>
    /// Четене на номенкалтурна стойност
    /// </summary>
    /// <param name="request">заявка</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<CodeableConceptResponse> GetCodeableConcept(GetCodeableConceptRequest request, ServerCallContext context)
    {
        CodeableConceptResponse reply = new();
        try
        {
           reply =  await nomenclatureInfoService.GetCodeableConcept(request.Id);
        }
        catch (ArgumentException aex)
        {
            logger.LogError(aex, "NomenclatureService/GetCodeableConcept");
            reply.ResultStatus = CommonGrpcHelper.CreateStatusBadRequest(aex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/GetCodeableConcept");
            reply.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    /// <summary>
    /// Запис на номенкалтурна стойност
    /// </summary>
    /// <param name="request">заявка</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<ResultStatus> EditCodeableConcept(CodeableConceptRequest request, ServerCallContext context)
    {
        try
        {
            await nomenclatureInfoService.EditCodeableConcept(request);
            return CommonGrpcHelper.CreateStatusOK();
        }
        catch (ArgumentException aex)
        {
            logger.LogError(aex, "NomenclatureService/EditCodeableConcept");
            return CommonGrpcHelper.CreateStatusBadRequest(aex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/EditCodeableConcept");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
    }

    /// <summary>
    /// Добавяне на номенкалтурна стойност
    /// </summary>
    /// <param name="request">заявка</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<ResultStatus> AddCodeableConcept(CodeableConceptRequest request, ServerCallContext context)
    {
        try
        {
            await nomenclatureInfoService.AddCodeableConcept(request);
            return CommonGrpcHelper.CreateStatusOK();
        }
        catch (ArgumentException aex)
        {
            logger.LogError(aex, "NomenclatureService/AddCodeableConcept");
            return CommonGrpcHelper.CreateStatusBadRequest(aex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/AddCodeableConcept");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
    }

    /// <summary>
    /// Списък стойности към номенклатурен тип
    /// </summary>
    /// <param name="request">заявка</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<CodeableConceptListResponse> GetCodeableConceptList(CodeableConceptListRequest request, ServerCallContext context)
    {
        CodeableConceptListResponse reply = new();
        try
        {
            reply = await nomenclatureInfoService.GetCodeableConceptList(request);
            reply.ResultStatus = CommonGrpcHelper.CreateStatusOK();
        }
        catch (ArgumentException aex)
        {
            logger.LogError(aex, "NomenclatureService/GetCodeableConceptList");
            reply.ResultStatus = CommonGrpcHelper.CreateStatusBadRequest(aex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/GetCodeableConceptList");
            reply.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    /// <summary>
    /// Номенклатурни данни за регистър
    /// </summary>
    /// <param name="request">заявка</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<NomenclaturePublicResponse> GetNomenclaturePublic(NomenclaturePublicRequest request, ServerCallContext context)
    {
        NomenclaturePublicResponse reply = new();
        try
        {
            reply = await nomenclatureInfoService.GetNomenclaturePublic(request);
            reply.ResultStatus = CommonGrpcHelper.CreateStatusOK();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/GetNomenclaturePublic");
            reply.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    /// <summary>
    /// Четене данни за номенклатурна стойност
    /// </summary>
    /// <param name="request">заявка</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<NomenclatureTypeResponse> GetNomenclatureTypeOnType(GetNomenclatureTypeOnTypeRequest request, ServerCallContext context)
    {
        var reply = new NomenclatureTypeResponse();
        try
        {
            reply = await nomenclatureInfoService.GetNomenclatureTypeOnType(request.Type);
            reply.ResultStatus = CommonGrpcHelper.CreateStatusOK();
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "NomenclatureService/GetNomenclatureTypeOnType");
            reply!.ResultStatus = CommonGrpcHelper.CreateStatusBadRequest(ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/GetNomenclatureTypeOnType");
            reply!.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    /// <summary>
    /// Импорт на НРНМ НСИ в номенклатури
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<ResultStatus> ImportNrnmNsi(Empty request, ServerCallContext context)
    {
        var reply = CommonGrpcHelper.CreateStatusOK();
        try 
        {
            await nomenclatureInfoService.ImportNrnmNsi();
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "NomenclatureService/ImportNrnmNsi");
            reply = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    /// <summary>
    /// Импорт улици на НРНМ НСИ в номенклатури
    /// Отделно е защото не с вързно с документ
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<ResultStatus> ImportNrnmNsiStreet(Empty request, ServerCallContext context)
    {
        var reply = CommonGrpcHelper.CreateStatusOK();
        try
        {
            await nomenclatureInfoService.ImportEkStreet(NomenclatureTypes.EkStreet);
            await nomenclatureInfoService.ImportEkStreet(NomenclatureTypes.EkKvartal);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "NomenclatureService/ImportNrnmNsiStreet");
            reply = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    /// <summary>
    /// Допустими номенклатурни типове
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<NomenclatureTypeListPublicResponse> GetNomenclatureTypeListPublic(NomenclaturePublicRequest request, ServerCallContext context)
    {
        NomenclatureTypeListPublicResponse reply = new();
        try
        {
            reply = await nomenclatureInfoService.GetNomenclatureTypesPublic(request.RegisterId);
            reply.ResultStatus = CommonGrpcHelper.CreateStatusOK();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/GetNomenclatureTypeListPublic");
            reply.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }
    
    /// <summary>
    /// Четене на населени места
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async override Task<EkattePublicResponse> GetEkattePublic(EkattePublicRequest request, ServerCallContext context)
    {
        EkattePublicResponse reply = new();
        try
        {
            reply.ResultStatus = CommonGrpcHelper.CreateStatusOK();
            var categories = await nomenclatureInfoService.GetEkattePublic(request);
            reply.Categories.Add(categories);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/GetEkattePublic");
            reply.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }


    public override async Task<ResultStatus> UpdateNomenclatureTypeRegister(UpdateNomenclatureTypeRegisterRequest request, ServerCallContext context)
    {
        try
        {
            await nomenclatureInfoService.UpdateNomenclatureTypeRegister(request);
            return CommonGrpcHelper.CreateStatusOK();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/UpdateNomenclatureTypeRegister");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<CodeableConceptRegisterListResponse> GetCodeableConceptRegisterList(CodeableConceptRegisterListRequest request, ServerCallContext context)
    {
        CodeableConceptRegisterListResponse reply = new();
        try
        {
            reply = await nomenclatureInfoService.GetCodeableConceptRegisterList(request);
            reply.ResultStatus = CommonGrpcHelper.CreateStatusOK();
        }
        catch (ArgumentException aex)
        {
            logger.LogError(aex, "NomenclatureService/GetCodeableConceptRegisterList");
            reply.ResultStatus = CommonGrpcHelper.CreateStatusBadRequest(aex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/GetCodeableConceptRegisterList");
            reply.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<NomenclatureTypeRegisterResponse> GetNomenclatureTypeRegisterOnType(GetNomenclatureTypeRegisterOnTypeRequest request, ServerCallContext context)
    {
        var reply = new NomenclatureTypeRegisterResponse();
        try
        {
            reply = await nomenclatureInfoService.GetNomenclatureTypeRegisterOnType(request.Type, request.RegisterId);
            reply.ResultStatus = CommonGrpcHelper.CreateStatusOK();
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "NomenclatureService/GetNomenclatureTypeRegisterOnType");
            reply!.ResultStatus = CommonGrpcHelper.CreateStatusBadRequest(ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/GetNomenclatureTypeRegisterOnType");
            reply!.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<ResultStatus> UpdateCodeableConceptRegister(UpdateCodeableConceptRegisterRequest request, ServerCallContext context)
    {
        try
        {
            await nomenclatureInfoService.UpdateCodeableConceptRegister(request);
            return CommonGrpcHelper.CreateStatusOK();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/UpdateCodeableConceptRegister");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
    }

    public override async Task<CheckNomenclatureResponse> CheckNomenclature(CheckNomenclatureRequest request, ServerCallContext context)
    {
        var result = new CheckNomenclatureResponse
        {
            ResultStatus = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            result.Data.AddRange(await nomenclatureInfoService.CheckNomenclature(request));
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/UpdateCodeableConceptRegister");
            result .ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
            return result;
        }
    }

    public override async Task<NomenclaturePublicResponse> GetNomenclatureOnHolderPublic(NomenclatureHolderRequest request, ServerCallContext context)
    {
        var result = new NomenclaturePublicResponse
        {
            ResultStatus = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            result.NomenclatureTypes.Add(await nomenclatureInfoService.GetNomenclatureOnHolderPublic(request));
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NomenclatureService/GetNomenclatureOnHolderPublic");
            result.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
            return result;
        }
    }

    /// <summary>
    /// Проверява дали подадената стойност е измежу позволените стойности за регистъра
    /// </summary>
    /// <param name="request">Заявка с параметри</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<AreNomenclatureCodesAllowedResponse> AreNomenclatureCodesAllowed(
        AreNomenclatureCodesAllowedRequest request, ServerCallContext context)
    {
        var result = new AreNomenclatureCodesAllowedResponse
        {
            ResultStatus = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            result.AreAllowed = await nomenclatureInfoService.AreNomenclatureCodesAllowed(request);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(NomenclatureCatalogService)}/{nameof(AreNomenclatureCodesAllowed)}");
            result.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
            return result;
        }
    }

    /// <summary>
    /// Върща текста на номенклатура по кода и
    /// </summary>
    /// <param name="request">Заявка с параметри</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<GetValueResponse> GetValueByCode(
        GetValueRequest request, ServerCallContext context)
    {
        var result = new GetValueResponse
        {
            ResultStatus = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            result.NomenclatureValue = await nomenclatureInfoService.GetValueByCode(request);
            if (string.IsNullOrEmpty(result.NomenclatureValue))
            {
                logger.LogWarning($"Не намерена стойност за номенклатура от тип {request.NomenclatureType} с код {request.NomenclatureCode} в {nameof(NomenclatureCatalogService)}{nameof(GetValueByCode)}");
            }
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(NomenclatureCatalogService)}/{nameof(GetValueByCode)}");
            result.ResultStatus = CommonGrpcHelper.CreateStatusInternalServerError(ex);
            return result;
        }
    }
}
