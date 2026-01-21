using Amazon.Runtime.Internal;
using Amazon.S3.Model;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.IO;
using System.Text;
using URegister.Common;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Helper;
using URegister.RegistersCatalog.Contracts;
using URegister.RegistersCatalog.Data.Models;

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

    public override async Task<GetRegisterResponse> GetRegisterByRegisterCode(GetRegisterByCodeRequest request, ServerCallContext context)
    {
        var reply = new GetRegisterResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data = await registerInfoService.GetRegisterByCode(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetRegisterByRegisterCode");
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

    public override async Task<AppAdministrations> GetAllAdministrations(Empty request, ServerCallContext context)
    {
        var reply = new AppAdministrations
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };

        try
        {
            var administrations = await registerInfoService.GetAdministrations();
            foreach (var administration in administrations)
            {
                reply.Administrations.Add(new AppAdministration
                {
                    Id = administration.Id.ToString(),
                    Name = administration.Name,
                    Uic = administration.Uic
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetAllAdministrations");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }

        return reply;
    }

    public override async Task<GetAdministrationResponse> GetAdministration(GetAdministrationRequest request, ServerCallContext context)
    {
        var reply = new GetAdministrationResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK(),
        };

        try
        {
            Guid administrationId = Guid.Empty;

            if (Guid.TryParse(request.AdministrationId.Trim(), out administrationId) == false)
            {
                reply.Status = new ResultStatus
                {
                    Code = ResultCodes.BadRequest,
                    Message = "AdministrationId is required and must be a valid Guid"
                };
                return reply;
            }

            Administration administration = await registerInfoService.GetAdministrationById(administrationId);
            if (administration == null)
            {
                reply.Status = new ResultStatus
                {
                    Code = ResultCodes.NotFound,
                    Message = "Administration not found."
                };
                return reply;
            }
            reply.Data = new AppAdministration
            {
                Id = administration.Id.ToString(),
                Name = administration.Name,
                NameEDelivery = administration.NameEDelivery,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetAdministration");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<GetAdministrationResponse> GetAdminAdministration(Empty request, ServerCallContext context)
    {
        var reply = new GetAdministrationResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK(),
        };

        try
        {
            Administration administration = await registerInfoService.GetAdminAdministration();
            if (administration == null)
            {
                reply.Status = new ResultStatus
                {
                    Code = ResultCodes.NotFound,
                    Message = "Admin administration not found."
                };
                return reply;
            }
            reply.Data = new AppAdministration
            {
                Id = administration.Id.ToString(),
                Name = administration.Name
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetAdminAdministration");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<GetRegistriesResponse> GetAdministrationRegistries(AppAdministration request, ServerCallContext context)
    {
        var reply = new GetRegistriesResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };

        try
        {
            reply.Data.AddRange(await registerInfoService.GetAdministrationRegistries(Guid.Parse(request.Id)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetAdministrationRegistries");
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

 
    public override async Task<GetRegisterResponse> GetRegister(GetRegisterRequest request, ServerCallContext context)
    {
        var userId = context.RequestHeaders.FirstOrDefault(m =>
            string.Equals(m.Key, "userid", StringComparison.Ordinal))?
            .Value;
        var reply = new GetRegisterResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data = await registerInfoService.GetRegister(request.RegisterId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetRegister");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

       public override async Task<MasterPersonRecordIndexAddResponse> AddMasterPersonRecordIndex(MasterPersonRecordIndexAddMessage request, ServerCallContext context)
    {
        var reply = new MasterPersonRecordIndexAddResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Id = await registerInfoService.AddMasterPersonRecordIndex(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/MasterPersonRecordsIndexAddResponse");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<MPRIListMessage> GetMasterPersonRecordIndex(GetMasterPersonRecordIndexMessage request, ServerCallContext context)
    {
        var reply = new MPRIListMessage
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Items.AddRange(await registerInfoService.GetMasterPersonRecordIndex(request));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetMasterPersonRecordIndex");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<MPRIListMessage> GetMasterPersonRecordIndexList(GetMPRIListMessage request, ServerCallContext context)
    {
        var reply = new MPRIListMessage
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Items.AddRange(
                await registerInfoService.GetMasterPersonRecordIndexList(
                    request.IdList.Select(x => Guid.Parse(x)).ToList()
                )
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetMasterPersonRecordIndex");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    /// <summary>
    /// Премахване на администрация от регистър
    /// </summary>
    /// <param name="request">Заявка с данни</param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<ResultStatus> RemoveAdministrationFromRegister(RemoveAdministrationFromRegisterRequest request, ServerCallContext context)
    {
        if (Guid.TryParse(request.RegisterAdministrationId, out Guid registerAdministrationId))
        {
            ResultStatus resultStatus = await registerInfoService.RemoveAdministrationFromRegister(registerAdministrationId);
            return resultStatus;
        }

        return new ResultStatus()
        {
            Code = ResultCodes.BadRequest,
            Message = $"Идентификаторът '{request.RegisterAdministrationId}' не е валиден Guid"
        };
    }

    public override async Task<AppAdministrations> GetAdministrationsByIds(AdministrationIds request, ServerCallContext context)
    {
        var reply = new AppAdministrations
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };

        try
        {
            IList<AppAdministration> administrations = await registerInfoService.GetAdministrationsByIds(request.Ids, request.RegisterId);

            reply.Administrations.AddRange(administrations);
        }
        catch (ArgumentException aex)
        {
            reply.Status = CommonGrpcHelper.CreateStatusBadRequest(aex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetAdministrationsByIds");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }

        return reply;
    }
    public override async Task<ResultStatus> AddRegisterStatus(RegisterStatusItem request, ServerCallContext context)
    {
        try
        {
            await registerInfoService.AddRegisterStatus(request);
            return CommonGrpcHelper.CreateStatusOK();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/AddRegisterStatus");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
    }

    public override async Task<UploadFileResponse> UploadFile(IAsyncStreamReader<FileContent> requestStream, ServerCallContext context)
    {
        var reply = new UploadFileResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            using var stream = new MemoryStream();
            var sourceId = Guid.Empty;
            var fileName = string.Empty;
            var contentType = string.Empty;
            int sourceType = 0;
            while (await requestStream.MoveNext())
            {
                var buffer = requestStream.Current.Buffer.ToByteArray();
                await stream.WriteAsync(buffer, 0, requestStream.Current.ReadedByte);
                if (sourceId == Guid.Empty)
                {
                    sourceId = Guid.Parse(requestStream.Current.FileInfo.SourceId);
                    sourceType = requestStream.Current.FileInfo.SourceTypeId;
                    fileName = requestStream.Current.FileName;
                    contentType = requestStream.Current.ContentType;
                }
            }
            stream.Position = 0;
            reply.MetaFileId = (await registerInfoService.UploadFile(stream.ToArray(), fileName, contentType, sourceType, sourceId)).ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/UploadFile");
            reply.Status =  CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task DownloadFile(FileDownLoadMessage request, IServerStreamWriter<FileContent> responseStream, ServerCallContext context)
    {
        try
        {
            (var metaFile, var fileData, var contentType) = await registerInfoService.DownloadFile(request.Id.ToGuid() ?? Guid.Empty);
            using var stream = new MemoryStream(fileData);
            byte[] buffer = new byte[2048];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var fileContent = new FileContent
                {
                    ContentType = contentType,
                    FileInfo = new FileMessage
                    {
                        SourceId = metaFile.SourceId,
                        SourceTypeId = metaFile.FileSourceTypeId,
                    },
                    FileName = metaFile.FileName,
                    ReadedByte = bytesRead,
                    Buffer = ByteString.CopyFrom(buffer, 0, bytesRead)
                };
                await responseStream.WriteAsync(fileContent);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/DownLoadFile");
        }
    }
    public override async Task<GetRegisterResponse> GetRegisterAndMarkAsStarted(GetRegisterByCodeRequest request, ServerCallContext context)
    {
        var reply = new GetRegisterResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data = await registerInfoService.GetRegisterByCode(request);
            await registerInfoService.SetRegisterAsStarted(reply.Data?.Id ?? 0);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetRegisterAndMarkAsStarted"+ request.RegisterCode);
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<AdministrationUicList> GetAdministrationUicList(Empty request, ServerCallContext context)
    {
        var reply = new AdministrationUicList
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data.AddRange(await registerInfoService.GetAdministrationUicList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetAdministrationUicList");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<ServiceList> GetServiceList(Empty request, ServerCallContext context)
    {
        var reply = new ServiceList
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data.AddRange(await registerInfoService.GetServiceList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetServiceList");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<ResultStatus> SaveService(ServiceItem request, ServerCallContext context)
    {
        try
        {
            await registerInfoService.SaveService(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/SaveService");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return CommonGrpcHelper.CreateStatusOK();
    }

    public override async Task<RegisterStatusListResponse> GetRegisterStatusList(RegisterStatusRequest request, ServerCallContext context)
    {
        var reply = new RegisterStatusListResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data.AddRange(await registerInfoService.GetRegisterStatusList(request.RegisterId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/GetAdministrationUicList");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<GetAdministrationResponse> GetAdministrationNameByUic(StringValue uic, ServerCallContext context)
    {
        var reply = new GetAdministrationResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data = await registerInfoService.GetAdministrationNameByUic(uic);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"RegisterService/{nameof(GetAdministrationNameByUic)}");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<ResultStatus> SaveCalendarDay(CalendarDayItem request, ServerCallContext context)
    {
        try
        {
            await registerInfoService.SaveCalendarDay(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/SaveService");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return CommonGrpcHelper.CreateStatusOK();
    }

    public override async Task<CalendarDayResponse> GetCalendarDay(CalendarDayRequest request, ServerCallContext context)
    {
        var reply = new CalendarDayResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data = await registerInfoService.GetCalendarDay(request.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"RegisterService/{nameof(GetAdministrationNameByUic)}");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }
    public override async Task<CalendarDayList> GetCalendarDayList(CalendarDayListRequest request, ServerCallContext context)
    {
        var reply = new CalendarDayList
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            (var data, var countAll) = await registerInfoService.GetCalendarDayList(request);
            reply.Data.AddRange(data);
            reply.CountAll = countAll;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"RegisterService/{nameof(GetCalendarDayList)}");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }
    public override async Task<CalendarDayCalcResponse> CalcWorkDays(CalendarDayCalcRequest request, ServerCallContext context)
    {
        var reply = new CalendarDayCalcResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            var toDate = await registerInfoService.CalcWorkDays(request.FromDate.ToDateTime().ConvertUtcToBGTime().Date, request.Days);
            reply.ToDate = toDate.SetToUtc().ToTimestamp();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"RegisterService/{nameof(CalcWorkDays)}");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<RegisterStatusResponse> GetRegisterStatus(GetRegisterStatusRequest request, ServerCallContext context)
    {
        var reply = new RegisterStatusResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data = await registerInfoService.GetRegisterStatus(request.RegisterStatusId.ToGuid() ?? Guid.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"RegisterService/{nameof(GetRegisterStatus)}");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;
    }

    public override async Task<OpenDataParamResponse> GetOpenDataParam(OpenDataParamRequest request, ServerCallContext context)
    {
        var reply = new OpenDataParamResponse
        {
            Status = CommonGrpcHelper.CreateStatusOK()
        };
        try
        {
            reply.Data = await registerInfoService.GetOpenDataParam(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"RegisterService/{nameof(GetOpenDataParam)}");
            reply.Status = CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return reply;

    }

    public override async Task<ResultStatus> SaveOpenDataRegister(OpenDataRegisterSaveRequest request, ServerCallContext context)
    {
        try
        {
            await registerInfoService.SaveOpenDataRegister(request);
}
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/SaveOpenDataRegister");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return CommonGrpcHelper.CreateStatusOK();
    }

    public override async Task<ResultStatus> SaveOpenDataAdministration(OpenDataAdministrationSaveRequest request, ServerCallContext context)
    {
        try
        {
            await registerInfoService.SaveOpenDataAdministration(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/SaveOpenDataAdministration");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return CommonGrpcHelper.CreateStatusOK();
    }

    public override async Task<ResultStatus> SaveOpenDataRegisterAdministration(OpenDataRegisterAdministrationSaveRequest request, ServerCallContext context)
    {
        try
        {
            await registerInfoService.SaveOpenDataRegisterAdministration(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/SaveOpenDataRegisterAdministration");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return CommonGrpcHelper.CreateStatusOK();
    }

    public override async Task<ResultStatus> SaveOpenDataRegisterAdministrationMeta(OpenDataRegisterAdministrationMetaSaveRequest request, ServerCallContext context)
    {
        try
        {
            await registerInfoService.SaveOpenDataRegisterAdministrationMeta(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterService/SaveOpenDataRegisterAdministrationMeta");
            return CommonGrpcHelper.CreateStatusInternalServerError(ex);
        }
        return CommonGrpcHelper.CreateStatusOK();
    }
}
