using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using URegister.Common;
using URegister.Infrastructure.Helper;
using URegister.RegistersCatalog;
using URegister.RegistersCatalog.Contracts;
using URegister.RegistersCatalog.Data;

namespace URegister.RegistersCatalog.Services;


public class RegisterService(
    ILogger<RegisterService> logger,
    IRegisterInfoService registerInfoService
    ) : RegistersCatalogGrpc.RegistersCatalogGrpcBase
{
    public override async Task<RegisterListResponse> GetRegisterList(Empty request, ServerCallContext context)
    {
        var reply = new RegisterListResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
           reply.Data.AddRange(await registerInfoService.GetRegisterList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetRegisterList");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }

        return reply;
    }

    public override async Task<RegisterFullListResponse> GetRegisterFullList(RegisterListRequest request, ServerCallContext context)
    {
        var reply = new RegisterFullListResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            (var data, var countAll) = await registerInfoService.GetRegisterFullList(request);
            reply.CountAll = countAll;
            reply.Data.AddRange(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetRegisterFullList");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }

        return reply;
    }

    public override async Task<ResultStatus> AddRegister(RegisterItem request, ServerCallContext context)
    {
        try
        {
            await registerInfoService.AddRegister(request);
            return CommonGrpcHelper.CreateStatusOK();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/AddRegister");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
    }

    public override async Task<AdministrationListResponse> GetAdministrationList(AdministrationListRequest request, ServerCallContext context)
    {
        var reply = new AdministrationListResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            (var data, var countAll) = await registerInfoService.GetAdministrationList(request);
            reply.CountAll = countAll;
            reply.Data.AddRange(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetAdministrationList");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }

        return reply;
    }
    public override async Task<PersonListResponse> GetPersonList(PersonListRequest request, ServerCallContext context)
    {
        var reply = new PersonListResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            (var data, var countAll) = await registerInfoService.GetPersonList(request);
            reply.CountAll = countAll;
            reply.Data.AddRange(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetPersonList");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }

        return reply;
    }

    public override async Task<GetRegisterResponse> GetRegisterForAddAdministration(GetRegisterRequest request, ServerCallContext context)
    {
        var reply = new GetRegisterResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data = await registerInfoService.GetRegisterForAddAdministration(request.RegisterId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetRegisterForAddAdministration");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }

        return reply;
    }

    public override async Task<GetRegisterResponse> CreateRegister(Empty request, ServerCallContext context)
    {
        var reply = new GetRegisterResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data = await registerInfoService.CreateRegister();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/CreateRegister");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<GetRegisterResponse> GetRegisterAndMarkAsStarted(GetRegisterRequest request, ServerCallContext context)
    {
        var reply = new GetRegisterResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data = await registerInfoService.GetRegister(request.RegisterId);
            await registerInfoService.SetRegisterAsStarted(request.RegisterId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetRegister");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }
    public override async Task<RegisterListResponse> GetRegisterNotStartedList(Empty request, ServerCallContext context)
    {
        var reply = new RegisterListResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data.AddRange(await registerInfoService.GetRegisterNotStartedList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetRegisterNotStartedList");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<MasterPersonRecordsIndexAddResponse> AddMasterPersonRecordsIndex(MasterPersonRecordsIndexAddMessage request, ServerCallContext context)
    {
        var reply = new MasterPersonRecordsIndexAddResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Id =  await registerInfoService.AddMasterPersonRecordsIndex(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/MasterPersonRecordsIndexAddResponse");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }
}
