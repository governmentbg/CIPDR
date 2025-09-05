using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using URegister.AuditLog.Contracts;
using URegister.Common;
using URegister.Infrastructure.Helper;

namespace URegister.AuditLog.Services;

/// <summary>
/// gRpc услуга за генериране на номера
/// <param name="logger"></param>
/// <param name="auditLogInfoService"></param>
/// </summary>
public class AuditLogGrpcService(
        ILogger<AuditLogGrpcService> logger,
        IAuditLogInfoService auditLogInfoService) : AuditLogGrpc.AuditLogGrpcBase
{
    public override async Task<ResultStatus> AddAuditLogAndEntities(AuditEntitiesMessage request, ServerCallContext context)
    {
        var reply = new ResultStatus
        {
            Code = ResultCodes.Ok
        };

        try
        {
            await auditLogInfoService.AddAuditLogAndEntities(request);
        }
        catch (ArgumentException aex)
        {
            logger.LogError(aex, "AuditLogGrpcService/AddAuditLogAndEntities");

            reply.Code = ResultCodes.BadRequest;
            reply.Message = aex.Message;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AuditLogGrpcService/AddAuditLogAndEntities");

            reply.Code = ResultCodes.InternalServerError;
            reply.Message = ex.Message;
        }

        return reply;
    }

    public override async Task<AuditListResponse> GetAuditLogRecordsList(DatatableRequestWithAuditLogFilter request, ServerCallContext context)
    {
        var reply = new AuditListResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };

        try
        {
            (var data, var countAll) = await auditLogInfoService.GetAuditLogRecordsList(request);            
            reply.AuditList.AddRange(data);
            reply.CountAll = countAll;
        }
        catch (ArgumentException aex)
        {
            logger.LogError(aex, "AuditLogGrpcService/GetAuditLogRecordsList");

            reply.Status = CommonGrpcHelper.CreateStatusBadRequest(aex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AuditLogGrpcService/GetAuditLogRecordsList");

            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);         
        }

        return reply;
    }

    public override async Task<AuditEntityListResponse> GetAuditEntityValues(StringValue auditId, ServerCallContext context)
    {
        var reply = new AuditEntityListResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };

        try
        {
            var data = await auditLogInfoService.GetAuditEntityValues(auditId.Value);
            reply.AuditEntities.AddRange(data);          
        }
        catch (ArgumentException aex)
        {
            logger.LogError(aex, "AuditLogGrpcService/GetAuditEntityValues");

            reply.Status = CommonGrpcHelper.CreateStatusBadRequest(aex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AuditLogGrpcService/GetAuditEntityValues");

            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }

        return reply;
    }

}