using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using URegister.Common;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Helper;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.ObjectsCatalog.Contracts;

namespace URegister.ObjectsCatalog.Services;

/// <summary>
/// Достъп до каталога на обекти
/// </summary>
/// <param name="logger"></param>
public class ObjectCatalogService(
    ILogger<ObjectCatalogService> logger,
    IObjectService objectService) : ObjectsCatalogGrpc.ObjectsCatalogGrpcBase
{
    /// <summary>
    /// Списък на полетата
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<CatalogFieldsListReply> GetFieldsList(Empty request, ServerCallContext context)
    {
        var result = new CatalogFieldsListReply()
        {
            Status = GetOkResultStatus()
        };

        try
        {
            var types = await objectService.GetFieldTypesAsync();

            result.FieldTypes.AddRange(types.Select(t => new CatalogFieldType()
            {
                Type = t.type,
                Label = t.label,
                TemplateName = t.template,
                IsComplex = t.isComplex
            }));
        }
        catch (Exception ex)
        {
            result.Status.Code = ResultCodes.InternalServerError;
            result.Status.Message = "Възникна непредвидена грешка";

            logger.LogError(ex, "ObjectCatalogService/GetFieldsList");
        }

        return result;
    }

    /// <summary>
    /// Вземане на поле
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<CatalogGetFieldReply> GetField(CatalogFieldRequest request, ServerCallContext context)
    {
        var result = new CatalogGetFieldReply()
        {
            Status = GetOkResultStatus(),
            Data = string.Empty
        };

        try
        {
            result.Data = await objectService.GetFieldDataAsync(request.FieldType);
        }
        catch (ArgumentException aex)
        {
            result.Status.Code = ResultCodes.BadRequest;
            result.Status.Message = aex.Message;
        }
        catch (Exception ex)
        {
            result.Status.Code = ResultCodes.InternalServerError;
            result.Status.Message = "Възникна непредвидена грешка";

            logger.LogError(ex, "ObjectCatalogService/GetField");
        }

        return result;
    }

    /// <summary>
    /// Запис на поле
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<CatalogSetFieldReply> SetField(CatalogSerializedData request, ServerCallContext context)
    {
        var result = new CatalogSetFieldReply()
        {
            Status = GetOkResultStatus(),
            Version = 0
        };

        // ToDo: Validate request against business rules

        try
        {
            FormField? data = request.Data.FromJson<FormField>();

            if (data == null)
            {
                result.Status.Code = ResultCodes.BadRequest;
                result.Status.Message = "Невалидни данни";
            }
            else
            {
                result.Version = await objectService.SetFieldDataAsync(data);
            }
        }
        catch (ArgumentException aex)
        {
            result.Status.Code = ResultCodes.BadRequest;
            result.Status.Message = aex.Message;
        }
        catch (Exception ex)
        {
            result.Status.Code = ResultCodes.InternalServerError;
            result.Status.Message = "Възникна непредвидена грешка";

            logger.LogError(ex, "ObjectCatalogService/SetField");
        }

        return result;
    }

    /// <summary>
    /// Запис на тип поле
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<ResultStatus> SetFieldType(CatalogFieldType request, ServerCallContext context)
    {
        return new ResultStatus
        {
            Code = (await objectService.SetFieldTypeAsync(request)) ? ResultCodes.Ok : ResultCodes.InternalServerError,
            Message = "Проблем при запис на тип поле " + request.Label
        };
    }

    /// <summary>
    /// Списък типове услуги
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<ServiceTypesListReply> GetServiceTypes(DatatableRequest request, ServerCallContext context)
    {
        var reply = new ServiceTypesListReply()
        {
            Status = CommonGrpcHelper.CreateStatusOK(),
        };
        try
        {
            (var data, var countAll) = await objectService.GetServiceTypes(request);
            reply.ServiceTypes.AddRange(data);
            reply.CountAll = countAll;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ObjectCatalogService/GetNomenclatureType");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    /// <summary>
    /// списък стъпки към услуга
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<StepListReply> GetStepList(DatatableRequest request, ServerCallContext context)
    {
        var reply = new StepListReply()
        {
            Status = CommonGrpcHelper.CreateStatusOK(),
        };
        try
        {
            (var data, var countAll) = await objectService.GetStepList(request);
            reply.Steps.AddRange(data);
            reply.CountAll = countAll;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ObjectCatalogService/GetStepList");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    /// <summary>
    /// Запис на стъпка
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async override Task<ResultStatus> AppendUpdateStep(StepMessage request, ServerCallContext context)
    {
        try
        {
            await objectService.AppendUpdateStep(request);
            return CommonGrpcHelper.CreateStatusOK();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ObjectCatalogService/AppendUpdateStep");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
    }
    /// <summary>
    /// Четене на стъпка към услуга
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async override Task<GetStepReply> GetStep(GetStepMessage request, ServerCallContext context)
    {
        var reply = new GetStepReply()
        {
            Status = CommonGrpcHelper.CreateStatusOK(),
        };
        try
        {
            reply.Step = await objectService.GetStep(request.StepId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ObjectCatalogService/GetStepList");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    /// <summary>
    /// Четене на тип услуга
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async override Task<GetServiceTypeReply> GetServiceType(GetServiceTypeRequest request, ServerCallContext context)
    {
        var reply = new GetServiceTypeReply()
        {
            Status = CommonGrpcHelper.CreateStatusOK(),
        };
        try
        {
            reply.ServiceType = await objectService.GetServiceType(request.ServiceId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ObjectCatalogService/GetServiceType");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public async override Task<ResultStatus> AppendUpdate(ServiceTypeMessage request, ServerCallContext context)
    {
        try
        {
            await objectService.AppendUpdate(request);
            return CommonGrpcHelper.CreateStatusOK();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ObjectCatalogService/AppendUpdate");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
    }

    private ResultStatus GetOkResultStatus()
    {
        return new ResultStatus
        {
            Code = ResultCodes.Ok
        };
    }
}
