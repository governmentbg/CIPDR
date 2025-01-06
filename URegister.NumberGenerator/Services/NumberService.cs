using Grpc.Core;
using URegister.Common;
using URegister.NumberGenerator.Contracts;

namespace URegister.NumberGenerator.Services;

/// <summary>
/// gRpc услуга за генериране на номера
/// <param name="logger"></param>
/// <param name="numberGeneratorService"></param>
/// </summary>
public class NumberService(
        ILogger<NumberService> logger,
        INumberGeneratorService numberGeneratorService) : NumberGenerator.NumberGeneratorBase
{
    /// <summary>
    /// Генериране на номер
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<NumberReply> GetNumber(NumberRequest request, ServerCallContext context)
    {
        NumberReply reply = new NumberReply()
        {
            Status = CreateStatusOK()
        };

        try
        {
            reply.Number = await numberGeneratorService.GenerateNumber(request.Register, Guid.Parse(request.InitialDocumentId));
        }
        catch (ArgumentException aex)
        {
            logger.LogError(aex, "NumberService/GetNumber");
            
            reply.Status.Code = ResultCodes.BadRequest;
            reply.Status.Message = aex.Message;
            reply.Number = 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NumberService/GetNumber");

            reply.Status.Code = ResultCodes.InternalServerError;
            reply.Status.Message = ex.Message;
            reply.Number = 0;
        }

        return reply;
    }

    /// <summary>
    /// Генериране на номер
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<NumberReply> GetNumberForExternalSystem(ExternalNumberRequest request, ServerCallContext context)
    {
        NumberReply reply = new NumberReply()
        {
            Status = CreateStatusOK()
        };

        try
        {
            reply.Number = await numberGeneratorService.GenerateNumberForExternalSystem(request.Ebk, request.SystemName, request.InitialDocumentNumber);
        }
        catch (ArgumentException aex)
        {
            logger.LogError(aex, "NumberService/GetNumber");

            reply.Status.Code = ResultCodes.BadRequest;
            reply.Status.Message = aex.Message;
            reply.Number = 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NumberService/GetNumber");

            reply.Status.Code = ResultCodes.InternalServerError;
            reply.Status.Message = ex.Message;
            reply.Number = 0;
        }

        return reply;
    }

    /// <summary>
    /// Проверка на номер
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<CheckNumberReply> CheckNumber(CheckNumberRequest request, ServerCallContext context)
    {
        CheckNumberReply reply = new CheckNumberReply()
        {
            Status = CreateStatusOK()
        };

        try
        {
            reply.IsValid = await numberGeneratorService.ValidateNumber(request.Number);
        }
        catch (ArgumentException aex)
        {
            logger.LogError(aex, "NumberService/CheckNumber");

            reply.Status.Code = ResultCodes.BadRequest;
            reply.Status.Message = aex.Message;
            reply.IsValid = false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "NumberService/CheckNumber");

            reply.Status.Code = ResultCodes.InternalServerError;
            reply.Status.Message = ex.Message;
            reply.IsValid = false;
        }

        return reply;
    }

    private ResultStatus CreateStatusOK()
    {
        return new ResultStatus
        {
            Code = ResultCodes.Ok
        };
    }
}
