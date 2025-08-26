using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RulesEngine.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Contracts;

namespace URegister.Infrastructure.Filters
{
    public class ServerAuditLogInterceptor(IAuditInfo auditInfo): Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            SetAuditInfo(request, MethodType.Unary, context);
            return await continuation(request, context);
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            ServerCallContext context,
            ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            SetAuditInfo(requestStream, MethodType.ClientStreaming, context);
            return base.ClientStreamingServerHandler(requestStream, context, continuation);
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(
            TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            SetAuditInfo(request, MethodType.ServerStreaming, context);
            return base.ServerStreamingServerHandler(request, responseStream, context, continuation);
        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            SetAuditInfo(requestStream, MethodType.DuplexStreaming, context);
            return base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
        }

        private void SetAuditInfo<TRequest>(TRequest request, MethodType methodType, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();
            var userIdStr = context.RequestHeaders.FirstOrDefault(m => string.Equals(m.Key, "userid", StringComparison.Ordinal))?.Value;
            Guid userId = Guid.Empty;
            Guid.TryParse(userIdStr, out userId);
            var regiserIdStr = context.RequestHeaders.FirstOrDefault(m => string.Equals(m.Key, "registerid", StringComparison.Ordinal))?.Value;
            int regiserId = 0;
            int.TryParse(regiserIdStr, out regiserId);
            var administration = context.RequestHeaders.FirstOrDefault(m => string.Equals(m.Key, "administrationid", StringComparison.Ordinal))?.Value;
            var administrationId = Guid.Empty;
            Guid.TryParse(administration, out administrationId);
            
            auditInfo.Action = context.Method;
            auditInfo.ActivityId = httpContext.TraceIdentifier.ToString();
            auditInfo.ActivityFromId = context.RequestHeaders.FirstOrDefault(m => string.Equals(m.Key, "activityid", StringComparison.Ordinal))?.Value;
            auditInfo.AssemblyName = Assembly.GetExecutingAssembly()?.GetName()?.Name!;
            auditInfo.Method = methodType.ToString();
            auditInfo.Controller = "GRPC";
            auditInfo.IpAddress = null!;
            auditInfo.Parameters = JsonConvert.SerializeObject(request);
            auditInfo.UserId = userId != Guid.Empty ? userId : null;
            auditInfo.AdministrationId = administrationId != Guid.Empty ? administrationId : Guid.Empty;
            auditInfo.RegisterId = regiserId;
            auditInfo.UserFullNameBase64 = context.RequestHeaders.FirstOrDefault(m => string.Equals(m.Key, "userfullname", StringComparison.Ordinal))?.Value;
        }
    }
}

