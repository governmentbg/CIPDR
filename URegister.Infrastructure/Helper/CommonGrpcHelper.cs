using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Common;

namespace URegister.Infrastructure.Helper
{
    public static class CommonGrpcHelper
    {
        public static ResultStatus CreateStatusOK()
        {
            return new ResultStatus
            {
                Code = ResultCodes.Ok
            };
        }
        public static ResultStatus CreateStatusBadRequest(Exception ex)
        {
            return new ResultStatus
            {
                Code = ResultCodes.BadRequest,
                Message = ex.Message
            };
        }
        public static ResultStatus CreateStatusBadRequest(string message)
        {
            return new ResultStatus
            {
                Code = ResultCodes.BadRequest,
                Message = message
            };
        }
        public static ResultStatus CreateStatusInternalServerError(Exception ex)
        {
            return new ResultStatus
            {
                Code = ResultCodes.InternalServerError,
                Message = ex.Message
            };
        }
        public static ResultStatus CreateStatusUniqueIndexError(Exception ex)
        {
            if (ex.InnerException != null)
            {
                if (ex.InnerException is PostgresException)
                {
                    var pexp = ex.InnerException as PostgresException;
                    if (pexp?.SqlState == "23505")
                    {
                        return CreateStatusBadRequest(ex);
                    }
                }
            }
            return CreateStatusInternalServerError(ex);
        }


    }
}
